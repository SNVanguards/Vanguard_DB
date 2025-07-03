

namespace Vanguard_DB.Providers;

/// <summary>
/// 委托，用于获取仓储实例
/// </summary>
public delegate IRepository RepositoryProvider(string dbCode = "Default"!);
