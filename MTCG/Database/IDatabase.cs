using System;
using System.Collections.Generic;
using System.Text;

namespace MTCG
{
    interface IDatabase
    {
        bool SavePlayer(User user);
        User ReadPlayer(string username);
        bool UpdatePlayer(User user);
    }
}
