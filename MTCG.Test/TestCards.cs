using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG.Test
{
    class TestCards
    {
        [Test]
        public void TestAttack()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElve", 10);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = c1.Attack(c2);
            Assert.AreSame(c2, c3);
        }
        [Test]
        public void TestDraw()
        {
            Card c1 = new Card(Guid.NewGuid(), Element.Fire, "FireElve", 20);
            Card c2 = new Card(Guid.NewGuid(), Element.Water, "Goblin", 20);
            Card c3 = c1.Attack(c2);
            Assert.IsNull(c3);
        }
        [Test]
        public void TestSpellFights2()
        {
            Card a = new Card(Guid.Empty, Element.Fire, "Firespell", 20);
            Card b = new Card(Guid.Empty, Element.Water, "Waterspell", 05);
            Card c = a.Attack(b);
            Assert.IsNull(c);
        }
        [Test]
        public void TestSpellFights()
        {
            Card a = new Card(Guid.Empty, Element.Fire, "Firespell", 10);
            Card b = new Card(Guid.Empty, Element.Water, "Waterspell", 20);
            Card c = a.Attack(b);
            Assert.AreEqual(b, c);
        }
        [Test]
        public void TestSpellFights3()
        {
            Card a = new Card(Guid.Empty, Element.Fire, "Firespell", 90);
            Card b = new Card(Guid.Empty, Element.Water, "Waterspell", 05);
            Card c = a.Attack(b);
            Assert.AreEqual(a, c);
        }
        [Test]
        public void TestMixedFights()
        {
            Card a = new Card(Guid.Empty, Element.Fire, "Firespell", 10);
            Card b = new Card(Guid.Empty, Element.Water, "Watergoblin", 10);
            Card c = a.Attack(b);
            Assert.AreEqual(b, c);
        }
        [Test]
        public void TestMixedFights2()
        {
            Card a = new Card(Guid.Empty, Element.Water, "Waterspell", 10);
            Card b = new Card(Guid.Empty, Element.Water, "Watergoblin", 10);
            Card c = a.Attack(b);
            Assert.IsNull(c);
        }
        [Test]
        public void TestMixedFights3()
        {
            Card a = new Card(Guid.Empty, Element.Normal, "Regularspell", 10);
            Card b = new Card(Guid.Empty, Element.Water, "Watergoblin", 10);
            Card c = a.Attack(b);
            Assert.AreEqual(a, c);
        }
       [Test]
        public void TestMixedFights4()
        {
            Card a = new Card(Guid.Empty, Element.Normal, "Regularspell", 10);
            Card b = new Card(Guid.Empty, Element.Normal, "Knight", 15);
            Card c = a.Attack(b);
            Assert.AreEqual(a, c);
        }
    }
}
