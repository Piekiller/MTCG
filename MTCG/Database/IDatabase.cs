using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    interface IDatabase
    {
        User ReadPlayer(string username);
        bool UpdatePlayer(User user);
        bool CreatePlayer(User user);

        bool CreateCard(Card card);
        Card ReadCard(Guid id);

        bool CreatePackage(Package pack);
        Package ReadPackage(Guid id);
        bool DeletePackage(Package pack);

        bool CreateDeck(User user);
        bool UpdateDeck(User user);
        List<Card> ReadDeck(User user);

        bool CreateStack(User user);
        bool UpdateStack(User user);
        List<Card> ReadStack(User user);

        bool CreateTrade(Trade trade);
        List<Trade> ReadTrades();
        bool UpdateTrade(Trade trade);

        List<User> ReadScoreboard();
    }
}
