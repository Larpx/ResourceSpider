using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

[ApiController]
[Route("api/proxies")]
[Authorize]
public class ProxyController : ControllerBase
{
    private readonly IProxyService _proxyService;

    public ProxyController(IProxyService proxyService)
    {
        _proxyService = proxyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProxyDto>>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _proxyService.GetListAsync(pageIndex, pageSize);
        return Ok(ApiResponse<List<ProxyDto>>.Success(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProxyDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Add([FromBody] CreateProxyRequest request)
    {
        var result = await _proxyService.AddAsync(request);
        return Ok(ApiResponse<ProxyDto>.Success(result, "Proxy added successfully"));
    }

    [HttpDelete("{proxyId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(string proxyId)
    {
        var result = await _proxyService.DeleteAsync(proxyId);
        if (!result)
        {
            return NotFound(ApiResponse<object>.Error(404, "Proxy not found"));
        }
        return Ok(ApiResponse<object>.Success(new { }, "Proxy deleted"));
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<ProxyTestResponse>), 200)]
    public async Task<IActionResult> Test([FromBody] ProxyTestRequest request)
    {
        var result = await _proxyService.TestAsync(request);
        return Ok(ApiResponse<ProxyTestResponse>.Success(result));
    }
}
