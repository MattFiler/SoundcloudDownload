namespace SoundcloudDownload
{
    partial class SoundcloudDownload
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
            this.startDownload = new System.Windows.Forms.Button();
            this.soundcloudURLs = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // startDownload
            // 
            this.startDownload.Location = new System.Drawing.Point(446, 298);
            this.startDownload.Name = "startDownload";
            this.startDownload.Size = new System.Drawing.Size(90, 29);
            this.startDownload.TabIndex = 9;
            this.startDownload.Text = "Start";
            this.startDownload.UseVisualStyleBackColor = true;
            this.startDownload.Click += new System.EventHandler(this.startDownload_Click);
            // 
            // soundcloudURLs
            // 
            this.soundcloudURLs.Location = new System.Drawing.Point(12, 25);
            this.soundcloudURLs.Multiline = true;
            this.soundcloudURLs.Name = "soundcloudURLs";
            this.soundcloudURLs.Size = new System.Drawing.Size(524, 267);
            this.soundcloudURLs.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "Soundcloud URLs (one per line)";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 298);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(428, 29);
            this.progressBar.TabIndex = 26;
            // 
            // SoundcloudDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 338);
            this.Controls.Add(this.startDownload);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.soundcloudURLs);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "SoundcloudDownload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Soundcloud Downloader";
            this.Load += new System.EventHandler(this.SoundcloudDownload_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button startDownload;
        private System.Windows.Forms.TextBox soundcloudURLs;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

