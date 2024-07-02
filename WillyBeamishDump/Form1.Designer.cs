namespace WillyBeamishDump
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
            this.lblResourceLoc = new System.Windows.Forms.Label();
            this.lblOut = new System.Windows.Forms.Label();
            this.tbResource = new System.Windows.Forms.TextBox();
            this.tbOut = new System.Windows.Forms.TextBox();
            this.btnBrowseResource = new System.Windows.Forms.Button();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.btnExtract = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblResourceLoc
            // 
            this.lblResourceLoc.AutoSize = true;
            this.lblResourceLoc.Location = new System.Drawing.Point(12, 9);
            this.lblResourceLoc.Name = "lblResourceLoc";
            this.lblResourceLoc.Size = new System.Drawing.Size(56, 13);
            this.lblResourceLoc.TabIndex = 0;
            this.lblResourceLoc.Text = "Resource:";
            // 
            // lblOut
            // 
            this.lblOut.AutoSize = true;
            this.lblOut.Location = new System.Drawing.Point(12, 45);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(72, 13);
            this.lblOut.TabIndex = 1;
            this.lblOut.Text = "Out Directory:";
            // 
            // tbResource
            // 
            this.tbResource.Location = new System.Drawing.Point(88, 6);
            this.tbResource.Name = "tbResource";
            this.tbResource.Size = new System.Drawing.Size(360, 20);
            this.tbResource.TabIndex = 2;
            this.tbResource.Text = "F:\\source\\WillyBeamishDump\\files\\willy\\resource.map";
            // 
            // tbOut
            // 
            this.tbOut.Location = new System.Drawing.Point(90, 40);
            this.tbOut.Name = "tbOut";
            this.tbOut.Size = new System.Drawing.Size(360, 20);
            this.tbOut.TabIndex = 3;
            this.tbOut.Text = "F:\\source\\WillyBeamishDump\\files\\willy\\out";
            // 
            // btnBrowseResource
            // 
            this.btnBrowseResource.Location = new System.Drawing.Point(454, 4);
            this.btnBrowseResource.Name = "btnBrowseResource";
            this.btnBrowseResource.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseResource.TabIndex = 4;
            this.btnBrowseResource.Text = "Browse...";
            this.btnBrowseResource.UseVisualStyleBackColor = true;
            this.btnBrowseResource.Click += new System.EventHandler(this.btnBrowseResource_Click);
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(456, 38);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOut.TabIndex = 5;
            this.btnBrowseOut.Text = "Browse...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(12, 82);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(75, 23);
            this.btnExtract.TabIndex = 6;
            this.btnExtract.Text = "Extract...";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.btnExtract_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 121);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.btnBrowseOut);
            this.Controls.Add(this.btnBrowseResource);
            this.Controls.Add(this.tbOut);
            this.Controls.Add(this.tbResource);
            this.Controls.Add(this.lblOut);
            this.Controls.Add(this.lblResourceLoc);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblResourceLoc;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.TextBox tbResource;
        private System.Windows.Forms.TextBox tbOut;
        private System.Windows.Forms.Button btnBrowseResource;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.Button btnExtract;
    }
}

