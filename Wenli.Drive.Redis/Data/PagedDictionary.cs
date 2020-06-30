/****************************************************************************
*项目名称：Wenli.Drive.Redis.Data
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Data
*类 名 称：PagedDictionary
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/30 9:51:39
*描述：
*=====================================================================
*修改时间：2020/6/30 9:51:39
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wenli.Drive.Redis.Data
{
    /// <summary>
    ///     分页信息类
    /// </summary>
    [Serializable]
    [DataContract]
    public class PagedDictionary<T1, T2>
    {
        /// <summary>
        /// 分页信息类
        /// </summary>
        public PagedDictionary()
        {
            PageIndex = 1;
            PageSize = 20;
            Count = 0;
            Dictionary = new Dictionary<T1, T2>();
        }
        /// <summary>
        /// 页号
        /// </summary>
        [DataMember]
        public int PageIndex { get; set; }
        /// <summary>
        /// 分页条数
        /// </summary>
        [DataMember]
        public int PageSize { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [DataMember]
        public long Count { get; set; }

        /// <summary>
        /// 分页数据内容
        /// </summary>
        [DataMember]
        public Dictionary<T1, T2> Dictionary { get; set; }

    }
}
