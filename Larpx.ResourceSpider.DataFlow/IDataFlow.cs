using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.DataFlow
{
    /// <summary>
    /// 数据流处理器
    /// </summary>
    public interface IDataFlow : IDisposable
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        void SetLogger(ILogger logger);

        /// <summary>
        /// 流处理
        /// </summary>
        /// <param name="context">处理上下文</param>
        /// <returns></returns>
        Task HandleAsync(DataContext context);
    }
}
