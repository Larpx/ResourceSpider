using Larpx.PersonalTools.ResourceSpider.Server.Observability;
using Serilog;

namespace Larpx.PersonalTools.ResourceSpider.Server;

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
            .WriteTo.Sink(new RuntimeOutputSink())
            .WriteTo.File("logs/server-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .CreateLogger();

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            Log.Fatal((Exception)e.ExceptionObject, "未处理的致命异常");

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Error(e.Exception, "未观察到的任务异常");
            e.SetObserved();
        };

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
