using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Webserver;

namespace WebserverTests
{
    class WebServerTests
    {
        [Test]
        public void RegisterRouteTest()
        {
            HTTPServer server = new HTTPServer();
            bool status=server.RegisterRoute("GET", "/messages", (RequestContext rc, StreamWriter sw) => { });
            Assert.AreEqual(status, true);
        }
        [Test]
        public void InvalidRegisterRouteTest()
        {
            HTTPServer server = new HTTPServer();
            bool status = server.RegisterRoute("GET", "/messages", (RequestContext rc, StreamWriter sw) => { });
            status = server.RegisterRoute("GET", "/messages", (RequestContext rc, StreamWriter sw) => { });
            Assert.AreEqual(status, false);
        }
    }
}
