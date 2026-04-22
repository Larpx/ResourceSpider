using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Services;

namespace ResourceSpider.Server.Controllers;

/// <summary>
/// 代理池控制器，提供代理服务器的增删查和测试功能
/// 管理可用的 HTTP 代理资源，支持代理可用性测试
/// </summary>
[ApiController]
[Route("api/proxies")]
[Authorize]
public class ProxyController : ControllerBase
{
    /// <summary>
    /// 代理服务实例，处理代理的业务逻辑
    /// </summary>
    private readonly IProxyService _proxyService;

    /// <summary>
    /// 初始化代理池控制器
    /// </summary>
    /// <param name="proxyService">代理服务</param>
    public ProxyController(IProxyService proxyService)
    {
        _proxyService = proxyService;
    }

    /// <summary>
    /// 获取代理列表，支持分页
    /// </summary>
    /// <param name="pageIndex">页码，默认第 1 页</param>
    /// <param name="pageSize">每页数量，默认 20 条</param>
    /// <returns>代理列表</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProxyDto>>), 200)]
    public async Task<IActionResult> GetList(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _proxyService.GetListAsync(pageIndex, pageSize);
        return Ok(ApiResponse<List<ProxyDto>>.Success(result));
    }

    /// <summary>
    /// 添加新的代理服务器到代理池
    /// </summary>
    /// <param name="request">创建代理请求，包含代理地址、端口和认证信息</param>
    /// <returns>添加成功返回代理详情</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProxyDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Add([FromBody] CreateProxyRequest request)
    {
        var result = await _proxyService.AddAsync(request);
        return Ok(ApiResponse<ProxyDto>.Success(result, "Proxy added successfully"));
    }

    /// <summary>
    /// 删除指定的代理服务器
    /// </summary>
    /// <param name="proxyId">代理 ID</param>
    /// <returns>删除成功返回确认，代理不存在返回 404 状态码</returns>
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

    /// <summary>
    /// 测试代理服务器的可用性，验证代理是否可以正常连接
    /// </summary>
    /// <param name="request">代理测试请求，包含代理地址和目标 URL</param>
    /// <returns>测试结果，包含响应时间和状态</returns>
    [HttpPost("test")]
    [ProducesResponseType(typeof(ApiResponse<ProxyTestResponse>), 200)]
    public async Task<IActionResult> Test([FromBody] ProxyTestRequest request)
    {
        var result = await _proxyService.TestAsync(request);
        return Ok(ApiResponse<ProxyTestResponse>.Success(result));
    }
}
