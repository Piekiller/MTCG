using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    public class Card
    {
        private Guid id;
        private Element element;
        private string name;
        private int damage;
        public Card Attack(Card other)
        {
            if (this.CalculateDamage(other) == other.CalculateDamage(this))
                return null;
            return this.CalculateDamage(other) > other.CalculateDamage(this) ? this : other;
        }

        private int CalculateDamage(Card other)
        {
            if (this.name == "Goblin" && other.name == "Dragon")
                return 0;

            if (this.name == "Ork" && other.name == "Wizard")
                return 0;

            if (this.name == "WaterSpell" && other.name == "Knight")
                return int.MaxValue;

            if (this is Spellcard && other.name == "Kraken")
                return 0;

            if (this.name=="Dragon" && other.name == "FireElve")
                return 0;

            if (this is Monstercard && other is Monstercard)
                return damage;
            /* water -> fire
            •  fire -> normal
            •  normal -> water*/
            if (this.element == Element.Water && other.element == Element.Fire) //water -> fire
                return damage * 2;

            if (this.element == Element.Fire && other.element == Element.Normal) //fire -> normal
                return damage * 2;

            if (this.element == Element.Normal && other.element == Element.Water)//normal -> water
                return damage * 2;


            if (this.element == Element.Fire && other.element == Element.Water) //fire <- water 
                return damage / 2;

            if (this.element == Element.Normal && other.element == Element.Fire)//normal <- fire
                return damage / 2;

            if (this.element == Element.Water && other.element == Element.Normal)//water <- normal
                return damage / 2;

            return damage;
        }
        public override string ToString()
        {
            return "Name: "+name+" Element: "+element+" Damage: "+damage;
        }
    }
}
