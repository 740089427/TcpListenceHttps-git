using System;
using System.IO;
using System.Net;
using System.Text;

namespace HttpWebRequestDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("请输入要发送的数据...");
                var str = Console.ReadLine();
                SendHttps(str);
            }
        }

        public static void SendHttps(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://localhost:1024/");
            req.Method = "POST";
            using (Stream Requeststream = req.GetRequestStream())
            {
                Requeststream.Write(data, 0, data.Length);
                Requeststream.Close();

                Console.WriteLine("发送完成");
                HttpWebResponse httpWebResponse = (HttpWebResponse)req.GetResponse();
                Console.WriteLine("等待返回数据...");
                using (Stream responseStream = httpWebResponse.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
                    Console.WriteLine(streamReader.ReadToEnd());
                    Console.WriteLine("数据已收到");
                }
            }
        }
    }
}
