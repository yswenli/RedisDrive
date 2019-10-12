/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisZSetOperation
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2019/10/12 14:29:59
*描述：
*=====================================================================
*修改时间：2019/10/12 14:29:59
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Data;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisZSetOperation
    /// </summary>
    public partial class SERedisOperation
    {

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
        /// <param name="min">最小score</param>
        /// <param name="max">最大score</param>
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
                    var count = cnn.GetDatabase().SortedSetLength(setid); //此处无法正确获取数量
                    if (count > 0)
                    {
                        var list = cnn.GetDatabase()
                            .SortedSetRangeByScoreWithScores(setid, min, max, Exclude.Stop,
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
        /// 获取SortedSet集合区间
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public PagedList<string> GetSortedSetRangeByRankBySocre(string setid, double minScore, double maxScore, int pageIndex = 1,
            int pageSize = 20, bool orderBy = true)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    var count = cnn.GetDatabase().SortedSetLength(setid, minScore, maxScore);
                    if (count > 0)
                    {
                        var list = cnn.GetDatabase().SortedSetRangeByScore(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending, (pageIndex - 1) * pageSize, pageSize);

                        if (list != null && list.Any())
                        {
                            var result = new List<string>();

                            foreach (var item in list)
                            {
                                result.Add(item.ToString());
                            }

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
        /// 获取zset
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public Dictionary<string, double> GetSortedSetRangeBySocreWithScore(string setid, double minScore, double maxScore, bool orderBy = true)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    Dictionary<string, double> list = null;

                    var data = cnn.GetDatabase().SortedSetRangeByScoreWithScores(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending);

                    if (data != null && data.Any())
                    {
                        list = new Dictionary<string, double>();

                        foreach (var item in data)
                        {
                            list.Add(item.Element.ToString(), item.Score);
                        }
                    }

                    return list;
                }
            });
        }

        /// <summary>
        /// 获取zset
        /// </summary>
        /// <param name="setid"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public List<string> GetSortedSetRangeBySocre(string setid, double minScore, double maxScore, bool orderBy = true)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    List<string> list = null;

                    var data = cnn.GetDatabase().SortedSetRangeByScore(setid, minScore, maxScore, Exclude.None, orderBy ? Order.Ascending : Order.Descending);

                    if (data != null && data.Any())
                    {
                        list = new List<string>();

                        foreach (var item in data)
                        {
                            list.Add(item.ToString());
                        }

                    }

                    return list;
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
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <returns></returns>
        public long GetSortedSetLength(string setId, double minScore, double maxScore)
        {
            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().SortedSetLength(setId, minScore, maxScore);
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
        public double SortedSetItemDecrement(string setId, string item, double score = 1)
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

    }
}
