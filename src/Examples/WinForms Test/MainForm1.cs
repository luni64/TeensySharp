using libTeensySharp;
using lunOptics.libTeensySharp;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

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

             object old = null;
        private void ConnectedTeensiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var teensy in e.NewItems)
                    {
                        bool b = old == teensy;
                        lbTeensies.Items.Add(teensy);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var teensy in e.OldItems)
                    {
                        old = teensy;
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
                string filename = lbHexfile.Text;
                if (File.Exists(filename))
                {                    
                    IProgress<int> p = new Progress<int>(v => progressBar.Value = v);
                    progressBar.Visible = true;                    

                    var result = await teensy.UploadAsync(filename,p);

                    MessageBox.Show(result.ToString(), "Message",MessageBoxButtons.OK, MessageBoxIcon.Information);
                    progressBar.Visible = false;
                    progressBar.Value = 0;
                }
                else MessageBox.Show("File does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    lbHexfile.Text = openFileDialog.FileName;

                    var fw = new TeensyFirmware(lbHexfile.Text);
                    lblFWType.Text = "Firmware type: "+  fw.boardType.ToString();
                }
            }
        }

      
    }
}
