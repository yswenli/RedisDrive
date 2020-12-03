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
        /// 倒序排例
        /// </summary>
        private const string DefaultOrder = "descending";

        private readonly int _dbIndex = -1;

        private readonly string _sectionName;

        SERedisConnection _cnn = null;

        /// <summary>
        /// SERedis操作类
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="dbIndex"></param>
        /// <param name="busyRetry"></param>
        /// <param name="busyRetryWaitMS"></param>
        public SERedisOperation(string sectionName, int dbIndex = -1, int busyRetry = 1000, int busyRetryWaitMS = 200)
        {
            _sectionName = sectionName;
            _dbIndex = dbIndex;
            _cnn = new SERedisConnection(_sectionName, _dbIndex);
        }

        #region 操作方法重试包装

        private T DoWithRetry<T>(Func<T> func)
        {
            try
            {
                if (_cnn.RedisConnection.Repairing) throw new Exception($"SERedisOperation操作失败,sectionName:{_sectionName},_cnn:{SerializeHelper.Serialize(_cnn)}", new Exception("连接正在修复中"));

                return func();
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("SERedisOperation操作失败") > -1) throw ex;

                if (!_cnn.RedisConnection.Repairing)
                {
                    _cnn.TryRepairConnection();
                }
                throw new Exception($"SERedisOperation操作失败,sectionName:{_sectionName},_cnn:{SerializeHelper.Serialize(_cnn)}", ex);
            }
        }

        private void DoWithRetry(Action action)
        {
            try
            {
                if (_cnn.RedisConnection.Repairing) throw new Exception($"SERedisOperation操作失败,sectionName:{_sectionName},_cnn:{SerializeHelper.Serialize(_cnn)}", new Exception("连接正在修复中"));

                action();
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("SERedisOperation操作失败") > -1) throw ex;

                if (!_cnn.RedisConnection.Repairing)
                {
                    _cnn.TryRepairConnection();
                }
                throw new Exception($"SERedisOperation操作失败,sectionName:{_sectionName},_cnn:{SerializeHelper.Serialize(_cnn)}", ex);
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
        /// 创建批量处理
        /// </summary>
        /// <returns></returns>
        public RedisBatcher CreateBatcher()
        {
            return _cnn.CreateBatcher();
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