using System;
using libTeensySharp;

namespace TeensySharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            var Watcher = new TeensyWatcher();
            foreach (var Teensy in Watcher.ConnectedTeensies)
            {
                Console.WriteLine($"Found {Teensy.Description}");
                Console.WriteLine($"  Serial number: {Teensy.Serialnumber}");
                Console.WriteLine($"  Board type:    {Teensy.BoardType}");
                Console.WriteLine($"  Ports:    ");
                foreach (var port in Teensy.Ports)
                {
                    Console.WriteLine($"    - {port}");
                }

                Console.WriteLine();

            }

        }
    }
}

