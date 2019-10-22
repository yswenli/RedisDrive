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

namespace Wenli.Drive.Redis
{
    /// <summary>
    ///     redis配置类
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        ///     当前配置名称
        ///     此属性为必须
        /// </summary>
        public string InstanceName
        {
            get; set;
        } = "RedisService";

        /// <summary>
        ///     配置类型
        /// </summary>
        public RedisConfigType Type
        {
            get; set;
        }

        /// <summary>
        ///     密码
        /// </summary>
        public string Password
        {
            get; set;
        }

        /// <summary>
        ///     主Redis库，亦可是sentinel服务器地址
        /// </summary>
        public string Masters
        {
            get; set;
        }

        /// <summary>
        ///     从redis库
        /// </summary>
        public string Slaves
        {
            get; set;
        }

        /// <summary>
        ///     哨兵模式下服务名称
        /// </summary>
        public string ServiceName
        {
            get; set;
        }

        /// <summary>
        ///     非集群模式下可以指定读写db
        /// </summary>
        public int DefaultDatabase
        {
            get; set;
        }

        /// <summary>
        ///     管理员模式
        /// </summary>
        public bool AllowAdmin
        {
            get; set;
        }

        /// <summary>
        ///     连接保持(s)
        /// </summary>
        public int KeepAlive
        {
            get; set;
        }

        /// <summary>
        ///     连接超时(ms)
        /// </summary>
        public int ConnectTimeout
        {
            get; set;
        }

        /// <summary>
        ///     重连次数
        /// </summary>
        public int ConnectRetry
        {
            get; set;
        } = 1000;

        /// <summary>
        ///     任务忙重试次数
        ///     0-50之间的整数
        /// </summary>
        public int BusyRetry
        {
            get; set;
        } = 1000;

        /// <summary>
        ///     重试等待时长(ms)
        /// </summary>
        public int BusyRetryWaitMS
        {
            get; set;
        } = 1000;

        /// <summary>
        ///     连接池大小
        /// </summary>
        public int PoolSize
        {
            get; set;
        } = 10;


        /// <summary>
        ///     命令超时时间 (ms)
        /// </summary>
        public int CommandTimeout
        {
            get; set;
        } = 60 * 1000;

        /// <summary>
        /// 扩展
        /// 有一些redis因为禁用了某些命令需要添加如下部分
        /// $CLIENT=,$CLUSTER=,$CONFIG=,$ECHO=,$INFO=,$PING=
        /// </summary>
        public string Extention
        {
            get; set;
        }
    }
}