using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using TeensySharp;

namespace TeensyWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var Watcher = new TeensyWatcher();
            Watcher.ConnectionChanged += ConnectedTeensiesChanged; //Connect an eventhandler to get information about changes (optional) 

            // Display currently connected Teensies
            Console.WriteLine("Currently the following Teensies are connected:");
            foreach (var Teensy in Watcher.ConnectedDevices)
            {
                Console.WriteLine("Serialnumber {0}, on Port {1}", Teensy.Serialnumber, Teensy.Port);
            }

            // Here is a good place to construct a SerialPort Object
            // for the sake of simplicity lets take the first one from the list
            var myTeensy = Watcher.ConnectedDevices.FirstOrDefault();
            if (myTeensy != null)
            {
                using (var Com = new SerialPort(myTeensy.Port))
                {
                    // lets talk to our Teensy: 
                    Com.Open();
                    Com.WriteLine("Hello Teensy");
                    Com.Close();
                }
            }

            Console.WriteLine("\nPlug Teensies in / out to see the watcher in action\n\nPress any key to exit");
            while (!Console.KeyAvailable) ;

            // CleanUp 
            Watcher.ConnectionChanged -= ConnectedTeensiesChanged;
            Watcher.Dispose();

        }

        //-------------------------------------------------------------------------------------------------------------------
        //In a real application you would use this eventhandler to inform the user, connect / disconnect the SerialPort etc..
        //
        static void ConnectedTeensiesChanged(object sender, ConnectionChangedEventArgs e)
        {
            // Write information about the added or removed Teensy to the console
            string Port = e.changedDevice.Port;
            string SN = e.changedDevice.Serialnumber;
            string changeText = e.changeType == UsbWatcher.ChangeType.add ? "added to" : "removed from";

            Console.WriteLine("\n-----------------------------------------------------------------");
            Console.WriteLine("The Teensy with Serialnumber {0} was {1} Port {2}", SN, changeText, Port);


            // Just for fun show the list of currently connected Teensies
            Console.WriteLine("\nCurrently the following Teensies are connected:");

            var Watcher = (TeensyWatcher)sender;
            foreach (var Teensy in Watcher.ConnectedDevices)
            {
                Console.WriteLine("- Serialnumber {0}, on Port {1}", Teensy.Serialnumber, Teensy.Port);
            }
            Console.WriteLine("--------------------------------------------------------------------");

            Console.WriteLine("\n\nPress any key to exit\n");
        }
    }
}
