/****************************************************************************
*项目名称：Im.Data.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Im.Data.Redis.Core
*类 名 称：RedisBatcher
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/9/17 14:42:04
*描述：
*=====================================================================
*修改时间：2020/9/17 14:42:04
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System;

namespace Wenli.Drive.Redis
{
    /// <summary>
    /// RedisBatcher
    /// </summary>
    public class RedisBatcher : IDisposable
    {
        IBatch _batch;

        /// <summary>
        /// RedisBatcher
        /// </summary>
        /// <param name="dataBase"></param>
        internal RedisBatcher(IDatabase dataBase)
        {
            _batch = dataBase.CreateBatch();
        }

        /// <summary>
        /// 批量
        /// </summary>
        public IBatch Batch
        {
            get => _batch;
        }

        /// <summary>
        /// Execute
        /// </summary>
        public void Execute()
        {
            _batch.Execute();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Execute();
        }
    }
}
