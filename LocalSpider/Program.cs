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
                UUMP4Spider uUMP4Spider = new UUMP4Spider(false);

                oArr.Add("ID", "94f0591a0d73fc445c613014d22aebdf");

                uUMP4Spider.DoExce(oArr);

                //DoExce2( obj );
                //DoExce( obj );

                //FilmModular2 filmModular1 = new FilmModular2();

                //filmModular1.DoExce( obj );

                //double hl = 691.4600;

                //double dR = Math.Round( ( hl / 100.00 ) * 9.99, 4 );
                //Console.WriteLine( "" + dR );

                //ThreadPool.QueueUserWorkItem( filmModular1.DoExce, obj );
                //ThreadPool.QueueUserWorkItem( filmModular1.DoExce, obj );

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
