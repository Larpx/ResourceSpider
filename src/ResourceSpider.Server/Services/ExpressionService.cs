using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

/// <summary>
/// 表达式服务接口，提供采集表达式的增删改查、配置获取及可用性管理功能
/// </summary>
public interface IExpressionService
{
    /// <summary>
    /// 创建新的采集表达式，包含字段定义
    /// </summary>
    /// <param name="request">创建表达式请求</param>
    /// <param name="createdBy">创建者标识</param>
    /// <returns>创建后的表达式 DTO</returns>
    Task<ExpressionDto> CreateAsync(CreateExpressionRequest request, string? createdBy = null);

    /// <summary>
    /// 根据表达式标识获取表达式详情
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <returns>表达式 DTO，若不存在返回 null</returns>
    Task<ExpressionDto?> GetByIdAsync(string expressionId);

    /// <summary>
    /// 分页获取表达式列表，支持按状态筛选
    /// </summary>
    /// <param name="pageIndex">页码（从 1 开始）</param>
    /// <param name="pageSize">每页数量</param>
    /// <param name="status">状态筛选条件，null 表示不筛选</param>
    /// <returns>表达式列表响应</returns>
    Task<ExpressionListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null);

    /// <summary>
    /// 更新表达式信息，包括字段定义的替换
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <param name="request">更新表达式请求</param>
    /// <returns>更新成功返回 true，表达式不存在返回 false</returns>
    Task<bool> UpdateAsync(string expressionId, UpdateExpressionRequest request);

    /// <summary>
    /// 删除表达式及其关联的字段定义
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <returns>删除成功返回 true</returns>
    Task<bool> DeleteAsync(string expressionId);

    /// <summary>
    /// 获取表达式的运行时配置，用于 Agent 端执行采集
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <returns>表达式配置 DTO</returns>
    /// <exception cref="KeyNotFoundException">表达式不存在时抛出</exception>
    Task<ExpressionConfigDto> GetConfigAsync(string expressionId);

    /// <summary>
    /// 上报表达式可用性检测结果，更新成功/失败计数
    /// </summary>
    /// <param name="expressionId">表达式唯一标识</param>
    /// <param name="agentId">检测 Agent 标识</param>
    /// <param name="isAvailable">表达式是否可用</param>
    /// <param name="failureReason">不可用时的失败原因</param>
    Task ReportAvailabilityAsync(string expressionId, string agentId, bool isAvailable, string? failureReason = null);

    /// <summary>
    /// 将连续失败次数超过阈值的表达式标记为失效
    /// </summary>
    /// <param name="consecutiveFailureThreshold">连续失败次数阈值，默认为 5</param>
    Task InvalidateExpiredExpressionsAsync(int consecutiveFailureThreshold = 5);

    /// <summary>
    /// 获取所有活跃状态的表达式配置列表
    /// </summary>
    /// <returns>活跃表达式配置 DTO 列表</returns>
    Task<List<ExpressionConfigDto>> GetActiveExpressionsAsync();
}

/// <summary>
/// 表达式服务实现，管理采集表达式的完整生命周期，包括创建、查询、更新、删除及可用性监控
/// </summary>
public class ExpressionService : IExpressionService
{
    /// <summary>
    /// 表达式数据仓库，用于表达式实体的持久化操作
    /// </summary>
    private readonly IExpressionRepository _expressionRepository;

    /// <summary>
    /// 表达式字段数据仓库，用于字段定义的持久化操作
    /// </summary>
    private readonly IExpressionFieldRepository _fieldRepository;

    /// <summary>
    /// 表达式可用性数据仓库，用于可用性检测记录的持久化操作
    /// </summary>
    private readonly IExpressionAvailabilityRepository _availabilityRepository;

    /// <summary>
    /// 日志记录器，用于记录表达式操作相关事件
    /// </summary>
    private readonly ILogger<ExpressionService> _logger;

