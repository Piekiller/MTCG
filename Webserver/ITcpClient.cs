using System.IO;
using System.Net.Sockets;

namespace Webserver
{
    public interface ITcpClient
    {
        Stream GetReadStream();
        Stream GetWriteStream();

        public void Close();
    }
}