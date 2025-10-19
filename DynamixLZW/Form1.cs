using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamixLZW
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rbCompress.Checked = true;
        }

        private void btnBrowseCompress_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult result = ofd.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                tbCompress.Text = ofd.FileName;
            }
        }

        private void btnBrowseExtract_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            DialogResult result = sfd.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                tbExtract.Text = sfd.FileName;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] file_bytes = File.ReadAllBytes(tbCompress.Text);
                if (file_bytes.Length > 0)
                {
                    lzwdecompressor lzw = new lzwdecompressor();

                    if (rbCompress.Checked)
                    {
                        lzw.compress(file_bytes, tbExtract.Text);
                    }
                    else
                    {
                        lzw.extract(file_bytes, tbExtract.Text);
                    }
                }
            }
            catch (IOException)
            {
            }
        }

        private void rbExtract_CheckedChanged(object sender, EventArgs e)
        {
            if (rbExtract.Checked)
            {
                lblCompress.Text = "Compressed File:";
                lblExtract.Text = "Uncompressed File:";
                btnProcess.Text = "Extract";
                tbCompress.Text = "G:\\source\\DynamixLZW\\files\\testme.dat";
                tbExtract.Text = "G:\\source\\DynamixLZW\\files\\ggg.lzw";
            }
        }

        private void rbCompress_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCompress.Checked)
            {
                lblCompress.Text = "Uncompressed File:";
                lblExtract.Text = "Compressed File:";
                btnProcess.Text = "Compress";
                tbExtract.Text = "G:\\source\\DynamixLZW\\files\\ccc.zzz";
                tbCompress.Text = "G:\\source\\DynamixLZW\\files\\bbb.lzw";
            }
        }
    }
}
