using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WillyBeamishDump
{
    class ResourceReader
    {
        public struct ResourceFileData
        {
            public string strFile;
            public string strExtention;
            public Int32 fileSize;
            public byte[] data;
            public char[] rawName;
        }

        List<ResourceFileData> m_FileData;
        public List<ResourceFileData> FileData { get { return m_FileData; } }

        public bool ReadResource(string strDirectory, List<MapReader.ResourceFileInfo> file_data)
        {
            m_FileData = new List<ResourceFileData>();

            foreach (MapReader.ResourceFileInfo rfd in file_data)
            {
                string strTempFile = strDirectory + rfd.strName;
                if (!File.Exists(strTempFile))
                {
                    continue; // The resource is missing
                }
                using (BinaryReader reader = new BinaryReader(File.Open(strTempFile, FileMode.Open)))
                {
                    foreach (MapReader.FileInfo fi in rfd.file_info)
                    {
                        ResourceFileData curData = new ResourceFileData();
                        curData.rawName = new char[13];
                        string strSubFile = "";
                        string strExtention = "";
                        bool bFoundExt = false;
                        reader.BaseStream.Seek(fi.offset, SeekOrigin.Begin);
                        // There seems to be a problem with UTF8 reading the character properly
                        byte[] outbyte = reader.ReadBytes(13);
                        for (int nIndex = 0; nIndex < 13; nIndex++)
                        {
                            char curChar = Convert.ToChar(outbyte[nIndex]);
                            curData.rawName[nIndex] = curChar;
                            if (curData.rawName[nIndex] == '\0')
                            {
                                break;
                            }
                            if (curData.rawName[nIndex] == '.')
                            {
                                bFoundExt = true;
                            }
                            strSubFile += curData.rawName[nIndex];
                            if (bFoundExt)
                            {
                                strExtention += curData.rawName[nIndex];
                            }
                        }

                        curData.strFile = strSubFile;
                        curData.strExtention = strExtention;
                        Byte[] bytes = reader.ReadBytes(4);
                        curData.fileSize = BitConverter.ToInt32(bytes, 0);

                        if (curData.fileSize > 0)
                        {
                            curData.data = reader.ReadBytes((int)curData.fileSize);
                        }
                        else
                        {
                            curData.data = null;
                        }

                        m_FileData.Add(curData);
                    }
                }
            }

            return true;
        }
    }
}
