using lunOptics.libTeensySharp.Implementation;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace TemperatureReader
{
    /// <summary>
    /// Handles all communication with the Teensy, 
    /// sets frequency and reads temperature
    /// </summary>
    public class TempReader : INotifyPropertyChanged
    {
        public TempReader()
        {
            worker.DoWork += Worker_DoWorkAsync;  // attach worker function but don't start yet
        }

        public string port { get; set; }
        public int f_cpu_target
        {
            get => _f_cpu_target;
            set
            {
                if (_f_cpu_target != value)
                {
                    _f_cpu_target = value;
                    sendFreqency = true;
                }
            }
        }
        public int f_cpu_actual { get; private set; }
        public double temperature { get; private set; }


        public bool startReading(string portNr)
        {
            if (worker.IsBusy) return false;
            worker.RunWorkerAsync(portNr);
            return true;
        }
        async public Task stopReading()
        {
            run = false;
            while (worker.IsBusy)
            {
                await Task.Delay(10);
            }
        }
        ////public string getFirmwareVersion(string portNr)
        //{
        //    if (run == true) return "Error";
        //    try
        //    {
        //        using (var p = new SerialPort(portNr))
        //        {
        //            p.Open();
        //            p.ReadTimeout = 100;
        //            if (p.IsOpen)
        //            {
        //                p.WriteLine("v   ");
        //                string version = p.ReadLine();
        //                if (version.StartsWith("TempMon"))
        //                    return version.Trim();
        //                else
        //                    return "Unknown";
        //            }
        //        }
        //    }
        //    catch
        //    {
        //    }

        //    return "Unknown Firmware";
        //}

        #region private methods and fields
        private void Worker_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            run = true;
            string port = (string)e.Argument;

            using (var p = new SerialPortStream(port))
            {
                p.Open();

                while (run && p.IsOpen)
                {
                    if (sendFreqency)                                      // send frequency to FW
                    {
                        p.WriteLine($"s {f_cpu_target}");                  // send the new frequency to the board (command s)
                        if (int.TryParse(p.ReadLine(), out int f_cpu))     // read the actually set frequency back
                        {
                            f_cpu_actual = f_cpu;
                            OnPropertyChanged("F_CPU");
                        }
                        sendFreqency = false;                              // we only send once
                    }

                    p.WriteLine("r   ");                                   // read current temperature (command r)
                    if (double.TryParse(p.ReadLine(), out double T))
                    {
                        temperature = T/100.0;
                        OnPropertyChanged("temperature");
                    }
                    Task.Delay(50).Wait();
                }
            }
            run = false;
        }

        private int _f_cpu_target;
        private bool sendFreqency = true;
        private bool run;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        #endregion

        #region IPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }
}
