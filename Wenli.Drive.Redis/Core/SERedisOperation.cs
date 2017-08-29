/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2016-2020
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：1cc1fa2b-c8c8-4627-bb40-dece98f2b73a
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：Wenli.Drive.Redis
 * 命名空间：Wenli.Drive.Redis
 * 创建时间：2016/12/28 9:59:30
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using StackExchange.Redis;
using Wenli.Drive.Redis.Data;
using Wenli.Drive.Redis.Interface;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    ///     SERedis操作类
    /// </summary>
    public partial class SERedisOperation : IRedisOperation
    {
        /// <summary>
        ///     倒序排例
        /// </summary>
        private const string DefaultOrder = "desc";

        private readonly int _busyRetry = 5;

        private readonly int _busyRetryWaitMS = 200;

        private readonly int _dbIndex = -1;
        private readonly string _sectionName;

        /// <summary>
        ///     SERedis操作类
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="dbIndex"></param>
        /// <param name="busyRetry"></param>
        /// <param name="busyRetryWaitMS"></param>
        public SERedisOperation(string sectionName, int dbIndex = -1, int busyRetry = 5, int busyRetryWaitMS = 200)
        {
            _sectionName = sectionName;
            _dbIndex = dbIndex;
            _busyRetry = busyRetry;
            _busyRetryWaitMS = busyRetryWaitMS;
            if ((_busyRetry < 0) || (_busyRetry > 50))
                throw new Exception("重试次数有误，请输入0-50之间整数");
            if ((_busyRetryWaitMS < 100) || (_busyRetryWaitMS > 3000))
                throw new Exception("失败重试等待时长有误，请输入100-3000之间整数");
        }

        #region 操作方法重试包装

        private T DoWithRetry<T>(Func<T> func)
        {
            var counter = 0;
            while (counter <= _busyRetry)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    counter++;

                    string retryCountMsg = string.Empty;

                    if (ex is TimeoutException)
                    {
                        retryCountMsg = string.Format("TimeoutException Redis<T>操作超时，等待随后重试。当前已经重试：{0};ex:{1}", counter, ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T>连接异常，等待随后重试。当前已重试：{0};ex:{1}", counter, ex.Message);
                    }
                    else if (ex is RedisServerException && ex.Message.Contains("MOVED"))
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> MOVED 异常，等待随后重试。当前已重试：{0};ex:{1}", counter, ex.Message);
                    }
                    else
                    {
                        throw ex;
                    }

                    // 将重试写成错误，引起重视
                    Log4NetHelper.WriteLog(retryCountMsg);

                    if (counter > _busyRetry)
                        throw ex;  // 大于重试次数，将直接抛出去
                    Thread.Sleep(counter * _busyRetryWaitMS);

                }
            }
            return default(T);
        }

        private void DoWithRetry(Action action)
        {
            var counter = 0;
            while (counter <= _busyRetry)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    counter++;

                    string retryCountMsg = string.Empty;

                    if (ex is TimeoutException)
                    {
                        retryCountMsg = string.Format("TimeoutException Redis<T>操作超时，等待随后重试。当前已经重试：{0};ex:{1}", counter, ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T>连接异常，等待随后重试。当前已重试：{0};ex:{1}", counter, ex.Message);
                    }
                    else
                    {
                        throw ex;
                    }

                    // 将重试写成info，引起重视
                    Log4NetHelper.WriteLog(retryCountMsg);

                    if (counter > _busyRetry)
                        throw ex;  // 大于重试次数，将直接抛出去
                    Thread.Sleep(counter * _busyRetryWaitMS);
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns></returns>
        public string GetServerInfo()
        {
            using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
            {
                return cnn.GetServerInfo();
            }
        }

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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().KeyExists(key);
                }
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
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().KeyExpire(key, datetime);
                }
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
                    using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                    {
                        return cnn.GetDatabase().KeyExpire(key, DateTime.Now.AddSeconds(timeout));
                    }
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
                    using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                    {
                        bResult = cnn.GetDatabase().StringSet(key, value, new TimeSpan(0, 0, timeout));
                    }
                else
                    using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                    {
                        cnn.GetDatabase().StringSet(key, value);
                    }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringSet(key, value, expire);
                }
            });
        }

        /// <summary>
        ///  设置一个值，仅在不存在的时候设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool StringSetIfNotExists(string key, string value)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringSet(key, value, when: When.NotExists);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringGetSet(key, value);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.Keys(patten);
                }
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

                using (var cnn = new SERedisConnection(_sectionName, dbIndex == -1 ? _dbIndex : dbIndex))
                {
                    var result = cnn.GetDatabase().ScriptEvaluate(LuaScript.Prepare("return  redis.call('KEYS', '*')"), CommandFlags.PreferSlave);

                    if (!result.IsNull)
                    {
                        var list = (RedisResult[])result;
                        foreach (var item in list)
                        {
                            var key = (RedisKey)item;
                            keys.Add(key.ToString());
                        }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringGet(key);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return StringSet(key, SerializeHelper.Serialize(t), timeout);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().KeyDelete(key);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().KeyRename(oldKey, newKey);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringIncrement(key, value);
                }
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
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().StringAppend(value, value, CommandFlags.None);
                }
            });
        }

        #endregion

        #region Hashes

        /// <summary>
        ///     检查hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HashExists(string hashId, string key)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    try
                    {

                        return cnn.GetDatabase().HashExists(hashId, key);

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("HashExists异常，参数：{0},{1},{2},{3},异常信息：{4}", _sectionName, _dbIndex, hashId, key, ex.Message));
                    }
                }
            });
        }

        /// <summary>
        ///     设置hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string hashId, string key, T t) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashSet(hashId, key, SerializeHelper.Serialize(t));
                }
            });
        }

        /// <summary>
        ///     设置hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet(string hashId, string key, string val)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashSet(hashId, key, val);
                }
            });
        }

        /// <summary>
        ///     在一个hash中设置一个key - value，仅仅当不存在的时候才设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSetIfNotExists(string hashId, string key, string value)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashSet(hashId, key, value, When.NotExists);
                }
            });
        }

        /// <summary>
        ///     移除hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HashDelete(string hashId, string key)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashDelete(hashId, key);
                }
            });
        }

        /// <summary>
        ///     hash计数器
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long HashIncrement(string hashId, string key, long value = 1)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashIncrement(hashId, key, value);
                }
            });
        }

        /// <summary>
        ///     获取hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public T HashGet<T>(string hashId, string key) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    string str = cnn.GetDatabase().HashGet(hashId, key);
                    if (!string.IsNullOrWhiteSpace(str))
                        return SerializeHelper.Deserialize<T>(str);
                    return default(T);
                }
            });
        }

        /// <summary>
        ///     获取hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HashGet(string hashId, string key)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashGet(hashId, key).ToString();
                }
            });
        }

        /// <summary>
        ///     获取hash数量
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public long GetHashCount(string hashId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashLength(hashId);
                }
            });
        }

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> HashGetAll(string hashId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<string>();
                    var list = cnn.GetDatabase().HashValues(hashId).ToList();
                    if ((list != null) && (list.Count > 0))
                        list.ForEach(x =>
                        {
                            if (x.HasValue)
                                result.Add(x.ToString());
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(string hashId) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<T>();
                    var list = cnn.GetDatabase().HashValues(hashId).ToList();
                    if (list.Count > 0)
                        list.ForEach(x =>
                        {
                            if (x.HasValue)
                            {
                                var str = x.ToString();
                                if (!string.IsNullOrWhiteSpace(str))
                                {
                                    var t = SerializeHelper.Deserialize<T>(str);
                                    if (t != null)
                                        result.Add(t);
                                }
                            }
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAllDic<T>(string hashId) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new Dictionary<string, T>();
                    var list = cnn.GetDatabase().HashGetAll(hashId).ToList();

                    if (list.Count > 0)
                        list.ForEach(x =>
                        {
                            var str = x.Value;
                            if (!string.IsNullOrWhiteSpace(str))
                            {
                                var value = SerializeHelper.Deserialize<T>(str);
                                result.Add(x.Name, value);
                            }
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public Dictionary<string, string> HashGetAllDic(string hashId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new Dictionary<string, string>();
                    var list = cnn.GetDatabase().HashGetAll(hashId).ToList();

                    if (list.Count > 0)
                        list.ForEach(x =>
                        {
                            result.Add(x.Name, x.Value);
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     分页某个hashid下面hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(string hashID, int pageIndex, int pageSize) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<T>();
                    pageIndex = pageIndex > 0 ? pageIndex : 1;
                    pageSize = pageSize > 0 ? pageSize : 20;
                    var list =
                        cnn.GetDatabase().HashValues(hashID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    if ((list != null) && (list.Count > 0))
                        list.ForEach(value =>
                        {
                            if (!string.IsNullOrEmpty(value) && value.HasValue)
                            {
                                var obj = SerializeHelper.Deserialize<T>(value.ToString());
                                if (obj != null)
                                    result.Add(obj);
                            }
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取指定的全部hashlist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashIds"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(List<string> hashIds) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                var result = new List<T>();
                foreach (var hashId in hashIds)
                {
                    var list = HashGetAll<T>(hashId);
                    if ((list != null) && (list.Count > 0))
                        result.AddRange(list);
                }
                return result;
            });
        }

        /// <summary>
        ///     获取某个hashid下面全部keys
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> GetHashKeys(string hashId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<string>();
                    var list = cnn.GetDatabase().HashKeys(hashId).ToList();
                    if (list.Count > 0)
                        list.ForEach(x =>
                        {
                            result.Add(x.ToString());
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> GetValuesFromHash<T>(string hashId, List<string> keys) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var list = new List<T>();
                    if ((keys != null) && (keys.Count > 0))
                    {
                        var rv = new RedisValue[keys.Count];
                        for (var i = 0; i < keys.Count; i++)
                            rv[i] = keys[i];
                        var vlts = cnn.GetDatabase().HashGet(hashId, rv);
                        foreach (var val in vlts)
                            if (!string.IsNullOrEmpty(val))
                            {
                                var obj = SerializeHelper.Deserialize<T>(val);
                                if (obj != null)
                                    list.Add(obj);
                            }
                    }
                    return list;
                }
            });
        }

        /// <summary>
        ///     从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> GetValuesFromHash(string hashId, List<string> keys)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var list = new List<string>();
                    if ((keys != null) && (keys.Count > 0))
                    {
                        var rv = new RedisValue[keys.Count];
                        for (var i = 0; i < keys.Count; i++)
                            rv[i] = keys[i];
                        var vlts = cnn.GetDatabase().HashGet(hashId, rv);
                        foreach (var val in vlts)
                            if (!string.IsNullOrEmpty(val) && val.HasValue)
                                list.Add(val);
                    }
                    return list;
                }
            });
        }

        #endregion

        #region Set

        /// <summary>
        ///     添加一个set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetAdd(string setId, string val)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SetAdd(setId, val);
                }
            });
        }

        /// <summary>
        ///     是否存在于set中
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetContains(string setId, string val)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SetContains(setId, val);
                }
            });
        }

        /// <summary>
        ///     获取某个key下面全部的value
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public List<string> SetMembers(string setId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var values = cnn.GetDatabase().SetMembers(setId);
                    var list = new List<string>();
                    if ((values != null) && (values.Count() > 0))
                        foreach (var sitem in values)
                            list.Add(sitem.ToString());
                    return list;
                }
            });
        }

        /// <summary>
        ///     移除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetRemove(string setId, string val)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SetRemove(setId, val);
                }
            });
        }

        /// <summary>
        ///     批量删除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="vals"></param>
        public void SetsRemove(string setId, string[] vals)
        {
            DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    for (var i = 0; i < vals.Length; i++)
                        cnn.GetDatabase().SetRemove(setId, vals[i]);
                }
            });
        }

        /// <summary>
        ///     返回指定set长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public long SetLength(string setId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SetLength(setId);
                }
            });
        }

        #endregion

        #region Sorted Sets

        /// <summary>
        ///     检查SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool SortedSetItemIsExist(string setId, string item)
        {
            return DoWithRetry(() =>
            {
                var value = GetItemScoreFromSortedSet(setId, item);
                if (value != null)
                    return true;
                return false;
            });
        }

        /// <summary>
        ///     添加SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool SortedSetAdd(string setId, string item, double score)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetAdd(setId, item, score);
                }
            });
        }

        /// <summary>
        ///     查询SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<string> GetSortedSetRangeByRank(string setId, long fromRank, long toRank,
            string order = DefaultOrder)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<string>();
                    var list =
                        cnn.GetDatabase()
                            .SortedSetRangeByRank(setId, fromRank, toRank,
                                order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                            .ToList();
                    if (list.Any())
                        list.ForEach(x =>
                        {
                            result.Add(x.ToString());
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     根据score查询SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public Dictionary<string, double> GetSortedSetRangeByRankWithScores(string setId, long fromRank, long toRank,
            string order = DefaultOrder)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new Dictionary<string, double>();
                    var list =
                        cnn.GetDatabase()
                            .SortedSetRangeByRankWithScores(setId, fromRank, toRank,
                                order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                            .ToList();
                    if (list.Any())
                        list.ForEach(x =>
                        {
                            result.Add(x.Element, x.Score);
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取SortedSet集合区间
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public PagedList<string> GetSortedSetRangeByRankWithSocres(string setid, long min, long max, int pageIndex = 1,
            int pageSize = 20, bool orderBy = true)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var count = cnn.GetDatabase().SortedSetLength(setid, min, max);
                    if (count > 0)
                    {
                        var list = cnn.GetDatabase()
                            .SortedSetRangeByScoreWithScores(setid, min, max, Exclude.None,
                                orderBy ? Order.Ascending : Order.Descending, (pageIndex - 1) * pageSize, pageSize);
                        if ((list != null) && (list.Length > 0))
                        {
                            var result = new List<string>();
                            list.ToList().ForEach(x =>
                            {
                                if ((x != null) && x.Element.HasValue)
                                {
                                    var value = x.Element.ToString();
                                    result.Add(value);
                                }
                            });
                            return new PagedList<string>
                            {
                                PageIndex = pageIndex,
                                PageSize = pageSize,
                                Count = count,
                                List = result
                            };
                        }
                    }
                    return new PagedList<string>();
                }
            });
        }

        /// <summary>
        ///     根据值范围获取SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public List<string> GetSortedSetRangeByValue(string setId, long minValue, long maxValue)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<string>();
                    var list = cnn.GetDatabase().SortedSetRangeByValue(setId, minValue, maxValue).ToList();
                    if (list.Any())
                        list.ForEach(x =>
                        {
                            if (x.HasValue)
                                result.Add(x.ToString());
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取SortedSet长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public long GetSortedSetLength(string setId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetLength(setId);
                }
            });
        }

        /// <summary>
        ///     根据值范围儿取SortedSet长度
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public long GetSortedSetLength(string setId, double minValue, double maxValue)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetLength(setId, minValue, maxValue);
                }
            });
        }

        /// <summary>
        ///     获取某个SortedSet所在的序号
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public long? GetItemRankFromSortedSet(string setId, string item, string order = DefaultOrder)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase()
                        .SortedSetRank(setId, item,
                            order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending);
                }
            });
        }

        /// <summary>
        ///     获取某个SortedSet的score值
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public double? GetItemScoreFromSortedSet(string setId, string item)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetScore(setId, item);
                }
            });
        }

        /// <summary>
        ///     SortedSet计数器+
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public double SetSortedSetItemIncrement(string setId, string item, double score = 1)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetIncrement(setId, item, score);
                }
            });
        }

        /// <summary>
        ///     SortedSet计数器-
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        public double SortedSetItemDecrement(string setId, string item, double score = -1)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetDecrement(setId, item, score);
                }
            });
        }

        /// <summary>
        ///     称除SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItemFromSortedSet(string setId, string item)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetRemove(setId, item);
                }
            });
        }

        /// <summary>
        ///     移除某个序号范围的SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <returns></returns>
        public long RemoveByRankFromSortedSet(string setId, long fromRank, long toRank)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetRemoveRangeByRank(setId, fromRank, toRank);
                }
            });
        }

        /// <summary>
        ///     移除某个score范围的SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public long RemoveByScoreFromSortedSet(string setId, double minValue, double maxValue)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetRemoveRangeByScore(setId, minValue, maxValue);
                }
            });
        }

        #endregion

        #region Lists

        /// <summary>
        ///     进队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        public void Enqueue(string listId, string value)
        {
            DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    cnn.GetDatabase().ListLeftPush(listId, value);
                }
            });
        }

        /// <summary>
        ///     出队
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public string Dnqueue(string listId)
        {
            return DoWithRetry(() =>
            {
                string result;
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    result = cnn.GetDatabase().ListRightPop(listId);

                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }

                    // 在多写队列中， 比如F5后面挂了多个redis。
                    for (int i = 0; i < cnn.Pool.PoolSize - 1; i++)
                    {
                        result = cnn.GetDatabaseFromNextConnection().ListRightPop(listId);

                        if (!string.IsNullOrEmpty(result))
                        {
                            return result;
                        }
                    }

                    return null;
                }
            });
        }

        /// <summary>
        ///     进队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        public void Enqueue<T>(string listId, T t) where T : class, new()
        {
            DoWithRetry(() =>
            {
                var value = SerializeHelper.Serialize(t);
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    cnn.GetDatabase().ListLeftPush(listId, value);
                }
            });
        }

        /// <summary>
        ///     出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        public T Dnqueue<T>(string listId) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var json = cnn.GetDatabase().ListRightPop(listId);
                    if (json.IsNullOrEmpty)
                        return default(T);
                    return SerializeHelper.Deserialize<T>(json.ToString());
                }
            });
        }

        /// <summary>
        ///     获取队列元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public List<T> GetList<T>(string listId, long start = 0, long stop = -1) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var result = new List<T>();
                    var list = cnn.GetDatabase().ListRange(listId, start, stop).ToList();
                    if (list.Count > 0)
                        list.ForEach(x =>
                        {
                            if (x.HasValue)
                            {
                                var value = SerializeHelper.Deserialize<T>(x);
                                result.Add(value);
                            }
                        });
                    return result;
                }
            });
        }

        /// <summary>
        ///     获取队列长度
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public long QueueCount(string listId)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().ListLength(listId);
                }
            });
        }

        /// <summary>
        /// 从出队方向入队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        public void REnqueue(string listId, string value)
        {
            DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    cnn.GetDatabase().ListRightPush(listId, value);
                }
            });
        }
        /// <summary>
        /// 从出队方向入队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        public void REnqueue<T>(string listId, T t) where T : class, new()
        {
            DoWithRetry(() =>
            {
                var value = SerializeHelper.Serialize(t);
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    cnn.GetDatabase().ListRightPush(listId, value);
                }
            });
        }
        #endregion

        #region SubPush

        private SERedisConnection subcnn;

        /// <summary>
        ///     订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        public void Subscribe(string channelPrefix, Action<string, string> action)
        {
            DoWithRetry(() =>
            {
                if (subcnn == null)
                    subcnn = new SERedisConnection(_sectionName, _dbIndex);
                var pub = subcnn.GetSubscriber();
                pub.Subscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), (x, y) =>
                {
                    action(x.ToString(), y.ToString());
                });
            });
        }

        /// <summary>
        ///     取消订阅
        /// </summary>
        /// <param name="channelPrefix"></param>
        public void Unsubscribe(string channelPrefix)
        {
            DoWithRetry(() =>
            {
                if (subcnn == null)
                    subcnn = new SERedisConnection(_sectionName, _dbIndex);
                var pub = subcnn.GetSubscriber();
                pub.Unsubscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto));
                subcnn.Dispose();
            });
        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="msg"></param>
        public void Publish(string channelPrefix, string msg)
        {
            DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var pub = cnn.GetSubscriber();
                    pub.PublishAsync(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), msg);
                }
            });
        }

        #endregion
    }
}