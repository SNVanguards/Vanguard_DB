
namespace Vanguard_DB.UnitOfWork;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// 开启事务（若已存在则创建 save-point）
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task BeginAsync(CancellationToken ct = default);

    /// <summary>
    ///提交事务
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// 回滚事务
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task RollbackAsync(CancellationToken ct = default);
}
