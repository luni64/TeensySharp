using lunOptics.TeensySharp;
using lunOptics.UsbTree.Implementation;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ViewModel
{
    public class MainVM : BaseViewModel
    {
        public ObservableCollection<TeensyVM> Teensies { get; }


        public MainVM()
        {
            var devices = UsbTree.getDevices();


            var rootDevices = devices.Where(d => d.Parent == null);

            var ports = devices.OfType<IUsbSerial>();


            foreach (var rootDevice in rootDevices)
            {
                UsbTree.print(rootDevice);
            }


          
            //var hids =  (IUsbSerial) UsbTree.getDevices().Where(d => d is IUsbSerial);

            var p = ports.FirstOrDefault().Port;


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
