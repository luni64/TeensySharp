using libTeensySharp;
using lunOptics.libTeensySharp;
using MaterialDesignThemes.Wpf;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ViewModel;

namespace TemperatureReader
{
    class MainVM : BaseViewModel
    {
        #region commands ---------------------------------------
        public RelayCommand cmdStart { get; private set; }
        void doStart(object o)
        {
            string portStr = SelectedBoard.Ports.FirstOrDefault();
            if (portStr != null)
            {
                tempReader.startReading(portStr);
            }
        }

        public RelayCommand cmdStop { get; private set; }
        void doStop(object o)
        {
            tempReader.stopReading();
        }

        public RelayCommand cmdReset { get; private set; }
        async void doReset(object o)
        {
            var board = SelectedBoard;
            await SelectedBoard.ResetAsync();
            SelectedBoard = board;
        }

        public RelayCommand cmdCheckFW { get; private set; }
        void doCheckFW(object o)
        {
            Trace.WriteLine("asdf");
            string fw = "Firmware: " + tempReader.getFirmwareVersion(SelectedBoard.Ports.FirstOrDefault());
            msgq.Enqueue(fw);



        }

        public RelayCommand cmdUpload { get; private set; }
        async void doUploadAsync(object o)
        {
            if (SelectedBoard == null) return;
            string hexFile;
            switch (SelectedBoard.BoardType)
            {
                case PJRC_Board.T4_0:
                    hexFile = "firmware/TempMon4_0.hex";
                    break;
                case PJRC_Board.T4_1:
                    hexFile = "firmware/TempMon4_1.hex";
                    break;
                case PJRC_Board.T3_6:
                    hexFile = "TempMon36.hex";
                    break;
                case PJRC_Board.T3_5:
                    hexFile = "TempMon35.hex";
                    break;
                //case PJRC_Board.T3_2:
                //    firmware = "TempMon32.hex";
                //    break;
                default:
                    hexFile = null;
                    break;
            }

            var teensy = SelectedBoard;
            if (!String.IsNullOrEmpty(hexFile))
            {
                var indicator = new Progress<int>(p => Message = p != 0 ? p.ToString() + "%" : "");
                await teensy.UploadAsync(hexFile, indicator);
            }
            SelectedBoard = teensy;
            Message = "";
            doCheckFW(null);
        }

        public WpfPlot plotControl { get; private set; }

        #endregion

        public SnackbarMessageQueue msgq { get; } = new SnackbarMessageQueue();

        public ObservableCollection<ITeensy> Boards => watcher.ConnectedTeensies;
        public ITeensy SelectedBoard
        {
            get => _selectedBoard;
            set
            {
                SetProperty(ref _selectedBoard, value);

                bool upl = true;
                if (SelectedBoard == null)
                {
                    upl = false;
                    Message = "";
                }
                else if (SelectedBoard.UsbType == UsbType.HalfKay)
                {
                    Message = "";
                }
                else if (!(_selectedBoard.BoardType == PJRC_Board.T4_1 || _selectedBoard.BoardType == PJRC_Board.T4_0))
                {
                    Message = "Wrong Board";
                    upl = false;
                }
                else
                {
                    Message = "";
                }
                Uploadable = upl;
            }
        }
        ITeensy _selectedBoard;



        public string Message
        {
            get => _uploadProgress;
            set => SetProperty(ref _uploadProgress, value);
        }
        string _uploadProgress = "";

        public bool Uploadable
        {
            get => _uploadable;
            set => SetProperty(ref _uploadable, value);
        }
        bool _uploadable = false;

        public string isUploading
        {
            get => _isUploading;
            set => SetProperty(ref _isUploading, value);
        }
        string _isUploading = "Collapsed";

        public bool isDialogOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }
        bool _isOpen = false;


        public MainVM()
        {
            cmdStart = new RelayCommand(doStart, p => SelectedBoard?.Ports.FirstOrDefault() != null);
            cmdStop = new RelayCommand(doStop, p => SelectedBoard?.Ports.FirstOrDefault() != null);
            cmdUpload = new RelayCommand(doUploadAsync, p => Uploadable);
            cmdCheckFW = new RelayCommand(doCheckFW);
            cmdReset = new RelayCommand(doReset);

            tempReader = new TempReader();
            tempReader.Temperatures.CollectionChanged += Temperatures_CollectionChanged;


            watcher = new TeensyWatcher(SynchronizationContext.Current);





            plotControl = new WpfPlot();
            plotControl.plt.Axis(x1: 0, x2: 200, y1: 40, y2: 60);
            plotControl.plt.PlotSignal(signal);

        }

        int idx = 0;
        double[] signal = new double[1000];
        private void Temperatures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                tempReader.Temperatures.TakeLast(200).ToArray().CopyTo(signal, 0);




                plotControl.Dispatcher.Invoke(() => plotControl.Render());

            }
        }

        private readonly TempReader tempReader;
        private readonly TeensyWatcher watcher;
        private readonly double[] Temperatures = new double[100];
    }
}
