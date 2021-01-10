using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    interface IDatabase
    {
        User ReadPlayer(string username);
        User ReadPlayer(Guid id);
        bool UpdatePlayer(User user);
        bool CreatePlayer(User user);

        bool CreateCard(Card card);
        Card ReadCard(Guid id);

        bool CreatePackage(Package pack);
        List<Package> ReadPackages();
        bool DeletePackage(Package pack);

        bool CreateDeck(User user);
        bool UpdateDeck(User user);
        List<Card> ReadDeck(Guid guid);

        bool CreateStack(User user);
        bool UpdateStack(User user);
        List<Card> ReadStack(Guid guid);

        bool CreateTrade(Trade trade);
        List<Trade> ReadTrades();
        bool DeleteTrade(Guid id);

        List<User> ReadScoreboard();
    }
}
