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
using Wenli.Drive.Redis.Data;

namespace Wenli.Drive.Redis.Interface
{
    /// <summary>
    ///     redis操作接口
    /// </summary>
    public interface IRedisOperation
    {
        /// <summary>
        /// ping
        /// </summary>
        /// <returns></returns>
        TimeSpan Ping();

        /// <summary>
        /// RedisBatcher
        /// </summary>
        /// <returns></returns>
        RedisBatcher CreateBatcher();

        #region Keys
        /// <summary>
        /// 获取keys
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <param name="patten"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<string> Keys(int dbIndex = -1, string patten = "*", int count = 20);

        /// <summary>
        ///     是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyExists(string key);

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        bool KeyExpire(string key, DateTime datetime);

        /// <summary>
        ///     设置key过期时间
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout">过期时间（秒）</param>
        /// <returns></returns>
        bool KeyExpire(string key, int timeout = 0);

        /// <summary>
        ///     设置key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout">过期时间（秒）</param>
        /// <returns></returns>
        bool StringSet(string key, string value, int timeout = 0);

        /// <summary>
        ///     设置key，同时设置该key的超时值
        /// </summary>
        /// <param name="key">设置的Key</param>
        /// <param name="value">设置的Value</param>
        /// <param name="expire">key的超时时常</param>
        /// <returns></returns>
        bool StringSet(string key, string value, TimeSpan expire);

        /// <summary>
        /// 设置一个值，仅在不存在的时候设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool StringSetIfNotExists(string key, string value);

        /// <summary>
        ///  设置一个值，仅在不存在的时候设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ts">超时时间</param>
        /// <returns></returns>
        bool StringSetIfNotExists(string key, string value, TimeSpan ts);

        /// <summary>
        ///     获取key的同时set该Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string StringGetSet(string key, string value);

        /// <summary>
        /// 获取全部keys
        /// </summary>
        /// <param name="patten"></param>
        /// <returns></returns>
        List<string> StringGetKeys(string patten = "*");

        /// <summary>
        /// 获取全部keys
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <param name="patten"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<string> StringGetKeys(int pageSize, int dbIndex = -1, string patten = "*");

        /// <summary>
        /// 遍历获取所有keys
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="patten"></param>
        /// <param name="size"></param>
        void StringGetKeys(Action<List<string>> callback, string patten = "*", int size = 1000);

        /// <summary>
        ///     获取key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string StringGet(string key);

        /// <summary>
        ///     设置key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <param name="timeout">过期时间（秒）</param>
        /// <returns></returns>
        bool StringSet<T>(string key, T t, int timeout = 0) where T : class, new();

        /// <summary>
        ///     获取key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T StringGet<T>(string key) where T : class, new();

        /// <summary>
        /// 批量获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<T> BatchStringGet<T>(List<string> keys) where T : class, new();

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<T> GetValues<T>(string[] keys) where T : class, new();

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<string> GetValues(string[] keys);

        /// <summary>
        ///     获取kv列表集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<T> GetValues<T>(List<string> keys) where T : class, new();

        /// <summary>
        ///     移除key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool KeyDelete(string key);

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        void KeysDelete(string[] keys);

        /// <summary>
        ///     批量删除
        /// </summary>
        /// <param name="keys"></param>
        void KeysDelete(List<string> keys);

        /// <summary>
        ///     重命名key
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        bool KeyRename(string oldKey, string newKey);

        /// <summary>
        ///     key计数器(加上相应的value)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        double StringIncrement(string key, double value);

        /// <summary>
        ///     key计数器(减去相应的value)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        double StringDecrement(string key, double value);

        /// <summary>
        ///     追加value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long StringAppend(string key, string value);

        #endregion

        #region Hashes

        /// <summary>
        ///     检查hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HashExists(string hashId, string key);

        /// <summary>
        ///     设置hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        bool HashSet<T>(string hashId, string key, T t) where T : class, new();

        /// <summary>
        ///     设置hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        bool HashSet(string hashId, string key, string val);


