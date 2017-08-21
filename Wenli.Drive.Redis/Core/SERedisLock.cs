using System;
using System.Threading;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis.Core
{
    public partial class SERedisOperation : IRedisOperation
    {

        /// <summary>
        /// 锁超时时间，防止线程在入锁以后，无限的执行等待
        /// </summary>
        private static readonly int _expireMsecs = 10 * 1000;
        /// <summary>
        /// 检查间隔
        /// </summary>
        private static readonly int DEFAULT_ACQUIRY_RESOLUTION_MILLIS = 1000;


        private static readonly string _prex = "lock_";

        /// <summary>
        /// 利用StringSetIfNotExists实现锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool Lock(string key, int timeout = 60 * 1000)
        {
            var lockKey = _prex + key;

            while (timeout > 0)
            {
                long expires = DateTime.Now.AddMilliseconds(_expireMsecs).Ticks;
                String expiresStr = expires.ToString();
                if (this.StringSetIfNotExists(lockKey, expiresStr))
                {
                    return true;
                }
                String currentValueStr = this.StringGet(lockKey);
                if (currentValueStr != null && long.Parse(currentValueStr) <= DateTime.Now.Ticks)
                {
                    String oldValueStr = this.StringGetSet(lockKey, expiresStr);
                    if (oldValueStr != null && oldValueStr == currentValueStr)
                    {
                        return true;
                    }
                }
                timeout -= DEFAULT_ACQUIRY_RESOLUTION_MILLIS;
                Thread.Sleep(DEFAULT_ACQUIRY_RESOLUTION_MILLIS);
            }
            return false;
        }
        /// <summary>
        /// 移除lock
        /// </summary>
        public void UnLock(string key)
        {
            this.KeyDelete(_prex + key);
        }

    }
}
