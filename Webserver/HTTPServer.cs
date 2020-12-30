using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Linq;

namespace Webserver
{
    public class HTTPServer
    {
        private ITcpListener server;
        private Dictionary<(string verb, string path), Action<RequestContext, StreamWriter>> routes = new Dictionary<(string verb, string path), Action<RequestContext, StreamWriter>>();
        public HTTPServer(int port)
        {
            server = new MyTcpListener(port);
        }
        public HTTPServer()
        {

        }
        public void Start()
        {
            try
            {
                server.Start();
                while (true)
                {
                    ITcpClient client = server.AcceptTcpClient();
                    Task.Run(() => HandleClient(client));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }
        public bool RegisterRoute(string verb, string path, Action<RequestContext, StreamWriter> action)=>routes.TryAdd((verb, path), action);
        public void HandleClient(ITcpClient client)
        {
            using StreamReader sr = new StreamReader(client.GetReadStream());
            using StreamWriter sw = new StreamWriter(client.GetWriteStream());
            Console.WriteLine("Client connected");

            string message = sr.ReadLine(); //Erste Line von dem HTTP Request
            Console.WriteLine("\nFirst Line: ");
            Console.WriteLine(message);
            string[] splits = message.Split(" ");
            if (splits.Length < 3)//Bei zu wenigen Parametern BadRequest
            {
                SendError(sw, HttpStatusCode.BadRequest);
                return;
            }
            string verb = splits[0];
            string path = splits[1];
            string httpVersion = splits[2];
            Dictionary<string, string> header = ReadHeaders(sr);
            RequestContext rc = ReadPayload(sr, path, header, verb, httpVersion);
            KeyValuePair<(string verb, string path), Action<RequestContext, StreamWriter>> res = GetBestRoute(verb, path);
            if (res.Value == null)
            {
                SendError(sw, HttpStatusCode.NotFound);
                return;
            }
            res.Value(rc, sw);
        }

        private KeyValuePair<(string verb, string path), Action<RequestContext, StreamWriter>> GetBestRoute(string verb, string path)
        {
            return routes.Where((keyval, action) =>
            {
                int indexofSlash = path.LastIndexOf('/');
                return keyval.Key.verb == verb && keyval.Key.path.Trim('*') == (indexofSlash == 0 ? path : path.Substring(0, indexofSlash + 1));
            }).FirstOrDefault();
        }

        private RequestContext ReadPayload(StreamReader sr, string path, Dictionary<string, string> header,string verb,string httpVersion)
        {
            Console.WriteLine("\nPayload: ");
            var contentset = header.Where((val) => val.Key == "Content-Length").FirstOrDefault();
            int contentlength = 0;
            int.TryParse(contentset.Value, out contentlength);//If contentlength not parsable content empty
            char[] payload = new char[contentlength];
            int requestedID = 0;
            string h = path.Substring(path.LastIndexOf('/') + 1);
            int.TryParse(h, out requestedID);
            sr.ReadBlock(payload);
            Console.WriteLine(payload);
            return new RequestContext(verb, path, httpVersion, header, new string(payload), requestedID);
        }
        private Dictionary<string,string> ReadHeaders(StreamReader sr)
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            string message = null;
            Console.WriteLine("\nHeaders: ");
            while (!sr.EndOfStream)
            {
                message = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(message))//Bei Leerzeile, also Head Block Ende
                    break;
                Console.WriteLine(message);
                int indexof = message.IndexOf(':');//get index of first : in case of host: localhost:10000
                if (indexof < 0)
                    continue;
                header.Add(message.Substring(0, indexof), message.Substring(indexof + 1).Trim(' '));
            }
            return header;
        }

        public static void SendSuccess(StreamWriter stream, HttpStatusCode statuscode, string message)
        {
            string response = $"HTTP/1.1 {(int)statuscode} {statuscode}\nContent-Length: {message.Length}\nContent-Type: text/plain; charset=utf-8\n\n{message}";
            stream.WriteLine(response);
            stream.Flush();
        }
        public static void SendError(StreamWriter stream, HttpStatusCode statuscode)
        {
            string response = $"HTTP/1.1 {(int)statuscode} {statuscode}\nContent-Length: 0\n";
            stream.WriteLine(response);
            stream.Flush();
        }
    }
}
