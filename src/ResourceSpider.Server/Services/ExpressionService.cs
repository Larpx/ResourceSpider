using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Repositories;

namespace ResourceSpider.Server.Services;

public interface IExpressionService
{
    Task<ExpressionDto> CreateAsync(CreateExpressionRequest request, string? createdBy = null);
    Task<ExpressionDto?> GetByIdAsync(string expressionId);
    Task<ExpressionListResponse> GetListAsync(int pageIndex, int pageSize, int? status = null);
    Task<bool> UpdateAsync(string expressionId, UpdateExpressionRequest request);
    Task<bool> DeleteAsync(string expressionId);
    Task<ExpressionConfigDto> GetConfigAsync(string expressionId);
    Task ReportAvailabilityAsync(string expressionId, string agentId, bool isAvailable, string? failureReason = null);
    Task InvalidateExpiredExpressionsAsync(int consecutiveFailureThreshold = 5);
    Task<List<ExpressionConfigDto>> GetActiveExpressionsAsync();
}

public class ExpressionService : IExpressionService
{
    private readonly IExpressionRepository _expressionRepository;
    private readonly IExpressionFieldRepository _fieldRepository;
    private readonly IExpressionAvailabilityRepository _availabilityRepository;
    private readonly ILogger<ExpressionService> _logger;

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

    public async Task<ExpressionDto?> GetByIdAsync(string expressionId)
    {
        var entity = await _expressionRepository.GetByIdAsync(expressionId);
        return entity != null ? await MapToDtoAsync(entity) : null;
    }

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

    public async Task<bool> DeleteAsync(string expressionId)
    {
        await _fieldRepository.DeleteByExpressionIdAsync(expressionId);
        await _expressionRepository.DeleteAsync(expressionId);
        _logger.LogInformation("Expression {ExpressionId} deleted", expressionId);
        return true;
    }

    public async Task<ExpressionConfigDto> GetConfigAsync(string expressionId)
    {
        var entity = await _expressionRepository.GetByIdAsync(expressionId);
        if (entity == null) throw new KeyNotFoundException($"Expression {expressionId} not found");

        var fields = await _fieldRepository.GetByExpressionIdAsync(expressionId);
        return MapToConfigDto(entity, fields);
    }

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
