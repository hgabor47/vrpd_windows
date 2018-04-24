namespace VRPrelimutensDesktopGUI
{
    partial class Form1
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
            this.net = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ib = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lc = new System.Windows.Forms.TrackBar();
            this.maxwin = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.outp = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.lc)).BeginInit();
            this.SuspendLayout();
            // 
            // net
            // 
            this.net.Location = new System.Drawing.Point(197, 10);
            this.net.Name = "net";
            this.net.Size = new System.Drawing.Size(134, 20);
            this.net.TabIndex = 0;
            this.net.Text = "192.168.42.100";
            this.net.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            this.net.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Local network card IP address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Imagebuffer IP address";
            // 
            // ib
            // 
            this.ib.Location = new System.Drawing.Point(197, 49);
            this.ib.Name = "ib";
            this.ib.Size = new System.Drawing.Size(134, 20);
            this.ib.TabIndex = 3;
            this.ib.Text = "127.0.0.1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(192, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Maximum number of spreaded windows";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // lc
            // 
            this.lc.Location = new System.Drawing.Point(211, 81);
            this.lc.Maximum = 20;
            this.lc.Name = "lc";
            this.lc.Size = new System.Drawing.Size(104, 45);
            this.lc.TabIndex = 5;
            this.lc.Value = 10;
            this.lc.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // maxwin
            // 
            this.maxwin.AutoSize = true;
            this.maxwin.Location = new System.Drawing.Point(312, 81);
            this.maxwin.Name = "maxwin";
            this.maxwin.Size = new System.Drawing.Size(19, 13);
            this.maxwin.TabIndex = 6;
            this.maxwin.Text = "10";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(373, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 29);
            this.button1.TabIndex = 7;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // outp
            // 
            this.outp.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outp.Font = new System.Drawing.Font("Courier New", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.outp.Location = new System.Drawing.Point(16, 119);
            this.outp.Multiline = true;
            this.outp.Name = "outp";
            this.outp.ReadOnly = true;
            this.outp.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outp.Size = new System.Drawing.Size(486, 165);
            this.outp.TabIndex = 8;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(373, 56);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(111, 25);
            this.button2.TabIndex = 9;
            this.button2.Text = "Stop";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(98, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "on android network";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 296);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.outp);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.maxwin);
            this.Controls.Add(this.lc);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ib);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.net);
            this.Name = "Form1";
            this.Text = "VR Prelimutens Desktop GUI";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.lc)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox net;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ib;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar lc;
        private System.Windows.Forms.Label maxwin;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox outp;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label4;
    }
}

