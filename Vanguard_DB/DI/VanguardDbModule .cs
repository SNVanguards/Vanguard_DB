

namespace Vanguard_DB.DI;
/// <summary>
///  负责仓储/多数据库自动切换/AutoMapper 等所有依赖的注入配置。
/// </summary>
public class VanguardDbModule : Autofac.Module
{
    /// <summary>
    /// 读取Appsetting.json
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 读取程序集
    /// </summary>
    private readonly Assembly[] _assemblies;

    /// <summary>
    /// 构造函数，传入全局配置与Profile所在程序集（支持自动注册AutoMapper Profile）。
    /// </summary>
    public VanguardDbModule(IConfiguration configuration, Assembly[] assemblies)
    {
        _configuration = configuration;
        _assemblies = assemblies;
    }

    /// <summary>
    /// 注册依赖注入相关的所有服务（自动多数据库支持、AutoMapper、仓储工厂）。
    /// </summary>
    protected override void Load(ContainerBuilder builder)
    {
        // 1. 注册 SqlSugar 多数据库工厂
        //    通过 dbCode 自动查找 appsettings.json 的连接字符串和数据库类型，实例化 SqlSugarClient
        builder.Register(ctx =>
        {
            // 读取所有数据库配置段
            var configSection = _configuration.GetSection("ConnectionStrings");
            // 解析成字典 { dbCode => DBConnectionStringInfo }
            var dbDict = configSection.GetChildren()
                .ToDictionary(
                    s => s.Key,
                    s => s.Get<DBConnectionStringInfo>()
                );

            // 返回工厂方法：传入 dbCode，返回对应 SqlSugarClient
            return new Func<string, SqlSugarClient>(dbCode =>
            {
                // 查找对应数据库配置
                if (!dbDict.TryGetValue(dbCode, out var info))
                    throw new InvalidOperationException($"未配置库 {dbCode}");

                // 解析字符串类型到 SqlSugar.DbType 枚举
                if (!Enum.TryParse<SqlSugar.DbType>(info.DbType, true, out var dbType))
                    throw new InvalidOperationException($"不支持的DbType: {info.DbType}");

                // 创建 SqlSugarClient
                return new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = info.ConnectionString,
                    DbType = dbType,
                    InitKeyType = InitKeyType.Attribute, // 优先走特性映射
                    IsAutoCloseConnection = true
                });
            });
        })
        .As<Func<string, SqlSugarClient>>()   // 以工厂委托形式注入
        .SingleInstance();

        // 2. 注册 AutoMapper（自动扫描所有 Profile）
        builder.Register(ctx =>
        {
            // 新建 AutoMapper 配置，自动扫描 assemblies 下的所有 Profile
            var cfg = new MapperConfiguration(c =>
            {
                var profiles = _assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => typeof(Profile).IsAssignableFrom(t)
                                && !t.IsAbstract && !t.IsInterface);
                foreach (var p in profiles)
                    c.AddProfile((Profile)Activator.CreateInstance(p)!);
            });
            // 创建并返回 IMapper 实例
            return cfg.CreateMapper();
        })
        .As<IMapper>()
        .InstancePerLifetimeScope();

        // 3. 注册内部的 RepositoryFactory
        builder.RegisterType<RepositoryFactory>()
            .AsSelf()
            .InstancePerLifetimeScope()
            .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

        // 4. 对外只注册 RepositoryProvider（这是个委托！业务只拿这个委托提供的DbContext！）
        builder.Register(ctx =>
        {
            var factory = ctx.Resolve<RepositoryFactory>();
            return new RepositoryProvider(dbCode => factory.Repository(dbCode));
        })
        .As<RepositoryProvider>()
        .InstancePerLifetimeScope();

        //5. 自动注册所有继承了Servicebase的类为瞬时服务
        builder.RegisterAssemblyTypes(_assemblies)
                .Where(t => typeof(DBServiceBase).IsAssignableFrom(t) && !t.IsAbstract)
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired();

    }

    
}
