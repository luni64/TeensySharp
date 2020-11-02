using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;


namespace ViewModel
{
    public sealed class MainVM : IDisposable
    {       
        public ReadOnlyObservableCollection<IUsbDevice> roots { get; }
        public ReadOnlyObservableCollection<IUsbDevice> list { get; }

        public MainVM()
        {            
            tree = new UsbTree(new TeensyFactory(), SynchronizationContext.Current);
            roots = new ReadOnlyObservableCollection<IUsbDevice>(tree.DeviceTree.children);
            list = new ReadOnlyObservableCollection<IUsbDevice>(tree.DeviceList);

            cmdReboot = new RelayCommand(doReboot);
            cmdReset = new RelayCommand(doReset);
            cmdUpload = new RelayCommand(doUpload);
        }

        public ITeensy foundTeensy { get; }

        
        private readonly UsbTree tree;

        public void Dispose()
        {
            tree.Dispose();
        }

        public RelayCommand cmdUpload { get; }
        async void doUpload(object o)
        {
            var teensy = list.OfType<ITeensy>().FirstOrDefault();
            if (teensy != null)
            {
                Debug.WriteLine("Uploading to " +teensy?.Description);
                ErrorCode result = await teensy.UploadAsync("portable41.hex");
                Debug.WriteLine(result);
            }
        }

        public RelayCommand cmdReboot { get; }

        async void doReboot(object o)
        {
            var teensy = list.OfType<ITeensy>().FirstOrDefault();
            if (teensy != null)
            {
                Debug.WriteLine("Rebooting " + teensy?.Description);
                var result = await teensy.RebootAsync();
                Debug.WriteLine(result);
            }
        }

        public RelayCommand cmdReset { get; }
        async void doReset(object o)
        {
            var teensy = list.OfType<ITeensy>().FirstOrDefault();
            if (teensy != null)
            {
                Debug.WriteLine("Resetting " + teensy?.Description);
                var result = await teensy.ResetAsync();
                Debug.WriteLine(result);
            }
        }
    }
}
