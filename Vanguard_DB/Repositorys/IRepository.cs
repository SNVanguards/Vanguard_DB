namespace Vanguard_DB.Repositorys;

/// <summary>
///     通用泛型仓储接口。<br/>
///     通过依赖倒置隐藏具体 ORM（默认由 <c>SqlSugar</c> 实现），
///     为应用层提供一致的数据访问 API。<br/>
///     <para>
///     ✦ <strong>全部方法均异步</strong>，并接受 <see cref="CancellationToken"/> 以支持请求取消。<br/>
///     ✦ <strong>不直接暴露事务</strong>，事务应由 <see cref="IUnitOfWork"/> 协调。<br/>
///     ✦ <em>注意：</em>：业务层仅依赖此接口，便于后期切换 ElasticSearch 装饰器、Dapper 极速通道等实现。
///     </para>
/// </summary>
public interface IRepository
{
    #region 通用

    /// <summary>
    ///     返回 <typeparamref name="T"/> 的 <see cref="IQueryable{T}"/> 查询管道，
    ///     可继续链式追加过滤、排序等表达式，再由具体实现执行。
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <returns>延迟执行的查询序列</returns>
    ISugarQueryable<T> IQueryable<T>() where T : class;

    #endregion 通用

    #region 原生 SQL

    /// <summary>
    ///     执行原生 SQL 并映射为 <typeparamref name="T"/> 集合。
    /// </summary>
    /// <typeparam name="T">要映射的目标类型</typeparam>
    /// <param name="sql">SQL 语句</param>
    /// <param name="parameters">匿名对象或参数数组，<c>null</c> 则无参数</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>实体列表</returns>
    Task<List<T>> QueryBySqlAsync<T>(
        string sql,
        object? parameters = null,
        CancellationToken ct = default) where T : class;

    /// <summary>
    ///     执行原生 SQL 并返回 <see cref="DataTable"/>（仅在无法直接映射成实体时使用）。
    /// </summary>
    /// <param name="sql">SQL 语句</param>
    /// <param name="parameters">参数数组，可空</param>
    /// <param name="ct">取消令牌</param>
    Task<DataTable> QueryDataTableAsync(
        string sql,
        SqlParameter[]? parameters = null,
        CancellationToken ct = default);

    /// <summary>
    ///     执行非查询类 SQL（INSERT / UPDATE / DELETE / DDL）。
    /// </summary>
    /// <param name="sql">SQL 语句</param>
    /// <param name="parameters">参数，可空</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>受影响行数</returns>
    Task<int> ExecuteSqlAsync(
        string sql,
        SqlParameter[]? parameters = null,
        CancellationToken ct = default);

    #endregion 原生 SQL

    #region 增

    /// <summary>
    ///     新增单个实体，并返回已持久化的实体（含数据库生成的字段，如自增主键）。  
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="entity">待新增的实体实例</param>
    /// <param name="ct">可选的取消令牌，用于中断操作</param>
    /// <returns>持久化后、带有数据库生成值（如主键）的实体对象</returns>
    Task<TEntity> InsertAsync<TEntity>(
        TEntity entity,
        CancellationToken ct = default)
        where TEntity : class , new();

    /// <summary>
    ///     批量新增实体（普通批量模式）。  
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="values">要新增的实体列表</param>
    /// <param name="ct">可选的取消令牌，用于中断操作</param>
    /// <returns>
    ///     <c>true</c> 表示至少有一条记录成功插入；  
    ///     <c>false</c> 则表示全部失败或列表为空。
    /// </returns>
    Task<bool> InsertListAsync<TEntity>(
        IReadOnlyList<TEntity> values,
        CancellationToken ct = default)
        where TEntity : class, new();

    /// <summary>
    ///     批量新增实体（高速批量模式），通常内部使用 <c>SqlBulkCopy</c> 或 SqlSugar Fastest。  
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="values">要新增的实体列表</param>
    /// <param name="ct">可选的取消令牌，用于中断操作</param>
    /// <returns>
    ///     <c>true</c> 表示至少有一条记录成功插入；  
    ///     <c>false</c> 则表示全部失败或列表为空。
    /// </returns>
    Task<bool> BulkInsertListAsync<TEntity>(
        IReadOnlyList<TEntity> values,
        CancellationToken ct = default)
        where TEntity : class, new();

