using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    class Battle
    {
        static private Random _rn = new Random();
        public static List<string> Startbattle(User player1, User player2)
        {
            List<string> results = new List<string>();

            List<Card> p1 = new List<Card>();
            List<Card> p2 = new List<Card>();
            p1.AddRange(player1.Deck);
            p2.AddRange(player2.Deck);
            int i;
            for (i = 0; !(!(i < 100)||!(p1.Count!=0)||!(p2.Count!=0)); i++)
            {
                Card c1 = p1[_rn.Next(p1.Count)];
                Card c2 = p2[_rn.Next(p2.Count)];

                Card winner = c1.Attack(c2);
                results.Add("Counts: Player 1: " + p1.Count + " Player 2: " + p2.Count);
                results.Add(c1.ToString() + " vs " + c2.ToString());
                if (winner == null)
                {
                    results.Add($"Round: {i} Draw");
                    continue;
                }  
                if (c1 == winner)
                {
                    results.Add($"Round: {i} Player 1 wins");
                    p1.Add(c2);
                    p2.Remove(c2);
                }
                else if (c2 == winner)
                {
                    results.Add($"Round: {i} Player 2 wins");
                    p2.Add(c1);
                    p1.Remove(c1);
                }
            }
            if (p1.Count == 0)
            {
                player2.Win();
                player1.Lose();
            }

            if (p2.Count == 0)
            {
                player1.Win();
                player2.Lose();
            }

            if (i == 100)
            {
                player1.Draw();
                player2.Draw();
            }
            return results;
        }
    }
}
