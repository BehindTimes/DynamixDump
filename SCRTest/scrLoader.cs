using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SCRTest
{
    internal class scrLoader
    {
        enum FileType
        {
            UNKNOWN,
            SCR,
            VQT
        }
        public void load(string strDir)
        {
            if (!strDir.EndsWith("\\"))
            {
                strDir += "\\";
            }
            string[] strFiles = Directory.GetFiles(strDir);
            for (int index = 0; index < strFiles.Length; ++index)
            {
                string strFile = strFiles[index];
               if(strFile.EndsWith("DYNAMIX.SCR"))
                {
                    processFile(strFile);
                }
            }
        }

        private bool processFile(string strFile)
        {
            bool ret = false;
            if (!File.Exists(strFile))
            {
                return false;
            }
            
            byte[] scrFile = File.ReadAllBytes(strFile);

            ret = processData(scrFile);

            return ret;
        }

        private bool processData(byte[] scrFile)
        {
            int pos = 0;
            uint section_size;
            FileType type;
            bool isContainer;

            if (!readHeader(scrFile, ref pos, out isContainer, out type, out section_size))
            {
                return false;
            }
            while (true)
            {
                byte[] byte_page = new byte[section_size];
                Buffer.BlockCopy(scrFile, pos, byte_page, 0, (int)section_size);
                if(type == FileType.SCR)
                {
                    processData(byte_page);
                }
                else if(type == FileType.VQT)
                {
                    processVqt(byte_page);
                }
                else
                {
                    return false;
                }
                pos += (int)section_size;
                if(pos >= scrFile.Length)
                {
                    break;
                }
            }

            return true;
        }

        private void processVqt(byte[] vqtData)
        {
            int j = 9;
        }

        private bool readHeader(byte[] scrFile, ref int pos, out bool isContainer, out FileType type, out uint section_size)
        {
            type = FileType.UNKNOWN;
            isContainer = false;
            section_size = 0;

            if(scrFile.Length < 8)
            {
                return false;
            }

            string strType = System.Text.Encoding.ASCII.GetString(scrFile, pos, 4);
            pos += 4;
            if(strType == "SCR:")
            {
                type = FileType.SCR;
            }
            else if(strType == "VQT:")
            {
                type = FileType.VQT;
            }
            else
            {
                return false;
            }
            section_size = BitConverter.ToUInt32(scrFile, pos);
            if((section_size & 0x80000000) > 0)
            {
                isContainer = true;
                section_size -= 0x80000000;
            }
            pos += 4;
            return true;
        }
    }
}
