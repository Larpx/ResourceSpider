using Xunit;
using Shouldly;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.MessageQueue;
using ResourceSpider.Core.Models;

namespace ResourceSpider.Tests.Unit;

/// <summary>
/// 内存消息队列单元测试
/// </summary>
public class InMemoryMessageQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_ShouldReturnMessage()
    {
        var ct = TestContext.Current.CancellationToken;
        var queue = new InMemoryMessageQueue();
        var message = new TestMessage { Id = 1, Name = "test" };

        await queue.EnqueueAsync(message, ct);
        var result = await queue.DequeueAsync<TestMessage>(ct);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(1);
        result.Name.ShouldBe("test");
    }

    [Fact]
    public async Task TryEnqueue_ShouldReturnTrue()
    {
        var ct = TestContext.Current.CancellationToken;
        var queue = new InMemoryMessageQueue();
        var result = await queue.TryEnqueueAsync("test message", ct);
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Dequeue_EmptyQueue_ShouldThrowOnCancellation()
    {
        var queue = new InMemoryMessageQueue();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            queue.DequeueAsync<string>(cts.Token));
    }

    [Fact]
    public async Task Enqueue_MultipleMessages_ShouldReturnInOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var queue = new InMemoryMessageQueue();
        await queue.EnqueueAsync("first", ct);
        await queue.EnqueueAsync("second", ct);
        await queue.EnqueueAsync("third", ct);

        var result1 = await queue.DequeueAsync<string>(ct);
        var result2 = await queue.DequeueAsync<string>(ct);
        var result3 = await queue.DequeueAsync<string>(ct);

        result1.ShouldBe("first");
        result2.ShouldBe("second");
        result3.ShouldBe("third");
    }
}

/// <summary>
/// HashSet 去重器单元测试
/// </summary>
public class HashSetDuplicateRemoverTests
{
    [Fact]
    public async Task AddAndCheck_ShouldReturnTrue()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var fingerprint = "test-fingerprint";

        await remover.AddAsync(fingerprint, ct);
        var isDuplicate = await remover.IsDuplicateAsync(fingerprint, ct);

        isDuplicate.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckNonExistent_ShouldReturnFalse()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var isDuplicate = await remover.IsDuplicateAsync("non-existent", ct);
        isDuplicate.ShouldBeFalse();
    }

    [Fact]
    public async Task GetCount_ShouldReturnCorrectCount()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();

        await remover.AddAsync("fp1", ct);
        await remover.AddAsync("fp2", ct);
        var count = await remover.GetCountAsync(ct);

        count.ShouldBe(2);
    }

    [Fact]
    public async Task AddDuplicate_ShouldNotIncreaseCount()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();

        await remover.AddAsync("fp1", ct);
        await remover.AddAsync("fp1", ct);
        var count = await remover.GetCountAsync(ct);

        count.ShouldBe(1);
    }
}

/// <summary>
/// 广度优先调度器单元测试
/// </summary>
public class BreadthFirstSchedulerTests
{
    [Fact]
    public async Task AddDuplicateRequest_ShouldNotAdd()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.BreadthFirstScheduler(remover);

        var request1 = new Request { Url = "http://example.com", Fingerprint = "fp1" };
        var request2 = new Request { Url = "http://example.com/2", Fingerprint = "fp1" };

        await scheduler.EnqueueAsync(new[] { request1 }, ct);
        await scheduler.EnqueueAsync(new[] { request2 }, ct);

