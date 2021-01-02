using System;
using System.Collections.Generic;

namespace MTCG
{
    public class Package
    {
        public Guid id;
        public List<Card> cards = new List<Card>();
        public Package(Guid id, List<Card> cards)
        {
            this.id = id;
            this.cards = cards;
        }
    }
}