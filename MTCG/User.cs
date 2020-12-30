using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    class User
    {
        public Guid ID { get; }
        public string Username { get;  }
        public string PWHash { get; }
        private List<Card> _stack = new List<Card>();
        public int Coins { get; }
        private List<Card> _deck = new List<Card>();
        public int ELO { get; }
        private int WonGames;

        public User(Guid id, string name, string pwhash, int coins)
        {
            this.ID = id;
            this.Username = name;
            this.PWHash = pwhash;
            this.Coins = coins;
            this.ELO = 100;
            this.WonGames = 0;
        }
    }
}
