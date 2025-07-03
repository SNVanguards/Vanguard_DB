
namespace Vanguard_DB.Repositorys;

/// <inheritdoc/>
public class Repository : IRepository
{
    private readonly SqlSugarClient _db;
    private readonly IMapper _mapper;

    /// <summary>
    ///     构造函数，注入 SqlSugarClient 和 AutoMapper 的 IMapper。
    /// </summary>
    /// <param name="db">SqlSugar 的数据库客户端</param>
    /// <param name="mapper">AutoMapper 的映射实例</param>
    public Repository(SqlSugarClient db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    #region 通用

    /// <inheritdoc/>
    public ISugarQueryable<T> IQueryable<T>() where T : class
    {
        return _db.Queryable<T>();
    }

    #endregion

    #region 原生 SQL

    /// <inheritdoc/>
    public async Task<List<T>> QueryBySqlAsync<T>(string sql, object? parameters = null, CancellationToken ct = default) where T : class
    {
        return await _db.Ado.SqlQueryAsync<T>(sql, parameters, ct);
    }

    /// <inheritdoc/>
    public async Task<DataTable> QueryDataTableAsync(
     string sql,
     SqlParameter[]? parameters = null,
     CancellationToken ct = default)
    {
        // 1) 准备连接，强转到 DbConnection
        var dbConn = (DbConnection)_db.Ado.Connection;
        if (dbConn.State != ConnectionState.Open)
        {
            await dbConn.OpenAsync(ct);
        }

        // 2) 创建命令，绑定事务
        using var cmd = dbConn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        if (_db.Ado.Transaction is DbTransaction tx)
            cmd.Transaction = tx;

        // 3) 挂参数
        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                var adoParam = cmd.CreateParameter();
                adoParam.ParameterName = p.ParameterName;
                adoParam.Value = p.Value ?? DBNull.Value;
                cmd.Parameters.Add(adoParam);
            }
        }

