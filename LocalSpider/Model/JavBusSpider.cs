using Ivony.Html;
using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using Larpx.ResourceSpider.Helpers.Encode;
using Larpx.ResourceSpider.Helpers.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static Larpx.ResourceSpider.BaseLibrary.Data.EnumData;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class JavBusSpider : BaseSpider
    {
        private const int m_oRepeatCount = 8;
        private string sWebSiteID = "be0f73a5590307e6dfe06015adbaa8a8";
        public JavBusSpider(Guid oWebGUID, DatabaseType oDatabaseType = DatabaseType.SqlServer, string sWebID = null, bool debug = true, string LoggerPath = null, Logger oLogger = null, bool bDeepClone = false, List<Website> _ListWebsites = null, List<Category> _ListCategory = null, List<Link> _ListLink = null) :
            base(oWebGUID, oDatabaseType, sWebID, debug, LoggerPath, oLogger, bDeepClone, _ListWebsites, _ListCategory, _ListLink)
        {
            //https://avmoo.host/cn
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

                Website websiteOther = new Website();
                websiteOther.Name = "JavBus - AV磁力連結分享 - 日本成人影片資料庫";
                websiteOther.URL = "https://www.javbus.com";
                websiteOther.Status = 1;
                websiteOther.Deleted = false;
                websiteOther.IsCookies = true;
                websiteOther.ID = MD5.GetBufferHash(websiteOther.URL).ToLower();

                //查重
                if (!oWebsites.IsAny(it => it.ID == websiteOther.ID))
                {
                    oWebsites.Insert(websiteOther);
                    sWebSiteID = websiteOther.ID;
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
                string sCategoryURL = "/genre";
                string sArtistURL = "/actresses";
                string sMetaData = "meta";
                string sCategoryCollection = ".genre-box a";
                string sArtistCollection = ".avatar-box";
                string sArtistCollectionNext = "[name=nextpage]";

                string sURL = oWebsite.URL;
                string sHTML = "";
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper(DatabaseType, Debug);

                switch (oWebsite.Processed)
                {
                    case 0:
                        //获取Cookies
                        var oCookies = EasyHttpHelper.GetCookie(new Uri(oWebsite.URL), false);

                        sURL += sCategoryURL;

                        //请求页面
                        if (oWebsite.IsCookies)
                            oPageDocument = ReadDataEncodeHTML(sURL, ref sHTML, oCookies);
                        else
                            oPageDocument = ReadDataEncodeHTML(sURL, ref sHTML);

                        //解析页面
                        if (oPageDocument != null)
                        {
                            //分类
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
                                    oCategory.Name = item.InnerText().Trim();
                                    oCategory.URL = item.Attribute("href").AttributeValue.Trim();

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

                            //Meta信息
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

                            //演员列表
                            if (!string.IsNullOrEmpty(sArtistURL))
                            {
                                int sPageCount = 1, iDataSum = 0;
                                sURL = oWebsite.URL + sArtistURL;

                                Guid oPropKeyGUID = Guid.NewGuid();
                                if (!oSQLSugarHelper.PropertyKeyDb.IsAny(it => it.Name == "女优" && it.WebsiteGUID == oWebsite.GUID))
                                {
                                    PropertyKey propertyKey = new PropertyKey();
                                    propertyKey.GUID = oPropKeyGUID;
                                    propertyKey.WebsiteGUID = oWebsite.GUID;
                                    propertyKey.Type = 0;
                                    propertyKey.Name = "女优";
                                    oSQLSugarHelper.PropertyKeyDb.Insert(propertyKey);
                                }
                                else
                                    oPropKeyGUID = oSQLSugarHelper.PropertyKeyDb.GetSingle(it => it.Name == "女优" && it.WebsiteGUID == oWebsite.GUID).GUID;

                                NextPage:

                                Console.WriteLine("当前任务页码：" + sPageCount);
                                Console.WriteLine("总任务量：" + iDataSum / 2);
                                Console.WriteLine("当前任务地址：" + sURL);

                                //请求页面
                                using (var oResponse = EasyHttpHelper.ReadData(sURL, oCookies))
                                {
                                    if (oResponse == null)
                                        return null;

                                    //页面解码
                                    oPageDocument = EncodeHTML(ref sHTML, oResponse);
                                }

                                if (oPageDocument != null)
                                {
                                    if (oPageDocument.Exists(sArtistCollectionNext))
                                        sURL = oWebsite.URL.Replace("/cn", "") + oPageDocument.FindFirst(sArtistCollectionNext).Attribute("href").AttributeValue;
                                    else
                                        goto End;

                                    if (oPageDocument.Exists(sArtistCollection))
                                    {
                                        var oCategoryEnmuar = oPageDocument.Find(sArtistCollection);
                                        foreach (var item in oCategoryEnmuar)
                                        {
                                            if (item.Attribute("href").AttributeValue == "javascript:void(0);" || item.Attribute("href").AttributeValue == "."
                                                || string.IsNullOrEmpty(item.Attribute("href").AttributeValue))
                                                continue;

                                            Category oCategory = new Category();
                                            oCategory.WebsiteGUID = oWebsite.GUID;
                                            oCategory.Status = 1;
                                            oCategory.Priority = 1;
                                            oCategory.Name = item.InnerText().Trim();
                                            oCategory.URL = item.Attribute("href").AttributeValue.Trim();

                                            if (!oSQLSugarHelper.CategoryDb.IsAny(it => it.URL == oCategory.URL && it.WebsiteGUID == oWebsite.GUID))
                                            {
                                                oSQLSugarHelper.CategoryDb.Insert(oCategory);
                                                iDataSum++;
                                            }

                                            if (!oSQLSugarHelper.PropertyValueDb.IsAny(it => it.Name == item.InnerText().Trim() && it.WebsiteGUID == oWebsite.GUID))
                                            {
                                                PropertyValue oPropertyValue = new PropertyValue();
                                                oPropertyValue.KeyGUID = oPropKeyGUID;
                                                oPropertyValue.WebsiteGUID = oWebsite.GUID;
                                                oPropertyValue.Type = 0;
                                                oPropertyValue.Name = item.InnerText().Trim();

                                                if (!oSQLSugarHelper.PropertyValueDb.IsAny(it => it.Name == oPropertyValue.Name && it.WebsiteGUID == oWebsite.GUID))
                                                {
                                                    oSQLSugarHelper.PropertyValueDb.Insert(oPropertyValue);
                                                    iDataSum++;
                                                }
                                            }
                                        }
                                        sPageCount++;
                                        goto NextPage;
                                    }
                                    else
                                    {
                                        //未找到分类标签
                                    }
                                }
                            }
                        }
                        else
                        {
                            //解析页面失败
                        }

                    End:
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
            int iSumCount = 0;
            int sThisPageNum = 1;
            string sGetUrl = "";

            const string sGetItems = ".movie-box";
            const string sGetNextURL = "[name=nextpage]";

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
                    //解析页面
                    if (oPageDocument.Exists(sGetItems))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(sGetItems);
                        foreach (var item in oCategoryEnmuar)
                        {
                            //判断重复
                            if (nReCount >= m_oRepeatCount)
                            {
                                Console.WriteLine("本页重复数据量为：" + nReCount + "，触发阈值，需跳过。");
                                Console.WriteLine("该分类完成总量：" + iSumCount);
                                Console.WriteLine("当前任务页码：" + sThisPageNum);
                                Console.WriteLine("当前任务地址：" + sGetUrl);
                                goto GetUrl;
                            }

                            //查找番号
                            if (!item.Exists("item"))
                                continue;

                            //获取番号并查重
                            string sSerialNumber = item.FindFirst("date").InnerText().Trim();
                            if (oSQLSugarHelper.LinkDb.IsAny(it => it.SN == sSerialNumber))
                            {
                                nReCount++;
                                continue;
                            }

                            Link oLink = new Link();
                            oLink.GUID = Guid.NewGuid();
                            oLink.CategoryGUID = oCategory.GUID;
                            oLink.WebsiteGUID = oCategory.WebsiteGUID;
                            oLink.URL = item.Attribute("href").AttributeValue;
                            oLink.SN =Helpers.CommonHelper. GenerateNonceStr();
                            oLink.ID = MD5.GetBufferHash(oLink.URL);
                            oLink.Date = Convert.ToDateTime(item.FindLast("date").InnerText().Trim() + " 00:00:01");
                            oLink.Name = item.InnerText();
                            oLink.NameChs = item.InnerText();
                            //0 视频，1图片，2文字 ，3其他
                            oLink.Type = 0;

                            oSQLSugarHelper.LinkDb.Insert(oLink);
                            iSumCount++;

                            //标题图
                            Resource oResource = new Resource();
                            oResource.WebsiteGUID = oLink.WebsiteGUID;
                            oResource.PageGUID = oLink.GUID;
                            oResource.URL = item.FindFirst("img").Attribute("src").AttributeValue;
                            oResource.Original = Path.GetFileName(oResource.URL);
                            oResource.Path = oResource.Original;
                            oResource.FileName = oResource.Path;
                            oResource.Type = (byte)ResourceType.Title;
                            oSQLSugarHelper.ResourceDb.Insert(oResource);

                        }
                    }
                    else
                    {

                    }

                    Console.WriteLine("该分类完成总量：" + iSumCount);
                    Console.WriteLine("当前任务页码：" + sThisPageNum);
                    Console.WriteLine("当前任务地址：" + sGetUrl);

                    //获取下页地址
                    if (oPageDocument.Exists(sGetNextURL))
                    {
                        sGetUrl = oPageDocument.FindFirst(sGetNextURL).Attribute("href").AttributeValue;
                        goto GetUrl;
                    }
                    else
                        goto End;
                }
                else
                {
                    //解析页面失败
                }

            End:
                oCategory.Processed = 2;
                oSQLSugarHelper.CategoryDb.Update(oCategory);

                return oSQLSugarHelper.LinkDb.GetList(it => it.CategoryGUID == oCategory.GUID && it.Deleted == false);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Console.WriteLine("当前任务页码：" + sThisPageNum);
                Console.WriteLine("总任务页码：" + iSumCount);
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
                //标题 innerText
                string sTitle = "h3";
                //海报 href
                string sBannerImages = ".bigImage";
                //简介
                string sDetail = ".info p";
                //演员 
                string sAvatar = ".avatar-box";
                //详情图
                string sResourceLinks = ".sample-box";

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

                    //标题
                    oResult.Title = oPageDocument.FindFirst(sTitle).InnerText().Trim();

                    //Banner图
                    Resource oResource = new Resource();
                    oResource.WebsiteGUID = oResult.WebsiteGUID;
                    oResource.PageGUID = oResult.GUID;
                    oResource.URL = oPageDocument.FindFirst(sBannerImages).Attribute("href").AttributeValue;
                    oResource.Original = Path.GetFileName(oResource.URL);
                    oResource.Path = oResource.Original;
                    oResource.FileName = oResource.Path;
                    oResource.Type = (byte)ResourceType.Banner;
                    oSQLSugarHelper.ResourceDb.Insert(oResource);

                    //演员
                    if (oPageDocument.Exists(sAvatar))
                    {
                        int nAvatarCount = 0;
                        var oAvatarList = oPageDocument.Find(sAvatar);
                        foreach (var item in oAvatarList)
                        {
                            string sName = item.InnerText().Trim();
                            Tag oTag = new Tag();
                            oTag.LinkGUID = oResult.GUID;
                            oTag.Name = "演员";
                            oTag.Value = sName;
                            oTag.URL = item.Attribute("href").AttributeValue;

                            if (oSQLSugarHelper.PropertyValueDb.IsAny(it => it.Name == sName && it.WebsiteGUID == oResult.WebsiteGUID))
                                oTag.ObjectGUID = oSQLSugarHelper.PropertyValueDb.GetList(it => it.Name == sName && it.WebsiteGUID == oResult.WebsiteGUID)[0].GUID;
                            else
                            {
                                PropertyValue propertyValue = new PropertyValue();
                                propertyValue.GUID = Guid.NewGuid();
                                propertyValue.KeyGUID = oSQLSugarHelper.PropertyKeyDb.GetList(it => it.Name == "演员" && it.WebsiteGUID == oResult.WebsiteGUID)[0].GUID;
                                propertyValue.Name = sName;
                                propertyValue.WebsiteGUID = oResult.WebsiteGUID;
                                oSQLSugarHelper.PropertyValueDb.Insert(propertyValue);

                                oTag.ObjectGUID = propertyValue.GUID;
                            }
                            oSQLSugarHelper.TagDb.Insert(oTag);
                            nAvatarCount++;
                        }
                        Console.WriteLine("插入演员信息：" + nAvatarCount + "个");
                    }

                    //详情图
                    if (oPageDocument.Exists(sResourceLinks))
                    {
                        int nDetailCount = 0;
                        int nDetailReCount = 0;
                        var oDetailList = oPageDocument.Find(sResourceLinks);
                        foreach (var item in oDetailList)
                        {
                            oResource = new Resource();
                            oResource.WebsiteGUID = oResult.WebsiteGUID;
                            oResource.PageGUID = oResult.GUID;
                            oResource.URL = item.Attribute("href").AttributeValue;
                            oResource.Original = Path.GetFileName(oResource.URL);
                            oResource.Path = oResource.Original;
                            oResource.FileName = oResource.Path;
                            oResource.Type = (byte)ResourceType.Banner;
                            if (!oSQLSugarHelper.ResourceDb.IsAny(it => it.FileName == oResource.FileName && it.WebsiteGUID == oResult.WebsiteGUID))
                            {
                                oSQLSugarHelper.ResourceDb.Insert(oResource);
                                nDetailCount++;
                            }
                            else
                                nDetailReCount++;
                        }
                        Console.WriteLine("插入详情图：" + nDetailCount + "个，已有" + nDetailReCount + "个");
                    }

                    //详情
                    if (oPageDocument.Exists(sDetail))
                    {
                        int nDetailCount = 0;
                        var oDetailList = oPageDocument.Find(sDetail);
                        Tag oTag = new Tag();

                        foreach (var item in oDetailList)
                        {
                            if (item.Attribute("class") == null)
                            {
                                if (item.Find("a").Count() == 1 && item.Find("span").Count() == 1)
                                {
                                    oTag = new Tag();
                                    oTag.LinkGUID = oResult.GUID;
                                    oTag.Name = item.FindFirst("span").InnerText().Trim();
                                    oTag.Value = item.FindFirst("a").InnerText().Trim();
                                    oTag.URL = item.FindFirst("a").Attribute("href").AttributeValue;
                                    oSQLSugarHelper.TagDb.Insert(oTag);
                                    nDetailCount++;
                                }
                                else if (item.Find("a").Count() == 0 && item.Find("span").Count() <= 2)
                                {
                                    oTag = new Tag();
                                    oTag.LinkGUID = oResult.GUID;
                                    oTag.Name = item.FindFirst("span").InnerText().Trim();
                                    if (item.Find("span").Count() == 2)
                                        oTag.Value = item.FindLast("span").InnerText().Trim();
                                    else
                                        oTag.Value = item.InnerText().Trim().Replace(oTag.Name, "").Trim();
                                    oSQLSugarHelper.TagDb.Insert(oTag);
                                    nDetailCount++;
                                }
                                else if (item.Find("a").Count() == 1 && item.Find("span").Count() == 0)
                                {
                                    oTag.URL = item.FindFirst("a").Attribute("href").AttributeValue;
                                    oTag.Value = item.FindFirst("a").InnerText().Trim();
                                    oSQLSugarHelper.TagDb.Insert(oTag);
                                    nDetailCount++;
                                }
                                else if (item.Find("span").Count() > 2)
                                {
                                    foreach (var itemValue in item.Find("span"))
                                    {
                                        var oTags = new Tag();
                                        oTags.LinkGUID = oResult.GUID;
                                        oTags.Name = oTag.Name;
                                        oTags.Value = item.FindFirst("a").InnerText().Trim();
                                        oTags.URL = item.FindFirst("a").Attribute("href").AttributeValue;
                                        oSQLSugarHelper.TagDb.Insert(oTags);
                                        nDetailCount++;
                                    }
                                }
                            }
                            else if (item.Attribute("class") != null)
                            {
                                oTag = new Tag();
                                oTag.LinkGUID = oResult.GUID;
                                oTag.Name = item.InnerText().Trim();
                            }
                        }
                        Console.WriteLine("插入详细信息：" + nDetailCount + "条");
                    }
                }
                else
                {
                    //解析页面失败
                }

                oResult.Processed = (byte)ProcessedType.Success;
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
