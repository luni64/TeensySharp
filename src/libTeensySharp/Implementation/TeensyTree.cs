using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Linq;
using System.Collections.Specialized;

namespace libTeensySharp
{
    public class TeensyWatcher : IDisposable
    {
        public ObservableCollection<ITeensy> ConnectedTeensies { get; }

        public TeensyWatcher(SynchronizationContext context = null)
        {
            usbTree = new UsbTree(new TeensyFactory(), context);
            ConnectedTeensies = new ObservableCollection<ITeensy>(usbTree.DeviceList.OfType<ITeensy>());
            usbTree.DeviceList.CollectionChanged += DeviceList_CollectionChanged;
        }

        private void DeviceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems.OfType<ITeensy>())
                    {
                        ConnectedTeensies.Remove(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.OfType<ITeensy>())
                    {
                        ConnectedTeensies.Add(item);
                    }
                    break;
            }
        }

        public void Dispose()
        {
            usbTree.Dispose();
            usbTree = null;
        }

        private UsbTree usbTree;
    }
}
