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
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Filters;
using ResourceSpider.Server.Hubs;
using ResourceSpider.Server.Middleware;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;
using Serilog;
using SqlSugar;
using StackExchange.Redis;

namespace ResourceSpider.Server;

/// <summary>
/// 应用程序启动配置类，负责服务注册和中间件管道配置
/// </summary>
public class Startup
{
    /// <summary>
    /// 应用程序配置实例
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// 初始化 Startup 实例
    /// </summary>
    /// <param name="configuration">应用程序配置</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// 配置应用程序服务，注册依赖注入容器中的服务
    /// </summary>
    /// <param name="services">服务集合</param>
    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureControllers(services);
        ConfigureOpenApi(services);
        ConfigureAuthentication(services);
        ConfigureSignalR(services);
        ConfigureHealthChecks(services);
        ConfigureDatabase(services);
        ConfigureRedis(services);
        ConfigureMessageQueue(services);
        ConfigureRepositories(services);
        ConfigureServicesLayer(services);
        ConfigureInfrastructure(services);
        ConfigureOptions(services);
        ConfigureCors(services);
    }

    /// <summary>
    /// 配置 MVC 控制器及全局过滤器
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureControllers(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<ApiResponseFilter>();
        });
    }

    /// <summary>
    /// 配置 OpenAPI/Swagger 文档生成器
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureOpenApi(IServiceCollection services)
    {
        services.AddOpenApiDocument(options =>
        {
            options.Title = "ResourceSpider API";
            options.Version = "1.0";
        });
    }

    /// <summary>
    /// 配置 JWT Bearer 认证
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["Jwt:Secret"] ?? "default-secret-key"))
                };
            });

        services.AddAuthorization();
    }

    /// <summary>
    /// 配置 SignalR 实时通信服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureSignalR(IServiceCollection services)
    {
        services.AddSignalR();
    }

    /// <summary>
    /// 配置健康检查服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddHealthChecks();
    }

    /// <summary>
    /// 配置 SQLSugar 数据库连接，支持 MySQL 和 PostgreSQL
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureDatabase(IServiceCollection services)
    {
        var dbTypeStr = Configuration["Database:Type"] ?? "MySQL";
        var dbType = dbTypeStr.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase)
            ? DbType.PostgreSQL
            : DbType.MySql;

        services.AddScoped<ISqlSugarClient>(sp =>
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection")
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
    }

    /// <summary>
    /// 配置 Redis 连接
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureRedis(IServiceCollection services)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = Configuration.GetConnectionString("Redis")
                ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(config);
        });
    }

    /// <summary>
    /// 配置消息队列服务，根据配置选择内存队列或 RabbitMQ
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureMessageQueue(IServiceCollection services)
    {
        var mqType = Configuration["MessageQueue:Type"] ?? "InMemory";
        if (mqType == "InMemory")
        {
            services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
        }
        else
        {
            services.AddSingleton<IMessageQueue, RabbitMqMessageQueue>();
        }
    }

    /// <summary>
    /// 注册所有数据仓储层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureRepositories(IServiceCollection services)
    {
        services.AddScoped<IAgentRepository, AgentRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProxyRepository, ProxyRepository>();
        services.AddScoped<IStatisticRepository, StatisticRepository>();
        services.AddScoped<IExpressionRepository, ExpressionRepository>();
        services.AddScoped<IExpressionFieldRepository, ExpressionFieldRepository>();
        services.AddScoped<ICollectionResultRepository, CollectionResultRepository>();
        services.AddScoped<IExpressionAvailabilityRepository, ExpressionAvailabilityRepository>();
        services.AddScoped<ITaskStepRepository, TaskStepRepository>();
        services.AddScoped<ITaskExecutionRepository, TaskExecutionRepository>();
        services.AddScoped<ICrawlResultRepository, CrawlResultRepository>();
        services.AddScoped<IConfigVersionRepository, ConfigVersionRepository>();
        services.AddScoped<ISystemLogRepository, SystemLogRepository>();
        services.AddScoped<IAgentGroupRepository, AgentGroupRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    /// <summary>
    /// 注册所有业务服务层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureServicesLayer(IServiceCollection services)
    {
        services.AddScoped<IAgentRegisterService, AgentRegisterService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<ITaskDispatchService, TaskDispatchService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        services.AddScoped<IProxyService, ProxyService>();
        services.AddScoped<IExpressionService, ExpressionService>();
        services.AddScoped<ICollectionResultService, CollectionResultService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskExecutionService, TaskExecutionService>();
        services.AddScoped<IConfigVersionService, ConfigVersionService>();
        services.AddScoped<IAgentGroupService, AgentGroupService>();
        services.AddScoped<ISystemLogService, SystemLogService>();
    }

    /// <summary>
    /// 注册基础设施层服务，包括去重、调度、下载器、解析器、代理池等
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureInfrastructure(IServiceCollection services)
    {
        services.AddSingleton<IDuplicateRemover, HashSetDuplicateRemover>();
        services.AddSingleton<IScheduler, BreadthFirstScheduler>();
        services.AddTransient<PlaywrightDownloader>();
        services.AddSingleton<IDownloaderFactory, DefaultDownloaderFactory>();
        services.AddSingleton<IParserFactory, DefaultParserFactory>();
        services.AddSingleton<IProxyPool, ProxyPool>();
    }

    /// <summary>
    /// 配置选项绑定，将配置文件中的节绑定到对应的选项类
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureOptions(IServiceCollection services)
    {
        services.Configure<DownloaderOptions>(Configuration.GetSection("Downloader"));
        services.Configure<PlaywrightOptions>(Configuration.GetSection("Playwright"));
    }

    /// <summary>
    /// 配置跨域资源共享策略
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// 配置 HTTP 请求处理管道，包括中间件、端点映射和数据库初始化
    /// </summary>
    /// <param name="app">Web 应用程序实例</param>
    public void Configure(WebApplication app)
    {
        InitializeDatabase(app);
        ConfigureMiddleware(app);
        ConfigureEndpoints(app);
    }

    /// <summary>
    /// 初始化数据库，创建数据库和表结构
    /// </summary>
    /// <param name="app">Web 应用程序实例</param>
    private void InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
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

    /// <summary>
    /// 配置中间件管道，定义请求处理的顺序
    /// </summary>
    /// <param name="app">Web 应用程序实例</param>
    private void ConfigureMiddleware(WebApplication app)
    {
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
    }

    /// <summary>
    /// 配置应用程序端点映射，包括控制器、SignalR Hub 和健康检查
    /// </summary>
    /// <param name="app">Web 应用程序实例</param>
    private void ConfigureEndpoints(WebApplication app)
    {
        app.MapControllers();
        app.MapHub<SpiderHub>("/hubs/spider");
        app.MapHealthChecks("/health");
    }
}
