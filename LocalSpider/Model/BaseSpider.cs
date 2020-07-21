using Larpx.Logs;
using Larpx.ResourceSpider.Engine;
using Larpx.ResourceSpider.Engine.BaseModel;
using System;
using System.Collections.Generic;
using System.IO;
using static Larpx.ResourceSpider.CommonHelper.CommonHelper;

namespace Larpx.ResourceSpider.LocalSpider.Model
{
    public abstract class BaseSpider : SpiderInterface
    {
        private bool _DeepClone = false;
        private string _WebID = null;
        private bool bDebug = true;
        private string sLoggerPath = "../Logs";
        private Guid _WebGUID = Guid.Empty;
        private DatabaseType _DatabaseType = DatabaseType.SqlServer;
        private Logger m_oLogger = new ConsoleLogger();

        private List<Website> _ListWebsites;
        private List<Category> _ListCategory;
        private List<Link> _ListLink;

        public delegate void BeginGetWebsiteListEventHandler(DatabaseType databaseType);
        public delegate void BeginGetCategoryListEventHandler(DatabaseType databaseType);
        public delegate void BeginGetLinkListEventHandler(DatabaseType databaseType);
        public delegate void BeginGetLinkDetailEventHandler(DatabaseType databaseType);
        public delegate void GetingWebsiteListEventHandler(DatabaseType databaseType, string sID);
        public delegate void GetingCategoryListEventHandler(DatabaseType databaseType, Website website);
        public delegate void GetingLinkListEventHandler(DatabaseType databaseType, Category category);
        public delegate void GetingLinkDetailEventHandler(DatabaseType databaseType, Link link);
        public delegate void EndGetWebsiteListEventHandler(DatabaseType databaseType);
        public delegate void EndGetCategoryListEventHandler(DatabaseType databaseType);
        public delegate void EndGetLinkListEventHandler(DatabaseType databaseType);
        public delegate void EndGetLinkDetailEventHandler(DatabaseType databaseType);

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

        public string WebID { get => _WebID; set => _WebID = value; }
        public DatabaseType DatabaseType { get => _DatabaseType; set => _DatabaseType = value; }
        public bool Debug { get => bDebug; set => bDebug = value; }
        public Guid WebGUID { get => _WebGUID; set => _WebGUID = value; }
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

        public BaseSpider(Guid oWebGUID, DatabaseType oDatabaseType = DatabaseType.SqlServer, string sWebID = null, bool debug = true, string LoggerPath = null, Logger oLogger = null, bool bDeepClone = false,
            List<Website> _ListWebsites = null, List<Category> _ListCategory = null, List<Link> _ListLink = null)
        {
            if (oWebGUID == Guid.Empty && string.IsNullOrEmpty(sWebID))
                throw new ArgumentNullException();

            //赋值
            WebID = sWebID;
            WebGUID = oWebGUID;
            DatabaseType = oDatabaseType;
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
        public virtual int PerExce(Dictionary<string, object> arr = null)
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
        /// <param name="arr">
        /// ID:处理网站的GUID
        /// DatabaseType：数据库类型
        /// </param>
        /// <returns></returns>
        public virtual int DoExce(Dictionary<string, object> arr = null)
        {
            try
            {
                PerExce(arr);

                string sWebsiteID = "";

                //解析参数
                if (arr != null)
                {
                    foreach (var item in arr)
                    {
                        switch (item.Key)
                        {
                            case "ID":
                                sWebsiteID = item.Value.ToString();
                                break;
                            case "DatabaseType":
                                DatabaseType = (DatabaseType)item.Value;
                                break;
                            case "WebGUID":
                                WebGUID = Guid.Parse(item.Value.ToString());
                                break;
                        }
                    }
                }

                //获取任务网站列表
                OnBeginGetWebsiteList?.Invoke(DatabaseType);
                ListWebsites.AddRange(GetWebsiteList(sWebsiteID));
                OnEndGetWebsiteList?.Invoke(DatabaseType);

                //获取分类列表
                OnBeginGetCategoryList?.Invoke(DatabaseType);
                foreach (var item in ListWebsites)
                {
                    OnGetingCategoryList?.Invoke(DatabaseType, item);
                    ListCategory.AddRange(GetCategoryList(item));
                }
                OnEndGetCategoryList?.Invoke(DatabaseType);

                //采集链接
                OnBeginGetLinkList?.Invoke(DatabaseType);
                foreach (var item in ListCategory)
                {
                    OnGetingLinkList?.Invoke(DatabaseType, item);
                    ListLink.AddRange(GetLinkList(item));
                }
                OnEndGetLinkList?.Invoke(DatabaseType);

                //采集详情
                OnBeginGetLinkDetail?.Invoke(DatabaseType);
                foreach (var item in ListLink)
                {
                    OnGetingLinkDetail?.Invoke(DatabaseType, item); ;
                    GetLinkDetail(item);
                }
                OnEndGetLinkDetail?.Invoke(DatabaseType);

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
        public virtual int DoWebSiteTask(string sWebsiteID, DatabaseType databaseType)
        {
            try
            {
                OnBeginGetWebsiteList?.Invoke(DatabaseType);

                ListWebsites.AddRange(GetWebsiteList(sWebsiteID));

                OnEndGetWebsiteList?.Invoke(DatabaseType);

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
        public virtual int DoCagegoryTask(DatabaseType databaseType)
        {
            try
            {
                OnBeginGetCategoryList?.Invoke(DatabaseType);

                var oTmpList = new List<Website>();
                if (ListWebsites.Count > 0)
                    oTmpList.AddRange(ListWebsites);
                else
                    return -1;

                foreach (var item in ListWebsites)
                {
                    OnGetingCategoryList?.Invoke(DatabaseType, item);
                    ListCategory.AddRange(GetCategoryList(item));
                }

                OnEndGetCategoryList?.Invoke(DatabaseType);

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
        public virtual int DoLinkTask(DatabaseType databaseType)
        {
            try
            {
                OnBeginGetLinkList?.Invoke(DatabaseType);

                var oTmpList = new List<Category>();
                if (ListCategory.Count > 0)
                    oTmpList.AddRange(ListCategory);
                else
                    return -1;

                foreach (var item in ListCategory)
                {
                    OnGetingLinkList?.Invoke(DatabaseType, item);
                    ListLink.AddRange(GetLinkList(item));
                }

                OnEndGetLinkList?.Invoke(DatabaseType);

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
        public virtual int DoDetailTask(DatabaseType databaseType)
        {
            try
            {
                //开始采集详情事件
                OnBeginGetLinkDetail?.Invoke(DatabaseType);

                //复制采集对象
                var oTmpList = new List<Link>();
                if (ListLink.Count > 0)
                    oTmpList.AddRange(ListLink);
                else
                    return -1;

                foreach (var item in oTmpList)
                {
                    OnGetingLinkDetail?.Invoke(DatabaseType, item);
                    GetLinkDetail(item);
                }

                //结束采集事件
                OnEndGetLinkDetail?.Invoke(DatabaseType);

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
