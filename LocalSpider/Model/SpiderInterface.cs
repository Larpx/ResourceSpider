using Larpx.ResourceSpider.Engine;
using System.Collections.Generic;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    interface SpiderInterface
    {
        /// <summary>
        /// 获取抓取网站地址
        /// </summary>
        /// <returns></returns>
        List<Website> GetWebsiteList(string sID);
        //List<Website> GetWebsiteList(string sID, DatabaseType databaseType = DatabaseType.SqlServer);

        /// <summary>
        /// 采集分类链接
        /// </summary>
        /// <returns></returns>
        List<Category> GetCategoryList(Website oWebsite);
        //List<Category> GetCategoryList(Website oWebsite, DatabaseType databaseType = DatabaseType.SqlServer);

        /// <summary>
        /// 采集资源链接
        /// </summary>
        /// <returns></returns>
        List<Link> GetLinkList(Category oCategory);
        //List<Link> GetLinkList(Category oCategory, DatabaseType databaseType = DatabaseType.SqlServer);

        /// <summary>
        /// 采集资源链接详情
        /// </summary>
        /// <param name="oResult"></param>
        void GetLinkDetail(Link oResult);
        //void GetLinkDetail(Link oResult, DatabaseType databaseType = DatabaseType.SqlServer);

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        int DoExce(Dictionary<string, object> arr = null);

        /// <summary>
        /// 预操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        int PerExce(Dictionary<string, object> arr = null);
    }
}
