


namespace Vanguard_DB.Factory;

/// <summary>
///     仓储工厂实现，按需缓存已创建的 <see cref="IRepository"/>，支持多数据库切换。
/// </summary>
/// <summary>
///     仓储工厂实现类，支持多数据库切换与仓储实例复用。
///     负责根据 dbCode 选择正确数据库，并为每个库维护独立仓储实例（IRepository）。
/// </summary>
public class RepositoryFactory : IRepositoryFactory
{
    //DI框架注入
    private Func<string, SqlSugarClient> _dbFactory;
    private IMapper _mapper;
    private readonly ConcurrentDictionary<string, IRepository> _repoCache = new();

    // 由容器进行属性注入
    public Func<string, SqlSugarClient> DbFactory
    {
        private get => _dbFactory;
        set => _dbFactory = value;
    }
    public IMapper Mapper
    {
        private get => _mapper;
        set => _mapper = value;
    }

    /// <summary>
    /// 获取/创建指定库的仓储。
    /// </summary>
    /// <param name="dbCode">数据库标识（默认 Default）</param>
    /// <returns>对应 IRepository 实例</returns>
    public IRepository Repository(string dbCode = "Default")
    {
        if (_dbFactory == null || _mapper == null)
            throw new InvalidOperationException("RepositoryFactory未正确注入依赖");

        return _repoCache.GetOrAdd(dbCode, code =>
        {
            var db = _dbFactory(code);
            return new Repository(db, _mapper);
        });
    }
}
