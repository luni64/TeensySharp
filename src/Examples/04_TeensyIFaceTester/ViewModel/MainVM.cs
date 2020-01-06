using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using libTeensySharp.Implementation.Teensy;

namespace ViewModel
{
    public sealed class MainVM :IDisposable
    {
        //public ReadOnlyObservableCollection<UsbDevice> devices { get; }
        public ReadOnlyObservableCollection<UsbDevice> roots { get; }
        public ReadOnlyObservableCollection<UsbDevice> list { get; }

        public MainVM()
        {
            factory = new TeensyFactory();
            tree = new UsbTree(SynchronizationContext.Current, factory);
            roots = new ReadOnlyObservableCollection<UsbDevice>(tree.DeviceTree.children);
            list = new ReadOnlyObservableCollection<UsbDevice>(tree.DeviceList);
                      
            cmdReboot = new RelayCommand(doReboot);                     
           
        }

        public UsbTeensy foundTeensy { get; }

        private TeensyFactory factory;

        private readonly UsbTree tree;

        public void Dispose()
        {
            tree.Dispose();
        }


        public RelayCommand cmdReboot { get; }
        
        void doReboot(object o)
        {
           

            //var teensy = factory.repo.OfType<UsbTeensy>();
            //if(teensy != null)
            //{
            //    Debug.WriteLine(teensy?.Description);
            //    teensy.Reboot();
            //}

        }
    }
}
