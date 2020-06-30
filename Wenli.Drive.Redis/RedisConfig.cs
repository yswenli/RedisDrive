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

using System.Configuration;

namespace Wenli.Drive.Redis
{
    /// <summary>
    ///     redis配置类
    /// </summary>
    public class RedisConfig : ConfigurationSection
    {
        /// <summary>
        ///     当前配置名称
        ///     此属性为必须
        /// </summary>
        public string SectionName
        {
            get; set;
        }

        /// <summary>
        ///     配置类型
        /// </summary>
        [ConfigurationProperty("Type", IsRequired = true)]
        public RedisConnectType Type
        {
            get
            {
                return (RedisConnectType)base["Type"];
            }
            set
            {
                base["Type"] = value;
            }
        }

        /// <summary>
        ///     密码
        /// </summary>
        [ConfigurationProperty("Password", IsRequired = false)]
        public string Password
        {
            get
            {
                return (string)base["Password"];
            }
            set
            {
                base["Password"] = value;
            }
        }

        /// <summary>
        ///     主Redis库，亦可是sentinel服务器地址
        /// </summary>
        [ConfigurationProperty("Masters", IsRequired = true)]
        public string Masters
        {
            get
            {
                return (string)base["Masters"];
            }
            set
            {
                base["Masters"] = value;
            }
        }

        /// <summary>
        ///     从redis库
        /// </summary>
        [ConfigurationProperty("Slaves", IsRequired = false)]
        public string Slaves
        {
            get
            {
                return (string)base["Slaves"];
            }
            set
            {
                base["Slaves"] = value;
            }
        }

        /// <summary>
        ///     哨兵模式下服务名称
        /// </summary>
        [ConfigurationProperty("ServiceName", IsRequired = false, DefaultValue = "mymaster")]
        public string ServiceName
        {
            get
            {
                return (string)base["ServiceName"];
            }
            set
            {
                base["ServiceName"] = value;
            }
        }

        /// <summary>
        ///     非集群模式下可以指定读写db
        /// </summary>
        [ConfigurationProperty("DefaultDatabase", IsRequired = false, DefaultValue = 0)]
        public int DefaultDatabase
        {
            get
            {
                return (int)base["DefaultDatabase"];
            }
            set
            {
                base["DefaultDatabase"] = value;
            }
        }

        /// <summary>
        ///     管理员模式
        /// </summary>
        [ConfigurationProperty("AllowAdmin", IsRequired = false, DefaultValue = true)]
        public bool AllowAdmin
        {
            get
            {
                return (bool)base["AllowAdmin"];
            }
            set
            {
                base["AllowAdmin"] = value;
            }
        }

        /// <summary>
        ///     连接保持(s)
        /// </summary>
        [ConfigurationProperty("KeepAlive", IsRequired = false, DefaultValue = 180)]
        public int KeepAlive
        {
            get
            {
                return (int)base["KeepAlive"];
            }
            set
            {
                base["KeepAlive"] = value;
            }
        }

        /// <summary>
        ///     连接超时(ms)
        /// </summary>
        [ConfigurationProperty("ConnectTimeout", IsRequired = false, DefaultValue = 10 * 1000)]
        public int ConnectTimeout
        {
            get
            {
                return (int)base["ConnectTimeout"];
            }
            set
            {
                base["ConnectTimeout"] = value;
            }
        }

        /// <summary>
        ///     重连次数
        /// </summary>
        [ConfigurationProperty("ConnectRetry", IsRequired = false, DefaultValue = 1)]
        public int ConnectRetry
        {
            get
            {
                return (int)base["ConnectRetry"];
            }
            set
            {
                base["ConnectRetry"] = value;
            }
        }

        /// <summary>
        ///     任务忙重试次数
        ///     0-10000之间的整数
        /// </summary>
        [ConfigurationProperty("BusyRetry", IsRequired = false, DefaultValue = 10000)]
        public int BusyRetry
        {
            get
            {
                return (int)base["BusyRetry"];
            }
            set
            {
                base["BusyRetry"] = value;
            }
        }

        /// <summary>
        ///     重试等待时长(ms)
        /// </summary>
        [ConfigurationProperty("BusyRetryWaitMS", IsRequired = false, DefaultValue = 15000)]
        public int BusyRetryWaitMS
        {
            get
            {
                return (int)base["BusyRetryWaitMS"];
            }
            set
            {
                base["BusyRetryWaitMS"] = value;
            }
        }

        /// <summary>
        ///     连接池大小
        /// </summary>
        [ConfigurationProperty("PoolSize", IsRequired = false, DefaultValue = 1)]
        public int PoolSize
        {
            get
            {
                return (int)base["PoolSize"];
            }
            set
            {
                base["PoolSize"] = value;
            }
        }


        /// <summary>
        ///     命令超时时间 (ms)
        /// </summary>
        [ConfigurationProperty("CommandTimeout", IsRequired = false, DefaultValue = 60000)]
        public int CommandTimeout
        {
            get
            {
                return (int)base["CommandTimeout"];
            }
            set
            {
                base["CommandTimeout"] = value;
            }
        }

        /// <summary>
        /// 扩展
        /// 有一些redis因为禁用了某些命令需要添加如下部分
        /// $CLIENT=,$CLUSTER=,$CONFIG=,$ECHO=,$INFO=,$PING=
        /// </summary>
        [ConfigurationProperty("Extention", IsRequired = false, DefaultValue = "")]
        public string Extention
        {
            get
            {
                return (string)base["Extention"];
            }
            set
            {
                base["Extention"] = value;
            }
        }

        #region 从配置文件中创建redis配置类

        /// <summary>
        ///     获取默认redis配置类
        /// </summary>
        /// <returns></returns>
        public static RedisConfig GetConfig()
        {
            return (RedisConfig)ConfigurationManager.GetSection("RedisConfig");
        }

        /// <summary>
        ///     获取指定的redis配置类
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static RedisConfig GetConfig(string sectionName)
        {
            var section = (RedisConfig)ConfigurationManager.GetSection(sectionName);
            //  跟默认配置相同的，可以省略
            if (section == null)
                section = GetConfig();
            if (section == null)
                throw new ConfigurationErrorsException("rediscofig节点 " + sectionName + " 未配置.");
            section.SectionName = sectionName;
            return section;
        }

        /// <summary>
        ///     从指定位置读取配置
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static RedisConfig GetConfig(string fileName, string sectionName)
        {
            return GetConfig(ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(fileName)),
                sectionName);
        }

        /// <summary>
        ///     从指定Configuration中读取配置
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static RedisConfig GetConfig(Configuration config, string sectionName)
        {
            if (config == null)
                throw new ConfigurationErrorsException("传入的配置不能为空");
            var section = (RedisConfig)config.GetSection(sectionName);
            if (section == null)
                throw new ConfigurationErrorsException("rediscofng节点 " + sectionName + " 未配置.");
            section.SectionName = sectionName;
            return section;
        }

        #endregion
    }
}