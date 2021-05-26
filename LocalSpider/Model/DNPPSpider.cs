using Ivony.Html;
using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using Larpx.ResourceSpider.Helpers.Encode;
using Larpx.ResourceSpider.Helpers.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static Larpx.ResourceSpider.BaseLibrary.Data.EnumData;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class DNPPSpider : BaseSpider
    {
        private bool bFirst = true;
        private const int m_oRepeatCount = 5;
        private const string m_sNorListPage = ".list-group .a-list";
        private const string m_sMovieListPage = ".card-link";
        private const string m_sThisPage = ".page-item.active";
        private const string m_sDetailImg = "#content img";
        private const string m_sXSDetail = "#content";
        private string sWebSiteID = "efec65b622aea9d13d68aac07fc61c6a";

        public DNPPSpider(Guid oWebGUID, DatabaseType oDatabaseType = DatabaseType.SqlServer, string sWebID = null, bool debug = true, bool bFirst = true, string LoggerPath = null, Logger Logger = null) :
            base(oWebGUID, oDatabaseType, sWebID, debug, bFirst, LoggerPath, Logger)
        {
            this.bFirst = bFirst;
        }

        /// <summary>
        /// 执行操作
        /// Done
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public new void TestExce(Dictionary<string, object> arr)
        {
            try
            {
                var ow = GetWebsiteList(sWebSiteID);
                List<Category> oCategoryList = new List<Category>();
                List<Link> oLinkList = new List<Link>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);
                foreach (var item in ow)
                {
                    oCategoryList.AddRange(GetCategoryList(item));
                }

                foreach (var item in oCategoryList)
                {
                    GetLinkList(item);
                }


                //while (bLooper)
                //{
                //    if (oSQLSugarHelper.LinkDb.Count(it => it.WebsiteGUID == oCategoryList[0].WebsiteGUID && it.Processed != 2 && it.Deleted == false) <= 0)
                //        break;

                //    foreach (var item in oCategoryList)
                //    {
                //        var c = oSQLSugarHelper.Db
                //            .Queryable<Link>()
                //            .Take(1000)
                //            //.Where(it => it.CategoryGUID == item.GUID && it.Processed != 2 && it.Deleted == false)
                //            //小说
                //            .Where(it => it.CategoryGUID == new Guid("1a250f05-4a4a-4233-9f6c-9815cb9806db"))
                //            //视频
                //            //.Where(it => it.GUID == new Guid("1a250f05-4a4a-4233-9f6c-9815cb9806db"))
                //            .OrderBy(it => it.GUID, OrderByType.Asc)
                //            .ToList();

                //        oLinkList.AddRange(c);
                //    }

                //    foreach (var item in oLinkList)
                //    {
                //        GetLinkDetail(item);
                //    }

                //    Console.WriteLine($"网站{sWebSiteID}采集完成");
                //}
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 执行操作
        /// Done
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public new void UpdateExce(Dictionary<string, object> arr)
        {
            try
            {
                var ow = GetWebsiteList(sWebSiteID);
                List<Category> oCategoryList = new List<Category>();
                List<Link> oLinkList = new List<Link>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);

                foreach (var item in ow)
                {
                    oCategoryList.AddRange(
                        oSQLSugarHelper
                        .CategoryDb
                        .GetList(it =>
                                it.WebsiteGUID == item.GUID &&
                                it.Status == 1 &&
                                it.Deleted == false &&
                                it.Processed == 2));
                }

                foreach (var item in oCategoryList)
                {
                    GetLinkList(item);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 执行操作
        /// Done
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public new int DoExce(Dictionary<string, object> arr)
        {
            try
            {
                arr = new Dictionary<string, object>();
                arr.Add("ID", sWebSiteID);

                base.DoExce(arr);

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 前置操作
        /// Done
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public override int PerExce(Dictionary<string, object> arr)
        {
            try
            {
                SQLSugarHelper<Website> oWebsites = new SQLSugarHelper<Website>(DatabaseType, Debug);

                Website website = new Website();
                website.Name = "8468.xyz";
                website.URL = "http://8468.xyz";
                website.Status = 1;
                website.Deleted = false;
                website.IsCookies = false;
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
                if (bFirst)
                    return oWebsites.GetList(it => it.ID == sID && it.Deleted == false && it.Status == 1 && it.Processed != 2);
                else
                    return oWebsites.GetList(it => it.ID == sID && it.Deleted == false && it.Status == 1 && it.Processed == 2);
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
                string sMetaData = "meta";
                string sCategoryCollection = ".dropdown-menu a";

                string sHTML = "";
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
                        var oCookies = EasyHttpHelper.GetCookie(new Uri(oWebsite.URL), false);

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

                                    if (!oSQLSugarHelper.CategoryDb.IsAny(it => it.URL == oCategory.URL))
                                    {
                                        oSQLSugarHelper.CategoryDb.Insert(oCategory);
                                    }
                                    else
                                        continue;
                                }
                            }
                            else
                            {
                                //未找到分类标签
                            }

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
                        }
                        else
                        {
                            //解析页面失败
                        }

                        return oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Processed != 2 && it.Status == 1 && it.Deleted == false);

                    case 1:
                    default:
                        if(this.bFirst)
                            return oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Status == 1 && it.Deleted == false && it.Processed != 2);
                        else
                            return oSQLSugarHelper.CategoryDb.GetList(it => it.WebsiteGUID == oWebsite.GUID && it.Status == 1 && it.Deleted == false && it.Processed == 2);
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
            int iEndPageNum = 0;
            int iThisPageNum = 1;
            string sGetUrl = "";

            try
            {
                int nSumReCount = 0;
                int nReCount = 0;

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

                //请求页面
                if (bGetCookie)
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML, oCookies);
                else
                    oPageDocument = ReadDataEncodeHTML(sGetUrl, ref sHTML);

                //解析页面
                if (oPageDocument != null)
                {
                    var oNextPage = oPageDocument.FindLast(".page-item a");

                    //组合下一页地址
                    sGetUrl = oNextPage.Attribute("href").AttributeValue;
                    if (!sGetUrl.StartsWith("http"))
                        sGetUrl = oWebs.URL + sGetUrl;

                    //获取本页页码
                    if (oPageDocument.Exists(m_sThisPage))
                        iThisPageNum = Convert.ToInt32(oPageDocument.FindFirst(m_sThisPage).InnerText());
                    else
                        iThisPageNum++;

                    //判断重复
                    if (nSumReCount >= 24)
                    {
                        oCategory.UpdateTime = DateTime.Now;
                        oCategory.UpdateTimes += 1;
                        oSQLSugarHelper.CategoryDb.Update(oCategory);
                        Console.WriteLine("更新完成。");
                        return oListLink;
                    }

                    //解析页面
                    if (oPageDocument.Exists(m_sNorListPage))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(m_sNorListPage);
                        foreach (var item in oCategoryEnmuar)
                        {
                            //判断重复
                            if (nReCount >= m_oRepeatCount)
                            {
                                Console.WriteLine("本页重复数据量为：" + iThisPageNum + "，触发阈值，需跳过。");
                                Console.WriteLine("当前任务页码：" + iThisPageNum);
                                Console.WriteLine("总任务页码：" + iEndPageNum);
                                Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                                goto GetUrl;
                            }

                            Link oLink = new Link();
                            oLink.GUID = Guid.NewGuid();
                            oLink.CategoryGUID = oCategory.GUID;
                            oLink.WebsiteGUID = oCategory.WebsiteGUID;
                            oLink.URL = oWebs.URL + item.Attribute("href").AttributeValue;
                            oLink.SN = Helpers.CommonHelper.GenerateNonceStr();
                            oLink.ID = MD5.GetBufferHash(oLink.URL);
                            oLink.Name = item.InnerText();
                            oLink.NameChs = item.InnerText();
                            //0 视频，1图片，2文字 ，3其他
                            oLink.Type = 1;

                            if (!oSQLSugarHelper.LinkDb.IsAny(it => it.ID == oLink.ID))
                                oSQLSugarHelper.LinkDb.Insert(oLink);
                            else
                            {
                                nReCount++;
                                nSumReCount += nReCount;
                                continue;
                            }
                        }
                    }
                    else if (oPageDocument.Exists(m_sMovieListPage))
                    {
                        //未找到分类标签 box movie_list
                        var oCategoryEnmuar = oPageDocument.Find(m_sMovieListPage);
                        foreach (var item in oCategoryEnmuar)
                        {
                            //判断重复
                            if (nReCount >= m_oRepeatCount)
                            {
                                Console.WriteLine("本页重复数据量为：" + iThisPageNum + "，触发阈值，需跳过。");
                                Console.WriteLine("当前任务页码：" + iThisPageNum);
                                Console.WriteLine("总任务页码：" + iEndPageNum);
                                Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                                goto GetUrl;
                            }

                            Link oLink = new Link();
                            oLink.GUID = Guid.NewGuid();
                            oLink.CategoryGUID = oCategory.GUID;
                            oLink.WebsiteGUID = oCategory.WebsiteGUID;
                            if (!item.Attribute("href").AttributeValue.StartsWith("http"))
                                oLink.URL = oWebs.URL + item.Attribute("href").AttributeValue;
                            else
                                oLink.URL = item.Attribute("href").AttributeValue;
                            oLink.SN = Helpers.CommonHelper.GenerateNonceStr();
                            oLink.ID = MD5.GetBufferHash(oLink.URL);
                            oLink.Name = item.InnerText();
                            oLink.NameChs = oLink.Name;
                            //0 视频，1图片，2文字 ，3其他
                            oLink.Type = 0;

                            if (!oSQLSugarHelper.LinkDb.IsAny(it => it.ID == oLink.ID))
                                oSQLSugarHelper.LinkDb.Insert(oLink);
                            else
                            {
                                nReCount++;
                                nSumReCount += nReCount;
                                continue;
                            }
                        }
                    }
                    else
                    {

                    }

                    Console.WriteLine("当前任务页码：" + iThisPageNum);
                    Console.WriteLine("总任务页码：" + iEndPageNum);
                    Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                    if (oNextPage.InnerText().Trim() == "〉")
                        goto GetUrl;
                }
                else
                {
                    Console.WriteLine($"页面解析失败，当前地址为{sGetUrl}");
                }

                oCategory.Processed = 2;
                oCategory.UpdateTime = DateTime.Now;
                oCategory.UpdateTimes += 1;
                oSQLSugarHelper.CategoryDb.Update(oCategory);

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
                string sDetail = ".message.break-all p";
                //截图 [1]
                string sScreenShot = ".message.break-all img";
                //资源链接
                string sResourceLinks = ".message.break-all p";
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
                    int nSum = 0;

                    if (oPageDocument.Exists("#content") && oPageDocument.FindFirst("#content").InnerText().Contains("magnet:?"))
                    {
                        //图片
                        var list = oPageDocument.Find("#content img");
                        foreach (var item in list)
                        {
                            if (item.Attribute("src") != null && !string.IsNullOrWhiteSpace(item.Attribute("src").AttributeValue))
                            {
                                Resource resource = new Resource();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.PageGUID = oResult.GUID;
                                resource.URL = item.Attribute("src").AttributeValue;
                                resource.Original = Path.GetFileName(resource.URL);
                                resource.Type = 1;
                                resource.Path = resource.URL;
                                resource.FileName = resource.Original;

                                var bResult = oSQLSugarHelper.ResourceDb.Insert(resource);
                                if (bResult)
                                {
                                    Console.WriteLine($"资源链接：{resource.URL} 存储成功！");
                                    nSum++;
                                }
                                else
                                    Console.WriteLine($"资源链接：{resource.URL} 存储失败！");
                            }
                            else
                                Console.WriteLine($"资源链接：获取失败。");
                        }
                        Console.WriteLine($"资源页面共存储{nSum}条数据。");

                        //视频
                        var list2 = oPageDocument.Find("#content");
                        foreach (var item in list2)
                        {
                            if (!string.IsNullOrWhiteSpace(item.InnerText()))
                            {
                                ResourceData resource = new ResourceData();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.ObjectGUID = oResult.GUID;
                                resource.URL = item.InnerText();

                                if (resource.URL.Contains("http") && !resource.URL.Contains("magnet:?xt=urn:btih:"))
                                {
                                    resource.Original = Path.GetFileName(resource.URL);
                                    resource.URLType = 0;
                                    resource.ResourceType = 1;

                                }
                                else if (resource.URL.Contains("thunder") &&
                                    (Regex.IsMatch(resource.URL, @"thunder://[a-za-z0-9]*")))
                                {
                                    resource.Original = Regex.Match(resource.URL, @"thunder://[a-za-z0-9]*").Value;
                                    resource.URLType = 0;
                                    //迅雷链接
                                    resource.ResourceType = 3;
                                }
                                else if (resource.URL.Contains("magnet") &&
                                    (Regex.IsMatch(resource.URL, @"(magnet:\?xt=urn:btih:)[\w]+")))
                                {
                                    resource.Original = Regex.Match(resource.URL, @"(magnet:\?xt=urn:btih:)[\w]+").Value;
                                    resource.URLType = 0;
                                    //magnet:?
                                    resource.ResourceType = 2;
                                }
                                else if (resource.URL.Contains("ed2k") &&
                                    (Regex.IsMatch(resource.URL, @"^ed2k:\/\/\|file\|.+\|\/$")))
                                {
                                    resource.Original = Regex.Match(resource.URL, @"^ed2k:\/\/\|file\|.+\|\/$").Value;
                                    resource.URLType = 0;
                                    //magnet:?
                                    resource.ResourceType = 5;
                                }
                                else
                                {
                                    resource.Original = item.Attribute("id").AttributeValue;
                                    resource.URLType = 0;
                                    resource.ResourceType = 4;
                                }
                                resource.Status = 1;
                                resource.Name = oPageDocument.FindFirst("title").InnerText().Trim();
                                if (resource.Name.EndsWith("抖內啪啪"))
                                    resource.Name = resource.Name.Replace("抖內啪啪", "");
                                resource.File = resource.Name;
                                resource.Processed = 0;

                                var bResult = oSQLSugarHelper.ResourceDataDb.Insert(resource);
                                if (bResult)
                                {
                                    Console.WriteLine($"资源链接：{resource.URL} 存储成功！");
                                    nSum++;
                                }
                                else
                                    Console.WriteLine($"资源链接：{resource.URL} 存储失败！");
                            }
                            else
                                Console.WriteLine($"资源链接：获取失败。");
                        }
                        Console.WriteLine($"资源页面共存储{nSum}条数据。");

                        goto End;
                    }
                    else if (oPageDocument.Exists(m_sDetailImg))
                    {
                        //图片
                        var list = oPageDocument.Find(m_sDetailImg);
                        foreach (var item in list)
                        {
                            if (item.Attribute("src") != null && !string.IsNullOrWhiteSpace(item.Attribute("src").AttributeValue))
                            {
                                Resource resource = new Resource();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.PageGUID = oResult.GUID;
                                resource.URL = item.Attribute("src").AttributeValue;
                                resource.Original = Path.GetFileName(resource.URL);
                                resource.Type = 1;
                                resource.Path = resource.URL;
                                resource.FileName = resource.Original;

                                var bResult = oSQLSugarHelper.ResourceDb.Insert(resource);
                                if (bResult)
                                {
                                    Console.WriteLine($"资源链接：{resource.URL} 存储成功！");
                                    nSum++;
                                }
                                else
                                    Console.WriteLine($"资源链接：{resource.URL} 存储失败！");
                            }
                            else
                                Console.WriteLine($"资源链接：获取失败。");
                        }
                        Console.WriteLine($"资源页面共存储{nSum}条数据。");
                        goto End;
                    }
                    else if (oPageDocument.Exists(m_sXSDetail))
                    {
                        //小说
                        StringBuilder stringBuilder = new StringBuilder();
                        var list = oPageDocument.Find(m_sXSDetail);
                        foreach (var item in list)
                        {
                            if (!string.IsNullOrWhiteSpace(item.InnerText()))
                            {
                                stringBuilder.Append(item.InnerText());
                            }
                            else
                                Console.WriteLine($"资源链接：获取失败。");
                        }

                        Resource resource = new Resource();
                        resource.WebsiteGUID = oResult.WebsiteGUID;
                        resource.PageGUID = oResult.GUID;
                        resource.URL = sGetUrl;
                        resource.Original = oPageDocument.FindFirst("title").InnerText();
                        //小说
                        resource.Type = 3;
                        resource.Path = stringBuilder.ToString();
                        resource.FileName = resource.Original;

                        var bResult = oSQLSugarHelper.ResourceDb.Insert(resource);
                        if (bResult)
                        {
                            Console.WriteLine($"小说资源：{resource.URL} 存储成功！");
                            nSum++;
                        }
                        else
                            Console.WriteLine($"小说资源：{resource.URL} 存储失败！");

                        Console.WriteLine($"小说资源页面共存储完成。");
                        goto End;
                    }
                }
                else
                {
                    //解析页面失败
                }

            End:
                oResult.Processed = 2;
                oSQLSugarHelper.LinkDb.Update(oResult);
                Console.WriteLine($"资源 {oResult.Name} 采集完毕。");
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
