namespace ResourceSpider.Agent.Config;

/// <summary>
/// 本地模式配置选项，定义本地模式下任务文件的加载路径和结果输出方式
/// </summary>
public class LocalModeOptions
{
    /// <summary>
    /// 任务文件路径，默认 "./tasks"
    /// </summary>
    public string TaskFilePath { get; set; } = "./tasks";

    /// <summary>
    /// 任务配置目录，存放 JSON 格式的任务配置文件
    /// </summary>
    public string? TaskDirectory { get; set; }

    /// <summary>
    /// 结果输出路径，默认 "./results"
    /// </summary>
    public string ResultOutputPath { get; set; } = "./results";

    /// <summary>
    /// 输出目录，采集结果文件的存储根目录
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// 输出格式，支持 "csv"、"json"、"txt"，默认 "csv"
    /// </summary>
    public string OutputFormat { get; set; } = "csv";

    /// <summary>
    /// 本地模式最大并发任务数，默认 5
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 5;
}
