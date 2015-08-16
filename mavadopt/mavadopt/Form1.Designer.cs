namespace mavadopt
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
            this.cb_source = new System.Windows.Forms.ComboBox();
            this.cb_baudrate = new System.Windows.Forms.ComboBox();
            this.pb_connect = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cb_source
            // 
            this.cb_source.FormattingEnabled = true;
            this.cb_source.Location = new System.Drawing.Point(25, 12);
            this.cb_source.Name = "cb_source";
            this.cb_source.Size = new System.Drawing.Size(121, 21);
            this.cb_source.TabIndex = 0;
            // 
            // cb_baudrate
            // 
            this.cb_baudrate.FormattingEnabled = true;
            this.cb_baudrate.Items.AddRange(new object[] {
            "9600",
            "57600",
            "115200"});
            this.cb_baudrate.Location = new System.Drawing.Point(167, 12);
            this.cb_baudrate.Name = "cb_baudrate";
            this.cb_baudrate.Size = new System.Drawing.Size(121, 21);
            this.cb_baudrate.TabIndex = 1;
            // 
            // pb_connect
            // 
            this.pb_connect.Location = new System.Drawing.Point(305, 10);
            this.pb_connect.Name = "pb_connect";
            this.pb_connect.Size = new System.Drawing.Size(75, 23);
            this.pb_connect.TabIndex = 2;
            this.pb_connect.Text = "Connect";
            this.pb_connect.UseVisualStyleBackColor = true;
            this.pb_connect.Click += new System.EventHandler(this.pb_connect_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(302, 93);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(13, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(302, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(13, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(302, 155);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(13, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 259);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pb_connect);
            this.Controls.Add(this.cb_baudrate);
            this.Controls.Add(this.cb_source);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cb_source;
        private System.Windows.Forms.ComboBox cb_baudrate;
        private System.Windows.Forms.Button pb_connect;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}

