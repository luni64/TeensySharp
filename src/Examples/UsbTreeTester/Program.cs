using lunOptics.LibUsbTree;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;


namespace UsbTreeTester
{
    class Program
    {
        static void Main()
        {
            var usbTree = new UsbTree();           
            usbTree.Devices.CollectionChanged += Program_CollectionChanged;

            Console.WriteLine("===============================================================");
            Console.WriteLine(" Currently connected USB devices");
            Console.WriteLine("===============================================================\n");

            var devices = usbTree.Devices;                        // get a list of all devices on the bus

            Console.WriteLine("FLAT DEVICE LIST: ---------------------------------------------");
            foreach (var device in devices)
            {
                Console.WriteLine($"  - {device.ToString()}");
            }

            Console.WriteLine("\nHIERARCHICAL DEVICE LIST: -----------------------------------");
            foreach (var root in devices.Where(d => d.Parent == null)) // we only need the root devices 
            {
                PrintRecursively(root);                                // print roots and all their descendannts   
            }

            Console.WriteLine("\n\nPlug in/out devices to see changes or press any key to stop\n");
            while (!Console.KeyAvailable) Thread.Sleep(100);
            
            // cleanup            
            usbTree.Dispose();
        }

        private static void Program_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach (UsbDevice i in e.NewItems)
                    {
                        Console.WriteLine($"+ {i.ToString()}");
                        foreach (var iface in i.interfaces)
                        {
                            Console.WriteLine($"  * {iface.ToString()}");
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (UsbDevice i in e.OldItems)
                    {
                        Console.WriteLine($"- {i.ToString()}");
                        foreach (UsbDevice iface in i.interfaces)
                        {
                            Console.WriteLine($"  * {iface.ToString()}");
                        }
                    }
                    break;
                default:
                    Console.WriteLine("ddd");
                    break;
            }
        }

        private static void PrintRecursively(UsbDevice device, int level = 0)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Console.Write(new String(' ', (level + 2) * 2));  // indent proprtional to level
            Console.WriteLine(device.ToString());
            foreach (var c in device.children)
            {
                PrintRecursively(c, level + 1);
            }
        }
    }
}
