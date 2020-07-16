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
                //string s= GenerateRandomNumber();
                //Console.WriteLine(s);
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

        public static string GenerateRandomNumber(int Length = 16)
        {
            char[] constant =
            {
                '0','1','2','3','4','5','6','7','8','9',
                'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
            };
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
    }
}
