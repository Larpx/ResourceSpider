using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Larpx.ResourceSpider.Engine.SQLServer
{
    public class DapperSQLHelper
    {
        /// <summary>
        /// Insert操作
        /// </summary>
        /// <param name="sConnectString">链接字符串</param>
        /// <param name="sSQLCommand">Insert语句</param>
        /// <param name="oObject">实体类</param>
        /// <returns>受影响行数</returns>
        public static int Insert(string sConnectString, string sSQLCommand, object oObject)
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand)
                || oObject == null)
                return -1;
            int nResult = 0;
            nResult = Execute(sConnectString, sSQLCommand, oObject);
            return nResult;
        }

        /// <summary>
        /// Update操作
        /// </summary>
        /// <param name="sConnectString">链接字符串</param>
        /// <param name="sSQLCommand">Insert语句</param>
        /// <param name="oObject">实体类</param>
        /// <returns>受影响行数</returns>
        public static int Update(string sConnectString, string sSQLCommand, object oObject)
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand)
                || oObject == null)
                return -1;
            int nResult = 0;
            nResult = Execute(sConnectString, sSQLCommand, oObject);
            return nResult;
        }

        /// <summary>
        /// 执行任意的SQL命令
        /// </summary>
        /// <param name="sConnectString">链接字符串</param>
        /// <param name="sSQLCommand">SQL命令</param>
        /// <param name="oObject">实体类</param>
        /// <returns>受影响行数</returns>
        public static int Execute(string sConnectString, string sSQLCommand, object oObject = null)
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand))
                return -1;
            int nResult = 0;
            using (IDbConnection connection = new SqlConnection(sConnectString))
            {
                if (oObject != null)
                    nResult = connection.Execute(sSQLCommand, oObject);
                else
                    nResult = connection.Execute(sSQLCommand);
            }
            return nResult;
        }

        /// <summary>
        /// Select操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sConnectString"></param>
        /// <param name="sSQLCommand"></param>
        /// <param name="oObject"></param>
        /// <returns></returns>
        public static List<T> Excute<T>(string sConnectString, string sSQLCommand,
            T oObject = default(T)) where T : class
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand))
                return null;
            List<T> oList = new List<T>();
            using (IDbConnection connection = new SqlConnection(sConnectString))
            {
                var oTmp = connection.Query<T>(sSQLCommand, oObject);
                oList = oTmp.AsList();
            }
            return oList;
        }

        /// <summary>
        /// Select操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sConnectString"></param>
        /// <param name="sSQLCommand"></param>
        /// <param name="oObject"></param>
        /// <returns></returns>
        public static int ExecuteScalar(string sConnectString, string sSQLCommand)
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand))
                return 0;
            using (IDbConnection connection = new SqlConnection(sConnectString))
            {
                return connection.ExecuteScalar<int>(sSQLCommand);
            }
        }

        /// <summary>
        /// Select操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sConnectString"></param>
        /// <param name="sSQLCommand"></param>
        /// <param name="oObject"></param>
        /// <returns></returns>
        public static T ExecuteScalar<T>(string sConnectString, string sSQLCommand)
        {
            if (string.IsNullOrEmpty(sConnectString) || string.IsNullOrEmpty(sSQLCommand))
                return default(T);
            using (IDbConnection connection = new SqlConnection(sConnectString))
            {
                return connection.ExecuteScalar<T>(sSQLCommand);
            }
        }
    }
}
