cmd  CERTMGR.MSC 打开证书管理器

1、设置 HttpWebRequestDemo 和 TcpListenceHttps 同时启动
2、使用 管理员  运行 Visual Studio Tools　里面的　VS 2017的开发人员命令提示符 　
输入　　makecert -n "CN=localhost" -ss MY -sr LocalMachine -b 08/09/2016 -e 09/09/2999 -a sha256 -sky exchange -r -pe D:\myCert.cer
设置 密码 为 123(好像没有密码)
可以自己修改证书　输出路径 有效期
3、双击 myCert.cer 安装证书 ，存储位置 选择 计算机 ，安装到手信任的跟证书颁发机构 里面 
4、在 HttpWebRequestDemo 输入  任意字符 回车
5、查看 TcpListenceHttps 是否收到数据

其他方案：
使用 httpListener 的时候 不需要 读取什么证书  仅仅 需要 
cmd netsh http add sslcert ipport=0.0.0.0:8443 certhash=110000000000003ed9cd0c315bbb6dc1c08da5e6 appid={00112233-4455-6677-8899-AABBCCDDEEFF}
绑定 程序集、证书、url 就行


据说 socket 也是可以的 

提示：
Solved: Save encoded certificate to store failed = 0x5 (5)
则需要管理员运行

可以使用 content-length 来判断 相应体里面已接收多少字节，还需要接收多少字节
而且 应该累加 byte[] 而不是 string  ，比如 传了 一个汉字， 结果还是需要读取两次，最终没法拼成一个汉字
