/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisOperationForSubPush
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/3 15:51:48
*描述：
*=====================================================================
*修改时间：2020/6/3 15:51:48
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using Wenli.Drive.Redis.Interface;
using StackExchange.Redis;
using System;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// SERedisOperationForSubPush
    /// </summary>
    public partial class SERedisOperation : IRedisOperation
    {
        #region SubPush

        private SERedisConnection subcnn;

        /// <summary>
        ///     订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        public void Subscribe(string channelPrefix, Action<RedisChannel, RedisValue> action)
        {
            DoWithRetry(() =>
            {
                if (subcnn == null)
                    subcnn = new SERedisConnection(_sectionName, _dbIndex);
                var pub = subcnn.GetSubscriber();
                pub.Subscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), action);
            });
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="action"></param>
        public void SubscribeWithChannel(string channelPrefix, Action<string, string> action)
        {
            DoWithRetry(() =>
            {
                if (subcnn == null)
                    subcnn = new SERedisConnection(_sectionName, _dbIndex);
                var pub = subcnn.GetSubscriber();

                var raction = new Action<RedisChannel, RedisValue>((c, m) =>
                {
                    action.Invoke(c, m);
                });

                pub.Subscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), raction);
            });
        }

        /// <summary>
        ///     取消订阅
        /// </summary>
        /// <param name="channelPrefix"></param>
        public void Unsubscribe(string channelPrefix)
        {
            DoWithRetry(() =>
            {
                if (subcnn == null)
                    subcnn = new SERedisConnection(_sectionName, _dbIndex);
                var pub = subcnn.GetSubscriber();
                pub.Unsubscribe(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto));
            });
        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="channelPrefix"></param>
        /// <param name="msg"></param>
        public void Publish(string channelPrefix, string msg)
        {
            DoWithRetry(() =>
            {
                var pub = _cnn.GetSubscriber();
                pub.PublishAsync(new RedisChannel(channelPrefix, RedisChannel.PatternMode.Auto), msg);
            });
        }

        #endregion
    }
}
