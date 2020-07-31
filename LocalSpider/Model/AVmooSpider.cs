using Brotli;
using Ivony.Html;
using Ivony.Html.Parser;
using Larpx.Logs;
using Larpx.ResourceSpider.CommonHelper;
using Larpx.ResourceSpider.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web.UI.WebControls;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class AVmooSpider : BaseSpider
    {
        private const int m_oRepeatCount = 8;
        private string sWebSiteID = "511f88db164b46662eb442c342d5649a";

        public AVmooSpider(Guid oWebGUID, CommonHelper.CommonHelper.DatabaseType oDatabaseType = CommonHelper.CommonHelper.DatabaseType.SqlServer, string sWebID = null, bool debug = true, string LoggerPath = null, Logger oLogger = null, bool bDeepClone = false, List<Website> _ListWebsites = null, List<Category> _ListCategory = null, List<Link> _ListLink = null) :
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
                websiteOther.Name = "AVMOO - 你的线上日本成人影片情报站。管理你的影片并分享你的想法。";
                websiteOther.URL = "https://avmoo.host/cn";
                websiteOther.Status = 1;
                websiteOther.Deleted = false;
                websiteOther.IsCookies = true;
                websiteOther.ID = CommonHelper.MD5.GetBufferHash(websiteOther.URL).ToLower();

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
                        var oCookies = CommonHelper.EasyHttpHelper.GetCookie(new Uri(sURL), false);

                        sURL += sCategoryURL;

                        CookieContainer cookies = new CookieContainer();
                        SmsWebClient client = new SmsWebClient(cookies);
                        string html = client.DownloadString(sURL);

                        //请求页面
                        using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(sURL, oCookies))
                        {
                            if (oResponse == null)
                                return null;

                            //页面解码
                            if (!string.IsNullOrEmpty(oResponse.ContentEncoding))
                            {
                                switch (oResponse.ContentEncoding.ToLower())
                                {
                                    case "gzip":
                                        using (GZipStream stream = new GZipStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                        {
                                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                            {
                                                sHTML = reader.ReadToEnd();
                                                oPageDocument = new JumonyParser().Parse(sHTML);
                                            }
                                        }
                                        break;
                                    case "deflate":
                                        using (DeflateStream stream = new DeflateStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                        {
                                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                            {
                                                sHTML = reader.ReadToEnd();
                                                oPageDocument = new JumonyParser().Parse(sHTML);
                                            }
                                        }
                                        break;
                                    case "br":
                                        using (BrotliStream stream = new BrotliStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                        {
                                            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                            {
                                                sHTML = reader.ReadToEnd();
                                                oPageDocument = new JumonyParser().Parse(sHTML);
                                            }
                                        }
                                        break;
                                    default:
                                        //未被压缩,直接解析
                                        oPageDocument = new JumonyParser().LoadDocument(oResponse);
                                        break;
                                }
                            }
                            else
                            {
                                //未被压缩,直接解析
                                oPageDocument = new JumonyParser().LoadDocument(oResponse);
                            }
                        }

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
                                        metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Charset;
                                    }
                                    else if (item.Attribute("name") != null)
                                    {
                                        switch (item.Attribute("name").AttributeValue.ToLower())
                                        {
                                            case "viewport":
                                                metaData.Name = "viewport";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Viewport;
                                                break;
                                            case "keywords":
                                                metaData.Name = "keywords";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Keywords;
                                                break;
                                            case "description":
                                                metaData.Name = "description";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Description;
                                                break;
                                            case "renderer":
                                                metaData.Name = "renderer";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Renderer;
                                                break;
                                            default:
                                                metaData.Name = "other";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Other;
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
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.X_UA_Compatible;
                                                break;
                                            case "cache-control":
                                                metaData.Name = "Cache-Control";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Cache_Control;
                                                break;
                                            default:
                                                metaData.Name = "other";
                                                if (item.Attribute("content") != null)
                                                    metaData.Content = item.Attribute("content").AttributeValue;
                                                metaData.Type = (byte)CommonHelper.CommonHelper.MetaType.Other;
                                                break;
                                        }
                                    }

                                    oSQLSugarHelper.MetaDataDb.Insert(metaData);
                                }
                            }

                            //分类
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
                                using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(sURL, oCookies))
                                {
                                    if (oResponse == null)
                                        return null;

                                    //页面解码
                                    if (!string.IsNullOrEmpty(oResponse.ContentEncoding))
                                    {
                                        switch (oResponse.ContentEncoding.ToLower())
                                        {
                                            case "gzip":
                                                using (GZipStream stream = new GZipStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                                {
                                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                                    {
                                                        sHTML = reader.ReadToEnd();
                                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                                    }
                                                }
                                                break;
                                            case "deflate":
                                                using (DeflateStream stream = new DeflateStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                                {
                                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                                    {
                                                        sHTML = reader.ReadToEnd();
                                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                                    }
                                                }
                                                break;
                                            case "br":
                                                using (BrotliStream stream = new BrotliStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                                {
                                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                                    {
                                                        sHTML = reader.ReadToEnd();
                                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                                    }
                                                }
                                                break;
                                            default:
                                                //未被压缩,直接解析
                                                oPageDocument = new JumonyParser().LoadDocument(oResponse);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //未被压缩,直接解析
                                        oPageDocument = new JumonyParser().LoadDocument(oResponse);
                                    }
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
                    oCookies = CommonHelper.EasyHttpHelper.GetCookie(new Uri(sGetUrl), false);

                GetUrl:
                nReCount = 0;

                //请求页面
                using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(sGetUrl, oCookies))
                {
                    if (oResponse == null)
                        return null;

                    //页面解码
                    if (!string.IsNullOrEmpty(oResponse.ContentEncoding))
                    {
                        switch (oResponse.ContentEncoding.ToLower())
                        {
                            case "gzip":
                                using (GZipStream stream = new GZipStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        sHTML = reader.ReadToEnd();
                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                    }
                                }
                                break;
                            case "deflate":
                                using (DeflateStream stream = new DeflateStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        sHTML = reader.ReadToEnd();
                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                    }
                                }
                                break;
                            default:
                                //未被压缩,直接解析
                                oPageDocument = new JumonyParser().LoadDocument(oResponse);
                                break;
                        }
                    }
                    else
                    {
                        //未被压缩,直接解析
                        oPageDocument = new JumonyParser().LoadDocument(oResponse);
                    }
                }

                //解析页面
                if (oPageDocument != null)
                {
                    //获取总页码
                    if (bFirst)
                    {
                        bFirst = false;
                        if (oPageDocument.Exists(".page .end"))
                            iEndPageNum = Convert.ToInt32(oPageDocument.FindFirst(".page .end").InnerText());
                        else
                            iEndPageNum = 1;
                    }

                    //获取本页页码
                    if (oPageDocument.Exists(".page .current"))
                        iThisPageNum = Convert.ToInt32(oPageDocument.FindFirst(".page .current").InnerText());
                    else
                        iThisPageNum++;

                    //解析页面
                    if (oPageDocument.Exists(".box.list.channel ul li a"))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(".box.list.channel ul li a");
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
                            oLink.SN = CommonHelper.CommonHelper.GenerateNonceStr();
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
                    else if (oPageDocument.Exists(".box.movie_list ul li a"))
                    {
                        //未找到分类标签 box movie_list
                        var oCategoryEnmuar = oPageDocument.Find(".box.movie_list ul li a");
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
                            oLink.SN = CommonHelper.CommonHelper.GenerateNonceStr();
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
                    oCookies = CommonHelper.EasyHttpHelper.GetCookie(new Uri(sGetUrl), false);

                //请求页面
                using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(sGetUrl, oCookies))
                {
                    if (oResponse == null)
                        return;

                    //页面解码
                    if (!string.IsNullOrEmpty(oResponse.ContentEncoding))
                    {
                        switch (oResponse.ContentEncoding.ToLower())
                        {
                            case "gzip":
                                using (GZipStream stream = new GZipStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        sHTML = reader.ReadToEnd();
                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                    }
                                }
                                break;
                            case "deflate":
                                using (DeflateStream stream = new DeflateStream(oResponse.GetResponseStream(), CompressionMode.Decompress))
                                {
                                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                    {
                                        sHTML = reader.ReadToEnd();
                                        oPageDocument = new JumonyParser().Parse(sHTML);
                                    }
                                }
                                break;
                            default:
                                //未被压缩,直接解析
                                oPageDocument = new JumonyParser().LoadDocument(oResponse);
                                break;
                        }
                    }
                    else
                    {
                        //未被压缩,直接解析
                        oPageDocument = new JumonyParser().LoadDocument(oResponse);
                    }
                }

                //解析页面
                if (oPageDocument != null)
                {




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
