/*
* 描述： 详细描述类能干什么
* 创建人：wenli
* 创建时间：2017/2/6 18:10:25
*/
/*
*修改人：wenli
*修改时间：2017/2/6 18:10:25
*修改内容：xxxxxxx
*/

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wenli.Drive.Redis.Data;
using Wenli.Drive.Redis.Tool;

namespace RedisDriveTest.Fun1
{
    public class ClusterSHelper:IDisposable
    {
        private static ConnectionMultiplexer _cnn;

        private static IDatabase _db;

        private string _cnnStr = string.Empty;

        public ClusterSHelper(string cnnStr)
        {
            _cnnStr = cnnStr;

            _cnn = ConnectionMultiplexer.Connect(_cnnStr);

            _db = _cnn.GetDatabase();
        }

        private static readonly string _prex = "Test_";

        private const string DefaultOrder = "desc";

        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns></returns>
        public string GetServerInfo()
        {
            var info = string.Empty;
            try
            {
                var eps = _cnn.GetEndPoints(true);
                if (eps != null && eps.Length > 0)
                {
                    foreach (var ep in _cnn.GetEndPoints(true))
                    {
                        info += string.Format("{0}{1}{2}{1}", ep.AddressFamily.ToString(),Environment.NewLine, _cnn.GetServer(ep).InfoRaw());
                    }
                }
            }
            catch { }
            return info;
        }

        #region Keys

        /// <summary>
        ///     是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {

