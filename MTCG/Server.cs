using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    class Server
    {
        List<User> _activeusers=new List<User>();

        void Login(string user, string pw)
        {

        }

        void Register(string user, string pw)
        {

        }

        List<Card> AquireCards(User user)
        {
            throw new NotImplementedException();
        }

        void JoinMatchmaking(User user)
        {

        }

        void LeaveMatchmaking(User user)
        {

        }

        Scoreboard Scoreboard()
        {
            throw new NotImplementedException();
        }

        bool ChangeUsername(string username)
        {
            throw new NotImplementedException();
        }

        bool ChangePassword(string password)
        {
            throw new NotImplementedException();
        }

        int GetELO(User user) => user.ELO;

        List<Card> GetPackage(User user)
        {
            throw new NotImplementedException();
        }


    }
}
