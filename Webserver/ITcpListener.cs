using System;
using System.Collections.Generic;
using System.Text;

namespace Webserver
{
    interface ITcpListener
    {
        ITcpClient AcceptTcpClient();
        void Start();
    }
}
