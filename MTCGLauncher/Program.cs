using System;
using MTCG;
namespace MTCGLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            PostgreSQLDB db = new PostgreSQLDB();
            //db.CreateCard(new Monstercard(Guid.NewGuid(), Element.Fire, "te", 1, false));
            Console.WriteLine();
            Console.ReadKey();
        }
    }
}
