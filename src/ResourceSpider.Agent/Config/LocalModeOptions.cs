namespace ResourceSpider.Agent.Config;

/// <summary>
/// 本地模式配置选项，定义本地任务文件路径和输出参数
/// </summary>
public class LocalModeOptions
{
    /// <summary>
    /// 本地任务文件目录路径，默认为 ./tasks
    /// </summary>
    public string TaskFilePath { get; set; } = "./tasks";

    /// <summary>
    /// 采集结果输出目录路径，默认为 ./results
    /// </summary>
    public string ResultOutputPath { get; set; } = "./results";

    /// <summary>
    /// 输出文件格式，支持 csv、json 等，默认为 csv
    /// </summary>
    public string OutputFormat { get; set; } = "csv";

    /// <summary>
    /// 最大并发任务数，默认为 5
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 5;
}
