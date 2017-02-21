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

using System;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Linq;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    ///     redis 连接
    /// </summary>
    public class SERedisConnection : IDisposable
    {
        private ConnectionMultiplexer _cnn;

        private readonly int _dbIndex;
        private readonly string _sectionName;

        /// <summary>
        ///     redis 连接
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="dbIndex"></param>
        public SERedisConnection(string sectionName, int dbIndex)
        {
            _sectionName = sectionName;
            _dbIndex = dbIndex;
            Pool = SERedisConnectionPoolManager.GetPool(_sectionName);
            _cnn = Pool.GetConnection();
        }

        /// <summary>
        ///     释放连接
        /// </summary>
        public void Dispose()
        {
            _cnn = null;
        }

        /// <summary>
        ///     获取db
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase()
        {


            return _cnn.GetDatabase(_dbIndex);
        }

        /// <summary>
        ///    从连接池中取出下一个连接并获得他的database 
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabaseFromNextConnection()
        {
            return Pool.GetConnection().GetDatabase(_dbIndex);
        }

        /// <summary>
        ///     获取订阅
        /// </summary>
        /// <returns></returns>
        public ISubscriber GetSubscriber()
        {
            return _cnn.GetSubscriber();
        }

        /// <summary>
        ///    获取当前section的poolSize
        /// </summary>
        /// <returns></returns>
        internal SERedisConnectPool Pool
        {
            get; private set;
        }

        /// <summary>
        /// 获取某服务器上全部keys
        /// </summary>
        /// <param name="patten"></param>
        /// <returns></returns>
        [Obsolete("此方法只用于兼容老数据,且本方法只能查询db0，建议使用sortedset来保存keys")]
        public List<string> Keys(string patten = "*")
        {
            return _cnn.GetServer(_cnn.GetEndPoints()[0]).Keys(pattern: "*").Select(b => b.ToString()).ToList();
        }

        /// <summary>
        /// 获取服务器信息
        /// </summary>
        /// <returns></returns>
        public string GetServerInfo()
        {
            var info = string.Empty;
            try
            {
                var eps = _cnn.GetEndPoints(true);
                if (eps != null && eps.Length > 0)
                {
                    foreach (var ep in _cnn.GetEndPoints(true))
                    {
                        info += string.Format("{0}{1}{2}{1}", ep.AddressFamily.ToString(), Environment.NewLine, _cnn.GetServer(ep).InfoRaw());
                    }
                }
            }
            catch { }
            return info;
        }

    }
}