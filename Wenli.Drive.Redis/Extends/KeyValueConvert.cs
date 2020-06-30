/****************************************************************************
*项目名称：Wenli.Drive.Redis.Extends
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：Wenli.Drive.Redis.Extends
*类 名 称：KeyValueConvert
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/6/30 9:50:15
*描述：
*=====================================================================
*修改时间：2020/6/30 9:50:15
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using Wenli.Drive.Redis.Data;
using Wenli.Drive.Redis.Tool;

namespace Wenli.Drive.Redis.Extends
{
    /// <summary>
    /// key value 转换
    /// </summary>
    public static class KeyValueConvert
    {
        /// <summary>
        /// 转换keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static List<string> ConvertTo(this IEnumerable<RedisKey> keys)
        {
            if (keys == null || !keys.Any()) return new List<string>();

            var result = new List<string>();

            foreach (var item in keys)
            {
                result.Add(item.ToString());
            }

            return result;
        }

        /// <summary>
        /// 转换values
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<string> ConvertTo(this IEnumerable<RedisValue> values)
        {
            if (values == null || !values.Any()) return new List<string>();

            var result = new List<string>();

            foreach (var item in values)
            {
                result.Add(item.ToString());
            }

            return result;
        }

        /// <summary>
        /// 转换value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this RedisValue value)
        {
            if (value.IsNullOrEmpty) return default(T);

            var json = value.ToString();

            return SerializeHelper.Deserialize<T>(json);
        }

        /// <summary>
        /// 转换values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<T> ConvertTo<T>(this IEnumerable<RedisValue> values)
        {
            if (values == null || !values.Any()) return new List<T>();

            var result = new List<T>();

            foreach (var item in values)
            {
                var json = item.ToString();

                if (!string.IsNullOrEmpty(json))
                {
                    result.Add(SerializeHelper.Deserialize<T>(json));
                }
            }

            return result;
        }

        /// <summary>
        /// 转换HashEntry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ConvertTo(this IEnumerable<HashEntry> entries)
        {
            if (entries == null || !entries.Any()) return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();

            foreach (var entry in entries)
            {
                var key = entry.Name.ToString();

                result[key] = entry.Value.ToString();
            }

            return result;
        }

        /// <summary>
        /// 转换HashEntry
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static Dictionary<string, T> ConvertTo<T>(this IEnumerable<HashEntry> entries)
        {
            if (entries == null || !entries.Any()) return new Dictionary<string, T>();

            var result = new Dictionary<string, T>();

            foreach (var entry in entries)
            {
                var key = entry.Name.ToString();

                result[key] = entry.Value.ConvertTo<T>();
            }

            return result;
        }

        /// <summary>
        /// 转换SortedSetEntry
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static Dictionary<string, double> ConvertTo(this IEnumerable<SortedSetEntry> entries)
        {
            if (entries == null || !entries.Any()) return new Dictionary<string, double>();

            var result = new Dictionary<string, double>();

            foreach (var entry in entries)
            {
                var key = entry.Element.ToString();

                result[key] = entry.Score;
            }

            return result;
        }

        /// <summary>
        /// 转换SortedSetEntry
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static PagedDictionary<string, double> ConvertTo(this IEnumerable<SortedSetEntry> entries, int pageIndex, int pageSize, long len)
        {
            if (entries == null || !entries.Any()) return new PagedDictionary<string, double>();

            var result = new PagedDictionary<string, double>() { PageIndex = pageIndex, PageSize = pageSize, Count = len };

            var data = new Dictionary<string, double>();

            foreach (var entry in entries)
            {
                var key = entry.Element.ToString();

                data[key] = entry.Score;
            }

            result.Dictionary = data;

            return result;
        }
    }
}
