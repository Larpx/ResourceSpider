using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class WebSite1 : Spider
    {
        private static bool bDebug = true;
        private static string sLoggerPath = "../Logs";
        private static Logger m_oLogger ;

        public WebSite1(bool debug = true, Logger Logger = null)
        {
            if (Logger != null)
                m_oLogger = Logger;
            else
                m_oLogger = new ConsoleLogger() + new TextFileLogger(new DirectoryInfo(sLoggerPath));
            bDebug = debug;
        }

        public int PerExce(params Dictionary<string, object>[] arr)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DoExce(params Dictionary<string, object>[] arr)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Website> GetWebsiteList()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Category> GetCategoryList()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Link> GetLinkList()
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetLinkDetail(List<Link> oResult)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
