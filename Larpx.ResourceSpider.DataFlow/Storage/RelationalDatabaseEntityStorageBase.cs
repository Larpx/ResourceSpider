using Larpx.ResourceSpider.BaseLibrary.Helpers;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Larpx.ResourceSpider.BaseLibrary.Data.EnumData;

namespace Larpx.ResourceSpider.DataFlow.Storage
{
    /// <summary>
    /// 关系型数据库保存实体解析结果
    /// </summary>
    public abstract class RelationalDatabaseEntityStorageBase : EntityStorageBase
    {
        private readonly ConcurrentDictionary<string, SqlStatements> _sqlStatementDict =
            new ConcurrentDictionary<string, SqlStatements>();

        private readonly ConcurrentDictionary<string, object> _executedCache =
            new ConcurrentDictionary<string, object>();

        private readonly ConcurrentDictionary<Type, TableMetadata> _tableMetadataDict =
            new ConcurrentDictionary<Type, TableMetadata>();

        protected const string BoolType = "Boolean";
        protected const string DateTimeType = "DateTime";
        protected const string DateTimeOffsetType = "DateTimeOffset";
        protected const string DecimalType = "Decimal";
        protected const string DoubleType = "Double";
        protected const string FloatType = "Single";
        protected const string IntType = "Int32";
        protected const string LongType = "Int64";
        protected const string ByteType = "Byte";
        protected const string ShortType = "Short";

        /// <summary>
        /// 注意：不能写成静态的
        /// 用来处理事务多表查询和复杂的操作
        /// </summary>
        public SqlSugarClient Db;

        /// <summary>
        /// 存储器类型
        /// </summary>
        public StorageMode Mode { get; set; }

        /// <summary>
        /// 数据库操作重试次数
        /// </summary>
        public int RetryTimes { get; set; } = 600;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// 是否使用事务操作。默认不使用。
        /// </summary>
        public bool UseTransaction { get; set; }

        /// <summary>
        /// 数据库忽略大小写
        /// </summary>
        public bool IgnoreCase { get; set; } = true;

        /// <summary>
        /// 输出sql查询语句日志
        /// </summary>
        public bool OutputLog { get; set; } = true;

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

        /// <summary>
        /// 创建数据库连接接口
        /// </summary>
        /// <param name="connectString">连接字符串</param>
        /// <returns></returns>
        protected abstract SqlSugarClient CreateDbConnection(string connectString, DatabaseType DatabaseType);

        /// <summary>
        /// 生成 SQL 语句
        /// </summary>
        /// <param name="tableMetadata">表元数据</param>
        /// <returns></returns>
        protected abstract SqlStatements GenerateSqlStatements(TableMetadata tableMetadata);

        /// <summary>
        /// 创建数据库和表
        /// </summary>
        /// <param name="conn">数据库连接</param>
        /// <param name="sqlStatements">SQL 语句</param>
        protected virtual void EnsureDatabaseAndTableCreated(SqlSugarClient conn,
            SqlStatements sqlStatements)
        {
            if (!string.IsNullOrWhiteSpace(sqlStatements.CreateDatabaseSql))
            {
                conn.Ado.ExecuteCommand(sqlStatements.CreateDatabaseSql);
            }

            conn.Ado.ExecuteCommand(sqlStatements.CreateTableSql);
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="model">存储器类型</param>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="bDbType">数据库类型</param>
        protected RelationalDatabaseEntityStorageBase(StorageMode model, string connectionString, DatabaseType bDbType)
        {
            connectionString.NotNullOrWhiteSpace(nameof(connectionString));
            ConnectionString = connectionString;
            Mode = model;

            switch (bDbType)
            {
                case DatabaseType.MySql:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.MySql,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Oracle:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.Oracle,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.PostgreSQL:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.PostgreSQL,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.Sqlite:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.Sqlite,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;

                case DatabaseType.SqlServer:
                    Db = new SqlSugarClient(new ConnectionConfig()
                    {
                        ConnectionString = connectionString,
                        DbType = DbType.SqlServer,
                        InitKeyType = InitKeyType.Attribute,//从特性读取主键和自增列信息
                        IsAutoCloseConnection = true,//开启自动释放模式和EF原理一样我就不多解释了

                    });
                    break;
            }

            if (OutputLog)
            {
                Db.Aop.OnLogExecuting = (sql, pars) =>
                {
                    Console.WriteLine(sql + "\r\n" + Db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                    Console.WriteLine();
                };
            }
        }

        protected override async Task StoreAsync(DataContext context, Dictionary<Type, List<dynamic>> dict)
        {

            foreach (var kv in dict)
            {
                var list = (IList)kv.Value;
                var tableMetadata = _tableMetadataDict.GetOrAdd(kv.Key,
                    type => ((IEntity)list[0]).GetTableMetadata());
                var sqlStatements = GetSqlStatements(tableMetadata);

                for (var i = 0; i < RetryTimes; ++i)
                {
                    try
                    {
                        if (UseTransaction)
                            Db.Ado.BeginTran();

                        switch (Mode)
                        {
                            case StorageMode.Insert:
                                {
                                    await Db.Ado.ExecuteCommandAsync(sqlStatements.InsertSql, list);
                                    break;
                                }
                            case StorageMode.InsertIgnoreDuplicate:
                                {
                                    await Db.Ado.ExecuteCommandAsync(sqlStatements.InsertIgnoreDuplicateSql, list);
                                    break;
                                }
                            case StorageMode.Update:
                                {
                                    if (string.IsNullOrWhiteSpace(sqlStatements.UpdateSql))
                                    {
                                        throw new Exception("未能生成更新 SQL");
                                    }

                                    await Db.Ado.ExecuteCommandAsync(sqlStatements.UpdateSql, list);
                                    break;
                                }
                            case StorageMode.InsertAndUpdate:
                                {
                                    await Db.Ado.ExecuteCommandAsync(sqlStatements.InsertAndUpdateSql, list);
                                    break;
                                }
                        }

                        if (UseTransaction)
                            Db.Ado.CommitTran();
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"尝试插入数据失败: {ex}");

                        // 网络异常需要重试，并且不需要 Rollback
                        var endOfStreamException = ex.InnerException as EndOfStreamException;
                        if (endOfStreamException == null)
                        {
                            try
                            {
                                if (UseTransaction)
                                    Db.Ado.RollbackTran();
                            }
                            catch (Exception e)
                            {
                                Logger?.LogError($"数据库回滚失败: {e}");
                            }

                            break;
                        }
                    }
                }
            }
        }

        private SqlStatements GetSqlStatements(TableMetadata tableMetadata)
        {
            var key = tableMetadata.TypeName;
            return _sqlStatementDict.GetOrAdd(key, str => GenerateSqlStatements(tableMetadata));
        }
    }
}
