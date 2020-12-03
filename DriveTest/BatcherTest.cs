/****************************************************************************
*项目名称：DriveTest
*CLR 版本：4.0.30319.42000
*机器名称：WALLE-PC
*命名空间：DriveTest
*类 名 称：BatcherTest
*版 本 号：V1.0.0.0
*创建人： yswenli
*电子邮箱：yswenli@outlook.com
*创建时间：2020/12/3 17:31:05
*描述：
*=====================================================================
*修改时间：2020/12/3 17:31:05
*修 改 人： yswenli
*版 本 号： V1.0.0.0
*描    述：
*****************************************************************************/
using System;
using Wenli.Drive.Redis;

namespace DriveTest
{
    static class BatcherTest
    {
        static void Test()
        {
            Console.WriteLine("Wenli.Drive.Redis 批量操作实例");

            var redisConfig = new RedisConfig()
            {
                SectionName = "Instance",
                Type = RedisConnectType.Instance,
                Masters = "127.0.0.1:6379",
                Slaves = "127.0.0.1:6380",
                Password = "12321",
                DefaultDatabase = 0,
                BusyRetryWaitMS = 1000
            };

            var redisHelper = RedisHelperBuilder.Build(redisConfig);

            using (var redisBatcher = redisHelper.GetRedisOperation().CreateBatcher())
            {
                for (int i = 0; i < 100; i++)
                {
                    redisBatcher.Batch.StringSetAsync($"batch_{i}", $"val_{i}");
                }
            }

            Console.Read();
        }
    }
}
