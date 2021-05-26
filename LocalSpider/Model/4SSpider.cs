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
using static Larpx.ResourceSpider.BaseLibrary.Data.EnumData;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class F4SSpider : BaseSpider
    {
        private const int m_oRepeatCount = 8;
        private const string m_sEndPages = ".pagination .hidden-xs";
        private const string m_sThisPage = ".pagination strong";
        private const string m_sListPage = ".box.list.channel ul li a";
        private const string m_sMoviePage = ".box.movie_list ul li a";
        private string sWebSiteID = "fabed58122c576d92c4eb81b9c32a7e6";

        public F4SSpider(Guid oWebGUID, DatabaseType oDatabaseType = DatabaseType.SqlServer, string sWebID = null, bool debug = true, bool bFirst = true, string LoggerPath = null, Logger Logger = null) :
            base(oWebGUID, oDatabaseType, sWebID, debug,bFirst, LoggerPath, Logger)
        {

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
                website.Name = "四色AV-sise,四色AV官网,四色AV最新网址ＷＷＷ．Ｄ８ＣＥＤ１Ｃ７６４８９Ｃ２ＥＢ．ＣＯＭＷＷＷ．Ｄ８ＣＥＤ１Ｃ７６４８９Ｃ２ＥＢ．ＣＯＭ";
                website.URL = "https://www.d8ced1c76489c2eb.com";
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
                string sMetaData = "meta";
                string sCategoryCollection = "#section-menu ul li a";

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
            bool bFirst = true;
            int iEndPageNum = 0;
            int iThisPageNum = 1;
            string sGetUrl = "";

            try
            {
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
                    //获取总页码
                    if (bFirst)
                    {
                        bFirst = false;
                        if (oPageDocument.Exists(m_sEndPages))
                            iEndPageNum = Convert.ToInt32(oPageDocument.FindFirst(m_sEndPages).InnerText());
                        else
                            iEndPageNum = 1;
                    }

                    //获取本页页码
                    if (oPageDocument.Exists(m_sThisPage))
                        iThisPageNum = Convert.ToInt32(oPageDocument.FindFirst(m_sThisPage).InnerText());
                    else
                        iThisPageNum++;

                    //解析页面
                    if (oPageDocument.Exists(m_sListPage))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(m_sListPage);
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
                                continue;
                            }
                        }
                    }
                    else if (oPageDocument.Exists(m_sMoviePage))
                    {
                        //未找到分类标签 box movie_list
                        var oCategoryEnmuar = oPageDocument.Find(m_sMoviePage);
                        foreach (var item in oCategoryEnmuar)
                        {
                            if (item.Exists("h3"))
                            {
                                if (string.IsNullOrEmpty(item.FindFirst("h3").InnerText()))
                                    continue;
                            }
                            else
                                continue;

                            Link oLink = new Link();
                            oLink.GUID = Guid.NewGuid();
                            oLink.CategoryGUID = oCategory.GUID;
                            oLink.WebsiteGUID = oCategory.WebsiteGUID;
                            oLink.URL = oWebs.URL + item.Attribute("href").AttributeValue;
                            oLink.SN = Helpers.CommonHelper.GenerateNonceStr();
                            oLink.ID = MD5.GetBufferHash(oLink.URL);
                            oLink.Name = item.InnerText();
                            if (oLink.Name.StartsWith("(v)"))
                                oLink.Name = oLink.Name.Remove(0, 3).Trim();
                            oLink.NameChs = oLink.Name;
                            //0 视频，1图片，2文字 ，3其他
                            oLink.Type = 0;

                            if (!oSQLSugarHelper.LinkDb.IsAny(it => it.ID == oLink.ID))
                                oSQLSugarHelper.LinkDb.Insert(oLink);
                            else
                            {
                                nReCount++;
                                continue;
                            }
                        }
                    }
                    else
                    {

                    }

                    //组合下一页地址
                    sGetUrl = oCategory.URL.Substring(0, oCategory.URL.Length - 5) + "-" + (iThisPageNum + 1) +
                        oCategory.URL.Substring(oCategory.URL.Length - 5, 5);

                    Console.WriteLine("当前任务页码：" + iThisPageNum);
                    Console.WriteLine("总任务页码：" + iEndPageNum);
                    Console.WriteLine("当前任务剩余页面数：" + (iEndPageNum - iThisPageNum));
                    if (iThisPageNum < iEndPageNum)
                        goto GetUrl;
                }
                else
                {
                    //解析页面失败
                }

                oCategory.Processed = 2;
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


                string sImgContext = "#main-container .content img";
                string sVedioContext = "#main-container .downlist input";
                string sXSContext = "#main-container .content p";
                string sLYContext = "#main-container video source";

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
                    if (oPageDocument.Exists(sImgContext))
                    {
                        //图片
                        var list = oPageDocument.Find(sImgContext);
                        foreach (var item in list)
                        {
                            if (item.Attribute("data-original") != null && !string.IsNullOrWhiteSpace(item.Attribute("data-original").AttributeValue))
                            {
                                Resource resource = new Resource();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.PageGUID = oResult.GUID;
                                resource.URL = item.Attribute("data-original").AttributeValue;
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
                    }
                    else if (oPageDocument.Exists(sVedioContext))
                    {
                        //视频
                        var list = oPageDocument.Find(sVedioContext);
                        foreach (var item in list)
                        {
                            if (item.Attribute("data-clipboard-text") != null && !string.IsNullOrWhiteSpace(item.Attribute("data-clipboard-text").AttributeValue))
                            {
                                ResourceData resource = new ResourceData();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.ObjectGUID = oResult.GUID;
                                resource.URL = item.Attribute("data-clipboard-text").AttributeValue;
                                if (resource.URL.StartsWith("http"))
                                {
                                    resource.Original = Path.GetFileName(resource.URL);
                                    resource.URLType = 0;
                                    resource.ResourceType = 1;

                                }
                                else if (resource.URL.StartsWith("thunder"))
                                {
                                    resource.Original = item.Attribute("id").AttributeValue;
                                    resource.URLType = 0;
                                    //迅雷链接
                                    resource.ResourceType = 3;
                                }
                                else if (resource.URL.StartsWith("magnet"))
                                {
                                    resource.Original = item.Attribute("id").AttributeValue;
                                    resource.URLType = 0;
                                    //magnet:?
                                    resource.ResourceType = 2;
                                }
                                else
                                {
                                    resource.Original = item.Attribute("id").AttributeValue;
                                    resource.URLType = 0;
                                    resource.ResourceType = 4;
                                }
                                resource.Status = 1;
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
                    }
                    else if (oPageDocument.Exists(sXSContext))
                    {
                        //小说
                        StringBuilder stringBuilder = new StringBuilder();
                        var list = oPageDocument.Find(sXSContext);
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
                    }
                    else if (oPageDocument.Exists(sLYContext))
                    {
                        //语音
                        var list = oPageDocument.Find(sLYContext);
                        foreach (var item in list)
                        {
                            if (item.Attribute("src") != null && !string.IsNullOrWhiteSpace(item.Attribute("src").AttributeValue))
                            {
                                Resource resource = new Resource();
                                resource.WebsiteGUID = oResult.WebsiteGUID;
                                resource.PageGUID = oResult.GUID;
                                resource.URL = item.Attribute("src").AttributeValue;
                                resource.Original = Path.GetFileName(resource.URL);
                                resource.Type = 4;
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
                    }
                }
                else
                {
                    //解析页面失败
                }

                oResult.Processed = 2;
                oSQLSugarHelper.LinkDb.Update(oResult);
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
