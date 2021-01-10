using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Linq;
namespace MTCG.Test
{
    class TestDatabase
    {
        [Test]
        public void TestCreateCard()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreateCard(c1));
            Card c2 = db.ReadCard(c1.id);
            Assert.IsNotNull(c2);
            Assert.AreEqual(c1, c2);
        }
        [Test]
        public void TestCreatePlayer()
        {
            User user = new User(Guid.NewGuid(), "testuser", "user");
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreatePlayer(user));
            User us = db.ReadPlayer(user.ID);
            Assert.IsNotNull(us);
            Assert.AreEqual(us.ID, user.ID);
        }
        [Test]
        public void TestCreateStack()
        {
            User user = new User(Guid.NewGuid(), "User1", "test");
            List<Card> stack = new List<Card>();
            user.Stack = stack;
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreateStack(user));
            List<Card> stack2 = db.ReadStack(user.ID);
            Assert.IsNotNull(stack2);
            Assert.AreEqual(stack2, user.Stack);
        }
        [Test]
        public void TestCreatePackage()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c4 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);
            Card c5 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Package package = new Package(Guid.NewGuid(), new List<Card>() { c1, c2, c3, c4, c5 });
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreatePackage(package));

            Package pack = db.ReadPackages().Where(v => v.id == package.id).FirstOrDefault();
            Assert.AreEqual(package.id, pack.id);
        }

        [Test]
        public void TestCreateTrade()
        {
            User user = new User(Guid.NewGuid(), "User1", "test");
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Trade trade = new Trade(user, Guid.NewGuid(), c1, Cardtype.Spell, 1000);
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreateTrade(trade));

            Trade trade2 = db.ReadTrades().Where(v => v.id == trade.id).FirstOrDefault();
            Assert.AreEqual(trade.id, trade2.id);
        }
        [Test]
        public void TestCreateDeck()
        {
            User user = new User(Guid.NewGuid(), "User1", "test");
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c4 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);
            List<Card> deck = new List<Card>();
            user.Deck = deck;
            IDatabase db = new PostgreSQLDB("Mtcg", "mtcg");
            Assert.IsTrue(db.CreateDeck(user));

            List<Card> deck2 = db.ReadDeck(user.ID);
            Assert.IsNotNull(deck2);
            Assert.AreEqual(deck2, user.Deck);
        }
    }
}
