/****************************************************************************
*项目名称：Wenli.Drive.Redis.Core
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Core
*类 名 称：SERedisConnectionDefender
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/30 9:53:54
*描述：
*=====================================================================
*修改时间：2020/6/30 9:53:54
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Wenli.Drive.Redis.Core
{
    /// <summary>
    /// 连接维护类
    /// </summary>
    internal class SERedisConnectionDefender
    {
        string _sectionName = string.Empty;

        string _connectStr = string.Empty;

        /// <summary>
        /// 连接维护类
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="connectStr"></param>
        public SERedisConnectionDefender(string sectionName, string connectStr)
        {
            _sectionName = sectionName;

            _connectStr = connectStr;
        }

        /// <summary>
        /// 建立连接并释放旧连接
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        internal ConnectionMultiplexer FreeAndConnect(ConnectionMultiplexer old = null)
        {
            SERedisConnectionDefenderEx.AutoResetEvent.WaitOne();
            try
            {
                #region 延迟修复

                if (SERedisConnectionDefenderEx.Repaired.ContainsKey(_sectionName))
                {
                    if (SERedisConnectionDefenderEx.Repaired[_sectionName].Created.AddMinutes(1) < DateTime.Now)
                    {
                        SERedisConnectionDefenderEx.Repaired.Remove(_sectionName);
                    }
                }

                if (SERedisConnectionDefenderEx.Repaired.ContainsKey(_sectionName))
                {
                    return old;
                }

                #endregion

                DisConnect(old);

                return Connect();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                SERedisConnectionDefenderEx.AutoResetEvent.Set();
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <returns></returns>
        ConnectionMultiplexer Connect()
        {
            try
            {
                var cnn = ConnectionMultiplexer.Connect(_connectStr);

                SERedisConnectionDefenderEx.Repaired[_sectionName] = new SectionInfo
                {
                    SectionName = _sectionName,
                    Created = DateTime.Now
                };

                SERedisConnectionCache.Set(_sectionName, cnn);

                return cnn;
            }
            catch (Exception ex)
            {
                throw new Exception("SERedisConnectionDefender.Connect异常，connectStr：" + _connectStr, ex);
            }
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        /// <param name="old"></param>
        void DisConnect(ConnectionMultiplexer old = null)
        {
            if (old == null) return;

            SERedisConnectionCache.Remove(_sectionName);

            old.Close();

            old.Dispose();

            old = null;
        }
    }

    /// <summary>
    /// SERedisConnectionDefender的辅助类，仅限SERedisConnectionDefender.GetConnection方法中使用
    /// </summary>
    internal static class SERedisConnectionDefenderEx
    {
        internal static Dictionary<string, SectionInfo> Repaired { get; set; } = new Dictionary<string, SectionInfo>();

        internal static AutoResetEvent AutoResetEvent { get; set; } = new AutoResetEvent(true);
    }

    /// <summary>
    /// section配置
    /// </summary>
    class SectionInfo
    {
        public string SectionName { get; set; }

        public DateTime Created { get; set; }
    }
}
