using Microsoft.Win32;
using System.Windows;
using TeensySharp;
using System.Linq;
using System.IO;

namespace WpfDownload
{
    public partial class MainWindow : Window
    {
        TeensyWatcher watcher;

        public MainWindow()
        {
            InitializeComponent();

            watcher = new TeensyWatcher();

            // fill board list with connected boards
            foreach (var teensy in watcher.ConnectedDevices)
            {
                cbTeensy.Items.Add(teensy);
            }
            cbTeensy.SelectedIndex = 0;

            watcher.ConnectionChanged += OnBoardPlugged; // event handler
        }


        // Handle life hot plugging of boards
        private void OnBoardPlugged(object sender, ConnectionChangedEventArgs e)
        {
            var board = e.changedDevice;

            if (e.changeType == TeensyWatcher.ChangeType.add)
            {
                Dispatcher.Invoke(() =>              // we can not access GUI thread directly from event handler, use dispatcher instead
                {
                    cbTeensy.Items.Add(board);                    
                    cbTeensy.SelectedItem = board;
                });
            }
            else if (e.changeType == TeensyWatcher.ChangeType.remove)
            {
                Dispatcher.Invoke(() =>             // we can not access GUI tread directly from event handler, use dispatcher instead
                {
                    cbTeensy.Items.Remove(board);
                    cbTeensy.SelectedIndex = 0;
                });
            }
        }



        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                tbFirmware.Text = dlg.FileName;
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(tbFirmware.Text))
            {
                MessageBox.Show("Firmware file not found!");
                return;
            }

            var board = cbTeensy.SelectedItem as USB_Device;
            var image = SharpUploader.GetEmptyFlashImage(board.Board);

            using (var stream = File.OpenText(tbFirmware.Text))
            {
                SharpHexParser.ParseStream(stream, image);
            }

            var fwType = SharpHexParser.IdentifyModel(image);
            if (fwType != board.Board)
            {
                MessageBox.Show("Firmware not compatible with Board");
                return;
            }

            SharpUploader.StartHalfKay(board.Serialnumber);
            int result = SharpUploader.Upload(image, board.Board, board.Serialnumber);

            if (result != 0)
            {
                MessageBox.Show("Error uploading board");
            }
        }
    }
}
