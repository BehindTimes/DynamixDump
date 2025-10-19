namespace DynamixLZW
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
            this.btnBrowseCompress = new System.Windows.Forms.Button();
            this.btnBrowseExtract = new System.Windows.Forms.Button();
            this.lblCompress = new System.Windows.Forms.Label();
            this.lblExtract = new System.Windows.Forms.Label();
            this.tbCompress = new System.Windows.Forms.TextBox();
            this.tbExtract = new System.Windows.Forms.TextBox();
            this.rbExtract = new System.Windows.Forms.RadioButton();
            this.rbCompress = new System.Windows.Forms.RadioButton();
            this.btnProcess = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnBrowseCompress
            // 
            this.btnBrowseCompress.Location = new System.Drawing.Point(447, -1);
            this.btnBrowseCompress.Name = "btnBrowseCompress";
            this.btnBrowseCompress.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseCompress.TabIndex = 0;
            this.btnBrowseCompress.Text = "Browse...";
            this.btnBrowseCompress.UseVisualStyleBackColor = true;
            this.btnBrowseCompress.Click += new System.EventHandler(this.btnBrowseCompress_Click);
            // 
            // btnBrowseExtract
            // 
            this.btnBrowseExtract.Location = new System.Drawing.Point(447, 28);
            this.btnBrowseExtract.Name = "btnBrowseExtract";
            this.btnBrowseExtract.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseExtract.TabIndex = 1;
            this.btnBrowseExtract.Text = "Browse...";
            this.btnBrowseExtract.UseVisualStyleBackColor = true;
            this.btnBrowseExtract.Click += new System.EventHandler(this.btnBrowseExtract_Click);
            // 
            // lblCompress
            // 
            this.lblCompress.AutoSize = true;
            this.lblCompress.Location = new System.Drawing.Point(12, 9);
            this.lblCompress.Name = "lblCompress";
            this.lblCompress.Size = new System.Drawing.Size(87, 13);
            this.lblCompress.TabIndex = 2;
            this.lblCompress.Text = "Compressed File:";
            // 
            // lblExtract
            // 
            this.lblExtract.AutoSize = true;
            this.lblExtract.Location = new System.Drawing.Point(12, 33);
            this.lblExtract.Name = "lblExtract";
            this.lblExtract.Size = new System.Drawing.Size(100, 13);
            this.lblExtract.TabIndex = 3;
            this.lblExtract.Text = "Uncompressed File:";
            // 
            // tbCompress
            // 
            this.tbCompress.Location = new System.Drawing.Point(156, 4);
            this.tbCompress.Name = "tbCompress";
            this.tbCompress.Size = new System.Drawing.Size(285, 20);
            this.tbCompress.TabIndex = 4;
            this.tbCompress.Text = "G:\\source\\DynamixLZW\\files\\testme.dat";
            // 
            // tbExtract
            // 
            this.tbExtract.Location = new System.Drawing.Point(156, 30);
            this.tbExtract.Name = "tbExtract";
            this.tbExtract.Size = new System.Drawing.Size(285, 20);
            this.tbExtract.TabIndex = 5;
            this.tbExtract.Text = "G:\\source\\DynamixLZW\\files\\out.lzw";
            // 
            // rbExtract
            // 
            this.rbExtract.AutoSize = true;
            this.rbExtract.Checked = true;
            this.rbExtract.Location = new System.Drawing.Point(15, 88);
            this.rbExtract.Name = "rbExtract";
            this.rbExtract.Size = new System.Drawing.Size(58, 17);
            this.rbExtract.TabIndex = 6;
            this.rbExtract.TabStop = true;
            this.rbExtract.Text = "Extract";
            this.rbExtract.UseVisualStyleBackColor = true;
            this.rbExtract.CheckedChanged += new System.EventHandler(this.rbExtract_CheckedChanged);
            // 
            // rbCompress
            // 
            this.rbCompress.AutoSize = true;
            this.rbCompress.Location = new System.Drawing.Point(79, 88);
            this.rbCompress.Name = "rbCompress";
            this.rbCompress.Size = new System.Drawing.Size(71, 17);
            this.rbCompress.TabIndex = 7;
            this.rbCompress.Text = "Compress";
            this.rbCompress.UseVisualStyleBackColor = true;
            this.rbCompress.CheckedChanged += new System.EventHandler(this.rbCompress_CheckedChanged);
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(447, 83);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(75, 23);
            this.btnProcess.TabIndex = 8;
            this.btnProcess.Text = "Extract";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 127);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.rbCompress);
            this.Controls.Add(this.rbExtract);
            this.Controls.Add(this.tbExtract);
            this.Controls.Add(this.tbCompress);
            this.Controls.Add(this.lblExtract);
            this.Controls.Add(this.lblCompress);
            this.Controls.Add(this.btnBrowseExtract);
            this.Controls.Add(this.btnBrowseCompress);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowseCompress;
        private System.Windows.Forms.Button btnBrowseExtract;
        private System.Windows.Forms.Label lblCompress;
        private System.Windows.Forms.Label lblExtract;
        private System.Windows.Forms.TextBox tbCompress;
        private System.Windows.Forms.TextBox tbExtract;
        private System.Windows.Forms.RadioButton rbExtract;
        private System.Windows.Forms.RadioButton rbCompress;
        private System.Windows.Forms.Button btnProcess;
    }
}

