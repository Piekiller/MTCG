using System;
using MTCG;
namespace MTCGLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            User user = new User(Guid.NewGuid(), "te1", "123");
            Console.ReadKey();
            /*Server s = new Server();
            s.RegisterRoutes();
            Console.ReadKey();*/
        }
    }
}
