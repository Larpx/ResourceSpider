using Larpx.Logs;
using Larpx.ResourceSpider.LocalSpider.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

                Dictionary<string, object> oArr = new Dictionary<string, object>();
                string s_877 = "d9e5780840f6766c7fcbac7cab9538f2",
                    sUU = "fe1213ba1c94e4a42b72bda9840af83c";

                UUMP4Spider uUMP4Spider = new UUMP4Spider(true);
                oArr.Add("ID", sUU);
                uUMP4Spider.DoExce(oArr);

                //_877JNSpider _877JNSpider = new _877JNSpider(false);
                //oArr.Add("ID", s_877);
                //_877JNSpider.DoExce(oArr);

                Console.WriteLine("Task is End,Running times 99999999+ ms");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}
