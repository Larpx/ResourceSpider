namespace Larpx.PersonalTools.ResourceSpider.Core.Enums;

/// <summary>
/// 结果数据去重策略枚举，定义不同的去重方式
/// </summary>
public enum DeduplicationStrategy
{
    /// <summary>
    /// 不去重，保留所有数据
    /// </summary>
    None,

    /// <summary>
    /// 基于 URL 去重，相同 SourceUrl 的记录视为重复
    /// </summary>
    Url,

    /// <summary>
    /// 基于指定字段组合去重，字段名由 DeduplicationConfig.DeduplicationFields 定义
    /// </summary>
    FieldCombination,

    /// <summary>
    /// 基于全字段指纹去重，所有字段完全一致视为重复
    /// </summary>
    FullFingerprint,

    /// <summary>
    /// 基于主键字段去重，由 DeduplicationConfig.PrimaryKeyFields 指定主键字段
    /// </summary>
    PrimaryKey
}
