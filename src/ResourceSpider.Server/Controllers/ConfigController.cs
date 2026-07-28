using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Services;

namespace Larpx.PersonalTools.ResourceSpider.Server.Controllers;

/// <summary>
/// 配置控制器，提供表达式测试和配置模板功能
/// 支持在线测试 XPath、CSS 选择器、正则表达式和 JSONPath 等提取表达式
/// </summary>
[ApiController]
[Route("api/admin/config")]
[Authorize]
public class ConfigController : ControllerBase
{
    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private readonly ILogger<ConfigController> _logger;

    /// <summary>
    /// 初始化配置控制器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ConfigController(ILogger<ConfigController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 测试提取表达式，根据指定的表达式类型对内容进行提取
    /// 支持 XPath、CSS 选择器、正则表达式和 JSONPath 四种表达式类型
    /// </summary>
    /// <param name="request">测试提取请求，包含内容、表达式和表达式类型</param>
    /// <returns>提取成功返回结果列表，失败返回错误信息</returns>
    [HttpPost("test-extraction")]
    [ProducesResponseType(typeof(ApiResponse<TestExtractionResponse>), 200)]
    public IActionResult TestExtraction([FromBody] TestExtractionRequest request)
    {
        try
        {
            var results = new List<string>();

            switch (request.ExpressionType.ToLower())
            {
                case "xpath":
                    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.LoadHtml(request.Content);
                    var xpathNodes = htmlDoc.DocumentNode.SelectNodes(request.Expression);
                    if (xpathNodes != null)
                    {
                        results.AddRange(xpathNodes.Select(n => n.InnerText.Trim()));
                    }
                    break;

                case "cssselector":
                    results = Larpx.PersonalTools.ResourceSpider.Infrastructure.Parser.AngleSharpCssParser.Extract(request.Content, request.Expression);
                    break;

                case "regex":
                    var regexMatches = System.Text.RegularExpressions.Regex.Matches(request.Content, request.Expression);
                    foreach (System.Text.RegularExpressions.Match match in regexMatches)
                    {
                        results.Add(match.Groups.Count > 1 ? match.Groups[1].Value : match.Value);
                    }
                    break;

                case "jsonpath":
                    var jsonData = Newtonsoft.Json.Linq.JToken.Parse(request.Content);
                    var jsonResults = jsonData.SelectTokens(request.Expression);
                    foreach (var token in jsonResults)
                    {
                        results.Add(token.ToString());
                    }
                    break;

                default:
                    return BadRequest(ApiResponse<TestExtractionResponse>.Error(10101, "不支持的表达式类型"));
            }

            return Ok(ApiResponse<TestExtractionResponse>.Success(
                new TestExtractionResponse(true, results, null)));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "表达式测试失败");
            return Ok(ApiResponse<TestExtractionResponse>.Success(
                new TestExtractionResponse(false, null, ex.Message)));
        }
    }

    /// <summary>
    /// 获取配置模板列表，用于快速创建任务配置
    /// </summary>
    /// <returns>配置模板列表</returns>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<List<ConfigTemplateDto>>), 200)]
    public IActionResult GetTemplates()
    {
        var templates = new List<ConfigTemplateDto>();
        return Ok(ApiResponse<List<ConfigTemplateDto>>.Success(templates));
    }
}
