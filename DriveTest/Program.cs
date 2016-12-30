/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2015-2016
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：c91279a5-8753-4ca4-bbdc-d89dfa9dd54d
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：Program
 * 命名空间：DriveTest
 * 类名称：DriveTest
 * 创建时间：2016/12/28 10:20:47
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/
using System;
using System.Threading;
using System.Threading.Tasks;
using Wenli.Drive.Redis;

namespace DriveTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Wenli.Drive.Redis驱动测试";

            Console.WriteLine("Wenli.Drive.Redis test");

            Console.WriteLine("输入s 测试哨兵模式，输入c测试cluster模式，M为连续，其它为单实例模式");

            while (true)
            {

                var c = Console.ReadLine();

                if (c.ToUpper() == "S")
                {
                    #region sentinel
                    Console.WriteLine("Wenli.Drive.Redis test 进入哨兵模式---------------------");

                    using (var redisHelper = RedisHelperBuilder.Build("ClusterConfig"))
                    {

                        #region string
                        Console.ReadLine();
                        Console.WriteLine("string get/set test");

                        redisHelper.GetRedisOperation().StringSet("abcabcabc", "123123");
                        Console.WriteLine("写入key：abcabcabc，value：123123");

                        var str = redisHelper.GetRedisOperation().StringGet("abcabcabc");
                        Console.WriteLine("查询key：abcabcabc，value：" + str);

                        redisHelper.GetRedisOperation().KeyDelete("abcabcabc");
                        Console.WriteLine("移除key：abcabcabc");
                        #endregion

                        #region hashset
                        Console.ReadLine();
                        Console.WriteLine("hashset get/set test");
                        var testModel = new DemoModel()
                        {
                            ID = Guid.NewGuid().ToString("N"),
                            Age = 18,
                            Name = "Kitty",
                            Created = DateTime.Now
                        };

                        redisHelper.GetRedisOperation().HashSet<DemoModel>(testModel.Name, testModel.ID, testModel);
                        Console.WriteLine(string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                        testModel = redisHelper.GetRedisOperation().HashGet<DemoModel>(testModel.Name, testModel.ID);
                        Console.WriteLine(string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                        redisHelper.GetRedisOperation().HashDelete(testModel.Name, testModel.ID);
                        Console.WriteLine("移除hash");
                        #endregion

                        #region 队列
                        Console.ReadLine();
                        Console.WriteLine("list test");

                        redisHelper.GetRedisOperation().Enqueue("list", "listvalue");
                        Console.WriteLine("入队：list，value：listvalue");

                        Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));

                        Console.WriteLine(string.Format("出队：list,value：{0}", redisHelper.GetRedisOperation().Dnqueue("list")));
                        Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));
                        #endregion

                        #region sortedset
                        Console.ReadLine();
                        Console.WriteLine("sortedset test");
                        Console.WriteLine(string.Format("sortedset add :{0}", redisHelper.GetRedisOperation().SortedSetAdd("sortedset", "sortedset", 0)));
                        var list = redisHelper.GetRedisOperation().GetSortedSetRangeByRankWithSocres("sortedset", 0, 10000, 1, 9999, true);
                        Console.WriteLine(string.Format("sortedset getlist :{0}", list));
                        Console.WriteLine(string.Format("sortedset remove :{0}", redisHelper.GetRedisOperation().RemoveItemFromSortedSet("sortedset", "sortedset")));
                        #endregion

                        #region pub/sub
                        Console.ReadLine();
                        Console.WriteLine("sub/pub test");

                        Console.WriteLine("订阅频道：happy");

                        redisHelper.GetRedisOperation().Subscribe("happy", (x, y) =>
                        {
                            Console.WriteLine(string.Format("订阅者收到消息；频道：{0},消息：{1}", x, y));
                        });

                        Console.WriteLine("发布频道happy 10 条测试消息");
                        for (int i = 1; i <= 10; i++)
                        {
                            redisHelper.GetRedisOperation().Publish("happy", "this is a test message" + i);
                            Thread.Sleep(400);
                        }
                        #endregion
                        Console.ReadLine();
                        redisHelper.GetRedisOperation().Unsubscribe("happy");

                    }
                    #endregion
                }
                else if (c.ToUpper() == "C")
                {
                    #region cluster
                    Console.WriteLine("Wenli.Drive.Redis test 进入集群模式---------------------");

                    var redisHelper = RedisHelperBuilder.Build("ClusterConfig");

                    #region string
                    Console.ReadLine();
                    Console.WriteLine("string get/set test");

                    redisHelper.GetRedisOperation().StringSet("abcabcabc", "123123");
                    Console.WriteLine("写入key：abcabcabc，value：123123");

                    var str = redisHelper.GetRedisOperation().StringGet("abcabcabc");
                    Console.WriteLine("查询key：abcabcabc，value：" + str);

                    redisHelper.GetRedisOperation().KeyDelete("abcabcabc");
                    Console.WriteLine("移除key：abcabcabc");
                    #endregion

                    #region hashset
                    Console.ReadLine();
                    Console.WriteLine("hashset get/set test");
                    var testModel = new DemoModel()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        Age = 18,
                        Name = "Kitty",
                        Created = DateTime.Now
                    };

                    redisHelper.GetRedisOperation().HashSet<DemoModel>(testModel.Name, testModel.ID, testModel);
                    Console.WriteLine(string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    testModel = redisHelper.GetRedisOperation().HashGet<DemoModel>(testModel.Name, testModel.ID);
                    Console.WriteLine(string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    redisHelper.GetRedisOperation().HashDelete(testModel.Name, testModel.ID);
                    Console.WriteLine("移除hash");
                    #endregion

                    #region 队列
                    Console.ReadLine();
                    Console.WriteLine("list test");

                    redisHelper.GetRedisOperation().Enqueue("list", "listvalue");
                    Console.WriteLine("入队：list，value：listvalue");

                    Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));

                    Console.WriteLine(string.Format("出队：list,value：{0}", redisHelper.GetRedisOperation().Dnqueue("list")));
                    Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));
                    #endregion


                    #region pub/sub
                    Console.ReadLine();
                    Console.WriteLine("sub/pub test");

                    Console.WriteLine("订阅频道：happy");

                    redisHelper.GetRedisOperation().Subscribe("happy", (x, y) =>
                    {
                        Console.WriteLine(string.Format("订阅者收到消息；频道：{0},消息：{1}", x, y));
                    });

                    Console.WriteLine("发布频道happy 10 条测试消息");
                    for (int i = 1; i <= 10; i++)
                    {
                        redisHelper.GetRedisOperation().Publish("happy", "this is a test message" + i);
                        Thread.Sleep(400);
                    }
                    #endregion

                    Console.ReadLine();

                    redisHelper.GetRedisOperation().Unsubscribe("happy");
                    #endregion
                }
                else if (c.ToUpper() == "M")
                {
                    #region default redis
                    Console.WriteLine("Wenli.Drive.Redis test 进入连续测试模式---------------------");

                    string value = "123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式123123进入连续测试模式";

                    var td1 = new Thread(new ThreadStart(() =>
                    {
                        using (var redisHelper = RedisHelperBuilder.Build("SentinelConfig"))
                        {
                            Parallel.For(0, 100000, countIndex =>
                            {
                                #region string
                                Console.WriteLine("string get/set test");
                                redisHelper.GetRedisOperation().StringSet(countIndex.ToString(), value);
                                Console.WriteLine("写入key：abcabcabc，value：123123");

                                var str = redisHelper.GetRedisOperation().StringGet(countIndex.ToString());
                                Console.WriteLine("查询key：abcabcabc，value：" + str);

                                redisHelper.GetRedisOperation().KeyDelete(countIndex.ToString());
                                Console.WriteLine("移除key：abcabcabc");
                                #endregion

                                #region hashset
                                Console.WriteLine("hashset get/set test");
                                var testModel = new DemoModel()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    Age = 18,
                                    Name = "Kitty",
                                    Created = DateTime.Now
                                };

                                redisHelper.GetRedisOperation().HashSet<DemoModel>(testModel.Name, testModel.ID, testModel);
                                Console.WriteLine(string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                                testModel = redisHelper.GetRedisOperation().HashGet<DemoModel>(testModel.Name, testModel.ID);
                                Console.WriteLine(string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                                redisHelper.GetRedisOperation().HashDelete(testModel.Name, testModel.ID);
                                Console.WriteLine("移除hash");
                                #endregion

                            });
                        }
                    }));
                    var td2 = new Thread(new ThreadStart(() =>
                    {
                        Parallel.For(0, 100000, countIndex =>
                        {
                            using (var redisHelper = RedisHelperBuilder.Build("SentinelConfig"))
                            {
                                #region string
                                Console.WriteLine("string get/set test");
                                redisHelper.GetRedisOperation().StringSet(countIndex.ToString(), value);
                                Console.WriteLine("写入key：abcabcabc，value：123123");

                                var str = redisHelper.GetRedisOperation().StringGet(countIndex.ToString());
                                Console.WriteLine("查询key：abcabcabc，value：" + str);

                                redisHelper.GetRedisOperation().KeyDelete(countIndex.ToString());
                                Console.WriteLine("移除key：abcabcabc");
                                #endregion

                                #region hashset
                                Console.WriteLine("hashset get/set test");
                                var testModel = new DemoModel()
                                {
                                    ID = Guid.NewGuid().ToString("N"),
                                    Age = 18,
                                    Name = "Kitty",
                                    Created = DateTime.Now
                                };

                                redisHelper.GetRedisOperation().HashSet<DemoModel>(testModel.Name, testModel.ID, testModel);
                                Console.WriteLine(string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                                testModel = redisHelper.GetRedisOperation().HashGet<DemoModel>(testModel.Name, testModel.ID);
                                Console.WriteLine(string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                                redisHelper.GetRedisOperation().HashDelete(testModel.Name, testModel.ID);
                                Console.WriteLine("移除hash");
                                #endregion
                            }
                        });
                    }));

                    td1.Start();
                    td2.Start();

                    while (td1.IsAlive || td2.IsAlive)
                    {
                        Thread.Sleep(50);
                    }

                    Console.WriteLine("Wenli.Drive.Redis test 任务已完成！---------------------");
                    #endregion
                }
                else
                {
                    #region default redis
                    Console.WriteLine("Wenli.Drive.Redis test 进入单实例模式---------------------");

                    var redisHelper = RedisHelperBuilder.Build("RedisConfig");

                    #region string
                    Console.ReadLine();
                    Console.WriteLine("string get/set test");

                    Console.WriteLine("string all keys:{0}", redisHelper.GetRedisOperation().StringGetKeys().Count);
                    Console.WriteLine("string all keys:{0}", redisHelper.GetRedisOperation().StringGetKeys(100).Count);

                    redisHelper.GetRedisOperation(12).StringSet("abcabcabc", "123123");
                    Console.WriteLine("写入key：abcabcabc，value：123123");

                    var str = redisHelper.GetRedisOperation(12).StringGet("abcabcabc");
                    Console.WriteLine("查询key：abcabcabc，value：" + str);

                    redisHelper.GetRedisOperation(12).KeyDelete("abcabcabc");
                    Console.WriteLine("移除key：abcabcabc");
                    #endregion

                    #region hashset
                    Console.ReadLine();
                    Console.WriteLine("hashset get/set test");
                    var testModel = new DemoModel()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        Age = 18,
                        Name = "Kitty",
                        Created = DateTime.Now
                    };

                    redisHelper.GetRedisOperation().HashSet<DemoModel>(testModel.Name, testModel.ID, testModel);
                    Console.WriteLine(string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    testModel = redisHelper.GetRedisOperation().HashGet<DemoModel>(testModel.Name, testModel.ID);
                    Console.WriteLine(string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    redisHelper.GetRedisOperation().HashGetAll<DemoModel>(testModel.Name, 1, 1);

                    redisHelper.GetRedisOperation().HashDelete(testModel.Name, testModel.ID);
                    Console.WriteLine("移除hash");
                    #endregion

                    #region 队列
                    Console.ReadLine();
                    Console.WriteLine("list test");

                    redisHelper.GetRedisOperation().Enqueue("list", "listvalue");
                    Console.WriteLine("入队：list，value：listvalue");

                    Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));

                    Console.WriteLine(string.Format("出队：list,value：{0}", redisHelper.GetRedisOperation().Dnqueue("list")));
                    Console.WriteLine("list.coumt：" + redisHelper.GetRedisOperation().QueueCount("list"));
                    #endregion

                    #region pub/sub
                    Console.ReadLine();
                    Console.WriteLine("sub/pub test");

                    Console.WriteLine("订阅频道：happy");

                    redisHelper.GetRedisOperation().Subscribe("happy", (x, y) =>
                    {
                        Console.WriteLine(string.Format("订阅者收到消息；频道：{0},消息：{1}", x, y));
                    });

                    Console.WriteLine("发布频道happy 10 条测试消息");
                    for (int i = 1; i <= 10; i++)
                    {
                        redisHelper.GetRedisOperation().Publish("happy", "this is a test message" + i);
                        Thread.Sleep(400);
                    }
                    #endregion

                    Console.ReadLine();

                    redisHelper.GetRedisOperation().Unsubscribe("happy");

                    #endregion
                }
            }

            Console.ReadLine();
        }
    }
}
