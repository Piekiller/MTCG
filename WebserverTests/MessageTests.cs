using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Webserver;

namespace WebserverTests
{
    class MessageTests
    {
        [Test]
        public void TestGetAllMessagesEmpty()
        {
            Messages messages = new Messages();
            string res=messages.GetMessages();

            Assert.AreEqual("", res);
        }
        [Test]
        public void TestGetAllMessagesOneMessage()
        {
            Messages messages = new Messages();
            messages.PostMessage("Message1");
            string res = messages.GetMessages();

            Assert.AreEqual("Message1\n", res);
        }
        [Test]
        public void TestPostMessages()
        {
            Messages messages = new Messages();
            int index=messages.PostMessage("Message1");
            string res = messages.GetMessage(index);

            Assert.AreEqual("Message1", res);
        }
        [Test]
        public void TestPutMessages()
        {
            Messages messages = new Messages();
            messages.PutMessage(1,"Message1");
            string res = messages.GetMessage(1);
            Assert.AreEqual("Message1", res);
        }
        [Test]
        public void TestPutMessagesError()
        {
            Messages messages = new Messages();
            messages.PutMessage(1001, "Message1");
            string res = messages.GetMessage(1);
            Assert.AreEqual(null, res);
        }
        [Test]
        public void TestDeleteMessages()
        {
            Messages messages = new Messages();
            messages.PostMessage("Message1");
            bool status=messages.DeleteMessage(1);
            string res = messages.GetMessage(1);
            Assert.AreEqual(null, res);
            Assert.AreEqual(true, status);
        }
        [Test]
        public void TestDeleteMessagesError()
        {
            Messages messages = new Messages();
            bool status = messages.DeleteMessage(1001);
            string res = messages.GetMessage(1);
            Assert.AreEqual(null, res);
            Assert.AreEqual(false, status);
        }
    }
}
