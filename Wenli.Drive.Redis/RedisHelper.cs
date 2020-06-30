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
using Wenli.Drive.Redis.Interface;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis
{
    /// <summary>
    ///     redis容器类
    ///     此类不要直接new(),需要用RedisHelperBuilder来构造
    /// </summary>
    public class RedisHelper
    {
        private IRedisHelper _redisHelper;

        /// <summary>
        /// redis容器类
        /// </summary>
        internal RedisHelper() { }

        /// <summary>
        /// ioc所需初始化方法
        /// </summary>
        /// <param name="redisHelper"></param>
        internal void CreateInstance(IRedisHelper redisHelper)
        {
            _redisHelper = redisHelper.DeepCloneForDynamic();
        }

        /// <summary>
        ///     初始化
        ///     使用RedisHelperBuilder.Build请不要调用此方法
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        internal void Init(string sectionName)
        {
            _redisHelper.Init(sectionName);
        }

        /// <summary>
        /// 自定义初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ipPort"></param>
        /// <param name="passwords"></param>
        /// <param name="type"></param>
        internal void Init(string name, string ipPort, string passwords, RedisConnectType type = RedisConnectType.Instance)
        {
            _redisHelper.Init(name, type, ipPort, passwords);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="type"></param>
        /// <param name="master"></param>
        /// <param name="password"></param>
        /// <param name="serviceName"></param>
        /// <param name="poolSize"></param>
        /// <param name="busyRetry"></param>
        /// <param name="busyRetryWaitMS"></param>
        internal void Init(string sectionName, RedisConnectType type, string master, string password = "", string serviceName = "", int poolSize = 1, int busyRetry = 10, int busyRetryWaitMS = 1000)
        {
            _redisHelper.Init(sectionName, type, master, password, serviceName, poolSize, busyRetry, busyRetryWaitMS);
        }

        /// <summary>
        ///     初始化
        ///     使用RedisHelperBuilder.Build请不要调用此方法
        /// </summary>
        /// <param name="config"></param>
        internal void Init(RedisConfig config)
        {
            _redisHelper.Init(config);
        }

        /// <summary>
        /// redis操作
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <param name="waitForFix"></param>
        /// <returns></returns>
        public IRedisOperation GetRedisOperation(int dbIndex = -1, bool waitForFix = true)
        {
            return _redisHelper.GetRedisOperation(dbIndex, waitForFix);
        }
    }
}