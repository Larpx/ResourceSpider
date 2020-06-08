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
        private static Logger m_oLogger;

        public WebSite1(bool debug = true, Logger Logger = null)
        {
            if (Logger != null)
                m_oLogger = Logger;
            else
                m_oLogger = new ConsoleLogger() + new TextFileLogger(new DirectoryInfo(sLoggerPath));
            bDebug = debug;
        }

        public int PerExce(Dictionary<string, object> arr)
        {
            try
            {

                return 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int DoExce(Dictionary<string, object> arr)
        {
            try
            {
                string sWebsiteID = "";

                foreach (var item in arr)
                {
                    switch (item.Key)
                    {
                        case "ID":
                            sWebsiteID = item.Value.ToString();
                            break;
                    }
                }

                List<Website> websitesList = GetWebsiteList(sWebsiteID);
                List<Category> oListCategory = new List<Category>();
                List<Link> oListLink = new List<Link>();

                //获取分类列表
                foreach (var item in websitesList)
                {
                    oListCategory.AddRange(GetCategoryList(item));
                }

                //采集链接
                foreach (var item in oListCategory)
                {
                    oListLink.AddRange(GetLinkList(item));
                }

                //采集详情
                foreach (var item in oListLink)
                {
                    GetLinkDetail(item);
                }

                return 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取网站列表
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public List<Website> GetWebsiteList(string sID)
        {
            try
            {
                SQLSugarHelper<Website> oWebsites = new SQLSugarHelper<Website>();
                return oWebsites.GetList(it => it.ID == sID && it.Deleted == false && it.Status == 1 && it.Processed != 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 获取网站分类列表
        /// </summary>
        /// <returns></returns>
        public List<Category> GetCategoryList(Website oWebsite)
        {
            try
            {
                List<Category> oListResult = new List<Category>();


                return oListResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Link> GetLinkList(Category oCategory)
        {
            try
            {
                List<Link> oListLink = new List<Link>();

                return oListLink;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetLinkDetail(Link oResult)
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
