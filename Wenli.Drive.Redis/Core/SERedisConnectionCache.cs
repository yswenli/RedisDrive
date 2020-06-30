/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisConnectionCache
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/30 9:53:34
*描述：
*=====================================================================
*修改时间：2020/6/30 9:53:34
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisConnectionCache
    /// </summary>
    internal static class SERedisConnectionCache
    {
        static ConcurrentDictionary<string, ConnectionMultiplexer> _cache = null;


        /// <summary>
        /// SERedisConnectionCache
        /// </summary>
        static SERedisConnectionCache()
        {
            _cache = new ConcurrentDictionary<string, ConnectionMultiplexer>();
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="connectionStr"></param>
        public static void Init(string sectionName, string connectionStr)
        {
            var old = Get(sectionName);

            new SERedisConnectionDefender(sectionName, connectionStr).FreeAndConnect(old);
        }


        #region by section


        /// <summary>
        /// 添加或更新连接
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="cnn"></param>
        public static void Set(string sectionName, ConnectionMultiplexer cnn)
        {
            _cache.AddOrUpdate(sectionName, cnn, (k, v) => cnn);
        }



        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static bool Exists(string sectionName)
        {
            return _cache.ContainsKey(sectionName);
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer Get(string sectionName)
        {
            if (_cache.TryGetValue(sectionName, out ConnectionMultiplexer cnn))
            {
                return cnn;
            }
            return null;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="sectionName"></param>
        public static void Remove(string sectionName)
        {
            _cache.TryRemove(sectionName, out ConnectionMultiplexer cnn);
        }

        #endregion

    }
}