            return _db.KeyExists(key);
        }

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, DateTime datetime)
        {

            return _db.KeyExpire(key, datetime);
        }

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, int timeout = 0)
        {
            if (timeout > 0)
            {
                return _db.KeyExpire(key, DateTime.Now.AddSeconds(timeout));
            }
            return false;

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
            var bResult = false;
            if (timeout > 0)
            {
                bResult = _db.StringSet(key, value, new TimeSpan(0, 0, timeout));
            }
            else
            {
                _db.StringSet(key, value);
            }
            return bResult;
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
            return _db.StringSet(key, value, expire);

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
            return _db.StringSet(key, value, when: When.NotExists);

        }

        /// <summary>
        ///     获取key的同时set该Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string StringGetSet(string key, string value)
        {
            return _db.StringGetSet(key, value);

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
            List<string> keys = new List<string>();

            var result = _db.ScriptEvaluate(LuaScript.Prepare("return  redis.call('KEYS', '*')"), CommandFlags.PreferSlave);

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
        }
        /// <summary>
        ///     获取key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string StringGet(string key)
        {
            return _db.StringGet(key);

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

            return StringSet(key, SerializeHelper.Serialize(t), timeout);

        }

        /// <summary>
        ///     获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T StringGet<T>(string key) where T : class, new()
        {

            var str = StringGet(key);
            if (!string.IsNullOrWhiteSpace(str))
                return SerializeHelper.Deserialize<T>(str);
            return default(T);

        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> GetValues<T>(List<string> keys) where T : class, new()
        {
            return GetValues<T>(keys.ToArray());

        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> GetValues<T>(string[] keys) where T : class, new()
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

        }

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> GetValues(string[] keys)
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

        }

        /// <summary>
        ///     移除key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyDelete(string key)
        {
            return _db.KeyDelete(key);

        }

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        public void KeysDelete(string[] keys)
        {

            for (var i = 0; i < keys.Length; i++)
                KeyDelete(keys[i]);

        }

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        public void KeysDelete(List<string> keys)
        {
            KeysDelete(keys.ToArray());

        }

        /// <summary>
        ///     重命名key
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool KeyRename(string oldKey, string newKey)
        {
            return _db.KeyRename(oldKey, newKey);

        }

        /// <summary>
        ///     key计数器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double StringIncrement(string key, double value)
        {
            return _db.StringIncrement(key, value);

        }

        /// <summary>
        ///     追加value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long StringAppend(string key, string value)
        {
            return _db.StringAppend(value, value, CommandFlags.None);

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
            return _db.HashExists(hashId, key);
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
            return _db.HashSet(hashId, key, SerializeHelper.Serialize(t));

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
            return _db.HashSet(hashId, key, val);

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

            return _db.HashSet(hashId, key, value, When.NotExists);

        }

        /// <summary>
        ///     移除hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HashDelete(string hashId, string key)
        {
            return _db.HashDelete(hashId, key);

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

            return _db.HashIncrement(hashId, key, value);

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
            string str = _db.HashGet(hashId, key);
            if (!string.IsNullOrWhiteSpace(str))
                return SerializeHelper.Deserialize<T>(str);
            return default(T);

        }

        /// <summary>
        ///     获取hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string HashGet(string hashId, string key)
        {
            return _db.HashGet(hashId, key).ToString();

        }

        /// <summary>
        ///     获取hash数量
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public long GetHashCount(string hashId)
        {
            return _db.HashLength(hashId);

        }

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> HashGetAll(string hashId)
        {

            var result = new List<string>();
            var list = _db.HashValues(hashId).ToList();
            if ((list != null) && (list.Count > 0))
                list.ForEach(x =>
                {
                    if (x.HasValue)
                        result.Add(x.ToString());
                });
            return result;

        }

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(string hashId) where T : class, new()
        {

            var result = new List<T>();
            var list = _db.HashValues(hashId).ToList();
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

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public Dictionary<string, T> HashGetAllDic<T>(string hashId) where T : class, new()
        {

            var result = new Dictionary<string, T>();
            var list = _db.HashGetAll(hashId).ToList();

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

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public Dictionary<string, string> HashGetAllDic(string hashId)
        {

            var result = new Dictionary<string, string>();
            var list = _db.HashGetAll(hashId).ToList();

            if (list.Count > 0)
                list.ForEach(x =>
                {
                    result.Add(x.Name, x.Value);
                });
            return result;

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

            var result = new List<T>();
            pageIndex = pageIndex > 0 ? pageIndex : 1;
            pageSize = pageSize > 0 ? pageSize : 20;
            var list =
                _db.HashValues(hashID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
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

        /// <summary>
        ///     获取指定的全部hashlist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashIds"></param>
        /// <returns></returns>
        public List<T> HashGetAll<T>(List<string> hashIds) where T : class, new()
        {
            var result = new List<T>();
            foreach (var hashId in hashIds)
            {
                var list = HashGetAll<T>(hashId);
                if ((list != null) && (list.Count > 0))
                    result.AddRange(list);
            }
            return result;

        }

        /// <summary>
        ///     获取某个hashid下面全部keys
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        public List<string> GetHashKeys(string hashId)
        {
            var result = new List<string>();
            var list = _db.HashKeys(hashId).ToList();
            if (list.Count > 0)
                list.ForEach(x =>
                {
                    result.Add(x.ToString());
                });
            return result;

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
            var list = new List<T>();
            if ((keys != null) && (keys.Count > 0))
            {
                var rv = new RedisValue[keys.Count];
                for (var i = 0; i < keys.Count; i++)
                    rv[i] = keys[i];
                var vlts = _db.HashGet(hashId, rv);
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

        /// <summary>
        ///     从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<string> GetValuesFromHash(string hashId, List<string> keys)
        {
            var list = new List<string>();
            if ((keys != null) && (keys.Count > 0))
            {
                var rv = new RedisValue[keys.Count];
                for (var i = 0; i < keys.Count; i++)
                    rv[i] = keys[i];
                var vlts = _db.HashGet(hashId, rv);
                foreach (var val in vlts)
                    if (!string.IsNullOrEmpty(val) && val.HasValue)
                        list.Add(val);
            }
            return list;

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

            return _db.SetAdd(setId, val);

        }

        /// <summary>
        ///     是否存在于set中
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetContains(string setId, string val)
        {

            return _db.SetContains(setId, val);

        }

        /// <summary>
        ///     获取某个key下面全部的value
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public List<string> SetMembers(string setId)
        {

            var values = _db.SetMembers(setId);
            var list = new List<string>();
            if ((values != null) && (values.Count() > 0))
                foreach (var sitem in values)
                    list.Add(sitem.ToString());
            return list;

        }

        /// <summary>
        ///     移除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool SetRemove(string setId, string val)
        {

            return _db.SetRemove(setId, val);

        }

        /// <summary>
        ///     批量删除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="vals"></param>
        public void SetsRemove(string setId, string[] vals)
        {

            for (var i = 0; i < vals.Length; i++)
                _db.SetRemove(setId, vals[i]);

        }

        /// <summary>
        ///     返回指定set长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public long SetLength(string setId)
        {

            return _db.SetLength(setId);

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

            var value = GetItemScoreFromSortedSet(setId, item);
            if (value != null)
                return true;
            return false;

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

            return _db.SortedSetAdd(setId, item, score);

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

            var result = new List<string>();
            var list =
                _db.SortedSetRangeByRank(setId, fromRank, toRank,
                        order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                    .ToList();
            if (list.Any())
                list.ForEach(x =>
                {
                    result.Add(x.ToString());
                });
            return result;

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

            var result = new Dictionary<string, double>();
            var list =
                _db.SortedSetRangeByRankWithScores(setId, fromRank, toRank,
                        order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                    .ToList();
            if (list.Any())
                list.ForEach(x =>
                {
                    result.Add(x.Element, x.Score);
                });
            return result;

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

            var count = _db.SortedSetLength(setid, min, max);
            if (count > 0)
            {
                var list = _db.SortedSetRangeByScoreWithScores(setid, min, max, Exclude.None,
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

        /// <summary>
        ///     根据值范围获取SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public List<string> GetSortedSetRangeByValue(string setId, long minValue, long maxValue)
        {

            var result = new List<string>();
            var list = _db.SortedSetRangeByValue(setId, minValue, maxValue).ToList();
            if (list.Any())
                list.ForEach(x =>
                {
                    if (x.HasValue)
                        result.Add(x.ToString());
                });
            return result;

        }

        /// <summary>
        ///     获取SortedSet长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        public long GetSortedSetLength(string setId)
        {

            return _db.SortedSetLength(setId);

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

            return _db.SortedSetLength(setId, minValue, maxValue);

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

            return _db
                .SortedSetRank(setId, item,
                    order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending);

        }

        /// <summary>
        ///     获取某个SortedSet的score值
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public double? GetItemScoreFromSortedSet(string setId, string item)
        {

            return _db.SortedSetScore(setId, item);

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

            return _db.SortedSetIncrement(setId, item, score);

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

            return _db.SortedSetDecrement(setId, item, score);

        }

        /// <summary>
        ///     称除SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveItemFromSortedSet(string setId, string item)
        {

            return _db.SortedSetRemove(setId, item);

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

            return _db.SortedSetRemoveRangeByRank(setId, fromRank, toRank);

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

            return _db.SortedSetRemoveRangeByScore(setId, minValue, maxValue);

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


            _db.ListLeftPush(listId, value);

        }

        /// <summary>
        ///     出队
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public string Dnqueue(string listId)
        {

            string result;

            result = _db.ListRightPop(listId);

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return null;

        }

        /// <summary>
        ///     进队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        public void Enqueue<T>(string listId, T t) where T : class, new()
        {

            var value = SerializeHelper.Serialize(t);


            _db.ListLeftPush(listId, value);

        }

        /// <summary>
        ///     出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        public T Dnqueue<T>(string listId) where T : class, new()
        {

            var json = _db.ListRightPop(listId);
            if (json.IsNullOrEmpty)
                return default(T);
            return SerializeHelper.Deserialize<T>(json.ToString());

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
            var result = new List<T>();
            var list = _db.ListRange(listId, start, stop).ToList();
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

        /// <summary>
        ///     获取队列长度
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        public long QueueCount(string listId)
        {

            return _db.ListLength(listId);

        }

        #endregion

        #region SubPush

        /// <summary>
        ///     订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        public void Subscribe(string channelPrefix, Action<RedisChannel, RedisValue> action)
        {
            var pub = _cnn.GetSubscriber();
            pub.Subscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), action);

        }

        /// <summary>
        ///     取消订阅
        /// </summary>
        /// <param name="channelPrefix"></param>
        public void Unsubscribe(string channelPrefix)
        {
            var pub = _cnn.GetSubscriber();
            pub.Unsubscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto));
            _cnn.Dispose();

        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="msg"></param>
        public void Publish(string channelPrefix, string msg)
        {

            var pub = _cnn.GetSubscriber();
            pub.PublishAsync(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), msg);

        }

        #endregion

        public void Dispose()
        {
            if (_cnn != null)
                _cnn.Close();
        }
    }
}
