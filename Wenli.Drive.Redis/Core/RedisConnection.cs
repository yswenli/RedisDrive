/****************************************************************************
*项目名称：Im.Data.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis
*类 名 称：RedisConnection
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/7/6 16:07:59
*描述：
*=====================================================================
*修改时间：2020/7/6 16:07:59
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;

namespace Wenli.Drive.Redis
{
    /// <summary>
    /// 自定义RedisConnection
    /// </summary>
    public class RedisConnection
    {
        /// <summary>
        /// ConnectionMultiplexer
        /// </summary>
        public ConnectionMultiplexer Connection { get; set; }

        /// <summary>
        /// 修复中
        /// </summary>
        public bool Repairing { get; set; } = false;
    }
}
