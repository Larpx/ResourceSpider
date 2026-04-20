namespace ResourceSpider.Agent.Config;

public class LocalModeOptions
{
    public string TaskFilePath { get; set; } = "./tasks";
    public string ResultOutputPath { get; set; } = "./results";
    public string OutputFormat { get; set; } = "csv";
    public int MaxConcurrentTasks { get; set; } = 5;
}
