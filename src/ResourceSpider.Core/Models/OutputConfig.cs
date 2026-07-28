namespace Larpx.PersonalTools.ResourceSpider.Core.Models;

/// <summary>
/// 输出配置模型，定义数据输出的字段选择和去重策略
/// </summary>
public class OutputConfig
{
    /// <summary>
    /// 输出字段列表，指定哪些字段需要包含在输出中
    /// </summary>
    public List<string> OutputFields { get; set; } = new();

    /// <summary>
    /// 去重字段列表，基于这些字段判断数据是否重复
    /// </summary>
    public List<string> DedupFields { get; set; } = new();
}
