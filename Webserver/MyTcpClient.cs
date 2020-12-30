using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Webserver
{
    class MyTcpClient : ITcpClient
    {
        private TcpClient _tcpClient;
        public MyTcpClient(TcpClient client)
        {
            _tcpClient = client;
        }
        public void Close()=>
            _tcpClient.Close();

        public Stream GetReadStream()=>
            _tcpClient.GetStream();

        public Stream GetWriteStream() =>
            _tcpClient.GetStream();
    }
}
