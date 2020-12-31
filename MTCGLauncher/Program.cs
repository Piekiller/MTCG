using System;
using MTCG;
namespace MTCGLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            PostgreSQLDB db = new PostgreSQLDB();
            User user = new User(Guid.NewGuid(),"bob", "test", 20);
            Console.WriteLine(   db.CreatePlayer(user));
        }
    }
}
