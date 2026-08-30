namespace SoundTray
{
    partial class SoundTrayStatus
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SoundTrayStatus));
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            outputDevicesDataGridView = new DataGridView();
            inputDevicesDataGridView = new DataGridView();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)outputDevicesDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)inputDevicesDataGridView).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Location = new Point(2, 6);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1162, 731);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(outputDevicesDataGridView);
            tabPage1.Controls.Add(inputDevicesDataGridView);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1154, 693);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Input/Output Audio Devices";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // outputDevicesDataGridView
            // 
            outputDevicesDataGridView.AllowUserToAddRows = false;
            outputDevicesDataGridView.AllowUserToDeleteRows = false;
            outputDevicesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            outputDevicesDataGridView.Location = new Point(0, 320);
            outputDevicesDataGridView.MultiSelect = false;
            outputDevicesDataGridView.Name = "outputDevicesDataGridView";
            outputDevicesDataGridView.RowHeadersWidth = 62;
            outputDevicesDataGridView.Size = new Size(1151, 373);
            outputDevicesDataGridView.TabIndex = 1;
            // 
            // inputDevicesDataGridView
            // 
            inputDevicesDataGridView.AllowUserToAddRows = false;
            inputDevicesDataGridView.AllowUserToDeleteRows = false;
            inputDevicesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            inputDevicesDataGridView.Location = new Point(-4, 0);
            inputDevicesDataGridView.MultiSelect = false;
            inputDevicesDataGridView.Name = "inputDevicesDataGridView";
            inputDevicesDataGridView.RowHeadersWidth = 62;
            inputDevicesDataGridView.Size = new Size(1162, 314);
            inputDevicesDataGridView.TabIndex = 0;
            // 
            // SoundTrayStatus
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1165, 739);
            Controls.Add(tabControl1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "SoundTrayStatus";
            Text = "Sound Tray Settings";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)outputDevicesDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)inputDevicesDataGridView).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private DataGridView outputDevicesDataGridView;
        private DataGridView inputDevicesDataGridView;
    }
}