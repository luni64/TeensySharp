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
    public sealed class MainVM : IDisposable
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
            cmdReset = new RelayCommand(doReset);

        }

        public UsbTeensy foundTeensy { get; }

        private TeensyFactory factory;

        private readonly UsbTree tree;

        public void Dispose()
        {
            tree.Dispose();
        }
        
        public RelayCommand cmdReboot { get; }

        async void doReboot(object o)
        {
            var teensy = list.OfType<UsbTeensy>().FirstOrDefault();
            if (teensy != null)
            {
                Debug.WriteLine(teensy?.Description);
                bool result = await teensy.RebootAsync();
                Debug.WriteLine("Reboot " + (result ? "OK" : "ERROR"));
            }
        }

        public RelayCommand cmdReset { get; }
        async void doReset(object o)
        {
            var teensy = list.OfType<UsbTeensy>().FirstOrDefault();
            if (teensy != null)
            {
                Debug.WriteLine(teensy?.Description);
                bool result = await teensy.ResetAsync();
                Debug.WriteLine("Reset" + (result ? "OK" : "ERROR"));
            }
        }
    }
}
