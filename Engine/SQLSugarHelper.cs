using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using static Larpx.ResourceSpider.CommonHelper.CommonHelper;

namespace Larpx.ResourceSpider.Engine
{
    public class SQLSugarHelper<T> where T : class, new()
    {
        public SQLSugarHelper(DatabaseType bDbType = DatabaseType.MySql, bool bLog = false)
        {
            switch (bDbType)
            {
                case DatabaseType.MySql:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["MySQL"].ToString(),
                        DbType = DbType.MySql,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Oracle:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["Oracle"].ToString(),
                        DbType = DbType.Oracle,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.PostgreSQL:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["PostgreSQL"].ToString(),
                        DbType = DbType.PostgreSQL,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Sqlite:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["SQLite"].ToString(),
                        DbType = DbType.Sqlite,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.SqlServer:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["SQLServer"].ToString(),
                        DbType = DbType.SqlServer,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;
            }

            //调式代码 用来打印SQL 
            if (bLog)
            {
                Db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql + "\r\n" + Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };
            }
        }

        //注意：不能写成静态的
        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作
        public SimpleClient<T> CurrentDb { get { return new SimpleClient<T>(Db); } }//用来操作当前表的数据

        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAny(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.IsAny(whereExpression);
        }

        /// <summary>
        /// 获取所有
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetList()
        {
            return CurrentDb.GetList();
        }

        /// <summary>
        /// 根据表达式查询
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetList(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.GetList(whereExpression);
        }

        /// <summary>
        /// 根据表达式查询分页
        /// </summary>
        /// <returns></returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel pageModel)
        {
            return CurrentDb.GetPageList(whereExpression, pageModel);
        }

        /// <summary>
        /// 根据表达式查询分页并排序
        /// </summary>
        /// <param name="whereExpression">it</param>
        /// <param name="pageModel"></param>
        /// <param name="orderByExpression">it=>it.id或者it=>new{it.id,it.name}</param>
        /// <param name="orderByType">OrderByType.Desc</param>
        /// <returns></returns>
        public virtual List<T> GetPageList(Expression<Func<T, bool>> whereExpression, PageModel pageModel, Expression<Func<T, object>> orderByExpression = null, OrderByType orderByType = OrderByType.Asc)
        {
            return CurrentDb.GetPageList(whereExpression, pageModel, orderByExpression, orderByType);
        }

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <returns></returns>
        public virtual T GetById(dynamic id)
        {
            return CurrentDb.GetById(id);
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(dynamic id)
        {
            return CurrentDb.Delete(id);
        }

        /// <summary>
        /// 根据实体删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(T data)
        {
            return CurrentDb.Delete(data);
        }

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(dynamic[] ids)
        {
            return CurrentDb.AsDeleteable().In(ids).ExecuteCommand() > 0;
        }

        /// <summary>
        /// 根据表达式删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Delete(Expression<Func<T, bool>> whereExpression)
        {
            return CurrentDb.Delete(whereExpression);
        }

        /// <summary>
        /// 根据实体更新，实体需要有主键
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Update(T obj)
        {
            return CurrentDb.Update(obj);
        }

        /// <summary>
        ///批量更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Update(List<T> objs)
        {
            return CurrentDb.UpdateRange(objs);
        }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Insert(T obj)
        {
            return CurrentDb.Insert(obj);
        }

        /// <summary>
        /// 批量
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool Insert(List<T> objs)
        {
            return CurrentDb.InsertRange(objs);
        }

        //自已扩展更多方法 
    }

    public class SQLSugarHelper
    {
        public SQLSugarHelper(DatabaseType bDbType = DatabaseType.SqlServer, bool bLog = false)
        {
            switch (bDbType)
            {
                case DatabaseType.MySql:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["MySQL"].ToString(),
                        DbType = DbType.MySql,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Oracle:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["Oracle"].ToString(),
                        DbType = DbType.Oracle,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.PostgreSQL:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["PostgreSQL"].ToString(),
                        DbType = DbType.PostgreSQL,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Sqlite:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["SQLite"].ToString(),
                        DbType = DbType.Sqlite,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.SqlServer:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = ConfigurationManager.AppSettings["SQLServer"].ToString(),
                        DbType = DbType.SqlServer,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;
            }

            //调式代码 用来打印SQL 
            if (bLog)
            {
                Db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql + "\r\n" + Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };
            }
        }

        //注意：不能写成静态的
        public SqlSugarClient Db;//用来处理事务多表查询和复杂的操作

        public SimpleClient<Setting> SettingDb { get { return new SimpleClient<Setting>(Db); } }//用来处理Setting表的常用操作
        public SimpleClient<Manager> ManagerDb { get { return new SimpleClient<Manager>(Db); } }//用来处理Manager表的常用操作
        public SimpleClient<SystemLog> SystemLogDb { get { return new SimpleClient<SystemLog>(Db); } }//用来处理SystemLog表的常用操作
        public SimpleClient<Website> WebsiteDb { get { return new SimpleClient<Website>(Db); } }//用来处理Website表的常用操作
        public SimpleClient<Category> CategoryDb { get { return new SimpleClient<Category>(Db); } }//用来处理Category表的常用操作
        public SimpleClient<Link> LinkDb { get { return new SimpleClient<Link>(Db); } }//用来处理Link表的常用操作
        public SimpleClient<PropertyKey> PropertyKeyDb { get { return new SimpleClient<PropertyKey>(Db); } }//用来处理PropertyKey表的常用操作
        public SimpleClient<PropertyValue> PropertyValueDb { get { return new SimpleClient<PropertyValue>(Db); } }//用来处理PropertyValue表的常用操作
        public SimpleClient<PropertyDetail> PropertyDetailDb { get { return new SimpleClient<PropertyDetail>(Db); } }//用来处理PropertyDetail表的常用操作
        public SimpleClient<ResourceData> ResourceDataDb { get { return new SimpleClient<ResourceData>(Db); } }//用来处理ResourceData表的常用操作
        public SimpleClient<Resource> ResourceDb { get { return new SimpleClient<Resource>(Db); } }//用来处理Resource表的常用操作
        public SimpleClient<MetaData> MetaDataDb { get { return new SimpleClient<MetaData>(Db); } }//用来处理Resource表的常用操作
        public SimpleClient<Tag> TagDb { get { return new SimpleClient<Tag>(Db); } }//用来处理Tag表的常用操作
    }
}
