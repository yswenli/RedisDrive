using System;
using System.Threading;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis.Core
{
    public partial class SERedisOperation : IRedisOperation
    {

        private static readonly string _prex = "lock_";

        int _timeout = 30 * 1000;

        string _key = string.Empty;

        string GetKey(string key)
        {
            return _prex + key;
        }


        /// <summary>
        /// 利用StringSetIfNotExists实现锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="timeout"></param>
        /// <param name="rolling"></param>
        /// <returns></returns>
        public bool Lock(string key, int timeout = 30 * 1000, int rolling = 500)
        {
            _key = key;

            _timeout = timeout;

            var ts = TimeSpan.FromMilliseconds(_timeout);

            String expiresStr = DateTime.Now.Add(ts).Ticks.ToString();

            while (_timeout > rolling)
            {
                if (this.StringSetIfNotExists(GetKey(_key), expiresStr, ts))
                {
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
        /// <param name="key"></param>
        public void UnLock(string key = "")
        {
            if (string.IsNullOrEmpty(key))
            {
                key = _key;
            }

            this.KeyDelete(GetKey(key));
        }

    }
}
