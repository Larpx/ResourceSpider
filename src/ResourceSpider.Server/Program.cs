using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.MessageQueue;
using ResourceSpider.Infrastructure.Parser;
using ResourceSpider.Infrastructure.Proxy;
using ResourceSpider.Infrastructure.Scheduler;
using ResourceSpider.Infrastructure.Storage;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Filters;
using ResourceSpider.Server.Hubs;
using ResourceSpider.Server.Middleware;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;
using Serilog;
using ResourceSpider.Server.Entities;
using SqlSugar;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/server-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .MinimumLevel.Information()
    .Enrich.FromLogContext());

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
});

builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "ResourceSpider API";
    options.Version = "1.0";
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "default-secret-key"))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();

builder.Services.AddHealthChecks();

var dbTypeStr = builder.Configuration["Database:Type"] ?? "MySQL";
var dbType = dbTypeStr.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
    ? DbType.PostgreSQL
    : DbType.MySql;

builder.Services.AddScoped<ISqlSugarClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=localhost;Database=ResourceSpider;Uid=root;Pwd=root;";

    var db = new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connectionString,
        DbType = dbType,
        IsAutoCloseConnection = true,
        InitKeyType = InitKeyType.Attribute
    });

    db.Aop.OnLogExecuting = (sql, pars) =>
    {
        Log.Debug(sql);
    };

    return db;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(config);
});

var mqType = builder.Configuration["MessageQueue:Type"] ?? "InMemory";
if (mqType == "InMemory")
{
    builder.Services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
}
else
{
    builder.Services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
}

builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProxyRepository, ProxyRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IExpressionRepository, ExpressionRepository>();
builder.Services.AddScoped<IExpressionFieldRepository, ExpressionFieldRepository>();
builder.Services.AddScoped<ICollectionResultRepository, CollectionResultRepository>();
builder.Services.AddScoped<IExpressionAvailabilityRepository, ExpressionAvailabilityRepository>();
builder.Services.AddScoped<ITaskStepRepository, TaskStepRepository>();
builder.Services.AddScoped<ITaskExecutionRepository, TaskExecutionRepository>();
builder.Services.AddScoped<ICrawlResultRepository, CrawlResultRepository>();
builder.Services.AddScoped<IConfigVersionRepository, ConfigVersionRepository>();
builder.Services.AddScoped<ISystemLogRepository, SystemLogRepository>();
builder.Services.AddScoped<IAgentGroupRepository, AgentGroupRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAgentRegisterService, AgentRegisterService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskDispatchService, TaskDispatchService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IProxyService, ProxyService>();
builder.Services.AddScoped<IExpressionService, ExpressionService>();
builder.Services.AddScoped<ICollectionResultService, CollectionResultService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskExecutionService, TaskExecutionService>();
builder.Services.AddScoped<IConfigVersionService, ConfigVersionService>();
builder.Services.AddScoped<IAgentGroupService, AgentGroupService>();
builder.Services.AddScoped<ISystemLogService, SystemLogService>();

builder.Services.AddSingleton<IDuplicateRemover, HashSetDuplicateRemover>();
builder.Services.AddSingleton<IScheduler, BreadthFirstScheduler>();
builder.Services.AddTransient<HttpClientDownloader>();
builder.Services.AddTransient<PlaywrightDownloader>();
builder.Services.AddSingleton<IDownloaderFactory, DefaultDownloaderFactory>();
builder.Services.AddSingleton<IParserFactory, DefaultParserFactory>();
builder.Services.AddSingleton<IProxyPool, ProxyPool>();

builder.Services.Configure<DownloaderOptions>(
    builder.Configuration.GetSection("Downloader"));
builder.Services.Configure<PlaywrightOptions>(
    builder.Configuration.GetSection("Playwright"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
    db.DbMaintenance.CreateDatabase();
    db.CodeFirst.InitTables(
        typeof(AgentEntity),
        typeof(TaskEntity),
        typeof(TaskRequestEntity),
        typeof(TaskStepEntity),
        typeof(TaskExecutionEntity),
        typeof(CrawlResultEntity),
        typeof(ConfigVersionEntity),
        typeof(ProxyEntity),
        typeof(StatisticEntity),
        typeof(ExpressionEntity),
        typeof(ExpressionFieldEntity),
        typeof(CollectionResultEntity),
        typeof(ExpressionAvailabilityEntity),
        typeof(SystemLogEntity),
        typeof(AgentGroupEntity),
        typeof(UserEntity)
    );
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.MapControllers();
app.MapHub<SpiderHub>("/hubs/spider");
app.MapHealthChecks("/health");

Log.Information("ResourceSpider Server 启动，地址：{Urls}", builder.Configuration["urls"] ?? "http://localhost:5000");
app.Run();
