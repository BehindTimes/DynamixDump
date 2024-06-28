using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WillyBeamishDump
{
    class MapReader
    {
        public struct FileInfo
        {
            public UInt32 hash;
            public UInt32 offset;
        }
        public struct ResourceFileInfo
        {
            public string strName1;
            public int numFiles;
            public List<FileInfo> file_info;
        }

        private int m_Version = 0;
        public int Version { get { return m_Version; } }

        private int m_Volumes = 0;
        public int Volumes { get { return m_Volumes; } }

        private List<ResourceFileInfo> m_rfd;
        public List<ResourceFileInfo> ResourceData { get { return m_rfd; } }

        public bool ReadMap(string strFile)
        {
            if (!File.Exists(strFile))
            {
                return false;
            }
            using (BinaryReader reader = new BinaryReader(File.Open(strFile, FileMode.Open)))
            {
                m_Version = reader.ReadInt32();
                m_Volumes = reader.ReadInt16();
                m_rfd = new List<ResourceFileInfo>();
                for (int index = 0; index < m_Volumes; index++)
                {
                    ResourceFileInfo rfd = new ResourceFileInfo();
                    rfd.file_info = new List<FileInfo>();
                    char[] name = reader.ReadChars(13);
                    foreach (char c in name)
                    {
                        if (c == 0)
                        {
                            break;
                        }
                        rfd.strName += c;
                    }

                    rfd.numFiles = reader.ReadInt16();
                    for (int file_index = 0; file_index < rfd.numFiles; file_index++)
                    {
                        FileInfo fi = new FileInfo();
                        fi.hash = reader.ReadUInt32();
                        fi.offset = reader.ReadUInt32();
                        rfd.file_info.Add(fi);
                    }
                    m_rfd.Add(rfd);
                }
            }
            return true;
        }
    }
}
