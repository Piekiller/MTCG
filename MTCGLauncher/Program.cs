using System;
using MTCG;
namespace MTCGLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            s.RegisterRoutes();
            Console.ReadKey();
        }
    }
}
