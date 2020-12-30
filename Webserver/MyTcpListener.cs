using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Webserver
{
    class MyTcpListener : ITcpListener
    {
        private TcpListener _listener;
        public MyTcpListener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }
        public ITcpClient AcceptTcpClient() =>
            new MyTcpClient(_listener.AcceptTcpClient());

        public void Start()
        {
            _listener.Start();
        }
    }
}
