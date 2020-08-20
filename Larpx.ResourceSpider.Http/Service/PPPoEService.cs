using Larpx.ResourceSpider.Http.Content;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Larpx.ResourceSpider.Http.Service
{
    public class PPPoEService
    {
        private readonly PPPoEOptions _options;

        public PPPoEService(IOptions<PPPoEOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// 异步拨号，直接先返回结果，爬虫会重试发到别的代理器上
        /// 拨号也不需要等待其它下载完成，除非先下线节点，再等待所有下载完成
        /// 再拨号，拨号成功后再重新订阅，怎么个逻辑太复杂。
        /// ADSL 本身就不可能非常快，因此直接拨号触发重试即可，只要节点够多，完全可以接受
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Response DetectAsync(Request request, out string sErrorMessage)
        {
            try
            {
                sErrorMessage = "";

                switch (request.DownloaderType)
                {
                    case DownloaderTypeNames.HttpClient:
                    case DownloaderTypeNames.Puppeteer:
                        return null;

                    case DownloaderTypeNames.HttpClientWithADSL:
                    case DownloaderTypeNames.PuppeteerWithADSL:
                        Redial();
                        return null;

                    default:
                        return null;
                }
            }
            catch (PlatformNotSupportedException ex)
            {
                sErrorMessage = ex.Message;
                return null;
            }
            catch (Exception ex)
            {
                sErrorMessage = ex.Message;

                return new Response
                {
                    RequestHash = request.Hash,
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new ResponseContent
                    {
                        Data = Encoding.UTF8.GetBytes($"ADSL拨号时出现错误，错误信息：{ex.Message}")
                    }
                };
            }
        }

        /// <summary>
        /// 宽带拨号
        /// </summary>
        private void Redial()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    KillPPPoEProcesses();
                    var process = Process.Start("/sbin/ifdown", "ppp0");
                    if (process == null)
                    {
                        return;
                    }

                    process.WaitForExit();
                    process = Process.Start("/sbin/ifup", "ppp0");
                    if (process == null)
                    {
                        return;
                    }

                    process.WaitForExit();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    RedialOnWindows();
                    return;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return;
                }
                else
                {
                    throw new PlatformNotSupportedException($"{Environment.OSVersion.Platform}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RedialOnWindows()
        {
            try
            {
                var process = new Process
                {
                    StartInfo =
                {
                    FileName = "rasdial.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = @"C:\Windows\System32",
                    Arguments = _options.ADSLInterface + @" /DISCONNECT"
                }
                };
                process.Start();
                process.WaitForExit(10000);

                process = new Process
                {
                    StartInfo =
                {
                    FileName = "rasdial.exe",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = @"C:\Windows\System32",
                    Arguments = _options.ADSLInterface + " " + _options.ADSLAccount + " " + _options.ADSLPassword
                }
                };
                process.Start();
                process.WaitForExit(10000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 杀线程
        /// </summary>
        private void KillPPPoEProcesses()
        {
            try
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var processes = Process.GetProcessesByName("pppd").ToList();
                    processes.AddRange(Process.GetProcessesByName("pppoe"));
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
