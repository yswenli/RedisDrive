/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2015-2016
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：29c08bfd-59ed-42b7-b2f9-b344225840a3
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：$projectname$
 * 命名空间：Wenli.Drive.Redis.Core
 * 类名称：SERedisConnection
 * 创建时间：2016/11/8 20:31:02
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/

using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Extends;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// redis 连接
    /// </summary>
    public class SERedisConnection
    {
        private readonly int _dbIndex;

        private readonly string _sectionName;

        /// <summary>
        /// RedisConnection
        /// </summary>
        public RedisConnection RedisConnection { get; private set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// redis 连接
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="dbIndex"></param>
        public SERedisConnection(string sectionName, int dbIndex)
        {
            _sectionName = sectionName;
            _dbIndex = dbIndex;
            RedisConnection = SERedisConnectionCache.Get(_sectionName);
            if (RedisConnection == null || RedisConnection.Connection == null || !RedisConnection.Connection.IsConnected || RedisConnection.Repairing) throw new Exception("Redis 连接未初始化或正在修复中,无法执行此操作，sectionName：" + sectionName);
            Configuration = RedisConnection.Connection.Configuration;
        }

        /// <summary>
        ///     获取db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {
            return RedisConnection.Connection.GetDatabase(_dbIndex);
        }

        /// <summary>
        /// 创建批量处理
        /// </summary>
        /// <returns></returns>
        public RedisBatcher CreateBatcher()
        {
            return new RedisBatcher(RedisConnection.Connection.GetDatabase(_dbIndex));
        }

        /// <summary>
        ///     获取订阅
        /// </summary>
        /// <returns></returns>
        public ISubscriber GetSubscriber()
        {
            return RedisConnection.Connection.GetSubscriber();
        }

        /// <summary>
        /// 尝试修复连接
        /// </summary>
        internal void TryRepairConnection()
        {
            RedisConnection.Repairing = true;

            RedisConnection = new SERedisConnectionDefender(_sectionName, Configuration).FreeAndConnect(RedisConnection);

            RedisConnection.Repairing = false;
        }

        /// <summary>
        /// 获取某服务器上全部keys
        /// </summary>
        /// <param name="patten"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据,且本方法只能查询db0，建议使用sortedset来保存keys")]
        public List<string> Keys(string patten = "*")
        {
            var firstConfigEndPoint = RedisConnection.Connection.GetEndPoints()[0];
            var anyServer = RedisConnection.Connection.GetServer(firstConfigEndPoint);

            var lastResult = new List<string>();
            if (anyServer.ServerType == ServerType.Cluster)
            {
                // 这个操作比较费时， 使用从来处理
                var allMasterNodes = anyServer.ClusterConfiguration.Nodes.Where(n => !n.IsSlave);

                foreach (var masterNode in allMasterNodes)
                {
                    var runCommandServer = masterNode;
                    if (masterNode.Children.Count > 0)
                    {
                        runCommandServer = masterNode.Children.First();
                    }
                    var resultsInOneServer = RedisConnection.Connection.GetServer(runCommandServer.EndPoint).Keys(pattern: patten).Select(b => b.ToString()).ToList();
                    lastResult.AddRange(resultsInOneServer);
                }

                return lastResult;
            }
            else
            {
                return anyServer.Keys(pattern: patten).Select(b => b.ToString()).ToList();
            }
        }

        /// <summary>
        /// 获取某服务器上全部keys(此方法目前只支持cluster模式) 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="patten"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public void Keys(Action<List<string>> callback, string patten = "*", int size = 10000)
        {
            if (callback == null) return;
            var firstConfigEndPoint = RedisConnection.Connection.GetEndPoints()[0];
            var anyServer = RedisConnection.Connection.GetServer(firstConfigEndPoint);

            if (anyServer.ServerType == ServerType.Cluster)
            {
                // 这个操作比较费时， 使用从来处理
                var allMasterNodes = anyServer.ClusterConfiguration.Nodes.Where(n => !n.IsSlave);

                foreach (var masterNode in allMasterNodes)
                {
                    var runCommandServer = masterNode;
                    //LogCom.WriteInfoLog($"master node ：NodeId [{masterNode.NodeId}] : EndPoint [{masterNode.EndPoint.ToString()}] : ChildrenCount[{masterNode.Children.Count}]");

                    // 驱动自带的 preferslave 不管用，还是运行在了master上，所以自己手动获取slave
                    var allAliveSlaves = masterNode.Children.Where(r => r.IsConnected).ToList();
                    if (allAliveSlaves.Count > 0)
                    {
                        runCommandServer = allAliveSlaves.First();
                    }
                    //LogCom.WriteInfoLog($"run keys command on Server ：NodeId [{runCommandServer.NodeId}] : EndPoint[{runCommandServer.EndPoint.ToString()}]");

                    var resultsInOneServer = RedisConnection.Connection.GetServer(runCommandServer.EndPoint).Keys(pattern: patten, pageSize: size).Select(b => b.ToString()).ToList();
                    callback(resultsInOneServer);
                }
            }
            else
            {
                callback(anyServer.Keys(pattern: patten, pageSize: size).Select(b => b.ToString()).ToList());
                //yield return anyServer.Keys(pattern: patten).Select(b => b.ToString()).ToList();
            }
        }

        /// <summary>
        /// 获取指定keys
        /// </summary>
        /// <param name="dbIndex"></param>
        /// <param name="patten"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据,且本方法只能查询db0，建议使用sortedset来保存keys")]
        public List<string> Keys(int dbIndex = -1, string patten = "*", int count = 20)
        {
            var result = new List<string>();

            var endpoint = RedisConnection.Connection.GetEndPoints()[0];

            var rs = RedisConnection.Connection.GetServer(endpoint);

            var data = rs.Keys(dbIndex, patten, count, CommandFlags.PreferSlave);

            if (data != null && data.Any())
            {
                result.AddRange(data.ToList().ConvertTo());
            }

            return result = result.Take(count).ToList();
        }


    }
}