using lunOptics.TeensySharp;
using lunOptics.UsbTree.Implementation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ViewModel
{
    public class MainVM : BaseViewModel
    {
        public RelayCommand cmdScan { get; private set; }
        void doScan(object o)
        {
            var devices = UsbTree.getDevices();
            roots = devices.Where(d => d.Parent == null).ToList();
            OnPropertyChanged("roots");
        }

        public ObservableCollection<TeensyVM> Teensies { get; }

        public List<IUsbDevice> roots { get; private set; }


        public MainVM()
        {
            cmdScan = new RelayCommand(doScan);
            cmdScan.Execute(null);

            var teensies = UsbTree.getDevices().Where(d=>d.vid == 0x16C0);
                       


            foreach (var rootDevice in roots)
            {
                UsbTree.print(rootDevice);
            }


          
            //var hids =  (IUsbSerial) UsbTree.getDevices().Where(d => d is IUsbSerial);

            //var p = ports.FirstOrDefault().Port;


            TeensySharp.SynchronizationContext(SynchronizationContext.Current);

            var boards = TeensySharp.ConnectedBoards;

            Teensies = new ObservableCollection<TeensyVM>(boards.Select(b => new TeensyVM(b)));


            TeensySharp.ConnectedBoardsChanged += Watcher_ConnectionChanged;

            var teensy = TeensySharp.ConnectedBoards.FirstOrDefault();
            //teensy.Reboot();
            //teensy.Reset();                                

        }

        private void Watcher_ConnectionChanged(object sender, ConnectedBoardsChangedArgs e)
        {
            if (e.changeType == ChangeType.add && !Teensies.Any(tvm => tvm.model == e.changedDevice))
            {



                Teensies.Add(new TeensyVM(e.changedDevice));
            }
            else
            {
                //if (Teensies.Contains(e.changedDevice)) Teensies.Remove(e.changedDevice);
            }
        }
    }
}
