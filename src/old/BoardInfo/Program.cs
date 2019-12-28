using System;
using TeensySharp;

namespace BoardInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            var teensies = TeensyWatcher.getConnectedTeensies();

            foreach(var teensy in teensies)
            {
                Console.WriteLine($"{teensy.boardId}");
                Console.WriteLine($"{teensy.boardType}");
            }
            
            Console.WriteLine("\nPress any key");
            while (!Console.KeyAvailable) ;
        }
    }
}
