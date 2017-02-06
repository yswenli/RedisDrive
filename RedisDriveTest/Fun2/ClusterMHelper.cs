/*
* 描述： 详细描述类能干什么
* 创建人：wenli
* 创建时间：2017/2/6 18:28:05
*/
/*
*修改人：wenli
*修改时间：2017/2/6 18:28:05
*修改内容：xxxxxxx
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wenli.Drive.Redis;
using Wenli.Drive.Redis.Core;

namespace RedisDriveTest.Fun2
{
    public class ClusterMHelper
    {
        static readonly string _configPath = Environment.CurrentDirectory + "\\Im.Data.Redis.Config";

        static SERedisHelper _helper = new SERedisHelper();

        private static object locker = new object();

        public ClusterMHelper(string configStr)
        {
            lock (locker)
            {
                if (File.Exists(_configPath))
                {
                    File.Delete(_configPath);
                }
                configStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?><configuration>  <configSections><section name=\"ClusterConfig\" type=\"Wenli.Drive.Redis.RedisConfig, Wenli.Drive.Redis\" />  </configSections>" + configStr;
                configStr += "</configuration>";
                File.WriteAllText(_configPath, configStr, Encoding.UTF8);
                _helper.Init(RedisConfig.GetConfig(_configPath, "ClusterConfig"));
            }
        }

        public SERedisHelper Helper
        {
            get
            {
                return _helper;
            }
        }
    }
}
