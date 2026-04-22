using Serilog;
using ResourceSpider.Server;

namespace ResourceSpider.Server;

/// <summary>
/// ResourceSpider 服务器应用程序入口点
/// </summary>
public class Program
{
    /// <summary>
    /// 应用程序主入口方法
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/server-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("ResourceSpider Server 正在启动");
            CreateBuilder(args).Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "ResourceSpider Server 启动失败");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// 创建 Web 应用程序构建器，配置主机和服务
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>配置完成的 WebApplication 实例</returns>
    public static WebApplication CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        startup.Configure(app);

        Log.Information("ResourceSpider Server 启动，地址：{Urls}",
            builder.Configuration["urls"] ?? "http://localhost:5000");

        return app;
    }
}
