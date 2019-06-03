using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RedisDriveTest
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var count = 10000;

            //Dictionary<string, string> dic = new Dictionary<string, string>();
            //List<string> keys = new List<string>();

            //for (int i = 0; i < count; i++)
            //{
            //    dic.Add("key" + i, "val" + i);
            //}

            //keys = dic.Keys.ToList();

            //var cnnStr = "127.0.0.1:6380";
            //var cnn = StackExchange.Redis.ConnectionMultiplexer.Connect(cnnStr);
            //var db = cnn.GetDatabase();

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            //foreach (var item in dic)
            //{
            //    db.StringSet(item.Key, item.Value);
            //}
            //var t = stopwatch.ElapsedMilliseconds;
            //var s = count * 1000 / t;


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
