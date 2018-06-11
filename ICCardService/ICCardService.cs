using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Collections;
using System.Globalization;

namespace ICCardService
{
    public partial class ICCardService : ServiceBase
    {

        #region 对USB接口的使用(PHILIPH卡)
        [DllImport("dcrf32.dll")]
        public static extern int dc_init(Int16 port, Int32 baud);  //初始化
        [DllImport("dcrf32.dll")]
        public static extern short dc_exit(int icdev);
        [DllImport("dcrf32.dll")]
        public static extern short dc_reset(int icdev, uint sec);
        [DllImport("dcrf32.dll")]
        public static extern short dc_request(int icdev, char _Mode, ref uint TagType);
        [DllImport("dcrf32.dll")]
        public static extern short dc_card(int icdev, char _Mode, ref ulong Snr);
        [DllImport("dcrf32.dll")]
        public static extern short dc_halt(int icdev);
        [DllImport("dcrf32.dll")]
        public static extern short dc_anticoll(int icdev, char _Bcnt, ref ulong IcCardNo);
        [DllImport("dcrf32.dll")]
        public static extern short dc_beep(int icdev, uint _Msec);
        [DllImport("dcrf32.dll")]
        public static extern short dc_authentication(int icdev, int _Mode, int _SecNr);

        [DllImport("dcrf32.dll")]
        public static extern short dc_load_key(int icdev, int mode, int secnr, [In] byte[] nkey);  //密码装载到读写模块中
        [DllImport("dcrf32.dll")]
        public static extern short dc_load_key_hex(int icdev, int mode, int secnr, string nkey);  //密码装载到读写模块中

        [DllImport("dcrf32.dll")]
        public static extern short dc_write(int icdev, int adr, [In] byte[] sdata);  //向卡中写入数据
        [DllImport("dcrf32.dll")]
        public static extern short dc_write(int icdev, int adr, [In] string sdata);  //向卡中写入数据
        [DllImport("dcrf32.dll")]
        public static extern short dc_write_hex(int icdev, int adr, [In] string sdata);  //向卡中写入数据(转换为16进制)

        [DllImport("dcrf32.dll")]
        public static extern short dc_read(int icdev, int adr, [Out] byte[] sdata);

        [DllImport("dcrf32.dll")]
        public static extern short dc_read(int icdev, int adr, [MarshalAs(UnmanagedType.LPStr)] StringBuilder sdata);  //从卡中读数据
        [DllImport("dcrf32.dll")]
        public static extern short dc_read_hex(int icdev, int adr, [MarshalAs(UnmanagedType.LPStr)] StringBuilder sdata);  //从卡中读数据(转换为16进制)
        [DllImport("dcrf32.dll")]
        public static extern int a_hex(string oldValue, ref string newValue, Int16 len);  //普通字符转换成十六进制字符
        [DllImport("dcrf32.dll")]
        public static extern void hex_a(ref string oldValue, ref string newValue, int len);  //十六进制字符转换成普通字符

        #endregion

        private int _icdev = -1;

        public struct ICCardData
        {
            public string cardId;
        }

        private HttpListener HttpListener {get; set;}

        private EventLog logger = new EventLog("Application");
        private string port = null;

        /// <summary>
        /// 
        /// </summary>
        public ICCardService()
        {
            InitializeComponent();
            logger.Source = "ICCardService";

            this.port = ConfigurationManager.AppSettings["ServerPort"];
            string prefix = String.Format("http://+:{0}/read/", this.port);
            HttpListener = new HttpListener();
            HttpListener.Prefixes.Add(prefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            HttpListener.Start();
            HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            logger.WriteEntry("Service started at port "+this.port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        private void RequestCallback(IAsyncResult result)
        {
            Encoding encoding = new UTF8Encoding(false); // Whenever I have UTF8 problems it's BOM's fault
            HttpListenerContext context = null;
            try
            {
                context = HttpListener.EndGetContext(result);
                context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                context.Response.ContentEncoding = Encoding.UTF8;

                ICCardData obj = DirectReadCard();
                string json = GetJsonByObject(obj);
                byte[] outputBytes = encoding.GetBytes(json);

                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = outputBytes.Length;
                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);
                //logger.WriteEntry("完成HTTP请求，已应答。");
            }
            catch(Exception ex)
            {
                byte[] outputBytes = encoding.GetBytes(ex.Message);
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = outputBytes.Length;
                context.Response.OutputStream.Write(outputBytes, 0, outputBytes.Length);

                logger.WriteEntry("读取身份证出错："+ ex.Message);
            }
            finally
            {
                if (context != null && context.Response != null)
                {
                    context.Response.Close();
                }

                HttpListener.BeginGetContext(new AsyncCallback(RequestCallback), null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            HttpListener.Close();
            logger.WriteEntry("HttpListener Closed.");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnShutdown()
        {
            HttpListener.Close();
            logger.WriteEntry("HttpListener Closed.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ICCardData DirectReadCard()
        {
            InitIC();
            ICCardData result = ReadIc();
            Beep();
            ExitIC();
            return result;
        }

        /// <summary>
        /// 初始化串口1
        /// </summary>
        public void InitIC()
        {
            if (_icdev <= 0)
            {
                _icdev = dc_init(100, 115200);
            }

            if (_icdev <= 0)
            {
                throw new ApplicationException("连接读卡器失败");
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void ExitIC()
        {
            dc_exit((Int16)_icdev);
            _icdev = -1;
        }

        /// <summary>
        /// 读卡
        /// </summary>
        /// <returns></returns>
        public ICCardData ReadIc()
        {
            ulong icCardNo = 0;
            char str = (char)0;

            dc_reset(_icdev, 0);
            dc_card((Int16)_icdev, str, ref icCardNo);

            ICCardData result = new ICCardData
            {
                cardId = icCardNo.ToString()
            };
            return result;
        }

        /// <summary>
        /// 蜂鸣器
        /// </summary>
        public void Beep()
        {
            dc_beep((Int16)_icdev, 50);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string FmtDate(string date)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.CurrentCulture);
                return dt.ToString("yyyy-MM-dd");
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetJsonByObject(Object obj)
        {
            //实例化DataContractJsonSerializer对象，需要待序列化的对象类型
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //实例化一个内存流，用于存放序列化后的数据
            MemoryStream stream = new MemoryStream();
            //使用WriteObject序列化对象
            serializer.WriteObject(stream, obj);
            //写入内存流中
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            //通过UTF8格式转换为字符串
            return Encoding.UTF8.GetString(dataBytes);
        }
    }
}
