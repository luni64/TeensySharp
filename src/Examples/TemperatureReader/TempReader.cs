using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace TemperatureReader
{
    class TempReader
    {
        public int cpuFreqency { get; set; }
        int _oldFreq = 0;

        public ObservableCollection<double> Temperatures { get; } = new ObservableCollection<double>();

        public void startReading(string portNr)
        {
            worker.RunWorkerAsync(portNr);
        }

        public void stopReading()
        {
            run = false;
        }

        public TempReader()
        {
            worker.DoWork += Worker_DoWorkAsync;
        }

        public string getFirmwareVersion(string portNr)
        {
            if (run == true) return "Error";
            try
            {
                using (var p = new SerialPort(portNr))
                {
                    p.Open();
                    p.ReadTimeout = 1000;
                    if (p.IsOpen)
                    {
                        p.WriteLine("v   ");
                        string version = p.ReadLine();
                        if (version.StartsWith("TempMon"))
                            return version.Trim();
                        else 
                            return "Unknown";
                    }
                }
            }
            catch
            {
            }

            return "Unknown Firmware";
        }

        private void Worker_DoWorkAsync(object sender, DoWorkEventArgs e)
        {
            run = true;
            string port = (string)e.Argument;

            using (var p = new SerialPort(port))
            {
                p.Open();

                while (run && p.IsOpen)
                {
                    if (_oldFreq != cpuFreqency)  // if we have a new frequency send set command to FW
                    {
                        _oldFreq = cpuFreqency;
                        p.WriteLine($"s {_oldFreq}");
                    }

                    p.WriteLine("r   ");
                    if (double.TryParse(p.ReadLine(), out double temperature))
                    {
                        Trace.WriteLine(temperature);
                        if (Temperatures.Count > 1000) Temperatures.RemoveAt(0);
                        Temperatures.Add(temperature);
                    }

                    Task.Delay(100).Wait();
                }
            }

            run = false;
        }



        private bool run;
        private BackgroundWorker worker = new BackgroundWorker();
    }
}
