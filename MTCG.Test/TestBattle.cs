using NUnit.Framework;
using MTCG;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Test
{
    public class Tests
    {

        [Test]
        public void TestBattle()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c4 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);

            Card c5 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c6 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c7 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c8 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);

            List<Card> deck1 = new List<Card>() { c1, c2, c3, c4 };
            List<Card> deck2 = new List<Card>() { c5, c6, c7, c8 };
            User user = new User(Guid.NewGuid(), "p1", "test");
            User user2 = new User(Guid.NewGuid(), "p2", "test");
            user.Deck = deck1;
            user.Deck = deck2;
            List<string> log = Battle.StartBattle(user, user2);
            Assert.IsNotNull(log);
        }
        [Test]
        public void TestRandomBattle()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c4 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);
            Card c5 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c6 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);

            Card c7 = new Card(Guid.NewGuid(), Element.Fire, "FireElf", 10);
            Card c8 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c9 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c10 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);
            Card c11 = new Card(Guid.NewGuid(), Element.Normal, "Ork", 100);
            Card c12 = new Card(Guid.NewGuid(), Element.Fire, "Dragon", 10);

            List<Card> stack1 = new List<Card>() { c1, c2, c3, c4, c5, c6 };
            List<Card> stack2 = new List<Card>() { c7, c8, c9, c10, c11, c12 };
            User user = new User(Guid.NewGuid(), "p1", "test");
            User user2 = new User(Guid.NewGuid(), "p2", "test");
            user.Stack = stack1;
            user2.Stack = stack2;
            List<string> log = Battle.StartRandomBattle(user, user2);
            Assert.IsNotNull(log);
        }

    }
}