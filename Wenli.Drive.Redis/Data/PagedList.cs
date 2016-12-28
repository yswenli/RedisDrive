/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2015-2016
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：8c468370-8519-4057-a58e-36fe1ad3ed8a
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：$projectname$
 * 命名空间：Wenli.Drive.Redis.Data
 * 类名称：PagedList
 * 创建时间：2016/12/28 10:02:03
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wenli.Drive.Redis.Data
{
    /// <summary>
    /// 分页信息类
    /// </summary>
    [Serializable, DataContract]
    public class PagedList<T>
    {
        [DataMember]
        public int PageIndex
        {
            get; set;
        }
        [DataMember]
        public int PageSize
        {
            get; set;
        }
        [DataMember]
        public long Count
        {
            get; set;
        }
        [DataMember]
        public List<T> List
        {
            get; set;
        }

        public PagedList()
        {
            this.PageIndex = 1;
            this.PageSize = 20;
            this.Count = 0;
            this.List = new List<T>();
        }
    }
}