        /// <summary>
        ///     在一个hash中设置一个key - value，仅仅当不存在的时候才设置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        bool HashSetIfNotExists(string hashId, string key, string t);

        /// <summary>
        ///     移除hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HashDelete(string hashId, string key);

        /// <summary>
        /// 批量移除hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        long HashDelete(string hashId, string[] keys);

        /// <summary>
        ///     hash计数器
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        long HashIncrement(string hashId, string key, long value = 1);

        /// <summary>
        ///     获取hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        T HashGet<T>(string hashId, string key) where T : class, new();

        /// <summary>
        ///     获取hash数量
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        long GetHashCount(string hashId);

        /// <summary>
        ///     获取hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string HashGet(string hashId, string key);

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        List<T> HashGetAll<T>(string hashId) where T : class, new();

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <returns></returns>
        Dictionary<string, T> HashGetAllDic<T>(string hashId) where T : class, new();

        /// <summary>
        ///     获取全部的hashkey,hashvalue字典
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        Dictionary<string, string> HashGetAllDic(string hashId);

        /// <summary>
        ///     分页获取某个hashid下面hash
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashID"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        List<T> HashGetAll<T>(string hashID, int pageIndex, int pageSize) where T : class, new();

        /// <summary>
        ///     获取某个hashid下面全部hash
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        List<string> HashGetAll(string hashId);

        /// <summary>
        ///     获取指定的全部hashlist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashIds"></param>
        /// <returns></returns>
        List<T> HashGetAll<T>(List<string> hashIds) where T : class, new();

        /// <summary>
        ///     获取某个hashid下面全部keys
        /// </summary>
        /// <param name="hashId"></param>
        /// <returns></returns>
        List<string> GetHashKeys(string hashId);

        /// <summary>
        ///     从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<T> GetValuesFromHash<T>(string hashId, List<string> keys) where T : class, new();

        /// <summary>
        ///     从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        List<string> GetValuesFromHash(string hashId, List<string> keys);

        /// <summary>
        /// 从指定hashid,keys中获取指定hash集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashId"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        Dictionary<string, T> GetValuesDicFromHash<T>(string hashId, List<string> keys) where T : class, new();
        #endregion

        #region Set

        /// <summary>
        ///     添加set值
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool SetAdd(string setId, string val);

        /// <summary>
        ///     是否存在于set中
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool SetContains(string setId, string val);

        /// <summary>
        ///     获取某个key下面全部的value
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        List<string> SetMembers(string setId);

        /// <summary>
        ///     移除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        bool SetRemove(string setId, string val);

        /// <summary>
        ///     批量删除set
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="vals"></param>
        void SetsRemove(string setId, string[] vals);

        /// <summary>
        ///     返回指定set长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        long SetLength(string setId);

        #endregion

        #region Sorted Sets

        /// <summary>
        ///     检查SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool SortedSetItemIsExist(string setId, string item);

        /// <summary>
        ///     添加SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        bool SortedSetAdd(string setId, string item, double score);

        /// <summary>
        ///     查询SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        List<string> GetSortedSetRangeByRank(string setId, long fromRank, long toRank, string order = "descending");

        /// <summary>
        ///     查询SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        Dictionary<string, double> GetSortedSetRangeByRankWithScores(string setId, long fromRank, long toRank,
            string order = "descending");

        /// <summary>
        ///     根据score分页查询SortedSet集合
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        PagedList<string> GetSortedSetRangeByRankWithSocres(string setid, long min, long max, int pageIndex = 1,
            int pageSize = 20, bool orderBy = true);

        /// <summary>
        /// 根据score分页查询SortedSet集合
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        PagedList<string> GetSortedSetRangeByRankBySocre(string setid, double minScore, double maxScore, int pageIndex = 1,
            int pageSize = 20, bool orderBy = true);

        /// <summary>
        /// 获取zset
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Dictionary<string, double> GetSortedSetRangeBySocreWithScore(string setid, double minScore, double maxScore, bool orderBy = true);

