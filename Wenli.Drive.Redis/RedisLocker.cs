/****************************************************************************
*项目名称：Wenli.Drive.Redis
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis
*类 名 称：RedisLocker
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2019/10/18 9:38:07
*描述：
*=====================================================================
*修改时间：2019/10/18 9:38:07
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using System;

namespace Wenli.Drive.Redis
{
    /// <summary>
    /// 分布式锁,
    /// using(RedisLocker)
    /// </summary>
    public class RedisLocker : IDisposable
    {
        RedisHelper _redisHelper = null;

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="rolling"></param>
        public RedisLocker(string sectionName, string key, int timeout = 30 * 1000, int rolling = 50)
        {
            _redisHelper = RedisHelperBuilder.Build(sectionName);
            _redisHelper.GetRedisOperation().Lock(key, timeout, rolling);
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="redisConfig"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="rolling"></param>
        public RedisLocker(RedisConfig redisConfig, string key, int timeout = 30 * 1000, int rolling = 50)
        {
            _redisHelper = RedisHelperBuilder.Build(redisConfig);
            _redisHelper.GetRedisOperation().Lock(key, timeout, rolling);
        }

        /// <summary>
        /// 分布式锁
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="ipPort"></param>
        /// <param name="passwords"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="rolling"></param>
        public RedisLocker(string serviceName, string ipPort, string passwords, int type, string key, int timeout = 30 * 1000, int rolling = 50)
        {
            _redisHelper = RedisHelperBuilder.Build(serviceName, ipPort, passwords, type);
            _redisHelper.GetRedisOperation().Lock(key, timeout, rolling);
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            _redisHelper.GetRedisOperation().UnLock();
        }
    }
}
