/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisOperationForString
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/3 15:45:03
*描述：
*=====================================================================
*修改时间：2020/6/3 15:45:03
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wenli.Drive.Redis.Interface;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisOperationForString
    /// </summary>
    public partial class SERedisOperation : IRedisOperation
    {

        #region Keys

        /// <summary>
        ///     是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().KeyExists(key);
            });
        }

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, DateTime datetime)
        {
            if (datetime.Kind == DateTimeKind.Unspecified)
            {
                datetime = DateTime.SpecifyKind(datetime, DateTimeKind.Local);
            }
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().KeyExpire(key, datetime);
            });
        }

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, int timeout = 0)
        {
            return DoWithRetry(() =>
            {
                if (timeout > 0)
                    return _cnn.GetDatabase().KeyExpire(key, DateTime.Now.AddSeconds(timeout));
                return false;
            });
        }

        /// <summary>
        ///     设置key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool StringSet(string key, string value, int timeout = 0)
        {
            return DoWithRetry(() =>
            {
                var bResult = false;
                if (timeout > 0)
                    bResult = _cnn.GetDatabase().StringSet(key, value, new TimeSpan(0, 0, timeout));
                else
                    bResult = _cnn.GetDatabase().StringSet(key, value);
                return bResult;
            });
        }

        /// <summary>
        ///     设置key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expire">key的超时时常</param>
        /// <returns></returns>
        public bool StringSet(string key, string value, TimeSpan expire)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringSet(key, value, expire);
            });
        }

        /// <summary>
        ///  设置一个值，仅在不存在的时候设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool StringSetIfNotExists(string key, string value)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringSet(key, value, when: When.NotExists);
            });
        }

        /// <summary>
        ///  设置一个值，仅在不存在的时候设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts">超时时间</param>
        /// <returns></returns>
        public bool StringSetIfNotExists(string key, string value, TimeSpan ts)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringSet(key, value, ts, when: When.NotExists);
            });
        }

        /// <summary>
        ///     获取key的同时set该Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string StringGetSet(string key, string value)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringGetSet(key, value);
            });
        }

        /// <summary>
        /// 获取全部keys
        /// </summary>
        /// <param name="patten"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据,且本方法只能查询db0，建议使用sortedset来保存keys")]
        public List<string> StringGetKeys(string patten = "*")
        {
            return DoWithRetry(() =>
            {
                return _cnn.Keys(patten);
            });
        }

        /// <summary>
        /// 获取全部keys
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="patten"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据,且本方法只能查询db0，建议使用sortedset来保存keys")]
        public void StringGetKeys(Action<List<string>> callback, string patten = "*", int size = 1000)
        {
            DoWithRetry(() =>
            {
                _cnn.Keys(callback, patten, size);
            });
        }

        /// <summary>
        /// 获取全部keys
        /// </summary>
        /// <param name="pageSize"></param>
        /// /// <param name="dbIndex"></param>
        /// <param name="patten"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据，建议使用sortedset来保存keys")]
        public List<string> StringGetKeys(int pageSize, int dbIndex = -1, string patten = "*")
        {
            return DoWithRetry(() =>
            {
                List<string> keys = new List<string>();

                var result = _cnn.GetDatabase().ScriptEvaluate(LuaScript.Prepare("return  redis.call('KEYS', '*')"), CommandFlags.PreferSlave);

                if (!result.IsNull)
                {
                    var list = (RedisResult[])result;
                    foreach (var item in list)
                    {
                        var key = (RedisKey)item;
                        keys.Add(key.ToString());
                    }
                }
                return keys;
            });
        }

        /// <summary>
        ///     获取key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringGet(key);
            });
        }

        /// <summary>
        ///     设置key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool StringSet<T>(string key, T t, int timeout = 0) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                return StringSet(key, SerializeHelper.Serialize(t), timeout);
            });
        }

        /// <summary>
        ///     获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                var str = StringGet(key);
                if (!string.IsNullOrWhiteSpace(str))
                    return SerializeHelper.Deserialize<T>(str);
                return default(T);
            });
        }

        /// <summary>
        /// 批量获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> BatchStringGet<T>(List<string> keys) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                var batch = _cnn.GetDatabase().CreateBatch();

                List<Task<RedisValue>> tasks = new List<Task<RedisValue>>();

                foreach (var key in keys)
                {
                    tasks.Add(batch.StringGetAsync(key));
                }
                batch.Execute();

                List<T> result = new List<T>();

                foreach (var task in tasks)
                {
                    result.Add(SerializeHelper.Deserialize<T>(task.Result));
                }

                return result;
            });
        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> GetValues<T>(List<string> keys) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                return GetValues<T>(keys.ToArray());
            });
        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> GetValues<T>(string[] keys) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                var list = new List<T>();
                if ((keys != null) && (keys.Length > 0))
                    foreach (var key in keys)
                    {
                        var item = StringGet<T>(key);
                        if (item != null)
                            list.Add(item);
                    }
                return list;
            });
        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> GetValues(string[] keys)
        {
            return DoWithRetry(() =>
            {
                var list = new List<string>();
                if ((keys != null) && (keys.Length > 0))
                    foreach (var key in keys)
                    {
                        var item = StringGet(key);
                        if (item != null)
                            list.Add(item);
                    }
                return list;
            });
        }

        /// <summary>
        ///     移除key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyDelete(string key)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().KeyDelete(key);
            });
        }

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        public void KeysDelete(string[] keys)
        {
            DoWithRetry(() =>
            {
                for (var i = 0; i < keys.Length; i++)
                    KeyDelete(keys[i]);
            });
        }

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        public void KeysDelete(List<string> keys)
        {
            DoWithRetry(() =>
            {
                KeysDelete(keys.ToArray());
            });
        }

        /// <summary>
        ///     重命名key
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool KeyRename(string oldKey, string newKey)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().KeyRename(oldKey, newKey);
            });
        }

        /// <summary>
        ///     key计数器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double StringIncrement(string key, double value)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringIncrement(key, value);
            });
        }

        /// <summary>
        ///     key计数器(减去相应的value)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double StringDecrement(string key, double value)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringDecrement(key, value);
            });
        }

        /// <summary>
        ///     追加value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long StringAppend(string key, string value)
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().StringAppend(value, value, CommandFlags.None);
            });
        }

        #endregion

    }
}
