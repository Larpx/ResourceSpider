using Xunit;
using FluentAssertions;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.MessageQueue;

namespace ResourceSpider.Tests.Unit;

public class MessageQueueTests
{
    [Fact]
    public async Task InMemoryMessageQueue_EnqueueAndDequeue_ShouldReturnMessage()
    {
        var queue = new InMemoryMessageQueue();
        var message = new TestMessage { Id = 1, Name = "test" };
        
        await queue.EnqueueAsync(message);
        var result = await queue.DequeueAsync<TestMessage>();
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("test");
    }

    [Fact]
    public async Task InMemoryMessageQueue_TryEnqueue_ShouldReturnTrue()
    {
        var queue = new InMemoryMessageQueue();
        var message = "test message";
        
        var result = await queue.TryEnqueueAsync(message);
        
        result.Should().BeTrue();
    }
}

public class TestMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DuplicateRemoverTests
{
    [Fact]
    public async Task HashSetDuplicateRemover_AddAndCheck_ShouldReturnTrue()
    {
        var remover = new HashSetDuplicateRemover();
        var fingerprint = "test-fingerprint";
        
        await remover.AddAsync(fingerprint);
        var isDuplicate = await remover.IsDuplicateAsync(fingerprint);
        
        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public async Task HashSetDuplicateRemover_CheckNonExistent_ShouldReturnFalse()
    {
        var remover = new HashSetDuplicateRemover();
        
        var isDuplicate = await remover.IsDuplicateAsync("non-existent");
        
        isDuplicate.Should().BeFalse();
    }

    [Fact]
    public async Task HashSetDuplicateRemover_GetCount_ShouldReturnCorrectCount()
    {
        var remover = new HashSetDuplicateRemover();
        
        await remover.AddAsync("fp1");
        await remover.AddAsync("fp2");
        var count = await remover.GetCountAsync();
        
        count.Should().Be(2);
    }
}

public class SchedulerTests
{
    [Fact]
    public async Task BreadthFirstScheduler_AddDuplicateRequest_ShouldNotAdd()
    {
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.BreadthFirstScheduler(remover);
        
        var request1 = new Core.Models.Request { Url = "http://example.com", Fingerprint = "fp1" };
        var request2 = new Core.Models.Request { Url = "http://example.com/2", Fingerprint = "fp1" };
        
        await scheduler.AddRequestAsync(request1);
        await scheduler.AddRequestAsync(request2);
        
        var isDuplicate = await scheduler.IsDuplicateAsync(request2);
        isDuplicate.Should().BeTrue();
    }

    [Fact]
    public async Task BreadthFirstScheduler_GetRequests_ShouldReturnRequestsInOrder()
    {
        var remover = new HashSetDuplicateRemover();
        var scheduler = new Infrastructure.Scheduler.BreadthFirstScheduler(remover);
        
        var request1 = new Core.Models.Request { Url = "http://example.com/1", Fingerprint = "fp1" };
        var request2 = new Core.Models.Request { Url = "http://example.com/2", Fingerprint = "fp2" };
        
        await scheduler.AddRequestAsync(request1);
        await scheduler.AddRequestAsync(request2);
        var requests = await scheduler.GetRequestsAsync(2);
        
        requests.Should().HaveCount(2);
    }
}

public class RequestFingerprinterTests
{
    [Fact]
    public void GenerateFingerprint_SameRequest_ShouldReturnSameFingerprint()
    {
        var request1 = new Core.Models.Request { Url = "http://example.com", Method = "GET" };
        var request2 = new Core.Models.Request { Url = "http://example.com", Method = "GET" };
        
        var fp1 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request1);
        var fp2 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request2);
        
        fp1.Should().Be(fp2);
    }

    [Fact]
    public void GenerateFingerprint_DifferentRequest_ShouldReturnDifferentFingerprint()
    {
        var request1 = new Core.Models.Request { Url = "http://example.com/1", Method = "GET" };
        var request2 = new Core.Models.Request { Url = "http://example.com/2", Method = "GET" };
        
        var fp1 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request1);
        var fp2 = Infrastructure.Utils.RequestFingerprinter.GenerateFingerprint(request2);
        
        fp1.Should().NotBe(fp2);
    }
}
