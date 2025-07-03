
namespace Vanguard_DB.Models;
/// <summary>
/// 数据库连接字符串信息
/// </summary>
public class DBConnectionStringInfo
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DbType { get; set; } = "SqlServer";
}
