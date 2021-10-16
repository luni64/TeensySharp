using libTeensySharp;
using lunOptics.libTeensySharp;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace WinForms_Test
{
    public partial class MainForm1 : Form
    {
        TeensyWatcher watcher;

        public MainForm1()
        {
            InitializeComponent();

            watcher = new TeensyWatcher(SynchronizationContext.Current);
            watcher.ConnectedTeensies.CollectionChanged += ConnectedTeensiesChanged;
            foreach (var teensy in watcher.ConnectedTeensies)
            {
                lbTeensies.Items.Add(teensy);
            }
            if (lbTeensies.Items.Count > 0) lbTeensies.SelectedIndex = 0;
        }
                
        private void ConnectedTeensiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var teensy in e.NewItems)
                    {                     
                        lbTeensies.Items.Add(teensy);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var teensy in e.OldItems)
                    {                      
                        lbTeensies.Items.Remove(teensy);
                    }
                    break;
            }
            if (lbTeensies.SelectedIndex == -1 && lbTeensies.Items.Count > 0) lbTeensies.SelectedIndex = 0;
        }

        private async void btnReset_click(object sender, EventArgs e)
        {
            var teensy = lbTeensies.SelectedItem as ITeensy;
            if (teensy != null)
            {
                await teensy.ResetAsync();
            }
        }

        private async void btnBoot_click(object sender, EventArgs e)
        {
            var teensy = lbTeensies.SelectedItem as ITeensy;
            if (teensy != null)
            {
                await teensy.RebootAsync();
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            var teensy = lbTeensies.SelectedItem as ITeensy;
            if (teensy != null)
            {
                string filename = tbHexfile.Text;
                if (File.Exists(filename))
                {
                    var progress = new Progress<int>(v => progressBar.Value = v);
                    progressBar.Visible = true;
                    var result = await teensy.UploadAsync(filename, progress);
                    MessageBox.Show(result.ToString(), "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    progressBar.Visible = false;
                    progressBar.Value = 0;
                }
                else MessageBox.Show("File does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbHexfile.Text = openFileDialog.FileName;

                    var fw = new TeensyFirmware(tbHexfile.Text);
                    lblFWType.Text = "Firmware type: " + fw.boardType.ToString();
                }
            }
        }
    }
}
