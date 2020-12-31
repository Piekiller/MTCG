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

        bool CreateDeck(Card[] deck);
        bool UpdateDeck(Card[] deck);
        Card[] ReadDeck(Guid player);

        bool CreateTrade(Trade trade);
        Trade[] ReadTrades();
        bool UpdateTrade(Trade trade);
    }
}
