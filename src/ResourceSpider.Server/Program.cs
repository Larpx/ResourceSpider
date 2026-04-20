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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// SqlSugar
builder.Services.AddScoped<ISqlSugarClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=localhost;Database=ResourceSpider;Uid=root;Pwd=root;";
    
    var db = new SqlSugarClient(new ConnectionConfig
    {
        ConnectionString = connectionString,
        DbType = DbType.MySql,
        IsAutoCloseConnection = true,
        InitKeyType = InitKeyType.Attribute
    });

    db.Aop.OnLogExecuting = (sql, pars) =>
    {
        Log.Debug(sql);
    };

    return db;
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis") 
        ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(config);
});

// Message Queue
var mqType = builder.Configuration["MessageQueue:Type"] ?? "InMemory";
if (mqType == "InMemory")
{
    builder.Services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
}
else
{
    builder.Services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
}

// Repositories
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IProxyRepository, ProxyRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();

// Services
builder.Services.AddScoped<IAgentRegisterService, AgentRegisterService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskDispatchService, TaskDispatchService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IProxyService, ProxyService>();

// Infrastructure
builder.Services.AddSingleton<IDuplicateRemover, HashSetDuplicateRemover>();
builder.Services.AddSingleton<IScheduler, BreadthFirstScheduler>();
builder.Services.AddTransient<HttpClientDownloader>();
builder.Services.AddTransient<PlaywrightDownloader>();
builder.Services.AddSingleton<IDownloaderFactory, DefaultDownloaderFactory>();
builder.Services.AddSingleton<IParserFactory, DefaultParserFactory>();
builder.Services.AddSingleton<IProxyPool, ProxyPool>();

// Options
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
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
    db.DbMaintenance.CreateDatabase();
    db.CodeFirst.InitTables(
        typeof(AgentEntity),
        typeof(TaskEntity),
        typeof(TaskRequestEntity),
        typeof(ProxyEntity),
        typeof(StatisticEntity)
    );
}

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

Log.Information("ResourceSpider Server starting on {Urls}", builder.Configuration["urls"] ?? "http://localhost:5000");
app.Run();
