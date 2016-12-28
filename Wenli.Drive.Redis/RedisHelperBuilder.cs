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
using System.Configuration;
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
        private static readonly IRedisHelper _AssemObject;

        /// <summary>
        ///     RedisClient容器
        /// </summary>
        static RedisHelperBuilder()
        {
            var appStr = ConfigurationManager.AppSettings["RedisClient"];
            var appStrArr = appStr.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            _TObjectName = appStrArr[0];
            _AssemName = appStrArr[1];
            _AssemObject = Assembly.Load(_AssemName).CreateInstance(_TObjectName) as IRedisHelper;
        }

        /// <summary>
        ///     根据指定配置依赖注入产生一个新的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Build<T>() where T : class, new()
        {
            var t = new T();
            var mf = t.GetType().GetMethod("CreateInstance");
            mf.Invoke(t, new[] {_AssemObject});
            return t;
        }

        /// <summary>
        ///     根据指定配置依赖注入产生一个新的实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section">redis配置实例名称RedisInfo</param>
        /// <returns></returns>
        public static T Build<T>(string section) where T : class, new()
        {
            var t = new T();
            var CreateInstance = t.GetType().GetMethod("CreateInstance");
            CreateInstance.Invoke(t, new[] {_AssemObject});
            var Init = t.GetType().GetMethod("Init");
            Init.Invoke(t, new[] {section});
            return t;
        }

        /// <summary>
        ///     根据指定配置产生一个新的实例
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public static RedisHelper Build(string section)
        {
            var redisHelper = new RedisHelper();
            redisHelper.CreateInstance(_AssemObject);
            redisHelper.Init(section);
            return redisHelper;
        }
    }
}