    #endregion 增

    #region 删

    /// <summary>
    ///     物理删除单个实体。
    /// </summary>
    Task<bool> DeleteAsync<T>(
        T entity,
        CancellationToken ct = default) where T : class, new();

    /// <summary>
    ///     根据条件批量删除。
    /// </summary>
    Task<bool> DeleteAsync<T>(
        Expression<Func<T, bool>> where,
        CancellationToken ct = default) where T : class, new();

    #endregion 删

    #region 改

    /// <summary>
    ///     更新实体。<br/>
    ///     <b>默认</b>：更新实体所有映射字段。<br/>
    ///     当 <paramref name="ignoreProps"/> 不为空时，将<strong>忽略</strong>这些属性，不对它们执行更新。  
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="entity">待更新的实体实例</param>
    /// <param name="ignoreProps">
    ///     需要<strong>排除</strong>在 UPDATE 语句之外的属性表达式列表。<br/>
    ///     例如：<c>repo.UpdateAsync(user, x => x.CreatedTime, x => x.CreatorId)</c>
    ///     将保留 <c>CreatedTime</c>、<c>CreatorId</c> 原值，仅更新其它字段。
    /// </param>
    /// <param name="ct">可选的取消令牌，用于中断操作</param>
    /// <returns><c>true</c> 表示受影响行数 &gt; 0</returns>
    Task<bool> UpdateAsync<TEntity>(
        TEntity entity,
         CancellationToken ct = default,
        params Expression<Func<TEntity, object>>[]? ignoreProps
        )
        where TEntity : class, new();

    #endregion 改

    #region 查

    /// <summary>
    ///     查询列表，可指定过滤与投影。
    /// </summary>
    Task<IReadOnlyList<T>> GetListAsync<T>(
        Expression<Func<T, bool>>? where = null,
        Expression<Func<T, T>>? select = null,
        CancellationToken ct = default) where T : class;

    /// <summary>
    ///     通用分页查询，返回指定实体类型的分页结果。  
    ///     支持可选的过滤、排序和取消令牌。  
    /// </summary>
    /// <typeparam name="TEntity">要查询的实体类型</typeparam>
    /// <param name="page">
    ///     分页请求信息，包含当前页码和每页大小。  
    ///     例如：new PageRequest(1, 20) 表示第 1 页，每页 20 条。
    /// </param>
    /// <param name="filter">
    ///     可选的过滤条件；传 <c>null</c> 表示不过滤。  
    ///     示例：x => x.IsActive == true
    /// </param>
    /// <param name="orderBy">
    ///     可选的排序字段表达式；传 <c>null</c> 表示不排序。  
    ///     示例：x => x.CreatedTime
    /// </param>
    /// <param name="desc">
    ///     指定排序方向：  
    ///     <c>true</c> - 按 <paramref name="orderBy"/> 降序；  
    ///     <c>false</c> - 按 <paramref name="orderBy"/> 升序（默认）。
    /// </param>
    /// <param name="ct">
    ///     取消令牌，用于在外部调用 <c>CancellationToken.Cancel()</c> 时中断查询操作；  
    ///     默认值为 <see cref="CancellationToken.None"/>。
    /// </param>
    /// <returns>
    ///     包含以下信息的 <see cref="PagedResult{TEntity}"/>：  
    ///     <list type="bullet">
    ///       <item><description><c>Items</c>：当前页的数据列表。</description></item>
    ///       <item><description><c>Total</c>：总记录数。</description></item>
    ///       <item><description><c>Page</c>：当前页码。</description></item>
    ///       <item><description><c>PageSize</c>：每页大小。</description></item>
    ///     </list>
    /// </returns>
    Task<PagedResult<TEntity>> GetPagedDataAsync<TEntity>(
        PageRequest page,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool desc = false,
        CancellationToken ct = default)
        where TEntity : class;

