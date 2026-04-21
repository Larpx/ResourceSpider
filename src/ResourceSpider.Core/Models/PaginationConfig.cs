using ResourceSpider.Core.Enums;

namespace ResourceSpider.Core.Models;

public class PaginationConfig
{
    public PaginationType PaginationType { get; set; }

    public string? PageParamName { get; set; }

    public int StartPage { get; set; } = 1;

    public int? EndPage { get; set; }

    public int PageIncrement { get; set; } = 1;

    public string? NextPageSelector { get; set; }

    public int ScrollWaitTime { get; set; } = 2000;

    public int? MaxPages { get; set; }

    public string? UrlPattern { get; set; }
}
