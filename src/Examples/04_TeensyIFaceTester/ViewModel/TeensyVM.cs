using lunOptics.TeensySharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ViewModel
{
    public class TeensyVM : BaseViewModel
    {
        public RelayCommand cmdReboot { get; }
        private void doReboot(object o)
        {
            model.Reboot();
        }

        public String SerialNumber => $"SN: {model.Serialnumber.ToString()}";

        public String BoardType
        {
            get
            {
                switch (model.BoardType)
                {
                    case PJRC_Board.Teensy_LC: return "LC";
                    case PJRC_Board.Teensy_31_2: return "T3.2";
                    case PJRC_Board.Teensy_35: return "T3.5";
                    case PJRC_Board.Teensy_36: return "T3.6";
                    case PJRC_Board.Teensy_40: return "T4.0";
                    default: return "?";
                }
            }
        }

        public String UsbType
        {
            get
            {
                String s = model.UsbType.ToString();
                //if(model.UsbType != UsbTypes.unknown && model.UsbType != UsbTypes.disconnected && model.UsbType != UsbTypes.HalfKay)
                if(model.UsbSubType != UsbSubTypes.none)
                {
                    s += $" ({model.UsbSubType.ToString()})";
                }
                if(model.UsbType == UsbTypes.Serial)
                {
                    s += $" on {model.Port}";
                }
                return s;

            }
        }

        public bool isNotHalfKay => model.UsbType != UsbTypes.HalfKay;

        public TeensyVM(ITeensy model)
        {
            this.model = model;
            this.model.PropertyChanged += Model_PropertyChanged;

            cmdReboot = new RelayCommand(doReboot, o => isNotHalfKay);

        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("UsbType");
            OnPropertyChanged("isHalfKay");
        }

        public ITeensy model;
    }
}