        // 4) 异步执行并装载
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var dt = new DataTable();
        dt.Load(reader);
        return dt;
    }



    /// <inheritdoc/>
    public async Task<int> ExecuteSqlAsync(string sql, SqlParameter[]? parameters = null, CancellationToken ct = default)
    {
        return await _db.Ado.ExecuteCommandAsync(sql, parameters, ct);
    }

    #endregion

    #region 增

    /// <inheritdoc/>
    public async Task<TEntity> InsertAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : class,new()
    {
        // 执行插入，支持取消
        var count = await _db.Insertable(entity)
                             .ExecuteCommandAsync(ct);

        if (count <= 0)
        {
            throw new InvalidOperationException("插入失败，无任何行受影响");
        }

        // SqlSugar 会把自增主键自动赋到 entity 对象上
        return entity;
    }

    /// <inheritdoc/>
    public async Task<bool> InsertListAsync<TEntity>(IReadOnlyList<TEntity> values, CancellationToken ct = default) where TEntity : class , new()
    {
        if (values == null || values.Count == 0) return false;
        var count = await _db.Insertable<TEntity>(values).ExecuteCommandAsync(ct);
        return count > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> BulkInsertListAsync<TEntity>(IReadOnlyList<TEntity> values, CancellationToken ct = default) where TEntity : class, new()
    {
        if (values == null || values.Count == 0)
            return false;

        // 1) 构造 DataTable
        var dt = new DataTable();
        var props = typeof(TEntity).GetProperties();
        foreach (var p in props)
            dt.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
        foreach (var item in values)
        {
            var row = dt.NewRow();
            foreach (var p in props)
                row[p.Name] = p.GetValue(item) ?? DBNull.Value;
            dt.Rows.Add(row);
        }

        // 2) 打开底层 SqlConnection
        var conn = (SqlConnection)_db.Ado.Connection;
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // 3) 批量写入
        using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, _db.Ado.Transaction as SqlTransaction)
        {
            DestinationTableName = _db.EntityMaintenance.GetTableName<TEntity>(),
            BatchSize = 5000
        };
        foreach (DataColumn col in dt.Columns)
            bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

        await bulk.WriteToServerAsync(dt, ct);
        return true;
    }

    #endregion

    #region 删

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync<T>(T entity, CancellationToken ct = default) where T : class,new()
    {
        var count = await _db.Deleteable(entity).ExecuteCommandAsync(ct);
        return count > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync<T>(Expression<Func<T, bool>> where, CancellationToken ct = default) where T : class, new()
    {
        var count = await _db.Deleteable<T>().Where(where).ExecuteCommandAsync(ct);
        return count > 0;
    }

    #endregion

    #region 改

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync<TEntity>(
        TEntity entity,
        CancellationToken ct = default,
        params Expression<Func<TEntity, object>>[]? ignoreProps)
        where TEntity : class, new()
    {
        // 1) 构造 Updateable
        var updater = _db.Updateable(entity);

        // 2) 如果指定了要忽略的属性，解析出列名并调用 IgnoreColumns(string[])
        if (ignoreProps != null && ignoreProps.Length > 0)
        {
            var cols = ignoreProps
                .Select(exp =>
                {
                    if (exp.Body is MemberExpression m) return m.Member.Name;
                    if (exp.Body is UnaryExpression u && u.Operand is MemberExpression mm)
                        return mm.Member.Name;
                    throw new InvalidOperationException($"无法解析表达式 {exp}");
                })
                .ToArray();

            updater = updater.IgnoreColumns(cols);
        }

        // 3) 真正执行更新，传入 CancellationToken
        var count = await updater.ExecuteCommandAsync(ct);

        // 4) 返回是否有行被影响
        return count > 0;
    }



    #endregion

    #region 查

    /// <inheritdoc/>
    public async Task<IReadOnlyList<T>> GetListAsync<T>(Expression<Func<T, bool>>? where = null,
                                                        Expression<Func<T, T>>? select = null,
                                                        CancellationToken ct = default) where T : class
    {
        var q = _db.Queryable<T>();
        if (where != null) q = q.Where(where);
        if (select != null) q = q.Select(select);
        return await q.ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PagedResult<TEntity>> GetPagedDataAsync<TEntity>(
        PageRequest page,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool desc = false,
        CancellationToken ct = default)
        where TEntity : class
    {
        // 1) 
        var q = _db.Queryable<TEntity>();

        // 2) 可选过滤
        if (filter != null)
        {
            q = q.Where(filter);
        }

        // 3) 可选排序（Expression<Func<TEntity, object>> 完全匹配 OrderBy）
        if (orderBy != null)
        {
            q = desc
                ? q.OrderBy(orderBy, OrderByType.Desc)
                : q.OrderBy(orderBy, OrderByType.Asc);
        }

        // 4) 分页查询，拿到数据和总数
        RefAsync<int> total = 0;
        var items = await q.ToPageListAsync(
            page.PageNumber,
            page.PageSize,
            total,
            ct);

        // 5) 构造并返回 PagedResult<TEntity>
        return new PagedResult<TEntity>(page, items, total);
    }


    /// <inheritdoc/>
    public async Task<PagedResult<TDto>> GetPagedDtoAsync<TDto, TEntity>(
        PageRequest page,
        bool desc = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken ct = default)
        where TDto : class
        where TEntity : class
    {
        // 1) 复用已有的分页查询，拿到实体类型的结果
        var entityPage = await GetPagedDataAsync<TEntity>(
            page, filter, orderBy, desc, ct);

        // 2) 把实体列表映射成 DTO 列表
        //    依赖注入时要保证有 IMapper _mapper 字段
        var dtoItems = _mapper.Map<IReadOnlyList<TDto>>(entityPage.Data);

        // 3) 返回新的分页结果，类型变成 TDto
        return new PagedResult<TDto>(
            page,         // 同样的分页请求
            dtoItems.ToList(),     // 映射后的 DTO 列表
            entityPage.TotalCount  // 总条数不变
        );
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TDto>> GetDtoListAsync<TDto, TEntity>(
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool desc = false,
        CancellationToken ct = default)
        where TDto : class
        where TEntity : class
    {
        var q = _db.Queryable<TEntity>();
        if (filter != null) q = q.Where(filter);
        if (orderBy != null)
        {
            q = desc ? q.OrderBy(orderBy, OrderByType.Desc) : q.OrderBy(orderBy, OrderByType.Asc);
        }
        var entities = await q.ToListAsync(ct);
        return _mapper.Map<IReadOnlyList<TDto>>(entities);
    }

    /// <inheritdoc/>
    public async Task<T?> GetFirstAsync<T>(Expression<Func<T, bool>> where, CancellationToken ct = default) where T : class
    {
        return await _db.Queryable<T>().FirstAsync(where, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ExistAsync<T>(Expression<Func<T, bool>> where, CancellationToken ct = default) where T : class
    {
        // 直接让 SqlSugar 用异步 Count 查询，并传入 ct
        var count = await _db.Queryable<T>()
                             .Where(where)
                             .CountAsync(ct);

        return count > 0;
    }

    #endregion
}