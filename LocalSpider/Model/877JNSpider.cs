using Ivony.Html;
using Ivony.Html.Parser;
using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public class _877JNSpider : BaseSpider
    {
        private string sWebSiteID = "d9e5780840f6766c7fcbac7cab9538f2";
        public _877JNSpider(bool debug = true, string LoggerPath = null, Logger Logger = null) : base(debug, LoggerPath, Logger)
        {
            //http://www.877jn.com
        }

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

        public override int PerExce(Dictionary<string, object> arr)
        {
            try
            {
                Website website = new Website();
                SQLSugarHelper<Website> oWebsites = new SQLSugarHelper<Website>();

                website.Name = "877jn";
                website.URL = "http://www.877jn.com";
                website.Status = 1;
                website.Deleted = false;
                website.ID = CommonHelper.MD5.GetBufferHash(website.URL).ToLower();

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
                string sHTML = "";
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                List<Category> oListResult = new List<Category>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper();

                //整理分类不规范页面地址
                if (!oWebsite.URL.StartsWith("http"))
                {
                    //处理不规范URL
                    oWebsite.URL = "http://" + oWebsite.URL;
                    oWebsite.ID = CommonHelper.MD5.GetBufferHash(oWebsite.URL).ToLower();
                    sWebSiteID = oWebsite.ID;
                    oSQLSugarHelper.WebsiteDb.Update(oWebsite);
                }

                //获取Cookies
                var oCookies = CommonHelper.EasyHttpHelper.GetCookie(new Uri(oWebsite.URL), false);

                //请求页面
                using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(oWebsite.URL, oCookies))
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
                    if (oPageDocument.Exists("#header_box ul li a"))
                    {
                        var oCategoryEnmuar = oPageDocument.Find("#header_box ul li a");
                        foreach (var item in oCategoryEnmuar)
                        {
                            if (item.Attribute("href").AttributeValue == "javascript:void(0);")
                                continue;

                            Category oCategory = new Category();
                            oCategory.WebsiteGUID = oWebsite.GUID;
                            oCategory.Name = item.InnerText();
                            oCategory.URL = oWebsite.URL + item.Attribute("href").AttributeValue;

                            if (!oSQLSugarHelper.CategoryDb.IsAny(it => it.URL == oCategory.URL))
                            {
                                oSQLSugarHelper.CategoryDb.Insert(oCategory);
                                oListResult.Add(oCategory);
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        //未找到分类标签
                    }
                }
                else
                {
                    //解析页面失败
                }

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
                string sHTML = "";
                bool bGetCookie = false;
                Random oRand = new Random();
                IHtmlDocument oPageDocument = null;
                List<Link> oListLink = new List<Link>();
                SQLSugarHelper oSQLSugarHelper = new SQLSugarHelper();

                //整理分类不规范页面地址
                if (!oCategory.URL.StartsWith("http"))
                {
                    //处理不规范URL
                    var oWebs = oSQLSugarHelper.WebsiteDb.GetById(oCategory.WebsiteGUID);

                    oCategory.URL = oWebs.URL + oCategory.URL;
                    oSQLSugarHelper.CategoryDb.Update(oCategory);
                }

                //获取Cookies
                var oCookies = new CookieCollection();
                if (bGetCookie)
                    oCookies = CommonHelper.EasyHttpHelper.GetCookie(new Uri(oCategory.URL), false);

                //请求页面
                using (var oResponse = CommonHelper.EasyHttpHelper.ReadData(oCategory.URL, oCookies))
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




                    do
                    {

                    } while (false);










                    if (oPageDocument.Exists(".box.list.channel ul li a"))
                    {
                        var oCategoryEnmuar = oPageDocument.Find(".box.list.channel ul li a");
                        foreach (var item in oCategoryEnmuar)
                        {
                            if (item.Attribute("href").AttributeValue == "javascript:void(0);")
                                continue;

                            Category oCategory = new Category();
                            oCategory.WebsiteGUID = oWebsite.GUID;
                            oCategory.Name = item.InnerText();
                            oCategory.URL = oWebsite.URL + item.Attribute("href").AttributeValue;

                            if (!oSQLSugarHelper.CategoryDb.IsAny(it => it.URL == oCategory.URL))
                            {
                                oSQLSugarHelper.CategoryDb.Insert(oCategory);
                                oListResult.Add(oCategory);
                            }
                            else
                                continue;
                        }
                    }
                    else
                    {
                        //未找到分类标签
                    }
                }
                else
                {
                    //解析页面失败
                }






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
