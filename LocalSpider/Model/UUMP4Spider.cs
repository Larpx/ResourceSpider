using Ivony.Html;
using Ivony.Html.ExpandedAPI;
using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using Larpx.ResourceSpider.Helpers.Encode;
using Larpx.ResourceSpider.Helpers.Web;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static Larpx.ResourceSpider.BaseLibrary.Data.EnumData;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class UUMP4Spider : BaseSpider
    {
        private const int m_oRepeatCount = 8;
        private const int m_oSumRepeatCount = 5;
        private const string m_oBaseURL = "https://www.uump4.net";
        private string sWebSiteID = "fe1213ba1c94e4a42b72bda9840af83c";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oDatabaseType"></param>
        /// <param name="oWebGUID"></param>
        /// <param name="sWebID"></param>
        /// <param name="debug"></param>
        /// <param name="LoggerPath"></param>
        /// <param name="Logger"></param>
        public UUMP4Spider(Guid oWebGUID, DatabaseType oDatabaseType = DatabaseType.SqlServer, string sWebID = null, bool debug = true, bool bFirst = true, string LoggerPath = null, Logger Logger = null) :
            base(oWebGUID, oDatabaseType, sWebID, debug,bFirst, LoggerPath, Logger)
        {
            //https://www.uump4.net/
        }

        public void TestExce(Dictionary<string, object> arr)
        {
            try
            {
                bool bLooper = true;
                var ow = GetWebsiteList(sWebSiteID);
                List<Category> oCategoryList = new List<Category>();
                List<Link> oLinkList = new List<Link>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);
                foreach (var item in ow)
                {
                    oCategoryList.AddRange(GetCategoryList(item));
                }

                while (bLooper)
                {
                    if (oSQLSugarHelper.LinkDb.Count(it => it.WebsiteGUID == oCategoryList[0].WebsiteGUID && it.Processed != 2 && it.Deleted == false) <= 0)
                        break;

                    foreach (var item in oCategoryList)
                    {


                        var c = oSQLSugarHelper.Db
                            .Queryable<Link>()
                            .Take(1000)
                            .Where(it => it.CategoryGUID == item.GUID && it.Processed != 2 && it.Deleted == false)
                            .OrderBy(it => it.GUID, OrderByType.Asc)
                            .ToList();

                        oLinkList.AddRange(c);
                    }

                    foreach (var item in oLinkList)
                    {
                        GetLinkDetail(item);
                    }

                    Console.WriteLine($"网站uump4采集完成");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        public new int DoExce(Dictionary<string, object> arr)
        {
            try
            {
                return base.DoExce(arr);
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
                Website website = new Website();
                SQLSugarHelper<Website> oWebsites = new SQLSugarHelper<Website>(DatabaseType, Debug);

                website.Name = "悠悠MP4-MP4电影下载-uump4-久久MP4-99mp4-悠悠鸟影视论坛-电影天堂";
                website.NameChs = website.Name;
                website.URL = m_oBaseURL;
                website.Status = 1;
                website.Deleted = false;
                website.IsCookies = true;
                website.ID = MD5.GetBufferHash(website.URL).ToLower();

                //查重
                if (!oWebsites.IsAny(it => it.ID == website.ID))
                {
                    oWebsites.Insert(website);
                    sWebSiteID = website.ID;
                }

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 获取网站列表
        /// Done
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public override List<Website> GetWebsiteList(string sID)
        {
            try
            {
                SQLSugarHelper<Website> oWebsites = new SQLSugarHelper<Website>(DatabaseType, Debug);
                return oWebsites.GetList(it => it.ID == sID && it.Deleted == false && it.Status == 1 && it.Processed != 2);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
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
                string sHTML = "";
                string sTagURL = "";
                //分类合集
                string sCategoryCollection = ".navbar-nav.mr-auto .nav-item a";
                //meta信息
                string sMetaData = "meta";
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);

                //整理分类不规范页面地址
                if (!oWebsite.URL.StartsWith("http"))
                {
                    //处理不规范URL
                    oWebsite.URL = "http://" + oWebsite.URL;
                    oWebsite.ID = MD5.GetBufferHash(oWebsite.URL).ToLower();
                    sWebSiteID = oWebsite.ID;
                    oSQLSugarHelper.WebsiteDb.Update(oWebsite);
                }

                switch (oWebsite.Processed)
                {
                    case 0:
                        //获取Cookies
                        CookieCollection oCookies = null;

                        if (oWebsite.IsCookies)
                            oCookies = EasyHttpHelper.GetCookie(new Uri(oWebsite.URL), false);

                        //请求页面
                        if (oWebsite.IsCookies)
                            oPageDocument = ReadDataEncodeHTML(oWebsite.URL, ref sHTML, oCookies);
                        else
                            oPageDocument = ReadDataEncodeHTML(oWebsite.URL, ref sHTML);

                        //解析页面
                        if (oPageDocument != null)
                        {
                            if (oPageDocument.Exists(sCategoryCollection))
                            {
                                var oCategoryEnmuar = oPageDocument.Find(sCategoryCollection);
                                foreach (var item in oCategoryEnmuar)
                                {
                                    if (item.Attribute("href").AttributeValue == "javascript:void(0);" || item.Attribute("href").AttributeValue == "."
                                        || string.IsNullOrEmpty(item.Attribute("href").AttributeValue))
                                        continue;

                                    Category oCategory = new Category();
                                    oCategory.WebsiteGUID = oWebsite.GUID;
                                    oCategory.Status = 1;
                                    oCategory.Name = item.InnerText();
                                    oCategory.URL = (oWebsite.URL.EndsWith("/") ? oWebsite.URL.TrimEnd('/') : oWebsite.URL)
                                        + (item.Attribute("href").AttributeValue.StartsWith("/") ? item.Attribute("href").AttributeValue : "/" + item.Attribute("href").AttributeValue);
                                    sTagURL = oCategory.URL;

                                    if (!oSQLSugarHelper.CategoryDb.IsAny(it => it.URL == oCategory.URL))
                                        oSQLSugarHelper.CategoryDb.Insert(oCategory);
                                    else
                                        continue;
                                }
                            }
                            else
                            {
                                //未找到分类标签
                                Console.WriteLine("当前任务未找到‘" + sCategoryCollection + "’元素");
                            }

                            //MetaData
                            if (oPageDocument.Exists(sMetaData))
                            {
                                var oListMeta = oSQLSugarHelper.MetaDataDb.GetList(it => it.WebsiteGUID == oWebsite.GUID);
                                foreach (var item in oListMeta)
                                {
                                    oSQLSugarHelper.MetaDataDb.Delete(item);
                                }

                                var oCategoryEnmuar = oPageDocument.Find(sMetaData);
                                foreach (var item in oCategoryEnmuar)
                                {
                                    MetaData metaData = new MetaData();

                                    metaData.WebsiteGUID = oWebsite.GUID;

                                    if (item.Attribute("charset") != null)
                                    {
                                        metaData.Name = "charset";
                                        metaData.Content = item.Attribute("charset").AttributeValue;
                                        metaData.Type = (byte)MetaType.Charset;
                                    }
                                    else if (item.Attribute("name") != null)
                                    {
                                        switch (item.Attribute("name").AttributeValue.ToLower())
                                        {
                                            case "viewport":
                                                metaData.Name = "viewport";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Viewport;
                                                break;
                                            case "keywords":
                                                metaData.Name = "keywords";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Keywords;
                                                break;
                                            case "description":
                                                metaData.Name = "description";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Description;
                                                break;
                                            case "renderer":
                                                metaData.Name = "renderer";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Renderer;
                                                break;
                                            default:
                                                metaData.Name = "other";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Other;
                                                break;
                                        }
                                    }
                                    else if (item.Attribute("http-equiv") != null)
                                    {
                                        switch (item.Attribute("http-equiv").AttributeValue.ToLower())
                                        {
                                            case "x-ua-compatible":
                                                metaData.Name = "X-UA-Compatible";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.X_UA_Compatible;
                                                break;
                                            case "cache-control":
                                                metaData.Name = "Cache-Control";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Cache_Control;
                                                break;
                                            default:
                                                metaData.Name = "other";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)MetaType.Other;
                                                break;
                                        }
                                    }

                                    oSQLSugarHelper.MetaDataDb.Insert(metaData);
                                }
                            }
                            else
                            {
                                //未找到分类标签
                                Console.WriteLine("当前任务未找到‘" + sMetaData + "’元素");
                            }

                            //category
                            var oListCate = oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Status == 1 && it.Deleted == false);
                            foreach (var oCates in oListCate)
                            {
                                //解析标签
                                string sTag = ".small.nav_tag_list tr";

                                //请求页面
                                if (oWebsite.IsCookies)
                                    oPageDocument = ReadDataEncodeHTML(oCates.URL, ref sHTML, oCookies);
                                else
                                    oPageDocument = ReadDataEncodeHTML(oCates.URL, ref sHTML);

                                if (oPageDocument != null)
                                {
                                    if (oPageDocument.Exists(sTag))
                                    {
                                        var oCategoryEnmuar = oPageDocument.Find(sTag);
                                        foreach (var item in oCategoryEnmuar)
                                        {
                                            Guid oGUID = Guid.NewGuid();
                                            if (item.Exists(".text-nowrap"))
                                            {
                                                PropertyKey propertyKey = new PropertyKey();
                                                var oTr = item.FindFirst(".text-nowrap");
                                                propertyKey.Type = 0;
                                                propertyKey.CategoryGUID = oCates.GUID;
                                                propertyKey.WebsiteGUID = oWebsite.GUID;
                                                propertyKey.Name = oTr.InnerText().Trim();
                                                if (propertyKey.Name.EndsWith(":"))
                                                    propertyKey.Name = propertyKey.Name.Trim().TrimEnd(':');
                                                propertyKey.NameChs = propertyKey.Name;

                                                if (!oSQLSugarHelper.PropertyKeyDb.IsAny(it => it.Name == propertyKey.Name && it.WebsiteGUID == oWebsite.GUID && it.CategoryGUID == propertyKey.CategoryGUID))
                                                {
                                                    oSQLSugarHelper.PropertyKeyDb.Insert(propertyKey);
                                                }

                                                var oList = oSQLSugarHelper.PropertyKeyDb.GetList(it => it.Name == propertyKey.Name && it.WebsiteGUID == propertyKey.WebsiteGUID && it.CategoryGUID == propertyKey.CategoryGUID);
                                                oGUID = oList[0].GUID;
                                            }
                                            else
                                                Console.WriteLine("当前任务未找到‘.text-nowrap’元素");

                                            if (item.Exists("td a"))
                                            {
                                                foreach (var itemChild in item.Find("td a"))
                                                {
                                                    PropertyValue propertyValue = new PropertyValue();
                                                    propertyValue.WebsiteGUID = oWebsite.GUID;
                                                    propertyValue.CategoryGUID = oCates.GUID;
                                                    propertyValue.Name = itemChild.InnerText().Trim();
                                                    propertyValue.Type = 0;
                                                    propertyValue.NameChs = propertyValue.Name;
                                                    propertyValue.KeyGUID = oGUID;

                                                    if (!oSQLSugarHelper.PropertyValueDb.IsAny(it => it.Name == propertyValue.Name && it.WebsiteGUID == oWebsite.GUID && it.CategoryGUID == propertyValue.CategoryGUID))
                                                        oSQLSugarHelper.PropertyValueDb.Insert(propertyValue);
                                                }
                                            }
                                            else
                                                Console.WriteLine("当前任务未找到‘td a’元素");
                                        }
                                    }
                                    else
                                        Console.WriteLine("当前任务未找到‘" + sTag + "’元素");

                                }
                                else
                                    Console.WriteLine("页面解析失败");
                            }
                        }
                        else
                        {
                            //解析页面失败
                            Console.WriteLine("页面解析失败");
                        }

                        oWebsite.Processed = (byte)ProcessedType.Success;
                        oSQLSugarHelper.WebsiteDb.Update(oWebsite);

                        return oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Status == 1 && it.Deleted == false);

                    case 1:
                    default:
                        return oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Status == 1 && it.Deleted == false && it.Processed != 2);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 采集目标Link
        /// </summary>
        /// <returns></returns>
        public override List<Link> GetLinkList(Category oCategory)
        {
            int nReSUM = 0;
            int nReCount = 0;

            bool bFirst = true;
            int iEndPageNum = 0;
            int iThisPageNum = 1;
            string sGetUrl = "";

            try
            {
                //列表页集合
                string sLinkColllration = ".card-body2 ul li div.subject";
                //当前页码
                string sThisPageNum = ".my-3 ul li.active a";
                //总页码
                string sTotlaPageNum = ".my-3 ul li a";
                //下一页地址
                string sNextURL = ".my-3 ul li a";

                string sHTML = "";
                bool bGetCookie = false;
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                List<Link> oListLink = new List<Link>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);

                //整理分类不规范页面地址
                var oWebs = oSQLSugarHelper.WebsiteDb.GetById(oCategory.WebsiteGUID);
                if (!oCategory.URL.StartsWith("http"))
                {
                    //处理不规范URL
                    oCategory.URL = oWebs.URL + oCategory.URL;
                    oSQLSugarHelper.CategoryDb.Update(oCategory);
                }

                sGetUrl = oCategory.URL;

                //获取Cookies
                var oCookies = new CookieCollection();

                if (bGetCookie)
                    oCookies = EasyHttpHelper.GetCookie(new Uri(sGetUrl), false);

                GetUrl:
                nReCount = 0;

                //判断重复页面数量,触发阈值直接结束
                if (m_oSumRepeatCount <= nReSUM)
                {
                    Console.WriteLine("当前分类任务重复页面数为：" + nReSUM + ",已触发阈值,结束采集任务.");
                    goto MainEnds;
                }

                //请求页面
                if (bGetCookie)
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML, oCookies);
                else
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML);

                //解析页面
                if (oPageDocument != null)
                {
                    //获取总页码
                    if (bFirst)
                    {
                        bFirst = false;
                        if (oPageDocument.Exists(sTotlaPageNum))
                        {
                            var oPageListTmp = oPageDocument.Find(sTotlaPageNum).ToArray();
                            var oTotalPageElement = oPageListTmp[oPageListTmp.Count() - 2];

                            string sTotalstr = oTotalPageElement.InnerText().TrimStart('.');
                            iEndPageNum = Convert.ToInt32(sTotalstr);
                        }
                        else
                            iEndPageNum = 1;
                    }

                    //获取本页页码
                    if (oPageDocument.Exists(sThisPageNum))
                        iThisPageNum = Convert.ToInt32(oPageDocument.FindFirst(sThisPageNum).InnerText());
                    else
                        iThisPageNum++;

                    //组合下一页地址
                    if (oPageDocument.Exists(sNextURL))
                    {
                        string sNextStr = oPageDocument.FindLastOrDefault(sNextURL)?.Attribute("href").AttributeValue;
                        sGetUrl = (oWebs.URL.EndsWith("/") ? oWebs.URL : oWebs.URL + "/") + sNextStr;
                    }
                    else
                        sGetUrl = oCategory.URL.Substring(0, oCategory.URL.Length - 4) + "-" + (iThisPageNum + 1) + ".htm?orderby=lastpid&digest=0";

                    //解析页面
                    if (oPageDocument.Exists(sLinkColllration))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(sLinkColllration);
                        foreach (var item in oCategoryEnmuar)
                        {
                            //判断重复
                            if (nReCount >= m_oRepeatCount)
                            {
                                Console.WriteLine("本页重复数据量为：" + nReCount + "，触发阈值，跳过本页，处理下一页。");
                                Console.WriteLine("当前任务页码：" + iThisPageNum);
                                Console.WriteLine("总任务页码：" + iEndPageNum);
                                Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                                nReSUM++;
                                goto GetUrl;
                            }

                            var itemA = item.FindLast("a");
                            if (itemA != null && itemA.Attribute("class") == null)
                            {
                                //Link
                                Link oLink = new Link();
                                oLink.GUID = Guid.NewGuid();
                                oLink.CategoryGUID = oCategory.GUID;
                                oLink.WebsiteGUID = oCategory.WebsiteGUID;
                                oLink.URL = oWebs.URL + "/" + itemA.Attribute("href").AttributeValue;
                                oLink.SN = Helpers.CommonHelper.GenerateNonceStr();
                                oLink.ID = MD5.GetBufferHash(oLink.URL);
                                oLink.Name = itemA.InnerText();
                                oLink.NameChs = itemA.InnerText();
                                //0 视频，1图片，2文字 ，3其他
                                oLink.Type = (byte)ResourceDataType.Vedio;

                                if (!oSQLSugarHelper.LinkDb.IsAny(it => it.ID == oLink.ID && it.WebsiteGUID == oCategory.WebsiteGUID && it.CategoryGUID == oCategory.GUID))
                                    oSQLSugarHelper.LinkDb.Insert(oLink);
                                else
                                {
                                    nReCount++;
                                    continue;
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("当前任务未找到‘" + sLinkColllration + "’元素");
                    }

                    Console.WriteLine("当前任务页码：" + iThisPageNum);
                    Console.WriteLine("总任务页码：" + iEndPageNum);
                    Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                    if (iThisPageNum < iEndPageNum)
                        goto GetUrl;
                }
                else
                {
                    //解析页面失败
                    Console.WriteLine("页面解析失败");
                }

                oCategory.Processed = (byte)ProcessedType.Success;
                oSQLSugarHelper.CategoryDb.Update(oCategory);

            MainEnds:
                Console.WriteLine("当前任务：" + oCategory.Name + "采集完成.");

                return oSQLSugarHelper.LinkDb.GetList(it => it.CategoryGUID == oCategory.GUID && it.Deleted == false);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Console.WriteLine("当前任务页码：" + iThisPageNum);
                Console.WriteLine("总任务页码：" + iEndPageNum);
                Console.WriteLine("当前任务分类名称为：" + oCategory.Name);
#if DEBUG
                Console.ReadLine();
#endif
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
                //标题
                string sTitle = ".media-body .break-all";
                //海报 [0]
                string sBannerImages = ".message.break-all img";
                //简介
                string sDetail = ".message.break-all";
                //截图 [1]
                string sScreenShot = ".message.break-all img";
                //资源链接
                string sResourceLinks = "strong";
                //种子
                string sTorrent = ".fieldset ul li a";

                string sGetUrl = "";
                string sHTML = "";
                bool bGetCookie = false;
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);

                //整理分类不规范页面地址
                var oWebs = oSQLSugarHelper.WebsiteDb.GetById(oResult.WebsiteGUID);
                if (!oResult.URL.StartsWith("http"))
                {
                    //处理不规范URL
                    oResult.URL = (oWebs.URL.EndsWith("/") ? oWebs.URL : oWebs.URL + "/") + (oResult.URL.StartsWith("/") ? oResult.URL.TrimStart('/') : oResult.URL);
                    oSQLSugarHelper.LinkDb.Update(oResult);
                }

                sGetUrl = oResult.URL;

                //获取Cookies
                var oCookies = new CookieCollection();

                if (bGetCookie)
                    oCookies = EasyHttpHelper.GetCookie(new Uri(sGetUrl), false);

                //请求页面
                if (bGetCookie)
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML, oCookies);
                else
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML);

                //解析页面
                if (oPageDocument != null)
                {
                    if (!oPageDocument.Exists(sTitle))
                    {
                        oResult.Processed = (byte)ProcessedType.Fail;
                        oSQLSugarHelper.LinkDb.Update(oResult);
                        return;
                    }

                    oResult.Title = oPageDocument.FindFirst(sTitle).InnerText();
                    //简介
                    oResult.Detail = oPageDocument.FindFirstOrDefault(sDetail)?.InnerText();
                    oResult.DetailChs = oPageDocument.FindFirstOrDefault(sDetail)?.InnerHtml();

                    //Banner图
                    Resource oResource = new Resource();
                    if (oPageDocument.Exists(sBannerImages))
                    {
                        int nBannerImages = 0, nReBannerImages = 0;
                        var oImgEle = oPageDocument.FindFirst(sBannerImages);
                        oResource.WebsiteGUID = oResult.WebsiteGUID;
                        oResource.PageGUID = oResult.GUID;
                        oResource.URL = oImgEle.Attribute("src").AttributeValue;
                        oResource.Original = Path.GetFileName(oResource.URL);
                        oResource.Path = oResource.Original;
                        oResource.FileName = oResource.Path;
                        oResource.Type = (byte)ResourceType.Banner;
                        if (!oSQLSugarHelper.ResourceDb.IsAny(it => it.URL == oResource.URL))
                        {
                            oSQLSugarHelper.ResourceDb.Insert(oResource);
                            nBannerImages++;
                        }
                        else
                            nReBannerImages++;
                        Console.WriteLine("当前任务添加Banner图：" + nBannerImages + "张，重复图" + nReBannerImages + "张");
                    }
                    else
                        Console.WriteLine("当前任务未找到‘" + sBannerImages + "’元素");

                    //截图
                    if (oPageDocument.Exists(sScreenShot))
                    {
                        int nScreenShot = 0, nResScreenShot = 0;
                        var oImgList = oPageDocument.Find(sScreenShot);
                        foreach (var item in oImgList)
                        {
                            if (item.Attribute("src") != null && item.Attribute("src").AttributeValue == oResource.URL)
                                continue;
                            if (item.Attribute("src") != null && item.Attribute("src").AttributeValue.Contains("thinkphp.php"))
                                continue;

                            Resource oResourceTmp = new Resource();
                            oResourceTmp.WebsiteGUID = oResult.WebsiteGUID;
                            oResourceTmp.PageGUID = oResult.GUID;
                            oResourceTmp.URL = item.Attribute("src").AttributeValue;
                            oResourceTmp.Original = Path.GetFileName(oResource.URL);
                            oResourceTmp.Path = oResourceTmp.Original;
                            oResourceTmp.FileName = oResourceTmp.Path;
                            oResourceTmp.Type = (byte)ResourceType.Detail;
                            if (!oSQLSugarHelper.ResourceDb.IsAny(it => it.URL == oResourceTmp.URL))
                            {
                                oSQLSugarHelper.ResourceDb.Insert(oResourceTmp);
                                nScreenShot++;
                            }
                            else
                                nResScreenShot++;

                        }
                        Console.WriteLine("当前任务添加内容图：" + nScreenShot + "张，重复图" + nResScreenShot + "张");
                    }
                    else
                        Console.WriteLine("当前任务未找到‘" + sScreenShot + "’元素");

                    //资源
                    if (oPageDocument.Exists(sResourceLinks))
                    {
                        int nStrong = 0, nReStrong = 0;
                        var oStrongList = oPageDocument.Find(sResourceLinks);
                        foreach (var item in oStrongList)
                        {
                            if (item.InnerText().Trim().Contains("http") || item.InnerText().Trim().Contains("ed2k") ||
                                item.InnerText().Trim().Contains("magnet"))
                            {
                                var oTmpArr = item.InnerText().Trim().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var oArrItem in oTmpArr)
                                {
                                    if (!oArrItem.StartsWith("http") && !oArrItem.StartsWith("ed2k") &&
                                        !oArrItem.StartsWith("magnet"))
                                        continue;

                                    ResourceData oResourceData = new ResourceData();
                                    oResourceData.WebsiteGUID = oResult.WebsiteGUID;
                                    oResourceData.ObjectGUID = oResult.GUID;
                                    oResourceData.URL = item.Attribute("href").AttributeValue;
                                    oResourceData.Original = item.InnerText().Trim();
                                    oResourceData.Name = oResourceData.Original;
                                    oResourceData.File = oResourceData.Original;
                                    oResourceData.URLType = (byte)URLType.URL;
                                    oResourceData.ResourceType = (byte)ResourceDataType.Vedio;
                                    oResourceData.Status = (byte)ResourceDataStatus.Normal;
                                    oResourceData.Processed = (byte)ProcessedType.Pending;
                                    if (!oSQLSugarHelper.ResourceDataDb.IsAny(it => it.URL == oResourceData.URL))
                                    {
                                        oSQLSugarHelper.ResourceDataDb.Insert(oResourceData);
                                        nStrong++;
                                    }
                                    else
                                        nReStrong++;
                                }
                            }
                        }
                        Console.WriteLine("当前任务添加资源链接：" + nStrong + "个，重复" + nReStrong + "个");
                    }
                    else
                        Console.WriteLine("当前任务未找到‘" + sResourceLinks + "’元素");

                    //种子
                    if (oPageDocument.Exists(sTorrent))
                    {
                        int nStrong = 0, nReStrong = 0;
                        var oTorrentList = oPageDocument.Find(sTorrent);
                        foreach (var item in oTorrentList)
                        {
                            if (item.Attribute("src") != null && item.Attribute("src").AttributeValue == oResource.URL)
                                continue;
                            if (item.Attribute("src") != null && item.Attribute("src").AttributeValue.Contains("thinkphp.php"))
                                continue;

                            ResourceData oResourceData = new ResourceData();
                            oResourceData.WebsiteGUID = oResult.WebsiteGUID;
                            oResourceData.ObjectGUID = oResult.GUID;
                            oResourceData.URL = item.Attribute("href").AttributeValue;
                            oResourceData.Original = item.InnerText().Trim();
                            oResourceData.Name = oResourceData.Original;
                            oResourceData.File = oResourceData.Original;
                            oResourceData.URLType = (byte)URLType.URL;
                            oResourceData.ResourceType = (byte)ResourceDataType.Vedio;
                            oResourceData.Status = (byte)ResourceDataStatus.Normal;
                            oResourceData.Processed = (byte)ProcessedType.Pending;
                            if (!oSQLSugarHelper.ResourceDataDb.IsAny(it => it.URL == oResourceData.URL))
                            {
                                oSQLSugarHelper.ResourceDataDb.Insert(oResourceData);
                                nStrong++;
                            }
                            else
                                nReStrong++;
                        }
                        Console.WriteLine("当前任务添加资源链接：" + nStrong + "个，重复" + nReStrong + "个");
                    }
                    else
                        Console.WriteLine("当前任务未找到‘" + sTorrent + "’元素");
                }
                else
                {
                    //解析页面失败
                }

                oResult.Processed = (byte)ProcessedType.Success;
                oSQLSugarHelper.LinkDb.Update(oResult);
                Console.WriteLine("当前任务已完成。");
                Console.WriteLine("当前任务链接：" + oResult.URL);
                Console.WriteLine("当前任务名称：" + oResult.Name);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Console.WriteLine("当前任务名称为：" + oResult.Name);
#if DEBUG
                Console.ReadLine();
#endif
                throw ex;
            }
        }
    }
}
