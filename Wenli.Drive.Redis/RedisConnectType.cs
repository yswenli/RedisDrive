/****************************************************************************
*项目名称：Wenli.Drive.Redis
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis
*类 名 称：RedisConnectType
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/30 9:37:49
*描述：
*=====================================================================
*修改时间：2020/6/30 9:37:49
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Wenli.Drive.Redis
{
    /// <summary>
    /// redis连接类型
    /// </summary>
    public enum RedisConnectType
    {
        /// <summary>
        /// Instance
        /// </summary>
        Instance = 0,
        /// <summary>
        /// Sentinel
        /// </summary>
        Sentinel = 1,
        /// <summary>
        /// Cluster
        /// </summary>
        Cluster = 2
    }
}
