/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisOperationForSortedSet
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/3 15:49:14
*描述：
*=====================================================================
*修改时间：2020/6/3 15:49:14
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Data;
using Wenli.Drive.Redis.Extends;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisOperationForSortedSet
    /// </summary>
    public partial class SERedisOperation : IRedisOperation
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
                return _cnn.GetDatabase().SortedSetAdd(setId, item, score);
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
                return _cnn.GetDatabase()
                         .SortedSetRangeByRank(setId, fromRank, toRank,
                             order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                         .ToList().ConvertTo();
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
                return _cnn.GetDatabase()
                        .SortedSetRangeByRankWithScores(setId, fromRank, toRank,
                            order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending)
                        .ToList().ConvertTo();
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
                var count = _cnn.GetDatabase().SortedSetLength(setid); //此处无法正确获取数量
                if (count > 0)
                {
                    var list = _cnn.GetDatabase()
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
                var count = _cnn.GetDatabase().SortedSetLength(setid, minScore, maxScore);
                if (count > 0)
                {
                    var list = _cnn.GetDatabase().SortedSetRangeByScore(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending, (pageIndex - 1) * pageSize, pageSize);

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
                return _cnn.GetDatabase().SortedSetRangeByScoreWithScores(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending).ConvertTo();
            });
        }

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
        public PagedDictionary<string, double> GetSortedSetRangeBySocreWithScore(string setid, double minScore, double maxScore, int pageIndex, int pageSize, bool orderBy = true)
        {
            return DoWithRetry(() =>
            {
                var len = _cnn.GetDatabase().SortedSetLength(setid, minScore, maxScore, Exclude.Stop);
                return _cnn.GetDatabase().SortedSetRangeByScoreWithScores(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending, skip: (pageIndex - 1) * pageSize, take: pageSize).ConvertTo(pageIndex, pageSize, len);
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
                return _cnn.GetDatabase().SortedSetRangeByScore(setid, minScore, maxScore, Exclude.Stop, orderBy ? Order.Ascending : Order.Descending).ConvertTo();
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
                return _cnn.GetDatabase().SortedSetRangeByValue(setId, minValue, maxValue, Exclude.Stop).ConvertTo();
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
                return _cnn.GetDatabase().SortedSetLength(setId);
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
                return _cnn.GetDatabase().SortedSetLength(setId, minScore, maxScore, Exclude.Stop);
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
                return _cnn.GetDatabase()
                        .SortedSetRank(setId, item,
                            order == Order.Descending.ToString().ToLower() ? Order.Descending : Order.Ascending);
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
                return _cnn.GetDatabase().SortedSetScore(setId, item);
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
                return _cnn.GetDatabase().SortedSetIncrement(setId, item, score);
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
                return _cnn.GetDatabase().SortedSetDecrement(setId, item, score);
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
                return _cnn.GetDatabase().SortedSetRemove(setId, item);
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
                return _cnn.GetDatabase().SortedSetRemoveRangeByRank(setId, fromRank, toRank);
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
                return _cnn.GetDatabase().SortedSetRemoveRangeByScore(setId, minValue, maxValue, Exclude.Stop);
            });
        }

        #endregion
    }
}
