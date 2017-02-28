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
using System.Collections.Concurrent;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    ///     SERedis操作处理类
    /// </summary>
    [Serializable]
    public class SERedisHelper : IRedisHelper, IDisposable
    {
        /// <summary>
        ///     哨兵监听集合
        /// </summary>
        private static readonly ConcurrentDictionary<string, SESentinelClient> _SentinelPool =
            new ConcurrentDictionary<string, SESentinelClient>();

        private static object _LockObj = new object();

        private int _BusyRetry = 5;

        private int _BusyRetryWaitMS = 200;

        private IRedisOperation _RedisOperation;

        //实例名称
        private string _sectionName;

        private static object locker = new object();


        /// <summary>
        ///     释放
        /// </summary>
        public void Dispose()
        {
            _RedisOperation = null;
        }

        /// <summary>
        ///     初始化池，类似于构造方法
        ///     不要重复调用
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public void Init(string section)
        {
            Init(RedisConfig.GetConfig(section));
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
        public void Init(string sectionName, int type, string master, string password = "", string serviceName = "", int poolSize = 1, int busyRetry = 10, int busyRetryWaitMS = 1000)
        {
            var redisConfig = new RedisConfig()
            {
                SectionName = sectionName,
                Type = type,
                Masters = master,
                PoolSize = poolSize,
                BusyRetry = busyRetry,
                BusyRetryWaitMS = busyRetryWaitMS
            };
            if (!string.IsNullOrEmpty(serviceName))
            {
                redisConfig.ServiceName = serviceName;
            }
            if (!string.IsNullOrEmpty(password))
            {
                redisConfig.Password = password;
            }
            this.Init(redisConfig);
        }


        /// <summary>
        ///     redis操作
        /// </summary>
        public IRedisOperation GetRedisOperation(int dbIndex = -1)
        {
            return new SERedisOperation(_sectionName, dbIndex, _BusyRetry, _BusyRetryWaitMS);
        }

        /// <summary>
        ///     初始化池，类似于构造方法
        ///     不要重复调用
        /// </summary>
        /// <param name="redisConfig"></param>
        public void Init(RedisConfig redisConfig)
        {
            if (redisConfig == null)
                throw new Exception("传入的redisConfig实例不能空");
            _sectionName = redisConfig.SectionName;
            _BusyRetry = redisConfig.BusyRetry;
            _BusyRetryWaitMS = redisConfig.BusyRetryWaitMS;
            if (string.IsNullOrWhiteSpace(_sectionName))
                throw new Exception("redisConfig.SectionName不能为空");

            lock (locker)
            {
                if (SERedisConnectionPoolManager.Exists(_sectionName))
                    return;

                var configStr = GenerateConnectionString(redisConfig);

                if ((redisConfig.PoolSize < 1) || (redisConfig.PoolSize > 100))
                    redisConfig.PoolSize = 1;

                //哨兵特殊处理
                if (redisConfig.Type == 1)
                {
                    var sentinel = new SESentinelClient(_sectionName, configStr, redisConfig.PoolSize, redisConfig.Password);

                    sentinel.OnRedisServerChanged += sentinel_OnRedisServerChanged;

                    var operateRedisConnecitonString = sentinel.Start();

                    _SentinelPool.AddOrUpdate(_sectionName + "_" + redisConfig.ServiceName, sentinel, (x, y) => sentinel);

                    SERedisConnectionPoolManager.Create(_sectionName, operateRedisConnecitonString, redisConfig.PoolSize);
                }
                else
                {
                    SERedisConnectionPoolManager.Create(_sectionName, configStr, redisConfig.PoolSize);
                }
            }
        }

        /// <summary>
        ///     根据redis使用类型来生成相应的连接字符串
        /// </summary>
        /// <param name="redisConfig"></param>
        /// <returns></returns>
        private static string GenerateConnectionString(RedisConfig redisConfig)
        {
            var configStr = string.Empty;

            switch (redisConfig.Type)
            {
                case 0:
                    if (!string.IsNullOrWhiteSpace(redisConfig.Slaves))
                        configStr = string.Format("{0},{1},defaultDatabase={2}", redisConfig.Masters, redisConfig.Slaves, redisConfig.DefaultDatabase);
                    else
                        configStr = string.Format("{0},defaultDatabase={1}", redisConfig.Masters, redisConfig.DefaultDatabase);
                    if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                        configStr += ",password=" + redisConfig.Password;
                    break;
                case 1:
                    //哨兵
                    configStr = string.Format("{0},defaultDatabase={1},serviceName={2}", redisConfig.Masters, redisConfig.DefaultDatabase, redisConfig.ServiceName);
                    break;
                case 2:
                    //集群
                    configStr = string.Format("{0}", redisConfig.Masters);
                    if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                        configStr += ",password=" + redisConfig.Password;
                    break;
                default:
                    if (!string.IsNullOrWhiteSpace(redisConfig.Slaves))
                        configStr = redisConfig.Masters + "," + redisConfig.Slaves;
                    else
                        configStr = redisConfig.Masters;

                    if (!string.IsNullOrWhiteSpace(redisConfig.Password))
                        configStr += ",password=" + redisConfig.Password;
                    break;
            }
            configStr +=
                string.Format(",allowAdmin={0},connectRetry={1},connectTimeout={2},keepAlive={3},syncTimeout={4},abortConnect=false", redisConfig.AllowAdmin, redisConfig.ConnectRetry, redisConfig.ConnectTimeout, redisConfig.KeepAlive, redisConfig.CommandTimeout);

            if (!string.IsNullOrWhiteSpace(redisConfig.Extention))
            {
                configStr += "," + redisConfig.Extention;
            }

            return configStr;
        }


        /// <summary>
        /// 哨兵监测事件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="newConnectionString"></param>
        /// <param name="poolSize"></param>
        private void sentinel_OnRedisServerChanged(string section, string newConnectionString, int poolSize)
        {
            SERedisConnectionPoolManager.Update(_sectionName, newConnectionString, poolSize);
        }
    }
}