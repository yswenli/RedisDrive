/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2015-2016
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：c52026f9-2b01-46ec-a21d-76ab1d34270f
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：$projectname$
 * 命名空间：Wenli.Drive.Redis.Tool
 * 类名称：SerializeHelper
 * 创建时间：2016/12/28 9:56:53
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace Wenli.Drive.Redis.Tool
{
    /// <summary>
    /// 序列化类
    /// </summary>
    public static class SerializeHelper
    {
        /// <summary>
        ///     newton.json序列化,日志参数专用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.ObjectCreationHandling = ObjectCreationHandling.Replace;
            jsetting.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            return JsonConvert.SerializeObject(obj, jsetting);
        }

        /// <summary>
        ///     newton.json反序列化,日志参数专用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            JsonSerializerSettings jsetting = new JsonSerializerSettings();
            jsetting.ObjectCreationHandling = ObjectCreationHandling.Replace;
            jsetting.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            return JsonConvert.DeserializeObject<T>(json, jsetting);
        }

        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ByteSerialize(object obj)
        {

            using (MemoryStream m = new MemoryStream())
            {

                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(m, obj);

                return m.ToArray();

            }

        }

        /// <summary>
        /// 二进制反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ByteDeserialize<T>(byte[] buffer)
        {

            using (MemoryStream m = new MemoryStream())
            {

                m.Write(buffer, 0, buffer.Length);
                m.Position = 0;

                BinaryFormatter bin = new BinaryFormatter();

                return (T)bin.Deserialize(m);

            }

        }
    }
}
