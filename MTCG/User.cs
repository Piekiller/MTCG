using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
namespace MTCG
{
    public class User
    {
        public Guid ID { get; }
        public string Username { get; set; }
        public byte[] PWHash { get; private set; }
        public List<Card> Stack { get; private set; } = new List<Card>();
        public int Coins { get; set; }
        public List<Card> Deck { get; set; } = new List<Card>();
        public int ELO { get; private set; }
        public int WonGames { get; private set; }
        public string Image { get; set; }
        public string Bio { get; set; }

        public User(Guid id, string name, string pw)
        {
            this.ID = id;
            this.Username = name;
            SetPassword(pw);
            this.Coins = 20;
            this.ELO = 100;
            this.WonGames = 0;
            this.Image = string.Empty;
            this.Bio = string.Empty;
        }
        public User(Guid id, string name, byte[] pwhash, int coins, int elo, int wongames, string image, string bio,List<Card> deck, List<Card> stack)
        {
            this.ID = id;
            this.Username = name;
            this.PWHash = pwhash;
            this.Coins = coins;
            this.ELO = elo;
            this.WonGames = wongames;
            this.Image = image;
            this.Bio = bio;
            this.Deck = deck;
            this.Stack = stack;
        }
        public void SetPassword(string pw) => PWHash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(ID.ToString() + pw));
        public bool CheckPassword(string pw)=> SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(ID.ToString() + pw)).SequenceEqual(this.PWHash);
        public void Win()
        {
            WonGames++;
            ELO += 3;
        }
        public void Lose()
        {
            ELO -= 5;
        }
        public void Draw()
        {
            //Could add functionality for handling draws
        }
    }
}
