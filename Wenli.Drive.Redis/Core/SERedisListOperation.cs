/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisListOperation
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2019/10/12 15:27:27
*描述：
*=====================================================================
*修改时间：2019/10/12 15:27:27
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisListOperation
    /// </summary>
    public partial class SERedisOperation
    {
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
        ///     进队
        /// </summary>
        /// <param name="listId"></param>
        /// <param name="values"></param>
        public long Enqueue(string listId, List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0;
            }

            return DoWithRetry(() =>
            {
                using (var cnn = new SERedisConnection(_sectionName, _dbIndex))
                {
                    return cnn.GetDatabase().ListLeftPush(listId, values.Select(r => (RedisValue)r).ToArray());
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
    }
}
