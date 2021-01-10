using System;
using MTCG;
namespace MTCGLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server(new PostgreSQLDB());
            s.RegisterRoutes();
            Console.ReadKey();
        }
    }
}
