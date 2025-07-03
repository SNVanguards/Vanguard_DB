


namespace Vanguard_DB.DI;

public static class VanguardDbHelperExtensions
{
    /// <summary>
    /// 快速注册 VanguardDbModule，调用本方法启用泛型仓储连接数据库
    /// </summary>
    /// <param name="builder">WebApplicationBuilder</param>
    /// <param name="assemblies">需要扫描的程序集（推荐传 typeof(XXX).Assembly）</param>
    /// <returns>builder本身，支持链式调用</returns>
    public static WebApplicationBuilder AddVanguardDbHelper(
        this WebApplicationBuilder builder,
        params Assembly[] assemblies)
    {
        // 启用 Autofac 作为 DI 容器
        builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        // 注册 VanguardDbModule
        builder.Host.ConfigureContainer<ContainerBuilder>((context, cb) =>
        {
            cb.RegisterModule(new VanguardDbModule(context.Configuration, assemblies));
        });
        return builder;
    }
}
