using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using Larpx.ResourceSpider.LocalSpider.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Larpx.ResourceSpider.CommonHelper.CommonHelper;

namespace Larpx.ResourceSpider.LocalSpider
{
    class Program
    {
        private static bool bDebug = true;
        private static string sLoggerPath = "../Logs";
        private static Logger m_oLogger = new ConsoleLogger() + new TextFileLogger(new DirectoryInfo(sLoggerPath));

        static void Main(string[] args)
        {
            try
            {
                ThreadPool.SetMaxThreads(32, 32);
                ThreadPool.SetMinThreads(8, 8);

                ThreadPool.UnsafeQueueUserWorkItem(DoExce1, args);
                ThreadPool.UnsafeQueueUserWorkItem(DoExce2, args);

                Console.WriteLine("Task is End,Running times 99999999+ ms");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        /// <summary>
        /// UUMP4
        /// </summary>
        /// <param name="obj"></param>
        private static void DoExce1(object obj)
        {
            Dictionary<string, object> oArr = new Dictionary<string, object>();
            string sUU = "fe1213ba1c94e4a42b72bda9840af83c";
            UUMP4Spider uUMP4Spider = new UUMP4Spider(Guid.Empty, DatabaseType.MySql, sUU, bDebug, sLoggerPath, m_oLogger);
            oArr.Add("ID", sUU);
            oArr.Add("DatabaseType", DatabaseType.MySql);
            uUMP4Spider.DoExce(oArr);
        }

        private static void DoExce2(object obj)
        {
            Dictionary<string, object> oArr = new Dictionary<string, object>();
            string sUU = "d9e5780840f6766c7fcbac7cab9538f2";
            _877JNSpider uUMP4Spider = new _877JNSpider(Guid.Empty, DatabaseType.MySql, sUU, bDebug, sLoggerPath, m_oLogger);
            oArr.Add("ID", sUU);
            oArr.Add("DatabaseType", DatabaseType.MySql);
            uUMP4Spider.DoExce(oArr);
        }
    }
}
