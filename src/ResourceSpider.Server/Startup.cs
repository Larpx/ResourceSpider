using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using ResourceSpider.Core.Interfaces;
using ResourceSpider.Infrastructure.Duplicate;
using ResourceSpider.Infrastructure.Downloader;
using ResourceSpider.Infrastructure.MessageQueue;
using ResourceSpider.Infrastructure.Parser;
using ResourceSpider.Infrastructure.Proxy;
using ResourceSpider.Infrastructure.Scheduler;
using ResourceSpider.Infrastructure.Storage;
using ResourceSpider.Server.Components;
using ResourceSpider.Server.Components.Services;
using ResourceSpider.Server.DTOs;
using ResourceSpider.Server.Entities;
using ResourceSpider.Server.Filters;
using ResourceSpider.Server.Hubs;
using ResourceSpider.Server.Middleware;
using ResourceSpider.Server.Observability;
using ResourceSpider.Server.Repositories;
using ResourceSpider.Server.Services;
using Serilog;
using SqlSugar;
using StackExchange.Redis;

namespace ResourceSpider.Server;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureAdminUiServices(services);
        ConfigureBlazor(services);
        ConfigureControllers(services);
        ConfigureOpenApi(services);
        ConfigureAuthentication(services);
        ConfigureSignalR(services);
        ConfigureHealthChecks(services);
        ConfigureDatabase(services);
        ConfigurePostgreSqlResultStorage(services);
        ConfigureRedis(services);
        ConfigureMessageQueue(services);
        ConfigureRepositories(services);
        ConfigureServicesLayer(services);
        ConfigureInfrastructure(services);
        ConfigureOptions(services);
        ConfigureCors(services);
    }

    /// <summary>
    /// 配置后台管理 UI 的会话与 API 客户端服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void ConfigureAdminUiServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<AdminSessionState>();
        services.AddScoped<AdminApiClient>();
    }

    /// <summary>
    /// 配置 Blazor Razor Components 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    private static void ConfigureBlazor(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
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
    private static void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddSingleton<StartupState>();
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", failureStatus: HealthStatus.Unhealthy, tags: ["ready", "db"]);
    }

    /// <summary>
    /// 配置主业务数据库（固定 MySQL）。
    /// </summary>
    private void ConfigureDatabase(IServiceCollection services)
    {
        services.AddScoped<ISqlSugarClient>(_ =>
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection")
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
    }

    /// <summary>
    /// 配置 PostgreSQL 结果存储（可选）。
    /// </summary>
    private void ConfigurePostgreSqlResultStorage(IServiceCollection services)
    {
        services.AddSingleton<IRuntimePostgreSqlResultDbAccessor, RuntimePostgreSqlResultDbAccessor>();

        var connectionString = Configuration.GetConnectionString("PostgreSqlResults");
        var configured = !string.IsNullOrWhiteSpace(connectionString);

        var section = Configuration.GetSection("PostgreSqlResults");
        var enabled = section.GetValue<bool?>("Enabled") ?? false;

        SqlSugarClient? postgreClient = null;
        var connected = false;

        if (configured && enabled)
        {
            try
            {
                postgreClient = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = connectionString!,
                    DbType = DbType.PostgreSQL,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                });

                postgreClient.Ado.GetInt("SELECT 1");
                connected = true;
            }
            catch (Exception ex)
            {
                connected = false;
                Log.Warning(ex, "PostgreSQL 结果库初始化失败，将回退到 MySQL 结果表");
            }
        }

        services.AddSingleton<IPostgreSqlResultStorageFeatureService>(_ =>
            new PostgreSqlResultStorageFeatureService(enabled, configured, connected));

        services.AddSingleton<IPostgreResultDbClient, PostgreResultDbClient>();
        services.AddSingleton(sp =>
        {
            var accessor = sp.GetRequiredService<IRuntimePostgreSqlResultDbAccessor>();
            accessor.SetClient(postgreClient);
            return accessor;
        });
    }

    /// <summary>
    /// 配置 Redis 连接
    /// </summary>
    /// <param name="services">服务集合</param>
    private void ConfigureRedis(IServiceCollection services)
    {
        services.AddSingleton<IRuntimeRedisConnectionAccessor, RuntimeRedisConnectionAccessor>();

        var redisConnection = Configuration.GetConnectionString("Redis");
        var redisConfigured = !string.IsNullOrWhiteSpace(redisConnection);

        var redisSection = Configuration.GetSection("Redis");
        var redisEnabled = redisSection.GetValue<bool?>("Enabled") ?? redisConfigured;
        var taskContentTtlSeconds = redisSection.GetValue<int?>("TaskContentTtlSeconds") ?? 300;

        IConnectionMultiplexer? redis = null;
        if (redisEnabled && redisConfigured)
        {
            try
            {
                redis = ConnectionMultiplexer.Connect(redisConnection!);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Redis 初始化失败，系统将以无 Redis 模式运行");
            }
        }

        services.AddSingleton<IRedisFeatureService>(_ =>
            new RedisFeatureService(
                enabled: redisEnabled,
                configured: redisConfigured,
                taskContentTtlSeconds: taskContentTtlSeconds,
                redis: redis));

        services.AddSingleton(sp =>
        {
            var accessor = sp.GetRequiredService<IRuntimeRedisConnectionAccessor>();
            accessor.SetConnection(redis);
            return accessor;
        });

        services.AddSingleton<IAgentTaskContentCache, RedisAgentTaskContentCache>();
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
        services.AddScoped<IPostgreCollectionResultRepository, PostgreCollectionResultRepository>();
        services.AddScoped<IStepResourceRepository, StepResourceRepository>();
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
        services.AddScoped<IStepStateMachineService, StepStateMachineService>();
        services.AddScoped<IStepResourcePoolService, StepResourcePoolService>();
        services.AddScoped<IDataEncryptionService, DataEncryptionService>();
        services.AddScoped<IStorageStrategyService, StorageStrategyService>();
        services.AddScoped<ISystemRuntimeSwitchService, SystemRuntimeSwitchService>();
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
        var startupState = app.Services.GetRequiredService<StartupState>();

        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

            SeedInitialTestData(db);
            InitializePostgreSqlResultStorage(scope.ServiceProvider);

            startupState.MarkDatabaseReady();
            Log.Information("数据库初始化完成");
        }
        catch (Exception ex)
        {
            startupState.MarkDatabaseFailed(ex);
            Log.Error(ex, "数据库初始化失败，应用将继续启动（请检查连接字符串）");
        }
    }

    private static void SeedInitialTestData(ISqlSugarClient db)
    {
        var now = DateTime.UtcNow;

        var databaseExists = true;
        try
        {
            db.Ado.GetInt("SELECT 1");
        }
        catch
        {
            databaseExists = false;
        }

        if (!databaseExists)
        {
            db.DbMaintenance.CreateDatabase();
        }

        var usersTableExists = false;
        try
        {
            usersTableExists = db.DbMaintenance.IsAnyTable("users", false);
        }
        catch
        {
            usersTableExists = false;
        }

        if (!usersTableExists)
        {
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
                typeof(StepResourceEntity),
                typeof(ExpressionAvailabilityEntity),
                typeof(SystemLogEntity),
                typeof(AgentGroupEntity),
                typeof(UserEntity)
            );
        }

        if (!db.DbMaintenance.IsAnyTable("users", false))
        {
            return;
        }

        if (db.Queryable<UserEntity>().Count() > 0)
        {
            return;
        }

        db.Insertable(new List<UserEntity>
        {
            new()
            {
                UserId = "u_admin_default",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456"),
                Role = "Admin",
                Status = 1,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                UserId = "u_operator_demo",
                Username = "operator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Operator@123456"),
                Role = "Operator",
                Status = 1,
                CreatedAt = now,
                UpdatedAt = now
            }
        }).ExecuteCommand();
    }

    private static void InitializePostgreSqlResultStorage(IServiceProvider serviceProvider)
    {
        var pgFeature = serviceProvider.GetRequiredService<IPostgreSqlResultStorageFeatureService>();
        if (!pgFeature.IsConfigured || !pgFeature.IsConnected)
        {
            return;
        }

        var pgClientHolder = serviceProvider.GetRequiredService<IPostgreResultDbClient>();
        var db = pgClientHolder.Client;
        if (db == null)
        {
            return;
        }

        try
        {
            if (!db.DbMaintenance.IsAnyTable("collection_results", false))
            {
                db.CodeFirst.InitTables(typeof(CollectionResultEntity));
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "初始化 PostgreSQL 结果表失败，系统将继续使用 MySQL 结果表");
        }
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

        app.UseStaticFiles();

        var exportsPath = Path.Combine(app.Environment.ContentRootPath, "exports");
        Directory.CreateDirectory(exportsPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(exportsPath),
            RequestPath = "/exports",
            ContentTypeProvider = new FileExtensionContentTypeProvider()
        });

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

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
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        app.MapControllers();
        app.MapHub<SpiderHub>("/hubs/spider");
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthResponse
        });
    }

    private static Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                x => x.Key,
                x => new
                {
                    status = x.Value.Status.ToString(),
                    description = x.Value.Description,
                    duration = x.Value.Duration.TotalMilliseconds,
                    exception = x.Value.Exception?.Message,
                    data = x.Value.Data
                }),
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
