using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class UUMP4Spider : BaseSpider
    {
        public UUMP4Spider(bool debug = true, string LoggerPath = null, Logger Logger = null) : base(debug, LoggerPath, Logger)
        {
        }

        public new int DoExce(Dictionary<string, object> arr)
        {
            try
            {

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        public override int PerExce(Dictionary<string, object> arr)
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

        /// <summary>
        /// 获取网站列表
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public override List<Website> GetWebsiteList(string sID)
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
        public override List<Category> GetCategoryList(Website oWebsite)
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
        /// 采集目标Link
        /// </summary>
        /// <returns></returns>
        public override List<Link> GetLinkList(Category oCategory)
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

        /// <summary>
        /// 采集链接详情
        /// </summary>
        /// <param name="oResult"></param>
        public override void GetLinkDetail(Link oResult)
        {
            try
            {
                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
