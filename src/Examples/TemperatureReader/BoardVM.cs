using lunOptics.libTeensySharp;
using MaterialDesignColors;
using System;
using System.Collections.Generic;
using System.Text;
using ViewModel;
using System.Linq;
using RJCP.IO.Ports;
using System.Diagnostics;

namespace TemperatureReader
{
    class BoardVM : BaseViewModel
    {
        public string type { get; private set; }
        public int serialNumber { get; private set; }

        public string description { get; private set; }

        public bool uploadable { get; private set; } = false;
        public bool resetable { get; private set; } = false;
        public bool startable { get; private set; } = false;
        public bool notStartable => !startable;
        public string firmware { get; private set; } = "";

        public bool running
        {
            get => _running;
            set
            {
                SetProperty(ref _running, value);
                OnPropertyChanged("notRunning");
                //checkStatus();
            }
        }
        bool _running = false;

        public bool notRunning => !running;


        (bool status, string fw) getFirmwareVersion(string portNr)
        {
            (bool, string) ret = (false, "");
            var p = new SerialPortStream(portNr);
            try
            {
                p.Open();
                p.ReadTimeout = 100;
                p.WriteLine("v   ");
                string version = p.ReadLine();
                if (version.StartsWith("TempMon"))
                    ret = (true, version.Trim());
                else
                    ret = (false, "Unknown");
            }
            catch
            {
                ret = (false, "not open");
            }
            finally
            {
                p?.Close();
            }

            return ret;
        }


        public void checkStatus()
        {
            resetable = uploadable = startable = false;
            firmware = "unknown";

            if (!model.Ports.Any())                          // hid, bootloader etc
            {
                resetable = true && !running;
                uploadable = true;
            }
            else                                            // Teensy with port
            {
                if (model.CheckPort() == ErrorCode.OK)      // port blocked
                {
                    resetable = true && !running;
                }
                if (resetable && (model.BoardType == PJRC_Board.T4_1 || model.BoardType == PJRC_Board.T4_0))
                {
                    uploadable = true;
                }
                if (uploadable)
                {
                    var result = getFirmwareVersion(model.Ports[0]);
                    if (result.status == true)
                    {
                        startable = true;
                        firmware = result.fw;
                    }
                }
            }
            OnPropertyChanged("");
        }

        public BoardVM(ITeensy Teensy, TempReader tempReader)
        {
            this.model = Teensy;

            switch (model.BoardType)
            {
                case PJRC_Board.T4_1: type = "T4.1"; break;
                case PJRC_Board.T4_0: type = "T4.0"; break;
                case PJRC_Board.T3_6: type = "T3.6"; break;
                case PJRC_Board.T3_5: type = "T3.5"; break;
                case PJRC_Board.T3_2: type = "T3.2"; break;
                default: type = "?"; break;
            }

            string ports = "";
            if (model.Ports.Any()) { ports += model.Ports[0]; }
            foreach (var port in model.Ports.Skip(1)) { ports += $"+{port}"; }

            serialNumber = model.Serialnumber;

            switch (model.UsbType)
            {
                case UsbType.Serial:
                    description = $"{serialNumber} - ({ports}) ";
                    break;

                case UsbType.HalfKay:
                    description = $"{serialNumber} - (bootloader) ";
                    break;

                case UsbType.COMPOSITE:
                    if (model.Ports.Any())
                    {
                        description = $"{serialNumber} - ({ports})";
                    }
                    else
                    {
                        description = $"{serialNumber} - (Composite)";
                    }
                    break;
            }

            checkStatus();

           
        }
         
       

        public ITeensy model { get; }
    }
}
