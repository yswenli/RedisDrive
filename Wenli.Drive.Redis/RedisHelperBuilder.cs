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
using System;
using System.Linq;
using System.Reflection;
using Wenli.Drive.Redis.Interface;

namespace Wenli.Drive.Redis
{
    /// <summary>
    ///     RedisClient容器
    /// </summary>
    public static class RedisHelperBuilder
    {
        private static readonly string _AssemName;

        private static readonly string _TObjectName;

        private static readonly IRedisHelper _instance;

        /// <summary>
        /// RedisClient容器
        /// </summary>
        static RedisHelperBuilder()
        {
            var redisClientConfig = "Wenli.Drive.Redis.Core.SERedisHelper;Wenli.Drive.Redis,Version=2.1.0.5,Culture=neutral,PublicKeyToken=null";
            var appStrArr = redisClientConfig.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            _TObjectName = appStrArr[0];
            _AssemName = appStrArr[1];

            _instance = Assembly.Load(_AssemName).CreateInstance(_TObjectName) as IRedisHelper;
        }

        /// <summary>
        ///     根据指定配置依赖注入产生一个新的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sectionName">redis配置实例名称RedisInfo</param>
        /// <returns></returns>
        public static RedisHelper Build(string sectionName)
        {
            var redisHelper = new RedisHelper();
            redisHelper.CreateInstance(_instance);
            redisHelper.Init(sectionName);
            return redisHelper;
        }

        /// <summary>
        /// 根据指定配置产生一个新的实例
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static RedisHelper Build(RedisConfig config)
        {
            var redisHelper = new RedisHelper();
            redisHelper.CreateInstance(_instance);
            redisHelper.Init(config);
            return redisHelper;
        }

        /// <summary>
        /// 自定义指定配置连接
        /// </summary>
        /// <param name="name">这个并非是配置节点名，而是逻辑名</param>
        /// <param name="ipPort"></param>
        /// <param name="passwords"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static RedisHelper Build(string name, string ipPort, string passwords, RedisConnectType type = 0)
        {
            var redisHelper = new RedisHelper();
            redisHelper.CreateInstance(_instance);
            redisHelper.Init(name, ipPort, passwords, type);
            return redisHelper;
        }

        static void Check(params string[] args)
        {
            if (args == null || !args.Any()) throw new ArgumentNullException("RedisHelperBuilder.Build 必填参数不能空！");

            foreach (var item in args)
            {
                if (string.IsNullOrEmpty(item))
                    throw new ArgumentNullException("RedisHelperBuilder.Build 必填参数不能空！");
            }
        }
    }
}