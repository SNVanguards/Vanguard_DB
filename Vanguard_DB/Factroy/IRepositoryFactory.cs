
namespace Vanguard_DB.Factory;


/// <summary>
///     仓储工厂接口，按数据库代号（dbCode）动态获取 <see cref="IRepository"/> 实例。<br/>
///     用于支持多数据库/多租户、主从库切换等高级场景。
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    ///     获取指定数据库代号（dbCode）对应的 <see cref="IRepository"/> 实例。<br/>
    ///     dbCode 通常为配置文件或上下文指定，如 "Default"、"ERP"、"CRM" 等。
    /// </summary>
    /// <param name="dbCode">数据库代号（如 "Default"）</param>
    /// <returns>与 dbCode 绑定的仓储实现</returns>
    IRepository Repository(string dbCode = "Default");
}
