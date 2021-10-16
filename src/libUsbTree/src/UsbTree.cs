//using MoreLinq;
using lunOptics.libUsbTree.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Diagnostics;
using System.Linq;
using System.Threading;

using static lunOptics.libUsbTree.NativeWrapper;


namespace lunOptics.libUsbTree
{
    public class UsbTree : IDisposable
    {
        #region properties -----------------------------------------------------------------------------

        public IUsbDevice DeviceTree => _deviceTree;

        private readonly UsbDevice _deviceTree = new UsbDevice();
        public ObservableCollection<IUsbDevice> DeviceList { get; } = new ObservableCollection<IUsbDevice>();

        //public List<IUsbDevice> dlist { get; } = new List<IUsbDevice>();

        internal static DeviceFactory deviceFactory { get; private set; }
        #endregion

        #region construction/deconstruction ------------------------------------------------------------

        public UsbTree(DeviceFactory factory = null, SynchronizationContext SyncContext = null)
        {
            deviceFactory = factory ?? new DeviceFactory();  // for simple use do not require (but allow) dependency injection
            rootNodes = FindUsbRoots();

            timer.Interval = 200;
            timer.Elapsed += (s, e) =>
            {
                if (SyncContext == null) CheckForChanges();
                else SyncContext.Post(state => CheckForChanges(), null);
              
            };
            CheckForChanges();  // implicitly starts timer            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && timer != null)
            {
                timer.Enabled = false;
                timer.Elapsed -= (s, e) => { };
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

        #region private fields and methods -------------------------------------------------------------

        private InfoNode oldTree, newTree;

        private void CheckForChanges()
        {
            if (timer != null)
            {
                timer.Stop(); // avoid reentrance
                //Trace.Write(".");

                newTree = new InfoNode(rootNodes);

                bool isEqual = newTree.isEqual(oldTree);
                
                if(!isEqual || DateTime.Now < lastChange + TimeSpan.FromSeconds(5))                
                {
                    //if (!isEqual) lastChange = DateTime.Now;
                    newTree.readDetails();
                    _deviceTree.update(newTree);  // update the complete device tree. Add/remove devices if necessary
                    UpdateDeviceList();           // reflect all changes in the flat device list     
                    oldTree = newTree;
                }
                timer.Start();
            }
        }

        DateTime lastChange = DateTime.Now;

        void UpdateDeviceList()
        {
            var flatList = DeviceTree.children.myFlatten(i => i.children);
            var newDevices = flatList.Except(DeviceList).ToList();
            var removedDevices = DeviceList.Except(flatList).ToList();

            newDevices.ForEach(d => { DeviceList.Add(d);((UsbDevice) d).IsConnected = true; });
            removedDevices.ForEach(d => { DeviceList.Remove(d); ((UsbDevice)d).IsConnected = false; });

            //newDevices.ForEach(d => dlist.Add(d));
            //removedDevices.ForEach(d => dlist.Remove(d));
        }

        private List<int> FindUsbRoots()
        {
            var roots = new HashSet<int>();
            foreach (var devInstID in cmGetDevInstIDs("USB")) // loop through all USB device interface ids
            {
                int node = cmLocateNode(devInstID);
                int parent;
                while ((parent = cmGetParentNode(node)) != -1)
                {
                    if (!cmGetDevInstIdFromNode(parent).StartsWith("USB")) // bubble up until parent is not a USB device
                    {
                        roots.Add(node);  // hash set will reject multiple addition of same node
                        break;
                    }
                    node = parent;
                }
            }
            return roots.ToList();
        }

        private readonly List<int> rootNodes;

        private System.Timers.Timer timer = new System.Timers.Timer();

        #endregion
    }

    static public partial class MyExtensions
    {
        public static IEnumerable<T> myFlatten<T>(this IEnumerable<T> e, Func<T, IEnumerable<T>> f)
        {
            return e.SelectMany(c => f(c).myFlatten(f)).Concat(e);
        }
    }
}

