using System;
using System.IO;
using TeensySharp;
using System.Linq;
using System.Collections.Generic;

namespace SimpleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Select the board to be programmed
            //var Board = PJRC_Board.Teensy_36;
            //var Board = PJRC_Board.Teensy_35;
            //var Board = PJRC_Board.Teensy_31;
            //var Board = PJRC_Board.Teensy_30;
            var Board = PJRC_Board.Teensy_LC;

            string hexFile;

            //select the firmware to download 
            switch (Board)
            {
                case PJRC_Board.Teensy_36:                       
                    hexFile = "blink_36.hex";
                    Console.WriteLine("Selected Board: Teensy 3.6");
                    break;
                case PJRC_Board.Teensy_35:
                    hexFile = "blink_35.hex";
                    Console.WriteLine("Selected Board: Teensy 3.5");
                    break;
                case PJRC_Board.Teensy_31_2:
                    hexFile = "blink_31.hex";
                    Console.WriteLine("Selected Board: Teensy 3.1 / 2");
                    break;
                case PJRC_Board.Teensy_LC:
                    hexFile = "blink_LC.hex";
                    Console.WriteLine("Selected Board: Teensy LC");
                    break;
                default:
                    Console.WriteLine("no hex file for slected board available. Exiting...");
                    Console.WriteLine("\nPress any key");
                    while (!Console.KeyAvailable) ;
                    return;                    
            }
            
            // Check if file is available
            if (!File.Exists(hexFile))
            {
                Console.WriteLine($"Specified hex file ({hexFile}) not found. Exiting...");
                Console.WriteLine("\nPress any key");
                while (!Console.KeyAvailable) ;
                return;
            }
            Console.WriteLine($"Using firmwarefile {hexFile} for download test");
            
            
            // Obtain an empty flash image with the correct size and all bytes cleared (set to 0xFF)
            var FlashImage = SharpUploader.GetEmptyFlashImage(Board);
            
            using (var HexStream = File.OpenText(hexFile))
            {
                // parse the file and write output to the image 
                SharpHexParser.ParseStream(HexStream, FlashImage);
                HexStream.Close();
            }

            using (var Watcher = new TeensyWatcher())
            {
                USB_Device Teensy = Watcher.ConnectedDevices.FirstOrDefault(); //We take the first Teensy we find...

                if(Teensy == null)
                {
                    Console.WriteLine("No Teensy found on the USB tree. Exiting....");
                    Console.WriteLine("\nPress any key");
                    while (!Console.KeyAvailable) ;
                    return;
                }

                Console.WriteLine($"- Starting Bootloader for Teensy {Teensy.Serialnumber}...");
                bool res = SharpUploader.StartHalfKay(Teensy.Serialnumber);
                Console.WriteLine(res ? "  OK" : "  Bootloader not running");

                // Upload firmware image to the board and reboot
                Console.WriteLine($"\n- Uploading {hexFile}...");
                int result = SharpUploader.Upload(FlashImage, Board, Teensy.Serialnumber, reboot: true);

                // Show result
                switch (result)
                {
                    case 0:
                        Console.WriteLine("  Successfully uploaded");
                        break;
                    case 1:
                        Console.WriteLine("  Found no board with running HalfKay. Did you press the programming button?");
                        Console.WriteLine("  Aborting...");
                        break;
                    case 2:
                        Console.WriteLine("  Error during upload.");
                        Console.WriteLine("  Aborting...");
                        break;
                }
                Console.WriteLine("\nPress any key");
                while (!Console.KeyAvailable) ;
            }
        }
    }
}
