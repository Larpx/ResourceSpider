using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public abstract class BaseSpider : SpiderInterface
    {
        private bool _DeepClone = false;
        private bool bDebug = true;
        private string sLoggerPath = "../Logs";
        private Logger m_oLogger = new ConsoleLogger();

        private List<Website> _ListWebsites;
        private List<Category> _ListCategory;
        private List<Link> _ListLink;

        public delegate void BeginGetWebsiteListEventHandler();
        public delegate void BeginGetCategoryListEventHandler();
        public delegate void BeginGetLinkListEventHandler();
        public delegate void BeginGetLinkDetailEventHandler();
        public delegate void GetingWebsiteListEventHandler(string sID);
        public delegate void GetingCategoryListEventHandler(Website website);
        public delegate void GetingLinkListEventHandler(Category category);
        public delegate void GetingLinkDetailEventHandler(Link link);
        public delegate void EndGetWebsiteListEventHandler();
        public delegate void EndGetCategoryListEventHandler();
        public delegate void EndGetLinkListEventHandler();
        public delegate void EndGetLinkDetailEventHandler();

        public event BeginGetWebsiteListEventHandler OnBeginGetWebsiteList;
        public event BeginGetCategoryListEventHandler OnBeginGetCategoryList;
        public event BeginGetLinkListEventHandler OnBeginGetLinkList;
        public event BeginGetLinkDetailEventHandler OnBeginGetLinkDetail;
        public event GetingWebsiteListEventHandler OnGetingWebsiteList;
        public event GetingCategoryListEventHandler OnGetingCategoryList;
        public event GetingLinkListEventHandler OnGetingLinkList;
        public event GetingLinkDetailEventHandler OnGetingLinkDetail;
        public event EndGetWebsiteListEventHandler OnEndGetWebsiteList;
        public event EndGetCategoryListEventHandler OnEndGetCategoryList;
        public event EndGetLinkListEventHandler OnEndGetLinkList;
        public event EndGetLinkDetailEventHandler OnEndGetLinkDetail;

        #region 属性

        public bool Debug { get => bDebug; set => bDebug = value; }
        public Logger Logger { get => m_oLogger; set => m_oLogger = value; }
        public List<Website> ListWebsites
        {
            get => _ListWebsites;
            set
            {
                if (_DeepClone)
                {
                    foreach (var item in value)
                    {
                        //深拷贝
                        this._ListWebsites.Add(CommonHelper.CommonHelper.Clone<Website>(item));
                    }
                }
                else
                    _ListWebsites = value;
            }
        }
        public List<Category> ListCategory
        {
            get => _ListCategory;
            set
            {
                if (_DeepClone)
                {
                    foreach (var item in value)
                    {
                        //深拷贝
                        this._ListCategory.Add(CommonHelper.CommonHelper.Clone<Category>(item));
                    }
                }
                else
                    _ListCategory = value;
            }
        }
        public List<Link> ListLink
        {
            get => _ListLink;
            set
            {
                if (_DeepClone)
                {
                    foreach (var item in value)
                    {
                        //深拷贝
                        this._ListLink.Add(CommonHelper.CommonHelper.Clone<Link>(item));
                    }
                }
                else
                    _ListLink = value;
            }
        }

        #endregion

        public BaseSpider(bool debug = true, string LoggerPath = null, Logger oLogger = null, bool bDeepClone = false,
            List<Website> _ListWebsites = null, List<Category> _ListCategory = null, List<Link> _ListLink = null)
        {
            Debug = debug;
            _DeepClone = bDeepClone;

            if (!string.IsNullOrEmpty(LoggerPath))
                this.sLoggerPath = LoggerPath;

            if (oLogger != null)
                this.Logger = oLogger;
            else
                this.Logger = new ConsoleLogger() + new TextFileLogger(new DirectoryInfo(sLoggerPath));

            if (_ListWebsites != null)
            {
                if (_DeepClone)
                {
                    foreach (var item in _ListWebsites)
                    {
                        //深拷贝
                        this._ListWebsites.Add(CommonHelper.CommonHelper.Clone<Website>(item));
                    }
                }
                else
                    this._ListWebsites.AddRange(_ListWebsites);
            }
            else
                this._ListWebsites = new List<Website>();

            if (_ListCategory != null)
            {
                if (_DeepClone)
                {
                    foreach (var item in _ListCategory)
                    {
                        //深拷贝
                        this._ListCategory.Add(CommonHelper.CommonHelper.Clone<Category>(item));
                    }
                }
                else
                    this._ListCategory.AddRange(_ListCategory);
            }
            else
                this._ListCategory = new List<Category>();

            if (_ListLink != null)
            {
                if (_DeepClone)
                {
                    foreach (var item in _ListLink)
                    {
                        //深拷贝
                        this._ListLink.Add(CommonHelper.CommonHelper.Clone<Link>(item));
                    }
                }
                else
                    this._ListLink.AddRange(_ListLink);
            }
            else
                this._ListLink = new List<Link>();
        }

        /// <summary>
        /// 预操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int PerExce(Dictionary<string, object> arr)
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
        /// 执行操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int DoExce(Dictionary<string, object> arr)
        {
            try
            {
                PerExce(arr);

                string sWebsiteID = "";

                //解析参数
                foreach (var item in arr)
                {
                    switch (item.Key)
                    {
                        case "ID":
                            sWebsiteID = item.Value.ToString();
                            break;
                    }
                }

                //获取任务网站列表
                OnBeginGetWebsiteList?.Invoke();
                ListWebsites.AddRange(GetWebsiteList(sWebsiteID));
                OnEndGetWebsiteList?.Invoke();

                //获取分类列表
                OnBeginGetCategoryList?.Invoke();
                foreach (var item in _ListWebsites)
                {
                    OnGetingCategoryList?.Invoke(item);
                    _ListCategory.AddRange(GetCategoryList(item));
                }
                OnEndGetCategoryList?.Invoke();

                //采集链接
                OnBeginGetLinkList?.Invoke();
                foreach (var item in _ListCategory)
                {
                    OnGetingLinkList(item);
                    ListLink.AddRange(GetLinkList(item));
                }
                OnEndGetLinkList?.Invoke();

                //采集详情
                OnBeginGetLinkDetail?.Invoke();
                foreach (var item in ListLink)
                {
                    OnGetingLinkDetail?.Invoke(item);;
                    GetLinkDetail(item);
                }
                OnEndGetLinkDetail?.Invoke(); 
                
                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        #region 采集任务方法

        /// <summary>
        /// 执行获取任务网站列表操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int DoWebSiteTask(string sWebsiteID)
        {
            try
            {
                OnBeginGetWebsiteList?.Invoke();

                ListWebsites.AddRange(GetWebsiteList(sWebsiteID));

                OnEndGetWebsiteList?.Invoke();

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 执行获取分类列表操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int DoCagegoryTask()
        {
            try
            {
                OnBeginGetCategoryList?.Invoke();

                var oTmpList = new List<Website>();
                if (ListWebsites.Count > 0)
                    oTmpList.AddRange(ListWebsites);
                else
                    return -1;

                foreach (var item in ListWebsites)
                {
                    OnGetingCategoryList?.Invoke(item);
                    ListCategory.AddRange(GetCategoryList(item));
                }

                OnEndGetCategoryList?.Invoke();

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 执行采集链接操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int DoLinkTask()
        {
            try
            {
                OnBeginGetLinkList?.Invoke();

                var oTmpList = new List<Category>();
                if (ListCategory.Count > 0)
                    oTmpList.AddRange(ListCategory);
                else
                    return -1;

                foreach (var item in ListCategory)
                {
                    OnGetingLinkList?.Invoke(item);
                    ListLink.AddRange(GetLinkList(item));
                }

                OnEndGetLinkList?.Invoke();

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        /// <summary>
        /// 执行采集详情操作
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public virtual int DoDetailTask()
        {
            try
            {
                //开始采集详情事件
                OnBeginGetLinkDetail?.Invoke();

                //复制采集对象
                var oTmpList = new List<Link>();
                if (ListLink.Count > 0)
                    oTmpList.AddRange(ListLink);
                else
                    return -1;

                foreach (var item in oTmpList)
                {
                    OnGetingLinkDetail?.Invoke(item);
                    GetLinkDetail(item);
                }

                //结束采集事件
                OnEndGetLinkDetail?.Invoke();

                return 1;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw ex;
            }
        }

        #endregion

        #region 采集操作类

        /// <summary>
        /// 获取网站列表
        /// </summary>
        /// <param name="sID"></param>
        /// <returns></returns>
        public virtual List<Website> GetWebsiteList(string sID)
        {
            try
            {
                List<Website> oListResult = new List<Website>();
                return oListResult;
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
        public virtual List<Category> GetCategoryList(Website oWebsite)
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
        public virtual List<Link> GetLinkList(Category oCategory)
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
        public virtual void GetLinkDetail(Link oResult)
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

        #endregion
    }
}
