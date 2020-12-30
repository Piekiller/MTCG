using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Webserver
{
    public class RequestContext
    {
        public string Http_verb { get; }
        public string Path { get; }
        public string Http_version { get; }
        public Dictionary<string, string> Header { get; } = new Dictionary<string, string>();
        public string Payload { get; }
        public int Requestedfield { get; }

        public RequestContext(string verb, string path, string http_version, Dictionary<string,string> header, string payload,int requestedfield)
        {
            this.Http_verb = verb;
            this.Path = path;
            this.Http_version = http_version;
            this.Header = header;
            this.Payload = payload;
            this.Requestedfield = requestedfield;
        }
    }
}
