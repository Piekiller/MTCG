using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    public class Card
    {
        public Guid id;
        public Element element;
        public string name;
        public int damage;
        public bool isLocked;
        public Cardtype type;
        public Card(Guid id, Element element, string name, int damage, bool isLocked)
        {
            this.id = id;
            this.element = element;
            this.name = name;
            this.damage = damage;
            this.isLocked = isLocked;
            this.type = name.ToLower().Contains("spell") ? Cardtype.Spell : Cardtype.Monster;
        }
        public Card(Guid id, Element element, string name, int damage)
        {
            this.id = id;
            this.element = element;
            this.name = name;
            this.damage = damage;
            this.isLocked = false;
            this.type=name.ToLower().Contains("spell")?Cardtype.Spell:Cardtype.Monster;

        }
        public Card Attack(Card other)
        {
            if (this.CalculateDamage(other) == other.CalculateDamage(this))
                return null;
            return this.CalculateDamage(other) > other.CalculateDamage(this) ? this : other;
        }
        private int CalculateDamage(Card other)
        {
            if (this.name.ToLower().Contains("goblin") && other.name.ToLower().Contains("dragon"))
                return 0;

            if (this.name.ToLower().Contains("ork") && other.name.ToLower().Contains("wizard"))
                return 0;

            if (this.name == "WaterSpell" && other.name.ToLower().Contains("knight"))
                return int.MaxValue;

            if (this.type==Cardtype.Spell&& other.name.ToLower().Contains("kraken"))
                return 0;

            if (this.name.ToLower().Contains("Dragon") && other.name == "FireElve")
                return 0;

            if (this.type == Cardtype.Monster && other.type==Cardtype.Spell)
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

        public override bool Equals(object obj)
        {
            return this.id.Equals((obj as Card).id);
        }
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}
