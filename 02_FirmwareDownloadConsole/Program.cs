using System;
using System.IO;
using TeensySharp;

namespace SimpleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Two test files 
            string file1 = "blink_slow.hex";
            string file2 = "blink_fast.hex";

            string testfile = file1;

            // Define the board to be programmed (all boards are implemented but currently only Teensy 3.1 is tested) 
            var Board = PJRC_Board.Teensy_31;

            // Obtain an empty flash image with the correct size and all bytes cleared (set to 0xFF)
            var FlashImage = SharpUploader.GetEmptyFlashImage(Board);

            using (var HexStream = File.OpenText(testfile))
            {
                // parse the file and write output to the image 
                SharpHexParser.ParseStream(HexStream, FlashImage);
                HexStream.Close();
            }

            // Upload image to the board and reboot
            int result = SharpUploader.Upload(FlashImage, Board, reboot: true);

            // Show result
            switch (result)
            {
                case 0:
                    Console.WriteLine("Successfully uploaded");
                    break;
                case 1:
                    Console.WriteLine("Found no board with running HalfKay. Did you press the programming button?");
                    Console.WriteLine("Aborting...");
                    break;
                case 2:
                    Console.WriteLine("Error during upload.");
                    Console.WriteLine("Aborting...");
                    break;
            }
            Console.WriteLine("\nPress any key");
            while (!Console.KeyAvailable) ;
        }
    }
}
