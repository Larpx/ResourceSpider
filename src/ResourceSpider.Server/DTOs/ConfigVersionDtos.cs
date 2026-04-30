namespace ResourceSpider.Server.DTOs;

/// <summary>
/// 配置版本数据传输对象
/// </summary>
/// <param name="VersionId">版本 ID</param>
/// <param name="TaskId">关联任务 ID</param>
/// <param name="Version">版本号</param>
/// <param name="ConfigContent">配置内容 JSON</param>
/// <param name="ChangeDescription">变更描述</param>
/// <param name="CreatedBy">创建者</param>
/// <param name="CreatedAt">创建时间</param>
public record ConfigVersionDto(
    string VersionId,
    string TaskId,
    int Version,
    string ConfigContent,
    string? ChangeDescription,
    string? CreatedBy,
    DateTime CreatedAt
);

/// <summary>
/// 配置版本列表响应
/// </summary>
/// <param name="Versions">版本列表</param>
/// <param name="Total">总数</param>
public record ConfigVersionListResponse(
    List<ConfigVersionDto> Versions,
    int Total
);

/// <summary>
/// 回滚配置请求
/// </summary>
/// <param name="Version">目标版本号</param>
public record RollbackConfigRequest(
    int Version
);

/// <summary>
/// 配置差异项，表示两个版本之间某个字段的变更
/// </summary>
/// <param name="FieldPath">字段路径（点分隔，如 RequestConfig.Url）</param>
/// <param name="OldValue">源版本的值</param>
/// <param name="NewValue">目标版本的值</param>
/// <param name="ChangeType">变更类型：Added（新增）、Modified（修改）、Removed（删除）</param>
public record ConfigDiffItem(
    string FieldPath,
    string? OldValue,
    string? NewValue,
    string ChangeType
);
