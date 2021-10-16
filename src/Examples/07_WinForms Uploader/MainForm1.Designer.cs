
namespace WinForms_Test
{
    partial class MainForm1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnReset = new System.Windows.Forms.Button();
            this.lbTeensies = new System.Windows.Forms.ListBox();
            this.btnBoot = new System.Windows.Forms.Button();
            this.tbHexfile = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblFWType = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(296, 262);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(81, 23);
            this.btnReset.TabIndex = 0;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_click);
            // 
            // lbTeensies
            // 
            this.lbTeensies.DisplayMember = "description";
            this.lbTeensies.FormattingEnabled = true;
            this.lbTeensies.ItemHeight = 15;
            this.lbTeensies.Location = new System.Drawing.Point(12, 12);
            this.lbTeensies.Name = "lbTeensies";
            this.lbTeensies.Size = new System.Drawing.Size(365, 244);
            this.lbTeensies.TabIndex = 1;
            // 
            // btnBoot
            // 
            this.btnBoot.Location = new System.Drawing.Point(209, 262);
            this.btnBoot.Name = "btnBoot";
            this.btnBoot.Size = new System.Drawing.Size(84, 23);
            this.btnBoot.TabIndex = 2;
            this.btnBoot.Text = "Bootloader";
            this.btnBoot.UseVisualStyleBackColor = true;
            this.btnBoot.Click += new System.EventHandler(this.btnBoot_click);
            // 
            // lbHexfile
            // 
            this.tbHexfile.Location = new System.Drawing.Point(12, 315);
            this.tbHexfile.Name = "lbHexfile";
            this.tbHexfile.Size = new System.Drawing.Size(191, 23);
            this.tbHexfile.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(209, 315);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(81, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(296, 315);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(81, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Upload";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 297);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Firmware";
            // 
            // lblFWType
            // 
            this.lblFWType.AutoSize = true;
            this.lblFWType.Font = new System.Drawing.Font("Segoe UI", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblFWType.Location = new System.Drawing.Point(12, 341);
            this.lblFWType.Name = "lblFWType";
            this.lblFWType.Size = new System.Drawing.Size(0, 12);
            this.lblFWType.TabIndex = 7;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(297, 344);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(80, 10);
            this.progressBar.TabIndex = 8;
            this.progressBar.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 374);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblFWType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbHexfile);
            this.Controls.Add(this.btnBoot);
            this.Controls.Add(this.lbTeensies);
            this.Controls.Add(this.btnReset);
            this.Name = "Form1";
            this.Text = "WinForms  - Uploader Example";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ListBox lbTeensies;
        private System.Windows.Forms.Button btnBoot;
        private System.Windows.Forms.TextBox tbHexfile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFWType;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

