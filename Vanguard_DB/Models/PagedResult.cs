

namespace Vanguard_DB;

/// <summary>
/// 翻页响应实体
/// </summary>
public class PagedResult<T> where T : class
{
    public PagedResult()
    {

    }

    public PagedResult(PageRequest pageRequest, List<T> datas, int totalCount)
    {
        Data = datas;
        TotalCount = totalCount;
        PageNumber = pageRequest.PageNumber;
        PageSize = pageRequest.PageSize;
    }

    /// <summary>
    /// 数据总量
    /// </summary>
    public int TotalCount { get; set; } = 9990;

    /// <summary>
    /// 当前页面
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 分页大小
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// 数据
    /// </summary>
    public List<T>? Data { get; set; }
}