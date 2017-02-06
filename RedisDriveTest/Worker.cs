/*
* 描述： 详细描述类能干什么
* 创建人：wenli
* 创建时间：2017/2/6 17:09:54
*/
/*
*修改人：wenli
*修改时间：2017/2/6 17:09:54
*修改内容：xxxxxxx
*/

using CCWin;
using RedisDriveTest.Fun1;
using RedisDriveTest.Fun2;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RedisDriveTest
{
    public static class Worker
    {
        static string linkStr1 = "127.0.0.1:6379,127.0.0.1:6380,keepAlive=180,allowAdmin=true,connectRetry=3,connectTimeout=1000,keepAlive=3000,syncTimeout=60000,abortConnect=false";

        static string linkStr2 = "<ClusterConfig Type=\"2\" Masters=\"127.0.0.1:6379,127.0.0.1:6380,127.0.0.1:6381\" />";

        static ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();

        static ClusterSHelper _redisHelper1;

        static ClusterMHelper _redisHelper2;

        /// <summary>
        /// 界面日志更新队列
        /// </summary>
        static Worker()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Action action = null;
                    if (_queue.TryDequeue(out action))
                    {
                        if (action != null)
                        {
                            action();
                        }
                    }
                    Thread.Sleep(50);
                }
            });
        }

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="form"></param>
        public static void Init(MainForm form)
        {
            SetDefaultValue(form);

            for (int i = 0; i < form.checkedListBox1.Items.Count; i++)
            {
                form.checkedListBox1.SetItemChecked(i, true);
            }
            form.progressBar1.Visible = false;
        }
        /// <summary>
        /// 连接字符串赋值
        /// </summary>
        /// <param name="form"></param>
        public static void SetDefaultValue(MainForm form)
        {
            if (string.IsNullOrEmpty(form.textBox1.Text) || form.textBox1.Text == linkStr1 || form.textBox1.Text == linkStr2)
            {
                if (form.skinRadioButton1.Checked)
                {
                    form.textBox1.Text = linkStr1;
                }
                if (form.skinRadioButton2.Checked)
                {
                    form.textBox1.Text = linkStr2;
                }
            }
        }

        /// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="isMyDrive"></param>
        /// <param name="connStr"></param>
        /// <param name="types"></param>
        /// <param name="ops"></param>
        /// <param name="opNum"></param>
        /// <param name="working"></param>
        /// <param name="complete"></param>
        public static void Work(MainForm form)
        {
            try
            {
                bool isMyDrive = false;

                if (form.skinRadioButton1.Checked)
                {
                    isMyDrive = false;
                }
                else
                {
                    isMyDrive = true;
                }

                string cnnStr = form.textBox1.Text;
                if (string.IsNullOrEmpty(cnnStr))
                {
                    MessageBox.Show(form.Owner, "连接字符串不能为空");
                    return;
                }

                var contractList = form.checkedListBox1.CheckedItems;
                if (contractList.Count == 0)
                {
                    MessageBox.Show(form.Owner, "结构选项不能为空");
                    return;
                }

                var types = new List<string>();
                foreach (var item in contractList)
                {
                    types.Add(item.ToString());
                }

                int operationNum = 1000;
                if (!string.IsNullOrWhiteSpace(form.skinTextBox1.Text))
                {
                    if (!int.TryParse(form.skinTextBox1.Text, out operationNum) || operationNum < 1)
                    {
                        MessageBox.Show(form.Owner, "操作次数输入有误");
                        return;
                    }
                }

                UIChange(form, true);

                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        WriteLog(form, "正在初始化操作对象...");

                        if (isMyDrive)
                            _redisHelper2 = new ClusterMHelper(cnnStr);
                        else
                            _redisHelper1 = new ClusterSHelper(cnnStr);

                        WriteLog(form, "初始化完成，正在并行处理任务...");

                        //开始并行处理任务
                        DoWork(operationNum, form, isMyDrive, cnnStr, types);

                        WriteLog(form, "任务已完成！");
                    }
                    catch (Exception ex)
                    {
                        WriteErr(form, "Worker.Work.Task", ex, cnnStr);
                    }
                    finally
                    {
                        UIChange(form, false);
                    }
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(form.Owner, "任务无法启动！" + ex.Message);
            }
        }



        #region do
        private static void DoWork(int operationNum, MainForm form, bool isMyDrive, string cnnStr, List<string> types)
        {
            if (!isMyDrive)
            {
                Parallel.For(0, operationNum, i =>
                {
                    DoA(form, cnnStr, types);
                });
            }
            else
            {
                Parallel.For(0, operationNum, i =>
                {
                    DoB(form, cnnStr, types);
                });
            }
        }
        private static void DoA(MainForm form, string cnnStr, List<string> types)
        {
            try
            {
                var prefix = Guid.NewGuid().ToString("N") + "_";

                if (types.Contains("Key"))
                {
                    #region string
                    WriteLog(form, "string get/set test");

                    _redisHelper1.StringSet(prefix + "abcabcabc", "123123");
                    WriteLog(form, "写入key：abcabcabc，value：123123");

                    var str = _redisHelper1.StringGet(prefix + "abcabcabc");
                    WriteLog(form, "查询key：abcabcabc，value：" + str);

                    _redisHelper1.KeyDelete(prefix + "abcabcabc");
                    WriteLog(form, "移除key：abcabcabc");
                    #endregion
                }
                if (types.Contains("Hash"))
                {
                    #region hashset
                    WriteLog(form, "hashset get/set test");
                    var testModel = new DemoModel()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        Age = 18,
                        Name = "Kitty",
                        Created = DateTime.Now
                    };
                    _redisHelper1.HashSet<DemoModel>(prefix + testModel.Name, testModel.ID, testModel);
                    WriteLog(form, string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    testModel = _redisHelper1.HashGet<DemoModel>(prefix + testModel.Name, testModel.ID);
                    WriteLog(form, string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    _redisHelper1.HashDelete(prefix + testModel.Name, testModel.ID);
                    WriteLog(form, "移除hash");
                    #endregion
                }

                #region 队列
                if (types.Contains("List"))
                {
                    WriteLog(form, "list test");

                    _redisHelper1.Enqueue(prefix + "list", "listvalue");
                    WriteLog(form, "入队：list，value：listvalue");

                    WriteLog(form, "list.coumt：" + _redisHelper1.QueueCount(prefix + "list"));

                    WriteLog(form, string.Format("出队：list,value：{0}", _redisHelper1.Dnqueue(prefix + "list")));
                    WriteLog(form, "list.coumt：" + _redisHelper1.QueueCount(prefix + "list"));
                }
                #endregion

                #region sortedset
                if (types.Contains("Set"))
                {
                    WriteLog(form, "set test");
                    WriteLog(form, string.Format("set add :{0}", _redisHelper1.SetAdd(prefix + "set", "set")));
                    var list = _redisHelper1.SetMembers(prefix + "set");
                    WriteLog(form, string.Format("set getlist :{0}", list));
                    WriteLog(form, string.Format("set remove :{0}", _redisHelper1.SetRemove(prefix + "set", "set")));
                }
                #endregion

                #region sortedset
                if (types.Contains("ZSet"))
                {
                    WriteLog(form, "sortedset test");
                    WriteLog(form, string.Format("sortedset add :{0}", _redisHelper1.SortedSetAdd(prefix + "sortedset", "sortedset", 0)));
                    var list = _redisHelper1.GetSortedSetRangeByRankWithSocres(prefix + "sortedset", 0, 10000, 1, 9999, true);
                    WriteLog(form, string.Format("sortedset getlist :{0}", list));
                    WriteLog(form, string.Format("sortedset remove :{0}", _redisHelper1.RemoveItemFromSortedSet(prefix + "sortedset", "sortedset")));
                }
                #endregion

                #region pub/sub
                if (types.Contains("Pub/Sub"))
                {
                    WriteLog(form, "sub/pub test");

                    WriteLog(form, "订阅频道：happy");

                    _redisHelper1.Subscribe(prefix + "happy", (x, y) =>
                    {
                        WriteLog(form, string.Format("订阅者收到消息；频道：{0},消息：{1}", x, y));
                    });

                    WriteLog(form, "发布频道happy 10 条测试消息");
                    for (int i = 1; i <= 10; i++)
                    {
                        _redisHelper1.Publish(prefix + "happy", "this is a test message" + i);
                        Thread.Sleep(400);
                    }
                    try
                    {
                        _redisHelper1.Unsubscribe(prefix + "happy");
                    }
                    catch { }
                }
                #endregion
            }
            catch (Exception ex)
            {
                WriteErr(form, "Worker.DoA", ex, cnnStr, types);
            }
        }
        private static void DoB(MainForm form, string cnnStr, List<string> types)
        {
            try
            {
                var prefix = Guid.NewGuid().ToString("N") + "_";

                if (types.Contains("Key"))
                {
                    #region string
                    WriteLog(form, "string get/set test");

                    _redisHelper2.Helper.GetRedisOperation().StringSet(prefix + "abcabcabc", "123123");
                    WriteLog(form, "写入key：abcabcabc，value：123123");

                    var str = _redisHelper2.Helper.GetRedisOperation().StringGet(prefix + "abcabcabc");
                    WriteLog(form, "查询key：abcabcabc，value：" + str);

                    _redisHelper2.Helper.GetRedisOperation().KeyDelete(prefix + "abcabcabc");
                    WriteLog(form, "移除key：abcabcabc");
                    #endregion
                }
                if (types.Contains("Hash"))
                {
                    #region hashset
                    WriteLog(form, "hashset get/set test");
                    var testModel = new DemoModel()
                    {
                        ID = Guid.NewGuid().ToString("N"),
                        Age = 18,
                        Name = "Kitty",
                        Created = DateTime.Now
                    };
                    _redisHelper2.Helper.GetRedisOperation().HashSet<DemoModel>(prefix + testModel.Name, testModel.ID, testModel);
                    WriteLog(form, string.Format("写入hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    testModel = _redisHelper2.Helper.GetRedisOperation().HashGet<DemoModel>(prefix + testModel.Name, testModel.ID);
                    WriteLog(form, string.Format("查询hashid：{0}，key：{1}", testModel.Name, testModel.ID));

                    _redisHelper2.Helper.GetRedisOperation().HashDelete(prefix + testModel.Name, testModel.ID);
                    WriteLog(form, "移除hash");
                    #endregion
                }

                #region 队列
                if (types.Contains("List"))
                {
                    WriteLog(form, "list test");

                    _redisHelper2.Helper.GetRedisOperation().Enqueue(prefix + "list", "listvalue");
                    WriteLog(form, "入队：list，value：listvalue");

                    WriteLog(form, "list.coumt：" + _redisHelper2.Helper.GetRedisOperation().QueueCount(prefix + "list"));

                    WriteLog(form, string.Format("出队：list,value：{0}", _redisHelper2.Helper.GetRedisOperation().Dnqueue(prefix + "list")));
                    WriteLog(form, "list.coumt：" + _redisHelper2.Helper.GetRedisOperation().QueueCount(prefix + "list"));
                }
                #endregion

                #region sortedset
                if (types.Contains("Set"))
                {
                    WriteLog(form, "set test");
                    WriteLog(form, string.Format("set add :{0}", _redisHelper2.Helper.GetRedisOperation().SetAdd(prefix + "set", "set")));
                    var list = _redisHelper2.Helper.GetRedisOperation().SetMembers(prefix + "set");
                    WriteLog(form, string.Format("set getlist :{0}", list));
                    WriteLog(form, string.Format("set remove :{0}", _redisHelper2.Helper.GetRedisOperation().SetRemove(prefix + "set", "set")));
                }
                #endregion

                #region sortedset
                if (types.Contains("ZSet"))
                {
                    WriteLog(form, "sortedset test");
                    WriteLog(form, string.Format("sortedset add :{0}", _redisHelper2.Helper.GetRedisOperation().SortedSetAdd(prefix + "sortedset", "sortedset", 0)));
                    var list = _redisHelper2.Helper.GetRedisOperation().GetSortedSetRangeByRankWithSocres(prefix + "sortedset", 0, 10000, 1, 9999, true);
                    WriteLog(form, string.Format("sortedset getlist :{0}", list));
                    WriteLog(form, string.Format("sortedset remove :{0}", _redisHelper2.Helper.GetRedisOperation().RemoveItemFromSortedSet(prefix + "sortedset", "sortedset")));
                }
                #endregion

                #region pub/sub
                if (types.Contains("Pub/Sub"))
                {
                    WriteLog(form, "sub/pub test");

                    WriteLog(form, "订阅频道：happy");

                    _redisHelper2.Helper.GetRedisOperation().Subscribe(prefix + "happy", (x, y) =>
                    {
                        WriteLog(form, string.Format("订阅者收到消息；频道：{0},消息：{1}", x, y));
                    });

                    WriteLog(form, "发布频道happy 10 条测试消息");
                    for (int i = 1; i <= 10; i++)
                    {
                        _redisHelper2.Helper.GetRedisOperation().Publish(prefix + "happy", "this is a test message" + i);
                        Thread.Sleep(400);
                    }
                    try
                    {
                        _redisHelper2.Helper.GetRedisOperation().Unsubscribe(prefix + "happy");
                    }
                    catch { }
                }
                #endregion
            }
            catch (Exception ex)
            {
                WriteErr(form, "Worker.DoB", ex, cnnStr, types);
            }
        }
        #endregion


        /// <summary>
        /// 调整界面
        /// </summary>
        /// <param name="form"></param>
        /// <param name="working"></param>
        private static void UIChange(MainForm form, bool working)
        {
            if (form.InvokeRequired)
            {
                form.BeginInvoke(new Action<MainForm, bool>(UIChange), form, working);
            }
            else
            {
                if (working)
                {
                    form.skinButton1.Text = "测试中..";
                    form.groupBox1.Enabled = false;
                    form.progressBar1.Visible = true;
                    form.textBox2.Text = "";
                    form.textBox3.Text = "";
                }
                else
                {
                    form.groupBox1.Enabled = true;
                    form.progressBar1.Visible = false;
                    form.skinButton1.Text = "开始测试";
                }
            }
        }

        /// <summary>
        /// 界面上打印日志
        /// </summary>
        /// <param name="form"></param>
        /// <param name="log"></param>
        public static void WriteLog(MainForm form, string log)
        {
            Task.Factory.StartNew(() =>
            {
                _queue.Enqueue(new Action(() =>
                {
                    if (form.InvokeRequired)
                    {
                        form.BeginInvoke(new Action(() =>
                        {
                            form.textBox2.Text += string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), Environment.NewLine);
                            form.textBox2.Text += string.Format("{0}{1}{1}{1}", log, Environment.NewLine);

                            if (form.textBox2.Text.Length > 5000)
                            {
                                form.textBox2.Text = form.textBox2.Text.Substring(3000);
                            }
                            form.textBox2.Focus();
                            form.textBox2.Select(form.textBox2.Text.Length - 1, 1);
                            form.textBox2.ScrollToCaret();
                        }), null);
                    }
                    else
                    {
                        form.textBox2.Text += string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), Environment.NewLine);
                        form.textBox2.Text += string.Format("{0}{1}{1}{1}", log, Environment.NewLine);

                        if (form.textBox2.Text.Length > 5000)
                        {
                            form.textBox2.Text = form.textBox2.Text.Substring(3000);
                        }
                        form.textBox2.Focus();
                        form.textBox2.Select(form.textBox2.Text.Length - 1, 1);
                        form.textBox2.ScrollToCaret();
                    }
                }));
            });
        }
        /// <summary>
        /// 打印异常信息
        /// </summary>
        /// <param name="funName"></param>
        /// <param name="ex"></param>
        /// <param name="params"></param>
        public static void WriteErr(MainForm form, string funName, Exception ex, params object[] @params)
        {
            Task.Factory.StartNew(() =>
            {
                var err = string.Format("{0}操作过程中发生异常，异常信息为:{1}{2}{3}{2}{4}", funName, ex.Message, Environment.NewLine, ex.Source, ex.StackTrace);
                if (@params != null && @params.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < @params.Count(); i++)
                    {
                        if (i == @params.Count() - 1)
                        {
                            sb.Append(@params[i].ToString());
                            sb.Append(Environment.NewLine);
                        }
                        else
                        {
                            sb.Append(@params[i].ToString());
                            sb.Append(",");
                        }
                    }
                    err = string.Format("{0}操作过程中发生异常，参数为：{5}，异常信息为:{1}{2}{3}{2}{4}", funName, ex.Message, Environment.NewLine, ex.Source, ex.StackTrace, sb.ToString());
                }
                _queue.Enqueue(new Action(() =>
                {
                    if (form.InvokeRequired)
                    {
                        form.BeginInvoke(new Action(() =>
                        {
                            form.textBox3.Text += string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), Environment.NewLine);
                            form.textBox3.Text += string.Format("{0}{1}{1}{1}", err, Environment.NewLine);

                            if (form.textBox3.Text.Length > 5000)
                            {
                                form.textBox3.Text = form.textBox3.Text.Substring(3000);
                            }
                            form.textBox3.Focus();
                            form.textBox3.Select(form.textBox3.Text.Length - 1, 1);
                            form.textBox3.ScrollToCaret();
                        }), null);
                    }
                    else
                    {
                        form.textBox3.Text += string.Format("{0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture), Environment.NewLine);
                        form.textBox3.Text += string.Format("{0}{1}{1}{1}", err, Environment.NewLine);

                        if (form.textBox3.Text.Length > 5000)
                        {
                            form.textBox3.Text = form.textBox3.Text.Substring(3000);
                        }
                        form.textBox3.Focus();
                        form.textBox3.Select(form.textBox3.Text.Length - 1, 1);
                        form.textBox3.ScrollToCaret();
                    }
                }));
            });
        }



    }
}
