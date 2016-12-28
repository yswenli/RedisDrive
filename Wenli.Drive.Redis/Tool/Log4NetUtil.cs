/*****************************************************************************************************
 * 本代码版权归@wenli所有，All Rights Reserved (C) 2015-2016
 *****************************************************************************************************
 * CLR版本：4.0.30319.42000
 * 唯一标识：d1ba0116-7a14-476b-94d2-e15d3d00e5bf
 * 机器名称：WENLI-PC
 * 联系人邮箱：wenguoli_520@qq.com
 *****************************************************************************************************
 * 项目名称：$projectname$
 * 命名空间：Wenli.Drive.Redis.Tool
 * 类名称：Log4NetUtil
 * 创建时间：2016/12/28 10:07:16
 * 创建人：wenli
 * 创建说明：
 *****************************************************************************************************/
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Wenli.Drive.Redis.Tool
{
    public class Log4NetHelper
    {
        #region 异常事件
        public delegate void OnErroredHandle(Exception ex, string msg);
        /// <summary>
        /// 异常触发事件
        /// </summary>
        public static event OnErroredHandle OnErrored;

        static void RaiseOnErrored(Exception ex, string msg)
        {
            if (OnErrored != null)
            {
                OnErrored(ex, msg);
            }
        }
        #endregion


        //log4net日志专用
        public static readonly log4net.ILog _loginfo = log4net.LogManager.GetLogger("loginfo");
        public static readonly log4net.ILog _logerror = log4net.LogManager.GetLogger("logerror");
        public static readonly log4net.ILog _logdebug = log4net.LogManager.GetLogger("logdebug");


        static Log4NetHelper()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void SetConfig(FileInfo configFile)
        {
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        /// <summary>
        /// 普通的文件记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void WriteLog(string info)
        {
            if (_loginfo.IsInfoEnabled)
            {
                _loginfo.Info(info);
            }
        }

        /// <summary>
        /// 调试的文件记录日志
        /// </summary>
        /// <param name="info"></param>
        public static void WriteDebugLog(string info)
        {
            if (_logdebug.IsDebugEnabled)
            {
                _logdebug.Debug(info);
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="info"></param>
        /// <param name="se"></param>
        public static void WriteLog(string info, Exception se)
        {
            if (_logerror.IsErrorEnabled)
            {
                _logerror.Error(info, se);
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="exp"></param>
        /// <param name="paramList"></param>
        public static void WriteErrLog(string funcName, Exception exp, params object[] paramList)
        {
            if (_logerror.IsErrorEnabled)
            {
                string msg = Log4NetHelper.GetErrorLogStr(funcName, exp, paramList);
                //LogServerHelper.SetLogForServer(GetIPAdress(), msg);
                RaiseOnErrored(exp, msg);
                _logerror.Error(msg, exp);
            }
        }
        /// <summary>
        /// 获取错误日志内容
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="exp"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static string GetErrorLogStr(string funcName, Exception exp, params object[] paramList)
        {
            return string.Format("|{0}|{1}|Params: {2}|ErrDesc: {3}", GetIPAdress(), funcName, GetSerializerString(paramList), exp.Message + exp.StackTrace);
        }
        /// <summary>
        /// 获取客户端的IP
        /// </summary>
        /// <returns></returns>
        public static string GetIPAdress()
        {
            try
            {
                OperationContext context = OperationContext.Current;
                if (context != null)
                {
                    string address = null;
                    MessageProperties messageProperties = context.IncomingMessageProperties;
                    try
                    {
                        var propertys = messageProperties["httpRequest"];
                        if (propertys != null)
                        {
                            var messageProperty = (HttpRequestMessageProperty)propertys;
                            address = messageProperty.Headers["X-Forwarded-For"];
                        }
                    }
                    catch
                    {
                        address = "";
                    }
                    if (string.IsNullOrEmpty(address))
                    {
                        RemoteEndpointMessageProperty endpointProperty = messageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                        address = endpointProperty.Address + ":" + endpointProperty.Port.ToString();
                    }
                    return address;
                }
                return "No IP Adress";
            }
            catch { }
            {
                return "No IP Adress";
            }
        }
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetSerializerString(object obj)
        {
            string str = string.Empty;
            try
            {
                str = SerializeHelper.Serialize(obj);
            }
            catch { }
            return str;
        }

        //
    }
}
