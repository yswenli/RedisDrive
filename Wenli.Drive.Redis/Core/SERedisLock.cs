using System;
using System.Threading;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis.Core
{
    public partial class SERedisOperation : IRedisOperation
    {



        private static readonly string _prex = "lock_";

        /// <summary>
        /// 利用StringSetIfNotExists实现锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="rolling"></param>
        /// <returns></returns>
        public bool Lock(string key, int timeout = 30 * 1000, int rolling = 500)
        {
            var lockKey = _prex + key;
            var expireDateTime = DateTime.Now.AddMilliseconds(timeout);
            long expires = expireDateTime.Ticks;
            String expiresStr = expires.ToString();

            while (timeout > rolling)
            {               
                if (this.StringSetIfNotExists(lockKey, expiresStr))
                {
                    this.KeyExpire(lockKey, expireDateTime);
                    return true;
                }
                timeout -= rolling;
                Thread.Sleep(rolling);
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
