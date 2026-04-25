using System.Collections.Concurrent;
using System.Threading.Channels;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace ResourceSpider.Server.Observability;

/// <summary>
/// 运行时输出日志项，用于在管理页面展示应用输出。
/// </summary>
/// <param name="Sequence">自增序号，用于保持日志顺序</param>
/// <param name="TimestampUtc">日志时间（UTC）</param>
/// <param name="Level">日志级别</param>
/// <param name="Source">日志来源</param>
/// <param name="Message">日志消息</param>
public record RuntimeOutputEntry(
    long Sequence,
    DateTime TimestampUtc,
    string Level,
    string Source,
    string Message
);

/// <summary>
/// 应用输出缓冲区，保存最近固定数量日志，用于实时监控页面读取。
/// </summary>
public static class RuntimeOutputStream
{
    private const int MaxEntries = 1000;
    private static readonly ConcurrentQueue<RuntimeOutputEntry> _entries = new();
    private static readonly Channel<RuntimeOutputEntry> _channel = Channel.CreateUnbounded<RuntimeOutputEntry>();
    private static long _sequence;

    /// <summary>
    /// 写入一条运行时输出。
    /// </summary>
    /// <param name="timestampUtc">时间（UTC）</param>
    /// <param name="level">级别</param>
    /// <param name="source">来源</param>
    /// <param name="message">消息</param>
    public static void Write(DateTime timestampUtc, string level, string source, string message)
    {
        var sequence = Interlocked.Increment(ref _sequence);
        var entry = new RuntimeOutputEntry(sequence, timestampUtc, level, source, message);

        _entries.Enqueue(entry);
        _channel.Writer.TryWrite(entry);

        while (_entries.Count > MaxEntries)
        {
            _entries.TryDequeue(out _);
        }
    }

    /// <summary>
    /// 获取当前输出快照，按时间顺序返回。
    /// </summary>
    /// <returns>输出列表</returns>
    public static List<RuntimeOutputEntry> Snapshot()
    {
        return _entries.ToList();
    }

    /// <summary>
    /// 实时读取输出流，用于推送到监控端。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步输出流</returns>
    public static IAsyncEnumerable<RuntimeOutputEntry> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}

/// <summary>
/// Serilog Sink：将程序输出写入内存缓冲，供后台实时查看。
/// </summary>
public sealed class RuntimeOutputSink : ILogEventSink
{
    private static readonly MessageTemplateTextFormatter _formatter = new("{Message:lj}{NewLine}{Exception}");

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        _formatter.Format(logEvent, writer);

        var source = logEvent.Properties.TryGetValue("SourceContext", out var sourceValue)
            ? sourceValue.ToString().Trim('"')
            : "Application";

        RuntimeOutputStream.Write(
            logEvent.Timestamp.UtcDateTime,
            logEvent.Level.ToString(),
            source,
            writer.ToString().Trim());
    }
}
