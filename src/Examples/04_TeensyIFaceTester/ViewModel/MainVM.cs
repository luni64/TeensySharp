using lunOptics.LibUsbTree;
using lunOptics.TeensySharp;
using lunOptics.TeensyTree.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace ViewModel
{



    public class MainVM : BaseViewModel
    {

        public RelayCommand cmdScan { get; private set; }
        void doScan(object o)
        {
            treeVM.children.First().children.Add(new TreeVM("ccc"));

            //devices = tree.Devices;
            //roots =  devices.Where(d => d.Parent == null).ToList();
            //OnPropertyChanged("roots");
        }

        public ObservableCollection<UsbDevice> Teensies { get; }

        public ObservableCollection<UsbDevice> roots { get; private set; }


        //TTTree tree = new TTTree();
        public UsbTree tree { get; }
        // ReadOnlyObservableCollection<UsbDevice> devices;

        // public ObservableCollection<UsbDevice> rr { get; } = new ObservableCollection<UsbDevice> ();

        public TreeVM treeVM { get; } = new TreeVM("root");

        public MainVM()
        {
            tree = new TeensyTree();
            tree.SyncContext = SynchronizationContext.Current;

            var roots = tree.Devices.Where(d => d.Parent == null);
            foreach(var root in roots)
            {
                treeVM.children.Add(new TreeVM(root));
            }



            roots = new ObservableCollection<UsbDevice>(tree.Devices);

           

            // rr.Add(roots.Skip(0).FirstOrDefault(d => d.Parent == null));



           // tree.Devices.CollectionChanged += MainVM_CollectionChanged;
            cmdScan = new RelayCommand(doScan);
            // cmdScan.Execute(null);


            //roots =tree.Devices;

            //lunOptics.TeensySharp.TeensySharp.SetSynchronizationContext(SynchronizationContext.Current);

            //var boards = lunOptics.TeensySharp.TeensySharp.ConnectedBoards;

            //Teensies = new ObservableCollection<TeensyVM>(boards.Select(b => new TeensyVM(b)));


            //lunOptics.TeensySharp.TeensySharp.ConnectedBoardsChanged += Watcher_ConnectionChanged;

            //var teensy = lunOptics.TeensySharp.TeensySharp.ConnectedBoards.FirstOrDefault();
            //teensy.Reboot();
            //teensy.Reset();                                

        }

        private void MainVM_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("asdf");

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (UsbDevice i in e.NewItems)
                    {
                        roots?.Add(i);
                        //TeensyIFaceTester.App.Current.Dispatcher.Invoke(() => roots?.Add(i));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (UsbDevice i in e.OldItems)
                    {
                        roots?.Remove(i);
                        //TeensyIFaceTester.App.Current.Dispatcher.Invoke(() => roots?.Remove(i));
                    }
                    break;
            }




        }

        private void Watcher_ConnectionChanged(object sender, ConnectedBoardsChangedEventArgs e)
        {
            //if (e.changeType == ChangeType.add && !Teensies.Any(tvm => tvm.Model == e.changedDevice))
            //{
            //    Teensies.Add(new TeensyVM(e.changedDevice));
            //}
            //else
            //{
            //    //if (Teensies.Contains(e.changedDevice)) Teensies.Remove(e.changedDevice);
            //}
        }
    }
}
