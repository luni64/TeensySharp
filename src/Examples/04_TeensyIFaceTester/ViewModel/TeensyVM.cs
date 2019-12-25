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
            Model.Reboot();
        }

        public String SerialNumber => $"SN: {Model.Serialnumber.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        public String BoardType
        {
            get
            {
                return Model.BoardType switch
                {
                    PJRC_Board.Teensy_LC => "LC",
                    PJRC_Board.Teensy_31_2 => "T3.2",
                    PJRC_Board.Teensy35 => "T3.5",
                    PJRC_Board.Teensy36 => "T3.6",
                    PJRC_Board.Teensy40 => "T4.0",
                    _ => "?",
                };
            }
        }

        public String UsbType
        {
            get
            {
                String s = Model.UsbType.ToString();
                //if(model.UsbType != UsbTypes.unknown && model.UsbType != UsbTypes.disconnected && model.UsbType != UsbTypes.HalfKay)
                if(Model.UsbSubType != UsbSubType.none)
                {
                    s += $" ({Model.UsbSubType.ToString()})";
                }
                if(Model.UsbType == lunOptics.TeensySharp.UsbType.Serial)
                {
                    s += $" on {Model.Port}";
                }
                return s;

            }
        }

        public bool isNotHalfKay => Model.UsbType != lunOptics.TeensySharp.UsbType.HalfKay;

        public ITeensy Model { get => model; set => model = value; }

        public TeensyVM(ITeensy model)
        {
            this.Model = model;
            this.Model.PropertyChanged += Model_PropertyChanged;

            cmdReboot = new RelayCommand(doReboot, o => isNotHalfKay);

        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("UsbType");
            OnPropertyChanged("isHalfKay");
        }

        private ITeensy model;
    }
}