    /// <summary>
    /// 初始化表达式服务实例
    /// </summary>
    /// <param name="expressionRepository">表达式数据仓库</param>
    /// <param name="fieldRepository">表达式字段数据仓库</param>
    /// <param name="availabilityRepository">表达式可用性数据仓库</param>
    /// <param name="logger">日志记录器</param>
    public ExpressionService(
        IExpressionRepository expressionRepository,
        IExpressionFieldRepository fieldRepository,
        IExpressionAvailabilityRepository availabilityRepository,
        ILogger<ExpressionService> logger)
    {
        _expressionRepository = expressionRepository;
        _fieldRepository = fieldRepository;
        _availabilityRepository = availabilityRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ExpressionDto> CreateAsync(CreateExpressionRequest request, string? createdBy = null)
    {
        var expressionId = Guid.NewGuid().ToString("N");
        var entity = new ExpressionEntity
        {
            ExpressionId = expressionId,
            Name = request.Name,
            Description = request.Description,
            SelectorType = request.SelectorType,
            ContainerExpression = request.ContainerExpression,
            Status = 1,
            CreatedBy = createdBy
        };

        await _expressionRepository.AddAsync(entity);

        if (request.Fields?.Count > 0)
        {
            var fields = request.Fields.Select((f, i) => new ExpressionFieldEntity
            {
                FieldId = Guid.NewGuid().ToString("N"),
                ExpressionId = expressionId,
                FieldName = f.FieldName,
                SelectorType = f.SelectorType,
                Expression = f.Expression,
                AttributeName = f.AttributeName,
                IsRequired = f.IsRequired,
                DefaultValue = f.DefaultValue,
                Formatter = f.Formatter,
                FormatterArgs = f.FormatterArgs,
                Order = i
            }).ToList();

            await _fieldRepository.AddRangeAsync(fields);
        }

        _logger.LogInformation("Expression {ExpressionId} created: {Name}", expressionId, request.Name);
        return await MapToDtoAsync(entity);
    }

    /// <inheritdoc />
    public async Task<ExpressionDto?> GetByIdAsync(string expressionId)
    {
        var entity = await _expressionRepository.GetByIdAsync(expressionId);
        return entity != null ? await MapToDtoAsync(entity) : null;
    }

    /// <inheritdoc />
    public async Task<ExpressionListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null)
    {
        var expressions = await _expressionRepository.GetAllAsync(pageIndex, pageSize, status);
        var total = await _expressionRepository.CountAsync(status);

        var dtos = new List<ExpressionDto>();
        foreach (var expr in expressions)
        {
            dtos.Add(await MapToDtoAsync(expr));
        }

        return new ExpressionListResponse(dtos, (int)total, pageIndex, pageSize);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(string expressionId, UpdateExpressionRequest request)
    {
        var entity = await _expressionRepository.GetByIdAsync(expressionId);
        if (entity == null) return false;

        if (request.Name != null) entity.Name = request.Name;
        if (request.Description != null) entity.Description = request.Description;
        if (request.SelectorType != null) entity.SelectorType = request.SelectorType;
        if (request.ContainerExpression != null) entity.ContainerExpression = request.ContainerExpression;
        if (request.Status.HasValue) entity.Status = request.Status.Value;

        await _expressionRepository.UpdateAsync(entity);

        if (request.Fields != null)
        {
            await _fieldRepository.DeleteByExpressionIdAsync(expressionId);
            var fields = request.Fields.Select((f, i) => new ExpressionFieldEntity
            {
                FieldId = Guid.NewGuid().ToString("N"),
                ExpressionId = expressionId,
                FieldName = f.FieldName,
                SelectorType = f.SelectorType,
                Expression = f.Expression,
                AttributeName = f.AttributeName,
                IsRequired = f.IsRequired,
                DefaultValue = f.DefaultValue,
                Formatter = f.Formatter,
                FormatterArgs = f.FormatterArgs,
                Order = i
            }).ToList();
            await _fieldRepository.AddRangeAsync(fields);
        }

        _logger.LogInformation("Expression {ExpressionId} updated", expressionId);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string expressionId)
    {
        await _fieldRepository.DeleteByExpressionIdAsync(expressionId);
        await _expressionRepository.DeleteAsync(expressionId);
        _logger.LogInformation("Expression {ExpressionId} deleted", expressionId);
        return true;
    }

    /// <inheritdoc />
    public async Task<ExpressionConfigDto> GetConfigAsync(string expressionId)
    {
        var entity = await _expressionRepository.GetByIdAsync(expressionId);
        if (entity == null) throw new KeyNotFoundException($"Expression {expressionId} not found");

        var fields = await _fieldRepository.GetByExpressionIdAsync(expressionId);
        return MapToConfigDto(entity, fields);
    }

    /// <inheritdoc />
    public async Task ReportAvailabilityAsync(string expressionId, string agentId, bool isAvailable, string? failureReason = null)
    {
        var availability = new ExpressionAvailabilityEntity
        {
            ExpressionId = expressionId,
            AgentId = agentId,
            IsAvailable = isAvailable,
            FailureReason = failureReason,
            LastCheckedAt = DateTime.UtcNow,
            ConsecutiveFailures = isAvailable ? 0 : 1
        };

        if (isAvailable)
        {
            availability.LastSuccessAt = DateTime.UtcNow;
            await _expressionRepository.IncrementSuccessAsync(expressionId);
        }
        else
        {
            availability.LastFailureAt = DateTime.UtcNow;
            await _expressionRepository.IncrementFailureAsync(expressionId);
        }

        await _availabilityRepository.AddOrUpdateAsync(availability);
        _logger.LogInformation(
            "Agent {AgentId} reported expression {ExpressionId} availability: {IsAvailable}",
            agentId, expressionId, isAvailable);
    }

    /// <inheritdoc />
    public async Task InvalidateExpiredExpressionsAsync(int consecutiveFailureThreshold = 5)
    {
        var expired = await _expressionRepository.GetExpiredExpressionsAsync(consecutiveFailureThreshold);
        foreach (var expr in expired)
        {
            expr.Status = 2;
            expr.ExpiredAt = DateTime.UtcNow;
            await _expressionRepository.UpdateAsync(expr);
            _logger.LogWarning(
                "Expression {ExpressionId} invalidated due to {Count} consecutive failures",
                expr.ExpressionId, expr.ConsecutiveFailures);
        }
    }

    /// <inheritdoc />
    public async Task<List<ExpressionConfigDto>> GetActiveExpressionsAsync()
    {
        var expressions = await _expressionRepository.GetActiveAsync();
        var result = new List<ExpressionConfigDto>();
        foreach (var expr in expressions)
        {
            var fields = await _fieldRepository.GetByExpressionIdAsync(expr.ExpressionId);
            result.Add(MapToConfigDto(expr, fields));
        }
        return result;
    }

    /// <summary>
    /// 将表达式实体映射为表达式 DTO（含字段列表）
    /// </summary>
    /// <param name="entity">表达式实体</param>
    /// <returns>表达式 DTO</returns>
    private async Task<ExpressionDto> MapToDtoAsync(ExpressionEntity entity)
    {
        var fields = await _fieldRepository.GetByExpressionIdAsync(entity.ExpressionId);
        return new ExpressionDto(
            entity.ExpressionId,
            entity.Name,
            entity.Description ?? string.Empty,
            entity.SelectorType,
            entity.ContainerExpression ?? string.Empty,
            fields.Select(f => new ExpressionFieldDto(
                f.FieldId, f.ExpressionId, f.FieldName, f.SelectorType,
                f.Expression, f.AttributeName, f.IsRequired, f.DefaultValue,
                f.Formatter, f.FormatterArgs, f.Order)).ToList(),
            entity.Status,
            entity.SuccessCount,
            entity.FailureCount,
            entity.ConsecutiveFailures,
            entity.LastValidatedAt,
            entity.LastUsedAt,
            entity.CreatedAt
        );
    }

    /// <summary>
    /// 将表达式实体和字段列表映射为表达式配置 DTO，用于 Agent 端运行时
    /// </summary>
    /// <param name="entity">表达式实体</param>
    /// <param name="fields">表达式字段实体列表</param>
    /// <returns>表达式配置 DTO</returns>
    private static ExpressionConfigDto MapToConfigDto(ExpressionEntity entity, List<ExpressionFieldEntity> fields)
    {
        return new ExpressionConfigDto(
            entity.ExpressionId,
            entity.Name,
            entity.SelectorType,
            entity.ContainerExpression ?? string.Empty,
            fields.Select(f => new ExpressionFieldConfigDto(
                f.FieldName, f.SelectorType, f.Expression,
                f.AttributeName, f.IsRequired, f.DefaultValue,
                f.Formatter, f.FormatterArgs, f.Order)).ToList()
        );
    }
}
