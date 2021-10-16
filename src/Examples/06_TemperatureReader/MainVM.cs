using libTeensySharp;
using lunOptics.libTeensySharp;
using MaterialDesignThemes.Wpf;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViewModel;

namespace TemperatureReader
{
    class MainVM : BaseViewModel
    {
        #region commands ---------------------------------------
        public RelayCommand cmdStart { get; private set; }
        void doStart(object o)
        {
            string portStr = selectedTeensy.Ports.FirstOrDefault();
            if (portStr != null)
            {
                SelectedBoard.running = tempReader.startReading(portStr);
            }
        }

        public RelayCommand cmdStop { get; private set; }
        async void doStop(object o)
        {
            await tempReader.stopReading();
            SelectedBoard.running = false;
        }

        public bool running
        {
            get => _running;
            set
            {
                SetProperty(ref _running, value);
                OnPropertyChanged("notRunning");
            }
        }
        bool _running = false;
        public bool notRunning => !running;

        public RelayCommand cmdReset { get; private set; }
        async void doReset(object o)
        {
            uint sn = selectedTeensy.Serialnumber;
            msgQueue.Enqueue("Resetting...");
            var result = await selectedTeensy.ResetAsync(TimeSpan.FromSeconds(2));
            SelectedBoard = Boards.FirstOrDefault(b => b.serialNumber == sn);
            if (result != ErrorCode.OK)
                msgQueue.Enqueue("Reset Error");
            //if (SelectedBoard != null) doCheckFW(null);
        }

        public RelayCommand cmdCheckFW { get; private set; }
        void doCheckFW(object o)
        {            
            msgQueue.Enqueue("Firmware: " + SelectedBoard.firmware);
        }

        public RelayCommand cmdUpload { get; private set; }
        async void doUploadAsync(object o)
        {
            if (selectedTeensy == null) return;
            string hexFile;
            switch (selectedTeensy.BoardType)
            {
                case PJRC_Board.T4_0:
                    hexFile = "firmware/.vsteensy/build/T40/TempMon_T40.hex";
                    break;
                case PJRC_Board.T4_1:
                    hexFile = "firmware/.vsteensy/build/T41/TempMon_T41.hex";
                    break;
                case PJRC_Board.T_MM:
                    hexFile = "firmware/.vsteensy/build/Tmm/TempMon_TMM.hex";
                    break;
                default:
                    hexFile = null;
                    break;
            }

            uint sn = SelectedBoard.serialNumber;
            if (!String.IsNullOrEmpty(hexFile))
            {
                //  var indicator = new Progress<int>(p => Message = p != 0 ? p.ToString() + "%" : "");
                uplv = 0;
                uploading = true;
                msgQueue.Enqueue("Uploading...");
                var indicator = new Progress<int>(p => uplv = p);
                await selectedTeensy.UploadAsync(hexFile, indicator);
                await Task.Delay(500);
            }
            SelectedBoard = Boards.FirstOrDefault(t => t.serialNumber == sn);           
            doCheckFW(null);
            uploading = false;
        }

        #endregion

        #region properties -----------------------------------

        public WpfPlot plotControl { get; private set; }

        public SnackbarMessageQueue msgQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(1));

        public ObservableCollection<BoardVM> Boards { get; } = new ObservableCollection<BoardVM>();

        public int F_CPU_TARGET
        {
            get => tempReader.f_cpu_target / 1_000_000;
            set => tempReader.f_cpu_target = value * 1_000_000;
        }
        public int F_CPU_ACTUAL => tempReader.f_cpu_actual / 1_000_000;

        public double curTemp
        {
            get => _curTemp;
            set => SetProperty(ref _curTemp, value);
        }
        double _curTemp = 0;

        public BoardVM SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                SetProperty(ref _selectedBoard, value);
                SelectedBoard?.checkStatus();          
            }
        }
        BoardVM _selectedBoard;

        public double uplv
        {
            get => _uplv;
            set => SetProperty(ref _uplv, value);
        }
        double _uplv;              

        public bool uploading
        {
            get => _uploading;
            set => SetProperty(ref _uploading, value);
        }
        bool _uploading = false;

        private ITeensy selectedTeensy => SelectedBoard?.model;

      

        #endregion


        public MainVM()
        {
            cmdStart = new RelayCommand(doStart);
            cmdStop = new RelayCommand(doStop);
            cmdUpload = new RelayCommand(doUploadAsync);
            cmdCheckFW = new RelayCommand(doCheckFW);
            cmdReset = new RelayCommand(doReset);

            tempReader = new TempReader();
            tempReader.PropertyChanged += TempReader_PropertyChanged;

            watcher = new TeensyWatcher(SynchronizationContext.Current);
            Boards = new ObservableCollection<BoardVM>(watcher.ConnectedTeensies.Select(t => new BoardVM(t, tempReader)));
            SelectedBoard = Boards.FirstOrDefault();
            watcher.ConnectedTeensies.CollectionChanged += ConnectedTeensies_CollectionChanged;

            plotControl = new WpfPlot();
            plotControl.plt.Axis(x1: 0, x2: 500, y1: 30, y2: 70);
            plotControl.Configure(lockHorizontalAxis: true);
            plotControl.plt.PlotSignal(Temperatures);
            plotControl.plt.YLabel("Temperature (°C)");
        }

        double[] Temperatures = new double[500];
        private void TempReader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "temperature":
                    curTemp = tempReader.temperature;

                    for (int i = 0; i < Temperatures.Length - 1; i++)  // shift all data one position left
                    {
                        Temperatures[i] = Temperatures[i + 1];
                    }
                    Temperatures[^1] = curTemp;
                    plotControl.Dispatcher.Invoke(() => plotControl.Render());
                    break;

                case "F_CPU":
                    OnPropertyChanged("F_CPU_ACTUAL");
                    break;
            }
        }


        // we need to relay changes in the underlying list of connected teensies to our list of teensy view models
        // the code tries to keep selection on the board even if the usb type changes (e.g. from bootloader to serial)
        private void ConnectedTeensies_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ITeensy teensy in e.NewItems)
                    {
                        Boards.Add(new BoardVM(teensy, tempReader));
                       
                            teensy.PropertyChanged += Teensy_PropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    lastSn = SelectedBoard?.model.Serialnumber ?? 0;
                    foreach (ITeensy teensy in e.OldItems)
                    {
                        teensy.PropertyChanged -= Teensy_PropertyChanged;
                        BoardVM bvm = Boards.FirstOrDefault(b => b.model == teensy);
                        Boards.Remove(bvm);
                    }
                    break;
            }

            if (SelectedBoard == null)
                SelectedBoard = Boards.FirstOrDefault(b => b.model.Serialnumber == lastSn);
            if (SelectedBoard == null)
                SelectedBoard = Boards.FirstOrDefault();
        }

        private void Teensy_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var s = sender as ITeensy;
            Trace.WriteLine($"MPS: {s.Description} {e.PropertyName}");
        }

        uint lastSn = 0;


        async public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            await tempReader?.stopReading();
            watcher?.Dispose();
        }

        private readonly TempReader tempReader;  // handling readout of temperature and setting cpu speed
        private readonly TeensyWatcher watcher;  // watching the usb bus for Teensies, managing firmware upload and board restting
    }
}
