using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlSugar;

namespace Larpx.PersonalTools.ResourceSpider.Server.Observability;

/// <summary>
/// 数据库健康检查，结合启动初始化状态和运行时连通性
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly StartupState _startupState;
    private readonly ISqlSugarClient _db;

    public DatabaseHealthCheck(StartupState startupState, ISqlSugarClient db)
    {
        _startupState = startupState;
        _db = db;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_startupState.DatabaseInitializationSucceeded)
        {
            return Task.FromResult(new HealthCheckResult(
                context.Registration.FailureStatus,
                "数据库初始化失败",
                data: new Dictionary<string, object>
                {
                    ["error"] = _startupState.DatabaseInitializationError??"",
                    ["updatedAtUtc"] = _startupState.UpdatedAtUtc
                }));
        }

        try
        {
            _db.Ado.GetInt("SELECT 1");
            return Task.FromResult(HealthCheckResult.Healthy("数据库连接正常"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new HealthCheckResult(
                context.Registration.FailureStatus,
                "数据库连接不可用",
                ex,
                new Dictionary<string, object>
                {
                    ["error"] = ex.Message,
                    ["updatedAtUtc"] = _startupState.UpdatedAtUtc
                }));
        }
    }
}
