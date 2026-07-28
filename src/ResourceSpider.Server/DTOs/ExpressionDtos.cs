using System.ComponentModel.DataAnnotations;

namespace Larpx.PersonalTools.ResourceSpider.Server.DTOs;

/// <summary>
/// 创建提取表达式请求
/// </summary>
/// <param name="Name">表达式名称，最大长度 128</param>
/// <param name="Description">表达式描述，最大长度 512，可选</param>
/// <param name="SelectorType">选择器类型（xpath/cssselector/regex/jsonpath），最大长度 32</param>
/// <param name="ContainerExpression">容器表达式，最大长度 1024，可选</param>
/// <param name="Fields">字段列表，可选</param>
public record CreateExpressionRequest(
    [Required, StringLength(128)] string Name,
    [StringLength(512)] string? Description,
    [Required, StringLength(32)] string SelectorType,
    [StringLength(1024)] string? ContainerExpression,
    List<CreateExpressionFieldRequest>? Fields
);

/// <summary>
/// 创建表达式字段请求
/// </summary>
/// <param name="FieldName">字段名称，最大长度 128</param>
/// <param name="SelectorType">选择器类型，最大长度 32</param>
/// <param name="Expression">提取表达式，最大长度 1024</param>
/// <param name="AttributeName">属性名称，最大长度 128，可选</param>
/// <param name="IsRequired">是否必填字段，默认 false</param>
/// <param name="DefaultValue">默认值，最大长度 256，可选</param>
/// <param name="Formatter">格式化器名称，最大长度 64，可选</param>
/// <param name="FormatterArgs">格式化器参数，最大长度 512，可选</param>
public record CreateExpressionFieldRequest(
    [Required, StringLength(128)] string FieldName,
    [Required, StringLength(32)] string SelectorType,
    [Required, StringLength(1024)] string Expression,
    [StringLength(128)] string? AttributeName,
    bool IsRequired = false,
    [StringLength(256)] string? DefaultValue = null,
    [StringLength(64)] string? Formatter = null,
    [StringLength(512)] string? FormatterArgs = null
);

/// <summary>
/// 更新提取表达式请求
/// </summary>
/// <param name="Name">表达式名称，可选</param>
/// <param name="Description">表达式描述，可选</param>
/// <param name="SelectorType">选择器类型，可选</param>
/// <param name="ContainerExpression">容器表达式，可选</param>
/// <param name="Status">表达式状态，可选</param>
/// <param name="Fields">字段列表，可选</param>
public record UpdateExpressionRequest(
    [StringLength(128)] string? Name,
    [StringLength(512)] string? Description,
    [StringLength(32)] string? SelectorType,
    [StringLength(1024)] string? ContainerExpression,
    int? Status,
    List<CreateExpressionFieldRequest>? Fields
);

/// <summary>
/// 提取表达式数据传输对象
/// </summary>
/// <param name="ExpressionId">表达式 ID</param>
/// <param name="Name">表达式名称</param>
/// <param name="Description">表达式描述</param>
/// <param name="SelectorType">选择器类型</param>
/// <param name="ContainerExpression">容器表达式</param>
/// <param name="Fields">字段列表</param>
/// <param name="Status">表达式状态</param>
/// <param name="SuccessCount">成功使用次数</param>
/// <param name="FailureCount">失败次数</param>
/// <param name="ConsecutiveFailures">连续失败次数</param>
/// <param name="LastValidatedAt">最后验证时间</param>
/// <param name="LastUsedAt">最后使用时间</param>
/// <param name="CreatedAt">创建时间</param>
public record ExpressionDto(
    string ExpressionId,
    string Name,
    string Description,
    string SelectorType,
    string ContainerExpression,
    List<ExpressionFieldDto> Fields,
    int Status,
    int SuccessCount,
    int FailureCount,
    int ConsecutiveFailures,
    DateTime? LastValidatedAt,
    DateTime? LastUsedAt,
    DateTime CreatedAt
);

/// <summary>
/// 表达式字段数据传输对象
/// </summary>
/// <param name="FieldId">字段 ID</param>
/// <param name="ExpressionId">所属表达式 ID</param>
/// <param name="FieldName">字段名称</param>
/// <param name="SelectorType">选择器类型</param>
/// <param name="Expression">提取表达式</param>
/// <param name="AttributeName">属性名称</param>
/// <param name="IsRequired">是否必填</param>
/// <param name="DefaultValue">默认值</param>
/// <param name="Formatter">格式化器名称</param>
/// <param name="FormatterArgs">格式化器参数</param>
/// <param name="Order">排序序号</param>
public record ExpressionFieldDto(
    string FieldId,
    string ExpressionId,
    string FieldName,
    string SelectorType,
    string Expression,
    string? AttributeName,
    bool IsRequired,
    string? DefaultValue,
    string? Formatter,
    string? FormatterArgs,
    int Order
);

/// <summary>
/// 表达式列表响应，包含分页信息
/// </summary>
/// <param name="Expressions">表达式列表</param>
/// <param name="Total">总数</param>
/// <param name="PageIndex">当前页码</param>
/// <param name="PageSize">每页数量</param>
public record ExpressionListResponse(
    List<ExpressionDto> Expressions,
    int Total,
    int PageIndex,
    int PageSize
);

/// <summary>
/// 表达式配置数据传输对象，供代理节点使用
/// </summary>
/// <param name="ExpressionId">表达式 ID</param>
/// <param name="Name">表达式名称</param>
/// <param name="SelectorType">选择器类型</param>
/// <param name="ContainerExpression">容器表达式</param>
/// <param name="Fields">字段配置列表</param>
public record ExpressionConfigDto(
    string ExpressionId,
    string Name,
    string SelectorType,
    string ContainerExpression,
    List<ExpressionFieldConfigDto> Fields
);

/// <summary>
/// 表达式字段配置数据传输对象，供代理节点使用
/// </summary>
/// <param name="FieldName">字段名称</param>
/// <param name="SelectorType">选择器类型</param>
/// <param name="Expression">提取表达式</param>
/// <param name="AttributeName">属性名称</param>
/// <param name="IsRequired">是否必填</param>
/// <param name="DefaultValue">默认值</param>
/// <param name="Formatter">格式化器名称</param>
/// <param name="FormatterArgs">格式化器参数</param>
/// <param name="Order">排序序号</param>
public record ExpressionFieldConfigDto(
    string FieldName,
    string SelectorType,
    string Expression,
    string? AttributeName,
    bool IsRequired,
    string? DefaultValue,
    string? Formatter,
    string? FormatterArgs,
    int Order
);

/// <summary>
/// 代理报告表达式可用性请求
/// </summary>
/// <param name="AgentId">代理 ID</param>
/// <param name="AgentToken">代理认证令牌</param>
/// <param name="ExpressionId">表达式 ID</param>
/// <param name="IsAvailable">表达式是否可用</param>
/// <param name="FailureReason">失败原因，最大长度 1024，可选</param>
public record ReportExpressionAvailabilityRequest(
    [Required] string AgentId,
    [Required] string AgentToken,
    [Required] string ExpressionId,
    bool IsAvailable,
    [StringLength(1024)] string? FailureReason
);
