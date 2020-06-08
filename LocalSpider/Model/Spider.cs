using Larpx.ResourceSpider.Engine;
using System.Collections.Generic;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    interface Spider
    {
        /// <summary>
        /// 获取抓取网站地址
        /// </summary>
        /// <returns></returns>
        List<Website> GetWebsiteList();

        /// <summary>
        /// 采集分类链接
        /// </summary>
        /// <returns></returns>
        List<Category> GetCategoryList();

        /// <summary>
        /// 采集资源链接
        /// </summary>
        /// <returns></returns>
        List<Link> GetLinkList();

        /// <summary>
        /// 采集资源链接详情
        /// </summary>
        /// <param name="oResult"></param>
        void GetLinkDetail(List<Link> oResult);

        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        int DoExce(params Dictionary<string, object>[] arr);

        /// <summary>
        /// 预操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        int PerExce(params Dictionary<string,object>[] arr);
    }
}
