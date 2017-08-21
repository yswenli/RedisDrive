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
        /**
         * 获得 lock.
         * 实现思路: 主要是使用了redis 的setnx命令,缓存了锁.
         * reids缓存的key是锁的key,所有的共享, value是锁的到期时间(注意:这里把过期时间放在value了,没有时间上设置其超时时间)
         * 执行过程:
         * 1.通过setnx尝试设置某个key的值,成功(当前没有这个锁)则返回,成功获得锁
         * 2.锁已经存在则获取锁的到期时间,和当前时间比较,超时的话,则设置新的值
         *
         * @return true if lock is acquired, false acquire timeouted
         * @throws InterruptedException in case of thread interruption
         */

        public bool Lock(string key, int timeout = 60 * 1000)
        {
            var lockKey = _prex + key;

            while (timeout > 0)
            {
                long expires = DateTime.Now.Millisecond + _expireMsecs + 1;
                String expiresStr = expires.ToString();
                if (this.StringSetIfNotExists(lockKey, expiresStr))
                {
                    return true;
                }
                String currentValueStr = this.StringGet(lockKey);
                if (currentValueStr != null && long.Parse(currentValueStr) < DateTime.Now.Millisecond)
                {
                    String oldValueStr = this.StringGetSet(lockKey, expiresStr);
                    if (oldValueStr != null && long.Parse(oldValueStr) <= long.Parse(currentValueStr))
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
