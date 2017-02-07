# RedisDrive
这是一个.net 的redis集成驱动，支持单实例、云集群、哨兵等模式的数据操作，支持keys等命令

本驱动基于stackexcnage.redis完成,在此基础上强化使用的简洁性、可靠性

wenli.Drive.Redis使用方法：

1.添加wenli.Drive.Redis引用

2.添加stackexcnage.redis nuget包引用

3.添加Wenli.Drive.Redis、log4net配置节点，添加RedisClient appsettings

app.config(web.config)配置如下：
&lt;?xml version="1.0" encoding="utf-8"?&gt;<br />
&lt;configuration&gt;<br />
&nbsp; &lt;configSections&gt;<br />
&nbsp; &nbsp; &lt;section name="RedisConfig" type="Wenli.Drive.Redis.RedisConfig, Wenli.Drive.Redis" /&gt;<br />
&nbsp; &nbsp; &lt;section name="SentinelConfig" type="Wenli.Drive.Redis.RedisConfig, Wenli.Drive.Redis" /&gt;<br />
&nbsp; &nbsp; &lt;section name="ClusterConfig" type="Wenli.Drive.Redis.RedisConfig, Wenli.Drive.Redis" /&gt;<br />
&nbsp; &nbsp; &lt;section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,log4net" /&gt;<br />
&nbsp; &lt;/configSections&gt;<br />
&nbsp; &lt;!--单点模式配置--&gt;<br />
&nbsp; &lt;RedisConfig Type="0" Masters="127.0.0.1:6379" Slaves="127.0.0.1:6379" DefaultDatabase="0" /&gt;<br />
&nbsp; &lt;!--哨兵模式配置--&gt;<br />
&nbsp; &lt;SentinelConfig Type="1" Masters="127.0.0.1:26379" ServiceName="mymaster" DefaultDatabase="0" /&gt;<br />
&nbsp; &lt;!--集群模式配置--&gt;<br />
&nbsp; &lt;ClusterConfig Type="2" Masters="127.0.0.1:16379,127.0.0.1:16380,127.0.0.1:16381" /&gt;<br />
&nbsp; &lt;log4net&gt;<br />
&nbsp; &nbsp; &lt;logger name="logerror"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;level value="ERROR" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;appender-ref ref="ErrorAppender" /&gt;<br />
&nbsp; &nbsp; &lt;/logger&gt;<br />
&nbsp; &nbsp; &lt;logger name="loginfo"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;level value="INFO" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;appender-ref ref="InfoAppender" /&gt;<br />
&nbsp; &nbsp; &lt;/logger&gt;<br />
&nbsp; &nbsp; &lt;logger name="logdebug"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;level value="DEBUG" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;appender-ref ref="DebugAppender" /&gt;<br />
&nbsp; &nbsp; &lt;/logger&gt;<br />
&nbsp; &nbsp; &lt;appender name="ErrorAppender" type="log4net.Appender.RollingFileAppender"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="File" value="ErrorLog.log" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="AppendToFile" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaxSizeRollBackups" value="100" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaximumFileSize" value="1MB" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="RollingStyle" value="Size" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="StaticLogFileName" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;layout type="log4net.Layout.PatternLayout"&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;param name="ConversionPattern" value="%-5p %d [%c] %m%n" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;/layout&gt;<br />
&nbsp; &nbsp; &lt;/appender&gt;<br />
&nbsp; &nbsp; &lt;appender name="InfoAppender" type="log4net.Appender.RollingFileAppender"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="File" value="InfoLog.log" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="AppendToFile" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaxSizeRollBackups" value="100" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaximumFileSize" value="1MB" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="RollingStyle" value="Size" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="StaticLogFileName" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;layout type="log4net.Layout.PatternLayout"&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;param name="ConversionPattern" value="%-5p %d [%c] %m%n" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;/layout&gt;<br />
&nbsp; &nbsp; &lt;/appender&gt;<br />
&nbsp; &nbsp; &lt;appender name="DebugAppender" type="log4net.Appender.RollingFileAppender"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="File" value="DebugLog.log" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="AppendToFile" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaxSizeRollBackups" value="100" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="MaximumFileSize" value="1MB" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="RollingStyle" value="Size" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;param name="StaticLogFileName" value="true" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;layout type="log4net.Layout.PatternLayout"&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;param name="ConversionPattern" value="%-5p %d [%c] %m%n" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;/layout&gt;<br />
&nbsp; &nbsp; &lt;/appender&gt;<br />
&nbsp; &lt;/log4net&gt;<br />
&nbsp; &lt;appSettings&gt;<br />
&nbsp; &nbsp; &lt;add key="RedisClient" value="Wenli.Drive.Redis.Core.SERedisHelper;Wenli.Drive.Redis,Version=1.0.0.0,Culture=neutral,PublicKeyToken=null" /&gt;<br />
&nbsp; &lt;/appSettings&gt;<br />
&nbsp; &lt;runtime&gt;<br />
&nbsp; &nbsp; &lt;assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1"&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;dependentAssembly&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;assemblyIdentity name="System.Runtime" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" /&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;bindingRedirect oldVersion="0.0.0.0-2.6.10.0" newVersion="2.6.10.0" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;/dependentAssembly&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;dependentAssembly&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;assemblyIdentity name="System.Threading.Tasks" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" /&gt;<br />
&nbsp; &nbsp; &nbsp; &nbsp; &lt;bindingRedirect oldVersion="0.0.0.0-2.6.10.0" newVersion="2.6.10.0" /&gt;<br />
&nbsp; &nbsp; &nbsp; &lt;/dependentAssembly&gt;<br />
&nbsp; &nbsp; &lt;/assemblyBinding&gt;<br />
&nbsp; &lt;/runtime&gt;<br />
&lt;/configuration&gt;<br />

也可以使用项目中的测试工具对集群进行相关测试

<img src="https://raw.githubusercontent.com/yswenli/RedisDrive/11e1e4c4303df3ba6d7847d4d310e400f3a26933/RedisDriveTest/2017-02-07_095814.png" alt="wenli.drive.redis"/>

实例代码：

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
    
    
    授权

wenli.Drive.Redis 采用LGPL开源协议：

如果您不对 wenli.Drive.Redis 程序代码进行任何修改，直接调用组件，可以以任意方式自由使用：开源、非开源、商业及非商业。

如果您对 wenli.Drive.Redis 程序代码进行任何的修改或者衍生，涉及修改部分的额外代码和衍生的代码都必须采用 LGPL 协议开放源代码。

无论您对 wenli.Drive.Redis 程序代码如何修改，都必须在程序文件头部声明版权信息的注释（包括压缩版）

Open Source (OSI) Logo

LGPL协议原文：GNU Lesser General Public License

商业授权

您可以将 wenli.Drive.Redis 程序直接使用在自己的商业或者非商业网站或者软件产品中。

您可以对 wenli.Drive.Redis 进行修改和美化，可以去除 wenli.Drive.Redis 版权注释或改变程序名称，无需公开您修改或美化过的 wenli.Drive.Redis 程序与界面。

商业授权每个公司只需要购买一次，而不限制产品域名。适用于 wenli.Drive.Redis 现有版本和所有后续版本，永久有效。

您享有反映和提出意见的优先权，相关意见将被作为首要考虑。

商业授权全文 | 授权价格：99元 | 支付方式：支付宝

联系Email/支付宝帐号: wenguoli_520@hotmail.com
