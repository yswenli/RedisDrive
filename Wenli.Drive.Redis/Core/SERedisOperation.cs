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
using System.Threading.Tasks;
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
        private const string DefaultOrder = "descending";

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
            if ((_busyRetry < 0) || (_busyRetry > 10000))
                throw new Exception("重试次数有误，请输入0-10000之间整数");
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
                        retryCountMsg = string.Format("TimeoutException Redis<T> {0}操作超时，等待随后重试。当前已经重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0}连接异常，等待随后重试。当前已重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }
                    else if (ex is RedisServerException && ex.Message.Contains("MOVED"))
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0} MOVED 异常，等待随后重试。当前已重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }
                    else
                    {
                        throw ex;
                    }


                    if (counter > _busyRetry)
                    {
                        throw ex;  // 大于重试次数，将直接抛出去
                    }

                    var actionSpan = counter * _busyRetryWaitMS;

                    if (actionSpan >= 10 * 1000) actionSpan = 10 * 1000;

                    Thread.Sleep(actionSpan);

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
                        retryCountMsg = string.Format("TimeoutException Redis<T> {0}操作超时，等待随后重试。当前已经重试：{1};ex:{2}", action.Method.Name, counter, ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0}连接异常，等待随后重试。当前已重试：{1};ex:{2}", action.Method.Name, counter, ex.InnerException.Message);
                    }
                    else if (ex is RedisServerException && ex.Message.Contains("MOVED"))
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0} MOVED 异常，等待随后重试。当前已重试：{1};ex:{2}", action.Method.Name, counter, ex.Message);
                    }
                    else
                    {
                        throw ex;
                    }


                    if (counter > _busyRetry)
                    {
                        throw ex;  // 大于重试次数，将直接抛出去
                    }

                    var actionSpan = counter * _busyRetryWaitMS;

                    if (actionSpan >= 10 * 1000) actionSpan = 10 * 1000;

                    Thread.Sleep(actionSpan);
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
    }
}