using Microsoft.Extensions.Logging;
using Moq;
using Larpx.PersonalTools.ResourceSpider.Server.DTOs;
using Larpx.PersonalTools.ResourceSpider.Server.Entities;
using Larpx.PersonalTools.ResourceSpider.Server.Repositories;
using Larpx.PersonalTools.ResourceSpider.Server.Services;
using Shouldly;
using Xunit;

namespace Larpx.PersonalTools.ResourceSpider.Tests.Unit;

/// <summary>
/// <see cref="ProxyService"/> 单元测试。
/// 
/// 这些测试覆盖核心 CRUD 场景：
/// - 新增代理
/// - 列表映射
/// - 删除成功/失败分支
/// </summary>
public class ProxyServiceTests
{
    /// <summary>
    /// 验证：新增代理时会调用仓储写入，并返回映射后的 DTO。
    /// </summary>
    [Fact]
    public async Task AddAsync_ShouldPersistEntityAndReturnDto()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();
        ProxyEntity? savedEntity = null;

        repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ProxyEntity>()))
            .Callback<ProxyEntity>(entity => savedEntity = entity)
            .Returns(Task.CompletedTask);

        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);
        var request = new CreateProxyRequest("127.0.0.1", 8080, "HTTP", "user", "pwd");

        var result = await service.AddAsync(request);

        savedEntity.ShouldNotBeNull();
        result.ProxyId.ShouldNotBeNullOrWhiteSpace();
        result.Host.ShouldBe("127.0.0.1");
        result.Port.ShouldBe(8080);
        result.Protocol.ShouldBe("HTTP");
        result.Username.ShouldBe("user");
    }

    /// <summary>
    /// 验证：查询列表时会正确把实体映射为 DTO。
    /// </summary>
    [Fact]
    public async Task GetListAsync_ShouldMapEntitiesToDtos()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();

        repositoryMock
            .Setup(x => x.GetAllAsync(1, 20))
            .ReturnsAsync(new List<ProxyEntity>
            {
                new()
                {
                    ProxyId = "p1",
                    Host = "10.0.0.1",
                    Port = 9001,
                    Protocol = "HTTP",
                    Username = "u1",
                    Status = 1,
                    SuccessCount = 3,
                    FailureCount = 1
                }
            });

        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);

        var result = await service.GetListAsync(1, 20);

        result.Count.ShouldBe(1);
        result[0].ProxyId.ShouldBe("p1");
        result[0].Host.ShouldBe("10.0.0.1");
        result[0].Status.ShouldBe(1);
        result[0].SuccessCount.ShouldBe(3);
    }

    /// <summary>
    /// 验证：删除存在的代理时，返回 true 且调用删除方法。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenProxyExists_ShouldDeleteAndReturnTrue()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();

        repositoryMock
            .Setup(x => x.GetByIdAsync("p1"))
            .ReturnsAsync(new ProxyEntity { ProxyId = "p1" });

        repositoryMock
            .Setup(x => x.DeleteAsync("p1"))
            .Returns(Task.CompletedTask);

        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);

        var result = await service.DeleteAsync("p1");

        result.ShouldBeTrue();
        repositoryMock.Verify(x => x.DeleteAsync("p1"), Times.Once);
    }

    /// <summary>
    /// 验证：删除不存在的代理时，返回 false 且不会调用删除。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenProxyMissing_ShouldReturnFalse()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();

        repositoryMock
            .Setup(x => x.GetByIdAsync("missing"))
            .ReturnsAsync((ProxyEntity?)null);

        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);

        var result = await service.DeleteAsync("missing");

        result.ShouldBeFalse();
        repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// 验证：当请求里缺少主机或端口时，测试接口直接返回参数错误，不触发网络调用。
    /// </summary>
    [Fact]
    public async Task TestAsync_WhenHostOrPortInvalid_ShouldReturnValidationError()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();
        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);

        var result = await service.TestAsync(new ProxyTestRequest(Host: "", Port: null));

        result.IsAvailable.ShouldBeFalse();
        result.Error.ShouldBe("代理主机或端口无效");
        repositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// 验证：按 ProxyId 测试时，如果代理不存在，应直接返回明确错误信息。
    /// </summary>
    [Fact]
    public async Task TestAsync_WhenProxyIdNotFound_ShouldReturnNotFoundError()
    {
        var repositoryMock = new Mock<IProxyRepository>();
        var loggerMock = new Mock<ILogger<ProxyService>>();

        repositoryMock
            .Setup(x => x.GetByIdAsync("missing"))
            .ReturnsAsync((ProxyEntity?)null);

        var service = new ProxyService(repositoryMock.Object, loggerMock.Object);

        var result = await service.TestAsync(new ProxyTestRequest(ProxyId: "missing"));

        result.IsAvailable.ShouldBeFalse();
        result.Error.ShouldBe("代理不存在");
        repositoryMock.Verify(x => x.GetByIdAsync("missing"), Times.Once);
    }
}
