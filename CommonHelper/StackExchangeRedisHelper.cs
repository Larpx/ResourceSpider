using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Text;

namespace Larpx.ResourceSpider.CommonHelper
{
    /// <summary>
    /// Redis 操作类
    /// </summary>
    public class StackExchangeRedisHelper
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["RedisConnectionString"].ConnectionString;

        /// <summary>
        /// 锁
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 连接对象
        /// </summary>
        private volatile IConnectionMultiplexer _connection;

        /// <summary>
        /// 数据库
        /// </summary>
        private IDatabase _db;

        /// <summary>
        /// 初始化
        /// </summary>
        public StackExchangeRedisHelper()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new Exception("链接字符串为空");

            _connection = ConnectionMultiplexer.Connect(ConnectionString);
            _db = GetDatabase();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public StackExchangeRedisHelper(string sConnectionString,int? db)
        {
            _connection = ConnectionMultiplexer.Connect(sConnectionString);
            _db = GetDatabase(db);
        }

        /// <summary>
        /// 指定数据库
        /// </summary>
        /// <param name="db"></param>
        public StackExchangeRedisHelper(int? db)
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new Exception("链接字符串为空");

            _connection = ConnectionMultiplexer.Connect(ConnectionString);
            _db = GetDatabase(db);
        }

        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public IDatabase GetDatabase(int? db = null)
        {
            return GetConnection().GetDatabase(db ?? -1);
        }

        /// <summary>
        /// 设置缓存键值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="data">值</param>
        /// <param name="cacheTime">时间</param>
        public virtual void Set(string key, object data, int cacheTime)
        {
            if (data == null)
            {
                return;
            }
            var entryBytes = Serialize(data);
            var expiresIn = TimeSpan.FromMinutes(cacheTime);

            _db.StringSet(key, entryBytes, expiresIn);
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="data">值</param>
        public virtual void Set(string key, object data)
        {
            if (data == null)
            {
                return;
            }
            var entryBytes = Serialize(data);

            _db.StringSet(key, entryBytes);
        }

        /// <summary>
        /// 根据键获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual T Get<T>(string key)
        {
            var rValue = _db.StringGet(key);
            if (!rValue.HasValue)
            {
                return default(T);
            }

            var result = Deserialize<T>(rValue);

            return result;
        }

        /// <summary>
        /// 根据键获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool Del(string key)
        {
            lock (_db)
            {
                return _db.KeyDelete(key);
            }
        }

        /// <summary>
        /// 判断是否已经设置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool IsSet(string key)
        {
            lock (_db)
            {
                return _db.KeyExists(key);
            }
        }

        #region 私有

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        private IConnectionMultiplexer GetConnection()
        {
            if (_connection != null && _connection.IsConnected)
            {
                return _connection;
            }
            lock (_lock)
            {
                if (_connection != null && _connection.IsConnected)
                {
                    return _connection;
                }

                if (_connection != null)
                {
                    _connection.Dispose();
                }
                _connection = ConnectionMultiplexer.Connect(ConnectionString);
            }

            return _connection;
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="data"></param>
        /// <returns>byte[]</returns>
        private byte[] Serialize(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        private T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject == null)
            {
                return default(T);
            }
            var json = Encoding.UTF8.GetString(serializedObject);
            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion
    }
}
