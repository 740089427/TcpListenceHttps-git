using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace TcpListenceHttps
{
    class Program
    {
        private static string InstallCertificate(byte[] bytes)
        {
            X509Certificate2 certificate = new X509Certificate2(bytes, "123");
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
            store.Close();
            return certificate.GetSerialNumberString();
        }

        public static string ReadOnce(SslStream sslStream, int size = 1024)
        {
            byte[] buffer = new byte[size];
            var bytes = sslStream.Read(buffer, 0, buffer.Length);
            Decoder decoder = Encoding.UTF8.GetDecoder();
            var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
            decoder.GetChars(buffer, 0, bytes, chars, 0);
            return new string(chars);
        }

        public static string ReadAll(SslStream sslStream, int contentlength)
        {
            int readLength = 0;
            List<byte> resultBytes = new List<byte>();
            while (readLength < contentlength)
            {
                byte[] buffer = new byte[1024];
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                readLength += bytes;
                resultBytes.AddRange(buffer.Take(bytes).ToArray());
            }
            return Encoding.UTF8.GetString(resultBytes.ToArray());
        }
        public static void RequestWork(TcpListener obj)
        {
            TcpClient client = obj.AcceptTcpClient();
            var networkStream = client.GetStream();
            Console.WriteLine("有连接接入...");
            while (networkStream.CanRead)
            {
                try
                {
                    using (var sslStream = new SslStream(networkStream))
                    {
                        #region 写入证书
                        try
                        {
                            var test = InstallCertificate(Properties.Resources.myCert);
                            X509Store store = new X509Store(StoreName.Root);
                            store.Open(OpenFlags.IncludeArchived);
                            X509Certificate2 cert =
                                store.Certificates.Find(X509FindType.FindBySerialNumber, test, false)[0];
                            sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Console.ReadLine();
                            continue;
                        }
                        #endregion

                        #region 读取数据
                        var strHead = ReadOnce(sslStream) + ReadOnce(sslStream);
                        if (string.IsNullOrWhiteSpace(strHead))
                        {
                            // todo:证书不对
                        }
                        var strHeadList = strHead.Split(new[] { "\r\n" }, StringSplitOptions.None);
                        string body = "";
                        foreach (var s in strHeadList)
                        {
                            int length = "Content-Length:".Length;
                            if (s.IndexOf("Content-Length:") > -1)
                            {
                                var contentlength = int.Parse(s.Substring(length, s.Length - length).Trim());
                                if (contentlength > 0)
                                {
                                    body = ReadAll(sslStream, contentlength);
                                    //todo:处理 body
                                    Console.WriteLine("rawBody:" + body);
                                }
                            }
                        }

                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        var rawUrlData = strHeadList[0].Split(' ')[1].TrimStart('?', '/');
                        NameValueCollection rawUrlDataCollection = HttpUtility.ParseQueryString(rawUrlData);
                        Console.WriteLine("url参数:" + rawUrlData);
                        foreach (string key in rawUrlDataCollection.Keys)
                        {
                            if (!string.IsNullOrWhiteSpace(key))
                            {
                                dic.Add(key, rawUrlDataCollection[key]);
                            }
                        }
                        string callbackStr = "";
                        if (dic.Keys.Contains("callback") && !string.IsNullOrWhiteSpace(dic["callback"]))
                        {
                            callbackStr = dic["callback"];
                        }
                        //todo:处理 url其他参数
                        #endregion

                        #region 返回数据
                        string statusLine = "HTTP/1.1 200 OK\r\n";
                        byte[] responsestatusLineBytes = Encoding.UTF8.GetBytes(statusLine);

                        //?callback = jQuery110205463076986739899_1532054092048 &
                        string resultStr = "123";
                        if (!string.IsNullOrWhiteSpace(callbackStr))
                        {
                            resultStr = string.Format("{0}({1})", callbackStr, resultStr);
                        }
                        byte[] responseBodyBytes = Encoding.UTF8.GetBytes(resultStr);
                        string responseHead = string.Format(
                            "Content-Type:text/html;charset=UTF-8\r\nContent-Length:{0}\r\n", resultStr.Length);
                        byte[] responseHeadBytes = Encoding.UTF8.GetBytes(responseHead);
                        sslStream.Write(responsestatusLineBytes, 0, responsestatusLineBytes.Length);
                        sslStream.Write(responseHeadBytes, 0, responseHeadBytes.Length);
                        sslStream.Write(new byte[] { 13, 10 }, 0, 2);
                        sslStream.Write(responseBodyBytes, 0, responseBodyBytes.Length);
                        #endregion

                        networkStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 1024);
            listener.Start();
            while (true)
            {
                Console.WriteLine("等待客户端的连接...");
                RequestWork(listener);
            }
        }
    }
}
