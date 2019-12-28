using lunOptics.LibUsbTree.Implementation;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;



namespace lunOptics.LibUsbTree
{
    public class UsbTree : IDisposable
    {
        public SynchronizationContext SyncContext { get; set; } = null;

        public UsbTree()
        {
            //_devices = new ObservableCollection<UsbDevice>(getDeviceList());
            // Devices = new ObservableCollection<UsbDevice>(getDeviceList());

            //timer.Elapsed += CheckForChanges;
            timer.Elapsed += (s, e) =>
            {
                if (SyncContext == null)
                {
                    // Debug.WriteLine("tt");
                    CheckForChanges();
                }
                else
                    SyncContext.Post(state => CheckForChanges(), null);
            };

            timer.Interval = 200;
            CheckForChanges();

            //timer.Start();

            Devices.Where(d => d.Parent == null).ForEach(r => Roots.Add(r));

        }

        public ObservableCollection<UsbDevice> Devices { get; } = new ObservableCollection<UsbDevice>();
        public ObservableCollection<UsbDevice> Roots { get; } = new ObservableCollection<UsbDevice>();

        protected virtual UsbDevice MakeDevice(UsbDeviceInfo inst) // Override to generate more specialized devices           
        {
            return new UsbDevice(inst);
        }


        #region private fields and methods -----------------------------------

        private List<UsbDevice> getDeviceList()
        {
            var devices = new List<UsbDevice>();
            using (var infoSet = new UsbInfoSet())
            {
                foreach (var deviceInfo in infoSet.getDeviceInfos())
                {
                    devices.Add(MakeDevice(deviceInfo)); // MakeDevice is virtual to allow subclasses adding specialized devices
                    deviceInfo.Dispose();
                }

                foreach (UsbDevice device in devices)    // setup parent/children structure
                {
                    device.Parent = devices.FirstOrDefault(d => d.DeviceInstanceId == device.ParentStr);
                    if (device.Parent != null)
                    {
                        device.Parent.children.Add(device);
                    }
                }


            }
            return devices;
        }


        private void CheckForChanges()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            timer.Stop(); // avoid reentrance

            using (var infoSet = new UsbInfoSet())
            {
                var newIDs = infoSet.getDeviceIDs();
                if (!newIDs.SetEquals(currentIDs))    // check if changed
                {
                    currentIDs = newIDs;

                    var newDevices = getDeviceList();
                    var removed = Devices.ExceptBy(newDevices, d => d.DeviceInstanceId).ToList();
                    var added = newDevices.ExceptBy(Devices, d => d.DeviceInstanceId).ToList();

                    foreach(var ad in added)
                    {
                        Devices.Add(ad);

                        var x = Roots.Flattenx().FirstOrDefault(ed => ad.Parent.DeviceInstanceId == ed.DeviceInstanceId);

                        ad.Parent = x;
                        x?.children.Add(ad);
                    }
                    

                    foreach (var rm in removed)
                    {
                        Devices.Remove(rm);

                        var x = Roots.Flattenx().FirstOrDefault(ed => ed.children.Contains(rm));
                        x?.children.Remove(rm);
                    }
                }
            }

            sw.Stop();
            Debug.WriteLine($"{Devices.Count} {sw.Elapsed.TotalMilliseconds}");

            timer.Start();
        }


        //void findRemove(ICollection<UsbDevice> collection, UsbDevice d)
        //{
        //    if (collection != null && !collection.Remove(d))
        //    {
        //        foreach (var device in collection)
        //        {
        //            findRemove(device?.children, d);
        //        }
        //    }
        //}





        private HashSet<string> currentIDs = new HashSet<string>();
        //private HashSet<string> oldIDs = new HashSet<string>();
        private System.Timers.Timer timer = new System.Timers.Timer();
        //private readonly ObservableCollection<UsbDevice> _devices;

        #endregion

        #region disposing ---------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class exx
    {
        public static IEnumerable<UsbDevice> Flattenx(this IEnumerable<UsbDevice> source)
        {
            foreach (UsbDevice item in source)
            {
                yield return item;
                foreach (var child in Flattenx(item.children))
                {
                    yield return child;
                };
            }
        }
    }
}
