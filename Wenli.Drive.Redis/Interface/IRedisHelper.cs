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
namespace Wenli.Drive.Redis.Interface
{
    /// <summary>
    ///     redis操作类实现接口
    /// </summary>
    public interface IRedisHelper
    {
        /// <summary>
        ///     初始化池
        /// </summary>
        /// <param name="section"></param>
        void Init(string section);

        /// <summary>
        ///     初始化池
        /// </summary>
        /// <param name="config"></param>
        void Init(RedisConfig config);

        /// <summary>
        /// 初始化池
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="type"></param>
        /// <param name="master"></param>
        /// <param name="password"></param>
        /// <param name="serviceName"></param>
        /// <param name="poolSize"></param>
        /// <param name="busyRetry"></param>
        /// <param name="busyRetryWaitMS"></param>
        void Init(string sectionName, int type, string master, string password = "", string serviceName = "", int poolSize = 1, int busyRetry = 10, int busyRetryWaitMS = 1000);

        /// <summary>
        ///     redis操作
        /// </summary>
        IRedisOperation GetRedisOperation(int dbIndex = -1);
    }
}