        var isDuplicate = await scheduler.IsDuplicateAsync(request2, ct);
        isDuplicate.ShouldBeTrue();
    }

    [Fact]
    public async Task Dequeue_ShouldReturnRequestsInOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.BreadthFirstScheduler(remover);

        var request1 = new Request { Url = "http://example.com/1", Fingerprint = "fp1" };
        var request2 = new Request { Url = "http://example.com/2", Fingerprint = "fp2" };

        await scheduler.EnqueueAsync(new[] { request1, request2 }, ct);
        var requests = (await scheduler.DequeueAsync(2, ct)).ToList();

        requests.Count.ShouldBe(2);
        requests[0].Url.ShouldBe("http://example.com/1");
        requests[1].Url.ShouldBe("http://example.com/2");
    }

    [Fact]
    public async Task Dequeue_MoreThanAvailable_ShouldReturnOnlyAvailable()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.BreadthFirstScheduler(remover);

        var request = new Request { Url = "http://example.com", Fingerprint = "fp1" };
        await scheduler.EnqueueAsync(new[] { request }, ct);

        var requests = await scheduler.DequeueAsync(10, ct);
        requests.Count().ShouldBe(1);
    }
}

/// <summary>
/// 深度优先调度器单元测试
/// </summary>
public class DepthFirstSchedulerTests
{
    [Fact]
    public async Task Dequeue_ShouldReturnInReverseOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.DepthFirstScheduler(remover);

        var request1 = new Request { Url = "http://example.com/1", Fingerprint = "fp1" };
        var request2 = new Request { Url = "http://example.com/2", Fingerprint = "fp2" };

        await scheduler.EnqueueAsync(new[] { request1, request2 }, ct);
        var requests = (await scheduler.DequeueAsync(2, ct)).ToList();

        requests.Count.ShouldBe(2);
        requests[0].Url.ShouldBe("http://example.com/2");
        requests[1].Url.ShouldBe("http://example.com/1");
    }
}

/// <summary>
/// 请求指纹生成器单元测试
/// </summary>
public class RequestFingerprinterTests
{
    [Fact]
    public void GenerateFingerprint_SameRequest_ShouldReturnSameFingerprint()
    {
        var request1 = new Request { Url = "http://example.com", Method = "GET" };
        var request2 = new Request { Url = "http://example.com", Method = "GET" };

        var fp1 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request1);
        var fp2 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request2);

        fp1.ShouldBe(fp2);
    }

    [Fact]
    public void GenerateFingerprint_DifferentRequest_ShouldReturnDifferentFingerprint()
    {
        var request1 = new Request { Url = "http://example.com/1", Method = "GET" };
        var request2 = new Request { Url = "http://example.com/2", Method = "GET" };

        var fp1 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request1);
        var fp2 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request2);

        fp1.ShouldNotBe(fp2);
    }

    [Fact]
    public void GenerateFingerprint_DifferentMethod_ShouldReturnDifferentFingerprint()
    {
        var request1 = new Request { Url = "http://example.com", Method = "GET" };
        var request2 = new Request { Url = "http://example.com", Method = "POST" };

        var fp1 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request1);
        var fp2 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request2);

        fp1.ShouldNotBe(fp2);
    }
}

/// <summary>
/// 核心模型单元测试
/// </summary>
public class ModelTests
{
    [Fact]
    public void SpiderTask_DefaultValues_ShouldBeCorrect()
    {
        var task = new SpiderTask();

        task.TaskId.ShouldNotBeNullOrEmpty();
        task.TaskType.ShouldBe(Core.Enums.TaskType.SinglePage);
        task.Status.ShouldBe(Core.Enums.TaskStatus.Pending);
        task.Priority.ShouldBe(5);
        task.ConfigVersion.ShouldBe(1);
        task.RequestConfig.ShouldBeNull();
    }

    [Fact]
    public void Request_DefaultValues_ShouldBeCorrect()
    {
        var request = new Request();

        request.RequestId.ShouldNotBeNullOrEmpty();
        request.Method.ShouldBe("GET");
        request.Priority.ShouldBe(5);
        request.MaxRetry.ShouldBe(3);
        request.Status.ShouldBe(Core.Enums.RequestStatus.Pending);
        request.Headers.ShouldNotBeNull();
        request.Metadata.ShouldNotBeNull();
    }

