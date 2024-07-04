using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WillyBeamishDump
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(tbOut.Text.Length <= 0)
            {
                tbOut.Text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            string strMap = tbResource.Text;
            string strOut = tbOut.Text;
            if(!strOut.EndsWith("\\"))
            {
                strOut += "\\";
            }

            if(File.Exists(strMap))
            {
                if(Directory.Exists(strOut))
                {
                    string strPath = Path.GetDirectoryName(strMap);
                    if (!strPath.EndsWith("\\"))
                    {
                        strPath += "\\";
                    }
                    MapReader mr = new MapReader();
                    ResourceDumper rd = new ResourceDumper();
                    if (mr.ReadMap(strMap))
                    {
                        ResourceReader rr = new ResourceReader();
                        if (rr.ReadResource(strPath, mr.ResourceData))
                        {
                            foreach (ResourceReader.ResourceFileData rfd in rr.FileData)
                            {
                                if (rd.WriteResource(rfd, strOut))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnBrowseResource_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog () == DialogResult.OK)
            {
                tbResource.Text = openFileDialog.FileName;
            }
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                tbOut.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }
}
