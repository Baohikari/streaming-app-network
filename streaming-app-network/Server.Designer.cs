﻿namespace streaming_app_network
{
    partial class Server
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
            this.stream_screen = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.stream_screen)).BeginInit();
            this.SuspendLayout();
            // 
            // stream_screen
            // 
            this.stream_screen.Location = new System.Drawing.Point(12, 12);
            this.stream_screen.Name = "stream_screen";
            this.stream_screen.Size = new System.Drawing.Size(564, 311);
            this.stream_screen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.stream_screen.TabIndex = 0;
            this.stream_screen.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(368, 402);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.stream_screen);
            this.Name = "Server";
            this.Text = "Server";
            ((System.ComponentModel.ISupportInitialize)(this.stream_screen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox stream_screen;
        private System.Windows.Forms.Button button1;
    }
}