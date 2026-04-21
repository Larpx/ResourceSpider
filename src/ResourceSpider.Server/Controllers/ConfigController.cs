using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/config")]
[Authorize]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ILogger<ConfigController> logger)
    {
        _logger = logger;
    }

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
                    results = ResourceSpider.Infrastructure.Parser.AngleSharpCssParser.Extract(request.Content, request.Expression);
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

    [HttpGet("templates")]
    [ProducesResponseType(typeof(ApiResponse<List<ConfigTemplateDto>>), 200)]
    public IActionResult GetTemplates()
    {
        var templates = new List<ConfigTemplateDto>();
        return Ok(ApiResponse<List<ConfigTemplateDto>>.Success(templates));
    }
}
