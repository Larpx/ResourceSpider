using System;
using System.IO;
using Larpx.ResourceSpider.BaseLibrary.Extension;

namespace Larpx.ResourceSpider.BaseLibrary.CodeGenerator
{
    /// <summary>
    /// 代码生成项目配置
    /// </summary>
    [Config.Type]
    internal static class Config
    {
        /// <summary>
        /// 日志配置
        /// </summary>
        [Config.Member]
        public static Log.Config LogConfig
        {
            get
            {
                string logPath = PubPath.ApplicationPath;
                DirectoryInfo directory = new DirectoryInfo(logPath);
                if (string.CompareOrdinal(directory.Parent.Name.ToLower(), "packet") == 0) logPath = directory.Parent.fullName();
                return new Log.Config { Type = Log.LogType.All, FilePath = logPath };
            }
        }
    }
}
