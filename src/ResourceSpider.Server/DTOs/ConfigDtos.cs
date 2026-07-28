using System.ComponentModel.DataAnnotations;

namespace Larpx.PersonalTools.ResourceSpider.Server.DTOs;

/// <summary>
/// 测试提取表达式请求
/// </summary>
/// <param name="Content">待测试的内容</param>
/// <param name="ExpressionType">表达式类型（xpath/cssselector/regex/jsonpath），最大长度 32</param>
/// <param name="Expression">提取表达式，最大长度 1024</param>
/// <param name="IsHtml">内容是否为 HTML，默认 true</param>
public record TestExtractionRequest(
    [Required] string Content,
    [Required, StringLength(32)] string ExpressionType,
    [Required, StringLength(1024)] string Expression,
    bool IsHtml = true
);

/// <summary>
/// 测试提取表达式响应
/// </summary>
/// <param name="Success">是否提取成功</param>
/// <param name="Results">提取结果列表</param>
/// <param name="Error">错误信息</param>
public record TestExtractionResponse(
    bool Success,
    List<string>? Results,
    string? Error
);

/// <summary>
/// 测试页面提取请求，从指定 URL 获取内容后进行提取
/// </summary>
/// <param name="Url">目标页面 URL，最大长度 2048</param>
/// <param name="ExpressionType">表达式类型，最大长度 32</param>
/// <param name="Expression">提取表达式，最大长度 1024</param>
/// <param name="Method">HTTP 方法，默认 GET，最大长度 16</param>
/// <param name="Body">请求体，可选</param>
/// <param name="Headers">请求头，可选</param>
public record TestPageRequest(
    [Required, StringLength(2048)] string Url,
    [Required, StringLength(32)] string ExpressionType,
    [Required, StringLength(1024)] string Expression,
    [StringLength(16)] string Method = "GET",
    string? Body = null,
    Dictionary<string, string>? Headers = null
);

/// <summary>
/// 测试页面提取响应
/// </summary>
/// <param name="Success">是否提取成功</param>
/// <param name="Results">提取结果列表</param>
/// <param name="RawContent">原始页面内容</param>
/// <param name="Error">错误信息</param>
public record TestPageResponse(
    bool Success,
    List<string>? Results,
    string? RawContent,
    string? Error
);

/// <summary>
/// 配置模板数据传输对象
/// </summary>
/// <param name="TemplateId">模板 ID</param>
/// <param name="Name">模板名称</param>
/// <param name="Description">模板描述</param>
/// <param name="ConfigContent">模板配置内容</param>
public record ConfigTemplateDto(
    string TemplateId,
    string Name,
    string Description,
    string ConfigContent
);
