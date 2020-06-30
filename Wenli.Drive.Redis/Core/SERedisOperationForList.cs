/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisOperationForList
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/3 15:50:38
*描述：
*=====================================================================
*修改时间：2020/6/3 15:50:38
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Interface;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisOperationForList
    /// </summary>
    public partial class SERedisOperation : IRedisOperation
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
                _cnn.GetDatabase().ListLeftPush(listId, value);
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
                return _cnn.GetDatabase().ListLeftPush(listId, values.Select(r => (RedisValue)r).ToArray());
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
                string result = _cnn.GetDatabase().ListRightPop(listId);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                result = _cnn.GetDatabase().ListRightPop(listId);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return null;
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
                _cnn.GetDatabase().ListLeftPush(listId, value);
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
                var json = _cnn.GetDatabase().ListRightPop(listId);
                if (json.IsNullOrEmpty)
                    return default(T);
                return SerializeHelper.Deserialize<T>(json.ToString());
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
                var result = new List<T>();
                var list = _cnn.GetDatabase().ListRange(listId, start, stop).ToList();
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
                return _cnn.GetDatabase().ListLength(listId);
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
                _cnn.GetDatabase().ListRightPush(listId, value);
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
                _cnn.GetDatabase().ListRightPush(listId, value);
            });
        }

        #endregion
    }
}
