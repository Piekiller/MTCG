using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    class User
    {
        public Guid ID { get; }
        public string Username { get; }
        public string PWHash { get; }
        public List<Card> Stack { get; } = new List<Card>();
        public int Coins { get; }
        public List<Card> Deck { get; } = new List<Card>();
        public int ELO { get; private set; }
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

        public void Win()
        {
            WonGames++;
            ELO += 3;
        }
        public void Lose()
        {
            WonGames++;
            ELO -= 5;
        }
        public void Draw()
        {
            WonGames++;
        }
    }
}
