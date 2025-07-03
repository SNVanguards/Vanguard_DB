
namespace Vanguard_DB;

/// <summary>
/// 分页请求
/// </summary>
public class PageRequest
{
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 分页请求实体(关键字)
/// </summary>
/// <typeparam name="TKeyWord">请求关键字</typeparam>
public class PageRequestDto<TKeyWord> : PageRequest
{
    /// <summary>
    /// 请求关键字
    /// </summary>
    public TKeyWord? KeyWord { get; set; }
}




