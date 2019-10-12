/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisHashesOperation
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2019/10/12 14:28:10
*描述：
*=====================================================================
*修改时间：2019/10/12 14:28:10
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisHashesOperation
    /// </summary>
    public partial class SERedisOperation
    {

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
        /// 批量移除hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public long HashDelete(string hashId, string[] keys)
        {
            return DoWithRetry(() =>
            {
                var hkeys = new RedisValue[keys.Length];

                for (int i = 0; i < keys.Length; i++)
                {
                    hkeys[i] = keys[i];
                }
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().HashDelete(hashId, hkeys);
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
        /// 从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Dictionary<string, T> GetValuesDicFromHash<T>(string hashId, List<string> keys) where T : class, new()
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var dic = new Dictionary<string, T>();
                    if ((keys != null) && (keys.Count > 0))
                    {
                        var rv = new RedisValue[keys.Count];
                        for (var i = 0; i < keys.Count; i++)
                            rv[i] = keys[i];
                        var vlts = cnn.GetDatabase().HashGet(hashId, rv);
                        for (int i = 0; i < vlts.Length; i++)
                            if (!string.IsNullOrEmpty(vlts[i]))
                            {
                                var obj = SerializeHelper.Deserialize<T>(vlts[i]);
                                if (obj != null)
                                    dic.Add(rv[i], obj);
                            }
                    }
                    return dic;
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

    }
}
