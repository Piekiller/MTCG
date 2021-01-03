namespace MTCG
{
    public class Trade
    {
        public Card card;
        public Cardtype cardtype;
        public Element element;
        public int minDamage;//minDamage of other Card

        public Trade(Card card,Cardtype cardtype,Element element)
        {
            this.card = card;
            this.cardtype = cardtype;
            this.element = element;
        }
        public Trade(Card card, Cardtype cardtype, int minDamage)
        {
            this.card = card;
            this.cardtype = cardtype;
            this.minDamage = minDamage;
        }

        public bool TryTrade(Card other)
        {
            if (cardtype == other.type)
            {
                if (element != default && other.element == element)//Schauen ob das Element stimmt (Damage egal)
                {
                    return true;
                }
                else if(minDamage != default && other.damage >= minDamage)//Schauen ob der Damage passt (Element egal)
                {
                    return true;
                }
            }
            return false;
        }
    }
}