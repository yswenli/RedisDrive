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
using StackExchange.Redis;
using System.Threading.Tasks;
using System.Threading;
using System.Text;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    ///     对connectionpool的管理
    /// </summary>
    internal static class SERedisConnectionPoolManager
    {
        private static readonly ConcurrentDictionary<string, SERedisConnectPool> _ConnectorCollection =
            new ConcurrentDictionary<string, SERedisConnectPool>();

        private static object locker = new object();

        static SERedisConnectionPoolManager()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);

                    var keys = _ConnectorCollection.Keys;

                    StringBuilder sb = new StringBuilder();

                    foreach (var key in keys)
                    {
                        SERedisConnectPool pool = null;
                        _ConnectorCollection.TryGetValue(key, out pool);
                        var con = pool.GetConnection();
                        sb.AppendFormat("{0}:{1}", key, con.Configuration);
                        sb.AppendLine();
                    }
                }
            });
        }


        /// <summary>
        ///     初始化池
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="connectionStr"></param>
        /// <param name="poolSize"></param>
        internal static void Create(string sectionName, string connectionStr, int poolSize = 1)
        {
            lock (locker)
            {
                if (_ConnectorCollection.ContainsKey(sectionName))
                    return;

                Update(sectionName, connectionStr, poolSize);
            }
        }

        internal static void Update(string sectionName, string connectionStr, int poolSize = 1)
        {
            lock (locker)
            {
                Func<SERedisConnectPool> addPoolFunc = () =>
                {
                    return new SERedisConnectPool(connectionStr, poolSize);
                };

                _ConnectorCollection.AddOrUpdate(sectionName, key => addPoolFunc(), (x, oldPool) =>
                {
                    // 延迟100秒卸载,避免卸载太快造成无法使用问题
                    new Task(() =>
                    {
                        Thread.Sleep(100000);
                        oldPool.Dispose();
                    }).Start();

                    return addPoolFunc();
                });
            }
        }

        /// <summary>
        ///     连接池是否存在
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static bool Exists(string sectionName)
        {
            return _ConnectorCollection.ContainsKey(sectionName);
        }

        /// <summary>
        ///     从池中取出一个连接
        ///     检查连接是否断开
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnectionMultiplexer(string sectionName)
        {
            var pool = GetPool(sectionName);
            return pool.GetConnection();
        }

        /// <summary>
        /// 获取特定section的connectionPool
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static SERedisConnectPool GetPool(string sectionName)
        {
            //IEMWorks.Util.Log4NetUtil.WriteErrLog("打印多路复用器配置信息", new Exception("打印多路复用器配置信息"), sectionName, _cnn.Configuration);

            SERedisConnectPool pool = null;
            if (!_ConnectorCollection.TryGetValue(sectionName, out pool))
                throw new Exception(string.Format("Redis Section [{0}] 没有被初始化", sectionName));

            return pool;
        }


    }
}