    [Fact]
    public void Response_TextContent_ShouldDecodeUtf8()
    {
        var response = new Response
        {
            Content = System.Text.Encoding.UTF8.GetBytes("Hello World")
        };

        response.TextContent.ShouldBe("Hello World");
    }

    [Fact]
    public void Response_EmptyContent_TextContentShouldBeNull()
    {
        var response = new Response();

        response.TextContent.ShouldBeNull();
    }

    [Fact]
    public void Proxy_Address_ShouldCombineHostAndPort()
    {
        var proxy = new Proxy { Host = "192.168.1.1", Port = 8080 };

        proxy.Address.ShouldBe("192.168.1.1:8080");
    }

    [Fact]
    public void DataRecord_DefaultValues_ShouldBeCorrect()
    {
        var record = new DataRecord();

        record.RecordId.ShouldNotBeNullOrEmpty();
        record.Fields.ShouldNotBeNull();
        record.FieldExpressionMap.ShouldNotBeNull();
    }
}

/// <summary>
/// 常量定义单元测试
/// </summary>
public class ConstantsTests
{
    [Fact]
    public void ApiRoutes_ShouldHaveCorrectPrefix()
    {
        Core.Constants.ApiRoutes.AgentRegister.ShouldStartWith("api/");
        Core.Constants.ApiRoutes.AgentHeartbeat.ShouldStartWith("api/");
        Core.Constants.ApiRoutes.Tasks.ShouldStartWith("api/");
    }

    [Fact]
    public void Defaults_ShouldHaveReasonableValues()
    {
        Core.Constants.Defaults.DefaultHttpMethod.ShouldBe("GET");
        Core.Constants.Defaults.DefaultPriority.ShouldBe(5);
        Core.Constants.Defaults.DefaultRetryCount.ShouldBe(3);
        Core.Constants.Defaults.DefaultMaxRetry.ShouldBe(3);
    }

    [Fact]
    public void ExecutionStatus_ShouldHaveExpectedValues()
    {
        Core.Constants.ExecutionStatus.Success.ShouldBe("Success");
        Core.Constants.ExecutionStatus.Failed.ShouldBe("Failed");
    }
}

/// <summary>
/// ApiResponse 单元测试
/// </summary>
public class ApiResponseTests
{
    [Fact]
    public void Success_ShouldReturnCorrectResponse()
    {
        var response = Core.Models.ApiResponse<string>.Success("data", "ok");

        response.Code.ShouldBe(200);
        response.Message.ShouldBe("ok");
        response.Data.ShouldBe("data");
    }

    [Fact]
    public void Error_ShouldReturnCorrectResponse()
    {
        var response = Core.Models.ApiResponse<string>.Error(500, "server error");

        response.Code.ShouldBe(500);
        response.Message.ShouldBe("server error");
        response.Data.ShouldBeNull();
    }
}

/// <summary>
/// 选择器工厂单元测试
/// </summary>
public class SelectorFactoryTests
{
    [Fact]
    public void Selectors_Regex_ShouldCreateRegexSelector()
    {
        var selector = Infrastructure.Selector.Selectors.Regex(@"\d+");
        selector.ShouldNotBeNull();
    }

    [Fact]
    public void Selectors_Css_ShouldCreateCssSelector()
    {
        var selector = Infrastructure.Selector.Selectors.Css("div.content");
        selector.ShouldNotBeNull();
    }

    [Fact]
    public void Selectors_XPath_ShouldCreateXPathSelector()
    {
        var selector = Infrastructure.Selector.Selectors.XPath("//div");
        selector.ShouldNotBeNull();
    }

    [Fact]
    public void Selectors_JsonPath_ShouldCreateJsonPathSelector()
    {
        var selector = Infrastructure.Selector.Selectors.JsonPath("$.data");
        selector.ShouldNotBeNull();
    }
}

public class TestMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
