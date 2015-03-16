namespace Morser
{
    partial class MorserUi
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MorserUi));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.wpmTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.currentWpm = new System.Windows.Forms.Label();
            this.recordButton = new System.Windows.Forms.Button();
            this.audioVolumePicture = new System.Windows.Forms.PictureBox();
            this.audioPanel = new System.Windows.Forms.Panel();
            this.volumeThreshold = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.recentText = new System.Windows.Forms.Label();
            this.autoSpace = new System.Windows.Forms.CheckBox();
            this.cbListening = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wpmTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioVolumePicture)).BeginInit();
            this.audioPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.volumeThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(305, 449);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(163, 464);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Words per Minute";
            // 
            // wpmTrackBar
            // 
            this.wpmTrackBar.Location = new System.Drawing.Point(59, 484);
            this.wpmTrackBar.Maximum = 40;
            this.wpmTrackBar.Minimum = 1;
            this.wpmTrackBar.Name = "wpmTrackBar";
            this.wpmTrackBar.Size = new System.Drawing.Size(105, 45);
            this.wpmTrackBar.TabIndex = 3;
            this.wpmTrackBar.Value = 15;
            this.wpmTrackBar.Scroll += new System.EventHandler(this.wmpTrackBar_Scroll);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 516);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(145, 516);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "40";
            // 
            // currentWpm
            // 
            this.currentWpm.AutoSize = true;
            this.currentWpm.Location = new System.Drawing.Point(138, 464);
            this.currentWpm.Name = "currentWpm";
            this.currentWpm.Size = new System.Drawing.Size(19, 13);
            this.currentWpm.TabIndex = 7;
            this.currentWpm.Text = "15";
            // 
            // recordButton
            // 
            this.recordButton.Location = new System.Drawing.Point(255, 495);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(75, 23);
            this.recordButton.TabIndex = 8;
            this.recordButton.Text = "Learn WPM";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.recordButton_Click);
            // 
            // audioVolumePicture
            // 
            this.audioVolumePicture.BackColor = System.Drawing.Color.Green;
            this.audioVolumePicture.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.audioVolumePicture.Location = new System.Drawing.Point(0, 397);
            this.audioVolumePicture.Name = "audioVolumePicture";
            this.audioVolumePicture.Size = new System.Drawing.Size(29, 20);
            this.audioVolumePicture.TabIndex = 9;
            this.audioVolumePicture.TabStop = false;
            // 
            // audioPanel
            // 
            this.audioPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.audioPanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.audioPanel.Controls.Add(this.audioVolumePicture);
            this.audioPanel.Location = new System.Drawing.Point(334, 13);
            this.audioPanel.Name = "audioPanel";
            this.audioPanel.Size = new System.Drawing.Size(29, 417);
            this.audioPanel.TabIndex = 10;
            // 
            // volumeThreshold
            // 
            this.volumeThreshold.Location = new System.Drawing.Point(369, 13);
            this.volumeThreshold.Maximum = 100;
            this.volumeThreshold.Name = "volumeThreshold";
            this.volumeThreshold.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.volumeThreshold.Size = new System.Drawing.Size(45, 417);
            this.volumeThreshold.TabIndex = 11;
            this.volumeThreshold.TickStyle = System.Windows.Forms.TickStyle.None;
            this.volumeThreshold.Value = 90;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(323, 436);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Audio Threshold";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 542);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Recent Text: ";
            // 
            // recentText
            // 
            this.recentText.AutoSize = true;
            this.recentText.Location = new System.Drawing.Point(91, 542);
            this.recentText.Name = "recentText";
            this.recentText.Size = new System.Drawing.Size(0, 13);
            this.recentText.TabIndex = 14;
            // 
            // autoSpace
            // 
            this.autoSpace.AutoSize = true;
            this.autoSpace.Location = new System.Drawing.Point(249, 525);
            this.autoSpace.Name = "autoSpace";
            this.autoSpace.Size = new System.Drawing.Size(80, 17);
            this.autoSpace.TabIndex = 15;
            this.autoSpace.Text = "Auto-space";
            this.autoSpace.UseVisualStyleBackColor = true;
            // 
            // cbListening
            // 
            this.cbListening.AutoSize = true;
            this.cbListening.Enabled = false;
            this.cbListening.Location = new System.Drawing.Point(329, 452);
            this.cbListening.Name = "cbListening";
            this.cbListening.Size = new System.Drawing.Size(68, 17);
            this.cbListening.TabIndex = 16;
            this.cbListening.Text = "Listening";
            this.cbListening.UseVisualStyleBackColor = true;
            // 
            // MorserUi
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 567);
            this.Controls.Add(this.cbListening);
            this.Controls.Add(this.autoSpace);
            this.Controls.Add(this.recentText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.volumeThreshold);
            this.Controls.Add(this.audioPanel);
            this.Controls.Add(this.recordButton);
            this.Controls.Add(this.currentWpm);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.wpmTrackBar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MorserUi";
            this.Text = "Morser - Press Control-Alt-M to enable.";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wpmTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.audioVolumePicture)).EndInit();
            this.audioPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.volumeThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar wpmTrackBar;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label currentWpm;
        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.PictureBox audioVolumePicture;
        private System.Windows.Forms.Panel audioPanel;
        private System.Windows.Forms.TrackBar volumeThreshold;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label recentText;
        private System.Windows.Forms.CheckBox autoSpace;
        private System.Windows.Forms.CheckBox cbListening;


    }
}

