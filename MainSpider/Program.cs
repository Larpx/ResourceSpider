using Larpx.Logs;
using System;
using System.IO;

namespace MainSpider
{
    class Program
    {
        static string sLoggerPath = "../Logs";
        static Logger m_oLogger = new ConsoleLogger() + new TextFileLogger(new DirectoryInfo(sLoggerPath));

        public static void Main(string[] args)
        {
            try
            {
                //m_oLogger.LogInfo("Test");
                //throw new Exception("66");
            }
            catch (Exception ex)
            {
                m_oLogger.LogException(ex);
            }
        }
    }
}
