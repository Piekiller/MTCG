using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    public class Battle
    {
        static private Random _rn = new Random();
        public static List<string> StartBattle(User player1, User player2)
        {
            List<string> results = new List<string>();

            List<Card> p1 = new List<Card>();
            List<Card> p2 = new List<Card>();
            p1.AddRange(player1.Deck);
            p2.AddRange(player2.Deck);//copy decks as they would change decks of the player itself
            int i;
            for (i = 0; !(!(i < 100) || !(p1.Count != 0) || !(p2.Count != 0)); i++)//check if i<100 and no one has lost
            {
                Card c1 = p1[_rn.Next(p1.Count)];//gets a random card of the deck
                Card c2 = p2[_rn.Next(p2.Count)];

                Card winner = c1.Attack(c2);//gets the winner of a battle
                results.Add("Counts: Player 1: " + p1.Count + " Player 2: " + p2.Count);//Log
                results.Add(c1.ToString() + " vs " + c2.ToString());
                if (winner == null)
                    results.Add($"Round: {i} Draw");
                else if (c1 == winner)
                {
                    results.Add($"Round: {i} Player 1 wins");
                    p1.Add(c2);//add card of this round to p1 deck and remove from p2 deck
                    p2.Remove(c2);
                }
                else if (c2 == winner)
                {
                    results.Add($"Round: {i} Player 2 wins");
                    p2.Add(c1);
                    p1.Remove(c1);
                }
            }
            if (p1.Count == 0)//Get the loser
            {
                player2.Win();
                player1.Lose();
            }
            else if (p2.Count == 0)
            {
                player1.Win();
                player2.Lose();
            }
            else if (i == 100)
            {
                player1.Draw();
                player2.Draw();
            }
            return results;
        }
        public static List<string> StartRandomBattle(User player1, User player2)
        {
            List<string> results = new List<string>();

            List<Card> p1 = new List<Card>();
            List<Card> p2 = new List<Card>();
            for (int j = 0; j < 4; j++)
            {
                Card c1 = player1.Stack[_rn.Next(player1.Stack.Count)];

                if (p1.Contains(c1)||c1.isLocked)
                {
                    j--;
                    continue;
                }
                p1.Add(c1);
            }
            for (int k = 0; k < 4; k++)
            {
                Card c2 = player2.Stack[_rn.Next(player2.Stack.Count)];
                if (p2.Contains(c2)||c2.isLocked)
                {
                    k--;
                    continue;
                }
                p2.Add(c2);
            }
            int i;
            for (i = 0; !(!(i < 100) || !(p1.Count != 0) || !(p2.Count != 0)); i++)//check if i<100 and no one has lost
            {
                Card c1 = p1[_rn.Next(p1.Count)];//gets a random card of the deck
                Card c2 = p2[_rn.Next(p2.Count)];

                Card winner = c1.Attack(c2);//gets the winner of a battle
                results.Add("Counts: Player 1: " + p1.Count + " Player 2: " + p2.Count);//Log
                results.Add(c1.ToString() + " vs " + c2.ToString());
                if (winner == null)
                    results.Add($"Round: {i} Draw");
                else if (c1 == winner)
                {
                    results.Add($"Round: {i} Player 1 wins");
                    p1.Add(c2);//add card of this round to p1 deck and remove from p2 deck
                    p2.Remove(c2);
                }
                else if (c2 == winner)
                {
                    results.Add($"Round: {i} Player 2 wins");
                    p2.Add(c1);
                    p1.Remove(c1);
                }
            }
            if (p1.Count == 0)//Get the loser
            {
                player2.Win();
                player1.Lose();
            }
            else if (p2.Count == 0)
            {
                player1.Win();
                player2.Lose();
            }
            else if (i == 100)
            {
                player1.Draw();
                player2.Draw();
            }
            return results;
        }
    }
}
