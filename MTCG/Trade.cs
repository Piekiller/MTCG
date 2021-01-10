using System;

namespace MTCG
{
    public class Trade
    {
        public User user;
        public Guid id;
        public Card card;
        public Cardtype cardtype;
        public Element element;
        public int minDamage;//minDamage of other Card

        public Trade(User user, Guid id, Card card, Cardtype cardtype, Element element)//two constructors for different requirements
        {
            this.id = id;
            this.user = user;
            this.card = card;
            this.cardtype = cardtype;
            this.element = element;
        }
        public Trade(User user, Guid id, Card card, Cardtype cardtype, int minDamage)
        {
            this.id = id;
            this.user = user;
            this.card = card;
            this.cardtype = cardtype;
            this.minDamage = minDamage;
        }

        public bool TryTrade(Card other)
        {
            if (cardtype == other.type)//check for the correct cardtype(Monster,Spell)
            {
                if (element != default && other.element == element)//check if the element is set and then if its the correct element
                {
                    return true;
                }
                else if (minDamage != default && other.damage >= minDamage)//same as previous but with mindamage
                {
                    return true;
                }
            }
            return false;
        }
    }
}