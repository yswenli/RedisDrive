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
using Wenli.Drive.Redis.Interface;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis
{
    /// <summary>
    ///     redis容器类
    ///     此类不要直接new(),需要用RedisHelperBuilder来构造
    /// </summary>
    public class RedisHelper : IDisposable
    {
        private IRedisHelper _redisHelper;


        private IRedisOperation _RedisOperation;

        public void Dispose()
        {
            _RedisOperation = null;
        }

        /// <summary>
        ///     ioc所需初始化方法
        /// </summary>
        /// <param name="redisHelper"></param>
        public void CreateInstance(IRedisHelper redisHelper)
        {
            _redisHelper = SerializeHelper.ByteDeserialize<IRedisHelper>(SerializeHelper.ByteSerialize(redisHelper));
        }

        /// <summary>
        ///     初始化
        ///     使用RedisHelperBuilder.Build请不要调用此方法
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        internal void Init(string section)
        {
            _redisHelper.Init(section);
        }

        /// <summary>
        ///     redis操作
        /// </summary>
        public IRedisOperation GetRedisOperation(int dbIndex = -1)
        {
            return _redisHelper.GetRedisOperation(dbIndex);
        }
    }
}