    /// <summary>
    ///     分页查询并<strong>自动映射</strong>为指定 DTO 类型。  
    ///     <list type="bullet">
    ///     <item><description>内部先按 <typeparamref name="TEntity"/> 查询分页数据，再用 <c>IMapper</c> 转为 <typeparamref name="TDto"/>。</description></item>
    ///     <item><description>若实现层使用支持 <c>ProjectTo</c> 的 ORM，可改为数据库端投影以减少列数。</description></item>
    ///     </list>
    /// </summary>
    /// <typeparam name="TDto">目标 DTO 类型</typeparam>
    /// <typeparam name="TEntity">实体类型（数据库映射）</typeparam>
    /// <param name="page">
    ///     分页请求信息，包含页码与大小。
    /// </param>
    /// <param name="desc">
    ///     是否按 <paramref name="orderBy"/> 字段降序。  
    ///     <c>true</c> - 降序；<c>false</c> - 升序（默认）。
    /// </param>
    /// <param name="filter">
    ///     过滤条件；为空则不过滤。
    /// </param>
    /// <param name="orderBy">
    ///     排序字段表达式；为空则使用默认排序。
    /// </param>
    /// <param name="ct">
    ///     取消令牌，支持请求级别取消。
    /// </param>
    /// <returns>
    ///     <see cref="PagedResult{T}"/>，其 <c>Items</c> 集合已映射为 <typeparamref name="TDto"/>。
    /// </returns>
    /// <remarks>
    ///     <para>示例调用：</para>
    ///     <code><![CDATA[
    ///     var page = new PageRequest(1, 20);
    ///     var result = await _repo.GetPagedDtoAsync<UserDto, UserEntity>(
    ///         page,
    ///         desc: true,
    ///         filter: u => u.IsActive,
    ///         orderBy: u => u.RegisterTime,
    ///         ct: cancellationToken);
    ///     ]]></code>
    /// </remarks>
    Task<PagedResult<TDto>> GetPagedDtoAsync<TDto, TEntity>(
        PageRequest page,
        bool desc = false,
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        CancellationToken ct = default)
        where TDto : class
        where TEntity : class;


    /// <summary>
    ///     查询实体后<strong>自动映射</strong>为指定 DTO 类型，返回全部结果（无分页）。  
    ///     <list type="bullet">
    ///       <item>
    ///         <description>
    ///           先按 <typeparamref name="TEntity"/> 执行查询（可过滤/排序），
    ///           再由 <c>IMapper</c> 将结果映射为 <typeparamref name="TDto"/> 列表。
    ///         </description>
    ///       </item>
    ///       <item>
    ///         <description>
    ///           若实现层支持数据库端投影（如 SqlSugar 的 <c>Select&lt;TDto&gt;()</c>
    ///           或 EF Core + AutoMapper <c>ProjectTo&lt;TDto&gt;()</c>），可改为 SQL 侧映射，
    ///           仅 SELECT DTO 所需列。
    ///         </description>
    ///       </item>
    ///     </list>
    /// </summary>
    /// <typeparam name="TDto">目标 DTO 类型</typeparam>
    /// <typeparam name="TEntity">实体类型（数据库映射）</typeparam>
    /// <param name="filter">可选过滤条件；传 <c>null</c> 表示不过滤</param>
    /// <param name="orderBy">可选排序字段表达式；传 <c>null</c> 表示不排序</param>
    /// <param name="desc">
    ///     排序方向：<c>true</c> 表示降序；<c>false</c> 表示升序（默认）。
    /// </param>
    /// <param name="ct">可选的取消令牌，用于中断操作</param>
    /// <returns>已映射为 <typeparamref name="TDto"/> 的结果列表</returns>
    Task<IReadOnlyList<TDto>> GetDtoListAsync<TDto, TEntity>(
        Expression<Func<TEntity, bool>>? filter = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool desc = false,
        CancellationToken ct = default)
        where TDto : class
        where TEntity : class;


    /// <summary>
    ///     根据条件返回首条记录，若无则返回 <c>null</c>。
    /// </summary>
    Task<T?> GetFirstAsync<T>(
        Expression<Func<T, bool>> where,
        CancellationToken ct = default) where T : class;

    /// <summary>
    ///     判断是否存在符合条件的记录。
    /// </summary>
    Task<bool> ExistAsync<T>(
        Expression<Func<T, bool>> where,
        CancellationToken ct = default) where T : class;

    #endregion 查
}