        /// <summary>
        /// 获取zset
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        PagedDictionary<string, double> GetSortedSetRangeBySocreWithScore(string setid, double minScore, double maxScore, int pageIndex, int pageSize, bool orderBy = true);

        /// <summary>
        /// 获取zset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        List<string> GetSortedSetRangeBySocre(string setid, double minScore, double maxScore, bool orderBy = true);

        /// <summary>
        ///     根据值范围获取SortedSet集合
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        List<string> GetSortedSetRangeByValue(string setId, long minValue, long maxValue);

        /// <summary>
        ///     获取SortedSet长度
        /// </summary>
        /// <param name="setId"></param>
        /// <returns></returns>
        long GetSortedSetLength(string setId);

        /// <summary>
        ///     根据值范围儿取SortedSet长度
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        long GetSortedSetLength(string setId, double minValue, double maxValue);

        /// <summary>
        ///     获取某个SortedSet所在的序号
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        long? GetItemRankFromSortedSet(string setId, string item, string order = "descending");

        /// <summary>
        ///     获取某个SortedSet的score值
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        double? GetItemScoreFromSortedSet(string setId, string item);

        /// <summary>
        ///     SortedSet计数器+
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        double SetSortedSetItemIncrement(string setId, string item, double score = 1);

        /// <summary>
        ///     SortedSet计数器-
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <param name="score"></param>
        /// <returns></returns>
        double SortedSetItemDecrement(string setId, string item, double score = 1);

        /// <summary>
        ///     称除SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        bool RemoveItemFromSortedSet(string setId, string item);

        /// <summary>
        ///     移除某个序号范围的SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="fromRank"></param>
        /// <param name="toRank"></param>
        /// <returns></returns>
        long RemoveByRankFromSortedSet(string setId, long fromRank, long toRank);

        /// <summary>
        ///     移除某个score范围的SortedSet
        /// </summary>
        /// <param name="setId"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        long RemoveByScoreFromSortedSet(string setId, double minValue, double maxValue);

        #endregion

        #region Lists

        /// <summary>
        ///     进队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        void Enqueue(string listId, string value);

        /// <summary>
        ///     批量进队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="values"></param>
        long Enqueue(string listId, List<string> values);

        /// <summary>
        ///     出队
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        string Dnqueue(string listId);

        /// <summary>
        ///     进队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        void Enqueue<T>(string listId, T t) where T : class, new();

        /// <summary>
        ///     出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <returns></returns>
        T Dnqueue<T>(string listId) where T : class, new();

        /// <summary>
        ///     获取队列元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        List<T> GetList<T>(string listId, long start = 0, long stop = -1) where T : class, new();

        /// <summary>
        ///     获取队列长度
        /// </summary>
        /// <param name="listId"></param>
        /// <returns></returns>
        long QueueCount(string listId);

        /// <summary>
        /// 从出队方向入队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="value"></param>
        void REnqueue(string listId, string value);

        /// <summary>
        /// 从出队方向入队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listId"></param>
        /// <param name="t"></param>
        void REnqueue<T>(string listId, T t) where T : class, new();

        #endregion

        #region SubPush

        /// <summary>
        ///     订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        //void Subscribe(string channelPrefix, Action<RedisChannel, RedisValue> action);

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        void SubscribeWithChannel(string channelPrefix, Action<string, string> action);

        /// <summary>
        ///     取消订阅
        /// </summary>
        /// <param name="channelPrefix"></param>
        void Unsubscribe(string channelPrefix);

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="msg"></param>
        void Publish(string channelPrefix, string msg);

        #endregion

        #region lock
        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout">过期时间（毫秒）</param>
        /// <param name="rolling"></param>
        /// <returns></returns>
        bool Lock(string key, int timeout = 30 * 1000, int rolling = 500);
        /// <summary>
        /// 移除锁
        /// </summary>
        /// <param name="key"></param>
        void UnLock(string key = "");
        #endregion
    }
}
