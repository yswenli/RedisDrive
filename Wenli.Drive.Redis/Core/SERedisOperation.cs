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
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using Wenli.Drive.Redis.Interface;

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

        private readonly int _busyRetry = 1000;

        private readonly int _busyRetryWaitMS = 200;

        private readonly int _dbIndex = -1;

        private readonly string _sectionName;

        SERedisConnection _cnn = null;

        private readonly bool _waitForFix;

        /// <summary>
        /// SERedis操作类
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="dbIndex"></param>
        /// <param name="waitForFix"></param>
        /// <param name="busyRetry"></param>
        /// <param name="busyRetryWaitMS"></param>
        public SERedisOperation(string sectionName, int dbIndex = -1, bool waitForFix = true, int busyRetry = 1000, int busyRetryWaitMS = 200)
        {
            _sectionName = sectionName;
            _dbIndex = dbIndex;
            _waitForFix = waitForFix;
            _busyRetry = busyRetry;
            _busyRetryWaitMS = busyRetryWaitMS;
            if (_busyRetry < 0)
                _busyRetry = 3;
            if (_busyRetryWaitMS < 100)
                _busyRetryWaitMS = 100;

            _cnn = new SERedisConnection(_sectionName, _dbIndex);
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
                    if (!_waitForFix)
                    {
                        throw ex;
                    }

                    counter++;

                    string retryCountMsg = string.Empty;

                    if (ex is TimeoutException)
                    {
                        retryCountMsg = string.Format("TimeoutException Redis<T> {0}操作超时，等待随后重试。当前已经重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0}连接异常，等待随后重试。当前已重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                        _cnn.TryFixConnection();

                    }
                    else if (ex is RedisServerException && ex.Message.Contains("MOVED"))
                    {
                        retryCountMsg = string.Format("RedisServerException Redis<T> {0} MOVED 异常，等待随后重试。当前已重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                        _cnn.TryFixConnection();
                    }
                    else
                    {
                        retryCountMsg = string.Format("Exception Redis<T> {0}操作异常，等待随后重试。当前已经重试：{1};ex:{2}", func.Method.Name, counter, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    }

                    //LogCom.WriteErrLog("SERedisOperation.DoWithRetry." + func.Method.Name, new Exception(retryCountMsg));

                    if (counter > _busyRetry)
                    {
                        //LogCom.WriteErrLog("SERedisOperation.DoWithRetry." + func.Method.Name, new Exception("已超出配置的连接次数!"));
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
                    if (!_waitForFix)
                    {
                        throw ex;
                    }

                    counter++;

                    string retryCountMsg = string.Empty;

                    if (ex is TimeoutException)
                    {
                        retryCountMsg = string.Format("TimeoutException Redis<T> {0}操作超时，等待随后重试。当前已经重试：{1};ex:{2}", action.Method.Name, counter, ex.Message);
                    }
                    else if (ex is RedisConnectionException)
                    {
                        retryCountMsg = string.Format("RedisConnectionException Redis<T> {0}连接异常，等待随后重试。当前已重试：{1};ex:{2}", action.Method.Name, counter, ex.InnerException.Message);

                        _cnn.TryFixConnection();
                    }
                    else if (ex is RedisServerException && ex.Message.Contains("MOVED"))
                    {
                        retryCountMsg = string.Format("RedisServerException Redis<T> {0} MOVED 异常，等待随后重试。当前已重试：{1};ex:{2}", action.Method.Name, counter, ex.Message);

                        _cnn.TryFixConnection();
                    }
                    else
                    {
                        retryCountMsg = string.Format("Exception Redis<T> {0}操作异常，等待随后重试。当前已经重试：{1};ex:{2}", action.Method.Name, counter, ex.Message);
                    }

                    //LogCom.WriteErrLog("SERedisOperation.DoWithRetry." + action.Method.Name, new Exception(retryCountMsg));

                    if (counter > _busyRetry)
                    {
                        //LogCom.WriteErrLog("SERedisOperation.DoWithRetry." + action.Method.Name, new Exception("已超出配置的连接次数!"));
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
        /// Ping
        /// </summary>
        /// <returns></returns>
        public TimeSpan Ping()
        {
            return DoWithRetry(() =>
            {
                return _cnn.GetDatabase().Ping();
            });
        }

        /// <summary>
        /// 获取keys
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <param name="patten"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<string> Keys(int dbIndex = -1, string patten = "*", int count = 20)
        {
            return DoWithRetry(() =>
            {
                return _cnn.Keys(dbIndex, patten, count);
            });
        }

    }
}