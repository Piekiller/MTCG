using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Webserver;

namespace MTCG.Test
{
    class TestAuthorization
    {
        [Test]
        public void TestCheckAuthorization()
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Authorization", "Basic altenhof-mtcgToken");

            RequestContext rc = new RequestContext("demo","egal","http",header,"",0);
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Server s = new Server(db);
            Assert.NotNull(s.CheckAuthorization(rc));
        }
        [Test]
        public void TestRegister()
        {
            User user = new User(Guid.NewGuid(), "testuser", "user");
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");

            Server s = new Server(db);
            Assert.IsTrue(s.Register(user));//only true if db empty

            Assert.AreEqual(user, db.ReadPlayer(user.Username));
            Assert.AreEqual(user, db.ReadPlayer(user.ID));
        }
        [Test]
        public void TestLogin()
        {
            User user = new User(Guid.NewGuid(), "testuser", "user");
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreatePlayer(user));

            Server s = new Server(db);
            Server.UserDes us = new Server.UserDes();
            us.Username = "testuser";
            us.Password = "user";
            Assert.IsTrue(s.Login(us));
        }
        

    }
}
