using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MTCG
{
    public class User
    {
        public Guid ID { get; }
        public string Username { get; }
        public byte[] PWHash { get; }
        public List<Card> Stack { get; } = new List<Card>();
        public int Coins { get; }
        public List<Card> Deck { get; } = new List<Card>();
        public int ELO { get; private set; }
        public int WonGames { get; private set; }
        public string Image { get; }
        public string Bio { get; }

        public User(Guid id, string name, byte[] pwhash, int coins)
        {
            this.ID = id;
            this.Username = name;
            this.PWHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(id.ToString()+pwhash)) ;
            this.Coins = coins;
            this.ELO = 100;
            this.WonGames = 0;
            this.Image = string.Empty;
            this.Bio = string.Empty;
        }
        public User(Guid id, string name, byte[] pwhash, int coins, int elo, int wongames, string image, string bio)
        {
            this.ID = id;
            this.Username = name;
            this.PWHash = pwhash;
            this.Coins = coins;
            this.ELO = elo;
            this.WonGames = wongames;
            this.Image = image;
            this.Bio = bio;
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
