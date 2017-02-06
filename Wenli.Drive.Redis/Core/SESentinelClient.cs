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
using System.Net;
using StackExchange.Redis;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    ///     SE哨兵操作类
    /// </summary>
    public class SESentinelClient
    {
        /// <summary>
        /// 主从切换通知事件委托
        /// </summary>
        /// <param name="section"></param>
        /// <param name="newconnectionString"></param>
        /// <param name="poolSize"></param>
        public delegate void OnRedisServerChangedHander(string section, string newconnectionString, int poolSize);

        private readonly string _Section = string.Empty;

        private ISubscriber _Sentinelsub;

        /// <summary>
        /// 初始化哨兵类
        /// </summary>
        /// <param name="section"></param>
        /// <param name="connectionStr"></param>
        /// <param name="poolSize"></param>
        public SESentinelClient(string section, string connectionStr, int poolSize)
        {
            if (SentinelConnection != null)
                SentinelConnection.Dispose();
            _Section = section;
            SentinelConfig = ConfigurationOptions.Parse(connectionStr);
            SentinelConfig.TieBreaker = string.Empty;
            SentinelConfig.CommandMap = CommandMap.Sentinel;
            PoolSize = poolSize;
        }

        /// <summary>
        ///     哨兵配置
        /// </summary>
        public ConfigurationOptions SentinelConfig
        {
            get;
        }

        /// <summary>
        ///     sentinel连接器
        /// </summary>
        public ConnectionMultiplexer SentinelConnection
        {
            get; private set;
        }

        public int PoolSize
        {
            get;
        }

        /// <summary>
        ///     主从切换通知事件
        /// </summary>
        public event OnRedisServerChangedHander OnRedisServerChanged;

        /// <summary>
        /// 触发主从切换通知事件
        /// </summary>
        /// <param name="section"></param>
        /// <param name="newconnectionString"></param>
        /// <param name="poolsize"></param>
        protected void RaiseOnRedisServerChanged(string section, string newconnectionString, int poolsize)
        {
            if (OnRedisServerChanged != null)
                OnRedisServerChanged(section, newconnectionString, poolsize);
        }

        /// <summary>
        ///     获取活动的哨兵服务器
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        private IServer GetActiveServer(ConnectionMultiplexer conn, EndPoint[] endpoints)
        {
            foreach (var endpoint in endpoints)
            {
                var server = conn.GetServer(endpoint);

                if (server.IsConnected)
                    return server;
            }
            throw new Exception("找不到可以连接的活动的哨兵服务器");
        }

        /// <summary>
        ///     中间处理slave endpoint列表
        /// </summary>
        /// <param name="slaves"></param>
        /// <returns></returns>
        private List<string> SanitizeHostsConfig(KeyValuePair<string, string>[][] slaves)
        {
            string ip;
            string port;
            string flags;

            var servers = new List<string>();
            foreach (var slave in slaves)
            {
                var dic = slave.ToDictionary();

                dic.TryGetValue("flags", out flags);
                dic.TryGetValue("ip", out ip);
                dic.TryGetValue("port", out port);

                if (ip == "127.0.0.1")
                    ip = (SentinelConfig.EndPoints.First() as IPEndPoint).Address.ToString();

                if ((ip != null) && (port != null) && !flags.Contains("s_down") && !flags.Contains("o_down"))
                    servers.Add(string.Format("{0}:{1}", ip, port));
            }
            return servers;
        }

        /// <summary>
        ///     根据当前哨兵获取对应redis实例的连接字符串
        /// </summary>
        /// <returns></returns>
        private string GetConnectionStringFromSentinel()
        {
            try
            {
                var activeSentinelServer = GetActiveServer(SentinelConnection, SentinelConnection.GetEndPoints());
                var masterConnectionInfo = activeSentinelServer.SentinelGetMasterAddressByName(SentinelConfig.ServiceName);
                var slaveConnectionInfos =
                    SanitizeHostsConfig(activeSentinelServer.SentinelSlaves(SentinelConfig.ServiceName));

                var redisConfigs = new ConfigurationOptions
                {
                    AllowAdmin = true,
                    DefaultDatabase = SentinelConfig.DefaultDatabase,
                    ConnectRetry = SentinelConfig.ConnectRetry,
                    ConnectTimeout = SentinelConfig.ConnectTimeout,
                    KeepAlive = SentinelConfig.KeepAlive,
                    SyncTimeout = SentinelConfig.SyncTimeout,
                    AbortOnConnectFail = false
                };
                redisConfigs.EndPoints.Add(masterConnectionInfo);
                foreach (var slaveInfo in slaveConnectionInfos)
                    redisConfigs.EndPoints.Add(slaveInfo);

                return redisConfigs.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SESentinelClient.GetConnectionStringFromSentinel 连接到哨兵服务器（{0}）失败：{1}", SentinelConfig, ex.Message));
            }
        }

        /// <summary>
        ///     连接到指定的Sentinel，获取 master 和 slave 信息并返回。同时，注册相应的事件用于接收 sentinel 的通知消息
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public string Start()
        {
            var redisConnectionString = string.Empty;
            try
            {
                SentinelConnection = ConnectionMultiplexer.Connect(SentinelConfig);

                redisConnectionString = GetConnectionStringFromSentinel();

                _Sentinelsub = SentinelConnection.GetSubscriber();

                _Sentinelsub.SubscribeAsync("+switch-master", (channle, msg) =>
                {
                    redisConnectionString = GetConnectionStringFromSentinel();
                    RaiseOnRedisServerChanged(_Section, redisConnectionString, PoolSize);
                });
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SESentinelClient.Start 连接到哨兵服务器（{0}）失败：{1}", SentinelConfig, ex.Message));
            }

            return redisConnectionString;
        }
    }
}