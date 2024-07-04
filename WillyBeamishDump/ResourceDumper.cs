using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Data.SqlTypes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ExplorerBar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WillyBeamishDump
{
    struct WavHeader
    {
        public byte[] riffID;
        public uint size;
        public byte[] wavID;
        public byte[] fmtChunkID;
        public uint fmtChunkSize;
        public ushort formatTag;
        public ushort channels;
        public uint sampleRate;
        public uint bytePerSec;
        public ushort blockAlign;
        public ushort bitsPerSample;
        public ushort padding;
        public byte[] dataChunkID;
        public uint dataChunkSize;
    }

    class ResourceDumper
    {
        string m_strOutDir;

        private void WriteWave(string strOut, byte[] data, uint hertz, ushort channels)
        {
            WavHeader header = new WavHeader();
            header.riffID = Encoding.ASCII.GetBytes("RIFF");
            header.size = (uint)data.Length + 36; // size of wave file from here on
            header.wavID = Encoding.ASCII.GetBytes("WAVE");
            header.fmtChunkID = Encoding.ASCII.GetBytes("fmt ");
            header.fmtChunkSize = 0x12;  // The length of the TAG format
            header.formatTag = 1; // should be 1 for PCM type data
            header.channels = channels; // Mono = 1, Stereo = 2, etc.
            header.sampleRate = hertz;  // 6000, 8000, 44100, etc.
            header.bytePerSec = hertz * channels; // Average Data Rate
            header.blockAlign = 2; // 1 for 8 bit data, 2 for 16 bit
            header.bitsPerSample = 8; // 8 for 8 bit data, 16 for 16 bit
            header.padding = 0;
            header.dataChunkID = Encoding.ASCII.GetBytes("data");
            header.dataChunkSize = (uint)data.Length;

            using (BinaryWriter writer = new BinaryWriter(File.Open(strOut, FileMode.Create)))
            {
                writer.Write(header.riffID);
                writer.Write(header.size);
                writer.Write(header.wavID);
                writer.Write(header.fmtChunkID);
                writer.Write(header.fmtChunkSize);
                writer.Write(header.formatTag);
                writer.Write(header.channels);
                writer.Write(header.sampleRate);
                writer.Write(header.bytePerSec);
                writer.Write(header.blockAlign);
                writer.Write(header.bitsPerSample);
                writer.Write(header.padding);
                writer.Write(header.dataChunkID);
                writer.Write(header.dataChunkSize);
                writer.Write(data);
            }
        }

        private void WriteRaw(string strRawDir, string strOut, byte[] data, uint hertz, ushort num_channels)
        {
            int nDataPos = 0;
            string strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "SND:")
            {
                return;
            }
            nDataPos += 4;
            int nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "RAW:")
            {
                return;
            }
            nDataPos += 4;
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            if (nSize + nDataPos > data.Length)
            {
                return;
            }
            string fileName = strRawDir + strOut;
            Byte[] outBuffer = new Byte[nSize];
            Buffer.BlockCopy(data, nDataPos, outBuffer, 0, nSize);
            WriteWave(fileName + ".wav", outBuffer, hertz, num_channels);
        }

        private void WriteRaw(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"raw\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            WriteRaw(strRawDir, strOut, data, 6000, 2);
        }

        private void WriteCDS(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"cds\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }
            int nDataPos = 0;
            string strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "VER:")
            {
                return;
            }
            nDataPos += 4;
            int nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            byte[] byte_version = new byte[nSize];
            Buffer.BlockCopy(data, nDataPos, byte_version, 0, nSize);
            string strVersion = "";
            for (int index = 0; index < byte_version.Length; index++)
            {
                if (byte_version[index] == 0)
                {
                    break;
                }
                strVersion += (char)byte_version[index];
            }
            nDataPos += nSize;
            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "PAG:")
            {
                return;
            }
            nDataPos += 4;
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            byte[] byte_page = new byte[nSize];
            Buffer.BlockCopy(data, nDataPos, byte_page, 0, nSize);
            nDataPos += nSize;
            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "TT3:")
            {
                return;
            }
            nDataPos += 4;
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            byte[] byte_tt3 = new byte[nSize];
            Buffer.BlockCopy(data, nDataPos, byte_tt3, 0, nSize);
            nDataPos += nSize;

            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "TTI:")
            {
                return;
            }
            nDataPos += 4;
            // The TTI size is the rest of the file plus perhaps a flag
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;

            if (nDataPos + 4 > data.Length)
            {
                return;
            }
            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);
            if (strType != "TAG:")
            {
                return;
            }
            nDataPos += 4;
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            byte[] byte_tag = new byte[nSize];
            Buffer.BlockCopy(data, nDataPos, byte_tag, 0, nSize);
            nDataPos += nSize;

            // things start going wrong around here

            int bmpStart = 0;
            bool hasbmp = false;

            while (true)
            {
                if (nDataPos + 4 > data.Length)
                {
                    if (hasbmp)
                    {
                        int bmpSize = nDataPos - bmpStart;
                        hasbmp = false;
                        byte[] byte_bmp = new byte[bmpSize];
                        Buffer.BlockCopy(data, bmpStart, byte_bmp, 0, bmpSize);
                        WriteBMP(strOut, byte_bmp);
                    }
                    break;
                }
                strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);

                if (strType == "BMP:")
                {
                    bmpStart = nDataPos;
                    hasbmp = true;
                    nDataPos += 4;
                    byte[] byte_packed_size = new byte[4];
                    byte_packed_size[3] = 0;
                    Buffer.BlockCopy(data, nDataPos, byte_packed_size, 0, 3);
                    nDataPos += 3;
                    int nPackedSize;
                    nPackedSize = BitConverter.ToInt32(byte_packed_size, 0);
                    byte flag = data[nDataPos];
                    nDataPos++;
                }
                else if (strType == "INF:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_inf = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_inf, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "SCN:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_scn = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_scn, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "BIN:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_bin = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_bin, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "VGA:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_bin = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_bin, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "VQT:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_vqt = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_vqt, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "OFF:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_off = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_off, 0, nSize);
                    nDataPos += nSize;
                }
                else if (strType == "SND:")
                {
                    if (hasbmp)
                    {
                        int bmpSize = nDataPos - bmpStart;
                        hasbmp = false;
                        byte[] byte_bmp = new byte[bmpSize];
                        Buffer.BlockCopy(data, bmpStart, byte_bmp, 0, bmpSize);
                        WriteBMP(strOut, byte_bmp);
                    }
                    nSize = data.Length - nDataPos;
                    byte[] byte_raw = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_raw, 0, nSize);
                    WriteRaw(strRawDir, strOut, byte_raw, 11025, 1);
                    break;
                }
                else
                {
                    // I'm missing something
                    break;
                }
            }
        }

        private void WriteFile(string strOut, byte[] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(strOut, FileMode.Create)))
            {
                writer.Write(data);
            }
        }

        uint code_size;
        uint bits_data, bits_size;
        uint buf_ptr;
        int buf_size;
        byte[] buf_in;

        uint dict_size;
        bool dict_full;
        uint code_len;
        uint cache_bits;
        uint dict_max;

        class dict_table
        {
            public dict_table()
            {
                str = new byte[256];
            }

            public byte[] str;
            public int len;
        }

        byte[] code_cur = new byte[256];
        dict_table[] m_dict_table = new dict_table[4000];

        private void ResetLZW()
        {
            m_dict_table = new dict_table[0x4000];
            for (int index = 0; index < 0x4000; index++)
            {
                m_dict_table[index] = new dict_table();
            }

            for (int lcv = 0; lcv < 256; lcv++)
            {
                m_dict_table[lcv].len = 1;
                m_dict_table[lcv].str[0] = (byte)lcv;
            }

            // 00-FF = ASCII
            // 100 = reset

            // start = 9 bit codes
            code_size = 9;
            code_len = 0;
            // 9-12 byte cache chunks
            cache_bits = 0;
            dict_size = 0x101;
            dict_max = 0x200;
            dict_full = false;
        }

        private bool get_bits_right(uint total_bits, out uint code)
        {
            code = 0;
            uint data, num_bits;
            uint[] bits_mask = new uint[]{
                0x00, 0x01, 0x03, 0x07, 0x0f,
                0x1f, 0x3f, 0x7f, 0xff };

            num_bits = total_bits;
            data = 0;

            while (num_bits > 0)
            {
                uint use_bits;

                // ERROR!
                if (buf_ptr >= buf_size) return false;

                // 8-bit buffer
                if (bits_size == 0)
                {
                    bits_size = 8;
                    bits_data = buf_in[buf_ptr++];
                }

                use_bits = num_bits;
                if (use_bits > 8) use_bits = 8;
                if (use_bits > bits_size) use_bits = bits_size;

                // add on bits
                data |= (bits_data & bits_mask[use_bits]) << (int)(total_bits - num_bits);


                // update cache buffer
                num_bits -= use_bits;
                bits_size -= use_bits;
                bits_data >>= (int)use_bits;
            }

            code = data;
            return true;
        }

        private void UnpackLZW(uint nSize, byte[] byte_data, ref byte[] byte_data_out)
        {
            ResetLZW();

            buf_ptr = 5;  // Skip the method and size

            bits_data = 0;
            bits_size = 0;
            uint out_ptr = 0;
            uint cache_bits = 0;
            buf_in = byte_data;
            buf_size = byte_data.Length;

            while (out_ptr < nSize)
            {
                bool bRet;
                uint code;
                uint lcv;
                bool hit;

                // get next code
                bRet = get_bits_right(code_size, out code);

                if (!bRet)
                {
                    return;
                }

                // refresh data cache
                cache_bits += code_size;
                if (cache_bits >= code_size * 8)
                {
                    cache_bits -= code_size * 8;
                }

                // reset
                if (code == 0x100)
                {
                    // Dynamix: dump data cache
                    if (cache_bits > 0)
                    {
                        uint nTempCode;
                        get_bits_right(code_size * 8 - cache_bits, out nTempCode);
                    }

                    ResetLZW();
                    continue;
                }

                // special case: expand for new entry
                if (code >= dict_size && dict_full == false)
                {
                    code_cur[code_len] = code_cur[0];
                    code_len++;


                    // write output - future expanded string
                    for (lcv = 0; lcv < code_len; lcv++)
                    {
                        byte_data_out[out_ptr] = code_cur[lcv];
                        out_ptr++;
                    }
                }
                else
                {
                    // write output
                    for (lcv = 0; lcv < m_dict_table[code].len; lcv++)
                    {
                        byte_data_out[out_ptr] = m_dict_table[code].str[lcv];
                        out_ptr++;
                    }

                    // expand string
                    code_cur[code_len] = m_dict_table[code].str[0];
                    code_len++;
                }

                // add to dictionary (2+ bytes only)
                if (code_len >= 2)
                {
                    hit = false;
                }
                else
                {
                    hit = true;
                }

                // add to dictionary
                if (hit == false)
                {
                    if (dict_full == false)
                    {
                        // check full condition
                        if (dict_size == dict_max && code_size == 12)
                        {
                            dict_full = true;

                            lcv = dict_size;
                        }
                        else
                        {
                            lcv = dict_size++;

                            cache_bits = 0;
                        }


                        // expand dictionary (adaptive LZW)
                        if (dict_size == dict_max && code_size < 12)
                        {
                            dict_max *= 2;
                            code_size++;
                        }


                        // add new entry
                        for (uint lcv2 = 0; lcv2 < code_len; lcv2++)
                        {
                            m_dict_table[lcv].str[lcv2] = code_cur[lcv2];
                            m_dict_table[lcv].len++;
                        }
                    }

                    // reset running code!
                    for (lcv = 0; lcv < m_dict_table[code].len; lcv++)
                        code_cur[lcv] = m_dict_table[code].str[lcv];

                    int codepost = (int)code;
                    code_len = (uint)m_dict_table[codepost].len;
                }
            }
        }

        private bool UnpackRLE(uint nSize, byte[] byte_data, ref byte[] byte_data_out)
        {
            List<byte> listBytes = new List<byte>();
            int nPos = 5; // Skip the method and size
            int left = (int)nSize;
            int lenR = 0;
            int lenW = 0;
            while (left > 0 && nPos < byte_data.Length - 1)
            {
                lenR = byte_data[nPos];
                nPos++;
                if (lenR == 128)
                {
                    lenW = 0;
                }
                else if (lenR <= 127)
                {
                    lenW = Math.Min(lenR, left);
                    for (int jj = 0; jj < lenW; ++jj)
                    {
                        if(nPos >= byte_data.Length)
                        {
                            return false;
                        }
                        listBytes.Add(byte_data[nPos]);
                        nPos++;
                    }
                    for (; lenR > lenW; lenR--)
                    {
                        nPos++;
                    }
                }
                else
                {
                    lenW = Math.Min(lenR & 0x7F, left);
                    byte val = byte_data[nPos];
                    nPos++;
                    for (int jj = 0; jj < lenW; ++jj)
                    {
                        listBytes.Add(val);
                    }
                }
                left -= lenW;
            }
            if (nSize == listBytes.Count)
            {
                Buffer.BlockCopy(listBytes.ToArray(), 0, byte_data_out, 0, (int)nSize);
            }
            else
            {
                return false;
            }
            return true;
        }

        private uint UnpackData(byte[] byte_data, out byte[] byte_data_out)
        {
            int method = byte_data[0];
            uint nSize = BitConverter.ToUInt32(byte_data, 1);

            byte_data_out = new byte[nSize];

            switch (method)
            {
                case 1:
                    // RLE, not implemented because it doesn't use it
                    if (!UnpackRLE(nSize, byte_data, ref byte_data_out))
                    {
                        byte_data_out = byte_data; // Investigate why this is failing
                        return 0;
                    }
                    break;
                case 2:
                    UnpackLZW(nSize, byte_data, ref byte_data_out);
                    break;
                default:
                    // I'm missing something
                    return 0;
            }
            return nSize;
        }

        private void CreateBMP(string strOut, byte[] data, List<int> nInfParams)
        {
            int nDataPos = 0;
            for (int index = 0; index < nInfParams.Count / 2; index++)
            {

                int width = nInfParams[index] / 2;
                int height = nInfParams[index + (nInfParams.Count / 2)];
                string strTemp = strOut + "_" + index.ToString() + "_" + width.ToString() + "_" + height.ToString() + ".bmp";

                byte[] byte_data = new byte[width * height];
                Buffer.BlockCopy(data, nDataPos, byte_data, 0, width * height);
                nDataPos += width * height;

                Bitmap bmp = new Bitmap(width, height);
                int nCurColor = 0;
                using (Graphics graph = Graphics.FromImage(bmp))
                {
                    for (int Ycount = 0; Ycount < height; Ycount++)
                    {
                        for (int Xcount = 0; Xcount < width; Xcount++)
                        {
                            bmp.SetPixel(Xcount, Ycount, Color.FromArgb(byte_data[nCurColor], byte_data[nCurColor], byte_data[nCurColor]));
                            nCurColor++;
                        }
                    }
                    bmp.Save(strTemp);
                }

            }
        }

        private void WriteSng(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"sng\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string header = Encoding.ASCII.GetString(data, 0, 4);
            if (header == "SSM:")
            {
                //string fileNameLZW = strRawDir + strOut + ".lzw";
                string fileNameLZW = strRawDir + strOut;
                uint length = BitConverter.ToUInt32(data, 4);
                bool hasFlag = false;
                if((length & 0x80000000) > 0)
                {
                    length ^= 0x80000000;
                    hasFlag = true;
                }
                if(data.Length - 4 >= length)
                {
                    byte[] databytes = new byte[length];
                    Buffer.BlockCopy(data, 8, databytes, 0, (int)length);
                    List < byte[]> sngList = new List <byte[]>();

                    int songpos = 0;
                    string headerSNG = Encoding.ASCII.GetString(databytes, songpos, 4);

                    uint totallength = 0;

                    while (headerSNG == "SNG:")
                    {
                        totallength += 13;
                        songpos += 4;
                        int snglength = BitConverter.ToInt32(databytes, songpos);
                        songpos += 4;
                        if (databytes.Length >= songpos + snglength)
                        {
                            byte[] sngbytes = new byte[snglength];
                            Buffer.BlockCopy(databytes, songpos, sngbytes, 0, (int)snglength);
                            byte[] unpack_song_data;
                            UnpackData(sngbytes, out unpack_song_data);
                            sngList.Add(unpack_song_data);
                            songpos += snglength;
                            totallength += (uint)unpack_song_data.Length;
                            headerSNG = Encoding.ASCII.GetString(databytes, songpos, 4);
                        }
                        else
                        {
                            throw new Exception("Invalid SNG");
                        }
                    }
                   
                    int otherbyteslength = databytes.Length - songpos;
                    byte[] otherbytes = new byte[otherbyteslength];
                    Buffer.BlockCopy(databytes, databytes.Length - otherbyteslength, otherbytes, 0, otherbyteslength);
                    totallength += (uint)otherbyteslength;
                    totallength |= 0x80000000;
                    headerSNG = "SNG:";

                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameLZW, FileMode.Create)))
                    {
                        byte compressType = 0;
                        writer.Write(Encoding.ASCII.GetBytes(header));
                        writer.Write(totallength);
                        for(int index = 0; index < sngList.Count; ++index)
                        {
                            writer.Write(Encoding.ASCII.GetBytes(headerSNG));
                            int songlength = 5 + sngList[index].Length;
                            writer.Write(songlength);
                            writer.Write(compressType);
                            writer.Write(sngList[index].Length);
                            writer.Write(sngList[index]);
                        }
                        writer.Write(otherbytes);
                    }
                }
            }
            else
            {
                throw new Exception("Invalid SNG");
            }
        }

        private void WriteScene(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"scene\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut + ".lzw";

            /*using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(data);
            }*/

            string header = Encoding.ASCII.GetString(data, 0, 4);
            if(header == "SDS:")
            {
                int length = BitConverter.ToInt32(data, 4);
                byte[] databytes = new byte[length];
               
                Buffer.BlockCopy(data, 8, databytes, 0, length);

                byte[] outdata;
                uint outsize = UnpackData(databytes, out outdata);

                if(0 == outsize) // Our decompression failed, investigate why
                {
                    string fileNameLZW = strRawDir + strOut;
                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameLZW, FileMode.Create)))
                    {
                        writer.Write(data);
                    }
                }
                else
                {
                    string fileNameLZW = strRawDir + strOut;
                    int totallength = 5 + outdata.Length;

                    using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameLZW, FileMode.Create)))
                    {
                        writer.Write(Encoding.ASCII.GetBytes(header));
                        writer.Write(totallength);
                        byte compressType = 0;
                        writer.Write(compressType);
                        writer.Write(outdata.Length);
                        writer.Write(outdata);
                    }
                }
            }
        }

        private void WriteAds(string strOut, byte[] data, string strExtension)
        {
            string strRawDir = m_strOutDir + @"ads\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut;
            string headerVer = Encoding.ASCII.GetString(data, 0, 4);
            if (headerVer == "VER:")
            {
                int lengthVer = BitConverter.ToInt32(data, 4);
                string version = Encoding.ASCII.GetString(data, 8, lengthVer);
                string headerAds = Encoding.ASCII.GetString(data, 8 + lengthVer, 4);
                if (headerAds == "ADS:")
                {
                    int lengthAds = BitConverter.ToInt32(data, 12 + lengthVer);
                    bool hasFlag = false;
                    if ((lengthAds & 0x80000000) > 0)
                    {
                        lengthAds = (int)((uint)lengthAds ^ 0x80000000);
                        hasFlag = true;
                    }
                    else
                    {
                        throw new Exception("Invalid ADS");
                    }
                    if(16 + lengthVer + lengthAds <= data.Length)
                    {
                        if(16 + lengthVer + lengthAds < data.Length)
                        {
                            // "BROOFTOP.ADL" seems to have an extra byte for some reason
                        }
                        byte[] databytes = new byte[lengthAds];

                        Buffer.BlockCopy(data, 16 + lengthVer, databytes, 0, lengthAds);

                        string headerRes = Encoding.ASCII.GetString(databytes, 0, 4);
                        if (headerRes == "RES:")
                        {
                            int lengthRes = BitConverter.ToInt32(databytes, 4);
                            byte[] resbytes = new byte[lengthRes];
                            Buffer.BlockCopy(databytes, 8, resbytes, 0, lengthRes);
                            string headerScr = Encoding.ASCII.GetString(databytes, 8 + lengthRes, 4);
                            if (headerScr == "SCR:")
                            {
                                int lengthScr = BitConverter.ToInt32(databytes, 12 + lengthRes);
                                byte[] scrbytes = new byte[lengthScr];
                                Buffer.BlockCopy(databytes, 16 + lengthRes, scrbytes, 0, lengthScr);
                                int lengthTag = (databytes.Length - 16) - (lengthRes + lengthScr);
                                byte[] tagbytes = new byte[lengthTag];
                                Buffer.BlockCopy(databytes, databytes.Length - lengthTag, tagbytes, 0, lengthTag);

                                byte[] outdata;
                                uint outsize = UnpackData(scrbytes, out outdata);

                                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                                {
                                    byte compressType = 0;
                                    writer.Write(Encoding.ASCII.GetBytes(headerVer));
                                    writer.Write(lengthVer);
                                    writer.Write(Encoding.ASCII.GetBytes(version));
                                    writer.Write(Encoding.ASCII.GetBytes(headerAds));
                                    uint newAdsLength = (uint)(resbytes.Length + tagbytes.Length + outdata.Length + 21);
                                    newAdsLength |= 0x80000000;
                                    writer.Write(newAdsLength);
                                    writer.Write(Encoding.ASCII.GetBytes(headerRes));
                                    writer.Write(lengthRes);
                                    writer.Write(resbytes);
                                    writer.Write(Encoding.ASCII.GetBytes(headerScr));
                                    int newScrLength = outdata.Length + 5;
                                    writer.Write(newScrLength);
                                    writer.Write(compressType);
                                    writer.Write(outdata.Length);
                                    writer.Write(outdata);
                                    writer.Write(tagbytes);
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid ADS");
                            }
                        }
                        else
                        {
                            throw new Exception("Invalid ADS");
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid ADS");
                    }
                }
                else
                {
                    throw new Exception("Invalid ADS");
                }
            }
            else
            {
                throw new Exception("Invalid ADS");
            }
        }

        private void processContainer(out List<byte> outlist, byte[] data)
        {
            outlist = new List<byte>();
            int nPos = 0;

            while (nPos < data.Length)
            {
                if (nPos + 1 >= data.Length)
                {
                    break;
                }
                string headerVer = Encoding.ASCII.GetString(data, nPos, 4);
                //outlist.AddRange(Encoding.ASCII.GetBytes(headerVer));
                nPos += 4;
                byte[] lengthbytes = new byte[4];
                Buffer.BlockCopy(data, nPos, lengthbytes, 0, 4);
                uint length = BitConverter.ToUInt32(lengthbytes, 0);
                bool isContainer = (length & 0x80000000) > 0;
                //outlist.AddRange(lengthbytes);
                if (isContainer)
                {
                    length ^= 0x80000000;
                }
                int ilength = (int)length;
                nPos += 4;
                byte[] tempbytes = new byte[ilength];
                if (ilength + nPos > data.Length)
                {
                    return;
                }
                Buffer.BlockCopy(data, nPos, tempbytes, 0, ilength);
                nPos += ilength;
                if (isContainer)
                {
                    List<byte> containerList;
                    processContainer(out containerList, tempbytes);
                    outlist.AddRange(Encoding.ASCII.GetBytes(headerVer));
                    uint outsize = (uint)containerList.Count | 0x80000000;
                    outlist.AddRange(BitConverter.GetBytes(outsize));
                    outlist.AddRange(containerList.ToArray());
                    //tempList.AddRange();
                }
                else
                {
                    byte[] outdata;
                    uint outsize = UnpackData(tempbytes, out outdata);
                    if(outsize > 0)
                    {
                        uint compresssize = outsize + 5;
                        byte compresstype = 0;
                        outlist.AddRange(Encoding.ASCII.GetBytes(headerVer));
                        outlist.AddRange(BitConverter.GetBytes(compresssize));
                        outlist.Add(compresstype);
                        outlist.AddRange(BitConverter.GetBytes(outsize));
                        outlist.AddRange(outdata);
                    }
                    else
                    {
                        // Really should be comparing against the header type to determine if it's compressed,
                        // but this is just a quick utility, and all the files I need, I know will pass
                        outlist.AddRange(Encoding.ASCII.GetBytes(headerVer));
                        outlist.AddRange(lengthbytes);
                        outlist.AddRange(tempbytes);
                    }
                }
            }
        }

        private void WriteOvl(string strOut, byte[] data)
        {
            List<byte> outlist;
            string strRawDir = m_strOutDir + @"ovl\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            processContainer(out outlist, data);
            string fileName = strRawDir + strOut;

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(outlist.ToArray());
            }
        }

        private void WriteTds(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"tds\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut;
            string headerVer = Encoding.ASCII.GetString(data, 0, 4);

            if (headerVer == "THD:")
            {
                int lengthTds = BitConverter.ToInt32(data, 4);
                byte[] tdsbytes = new byte[lengthTds];
                Buffer.BlockCopy(data, 8, tdsbytes, 0, lengthTds);
                byte[] outdata;
                uint outsize = UnpackData(tdsbytes, out outdata);

                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    byte compressType = 0;
                    writer.Write(Encoding.ASCII.GetBytes(headerVer));
                    writer.Write(outdata.Length + 5);
                    writer.Write(compressType);
                    writer.Write(outdata.Length);
                    writer.Write(outdata);
                }
            }
            else
            {
                throw new Exception("Invalid TDS");
            }
        }

        private void WriteDds(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"dds\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut;
            string headerVer = Encoding.ASCII.GetString(data, 0, 4);

            if (headerVer == "DDS:")
            {
                int lengthTds = BitConverter.ToInt32(data, 4);
                byte[] tdsbytes = new byte[lengthTds];
                Buffer.BlockCopy(data, 8, tdsbytes, 0, lengthTds);
                byte[] outdata;
                uint outsize = UnpackData(tdsbytes, out outdata);

                using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    byte compressType = 0;
                    writer.Write(Encoding.ASCII.GetBytes(headerVer));
                    writer.Write(outdata.Length + 5);
                    writer.Write(compressType);
                    writer.Write(outdata.Length);
                    writer.Write(outdata);
                }
            }
            else
            {
                throw new Exception("Invalid DDS");
            }
        }

        private void WriteTtm(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"ttm\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut;
            string headerVer = Encoding.ASCII.GetString(data, 0, 4);
            if (headerVer == "VER:")
            {
                int lengthVer = BitConverter.ToInt32(data, 4);
                string version = Encoding.ASCII.GetString(data, 8, lengthVer);
                string headerPag = Encoding.ASCII.GetString(data, 8 + lengthVer, 4);
                if (headerPag == "PAG:")
                {
                    int lengthPag = BitConverter.ToInt32(data, 12 + lengthVer);
                    if(lengthPag != 2)
                    {
                        throw new Exception("Invalid TTM");
                    }
                    short page = BitConverter.ToInt16(data, 16 + lengthVer);
                    string headerTt3 = Encoding.ASCII.GetString(data, 18 + lengthVer, 4);
                    if (headerTt3 == "TT3:")
                    {
                        int lengthTt3 = BitConverter.ToInt32(data, 22 + lengthVer);

                        int lengthTag = (data.Length - (26 + lengthVer + lengthTt3));
                        byte[] tagbytes = new byte[lengthTag];
                        Buffer.BlockCopy(data, data.Length - lengthTag, tagbytes, 0, lengthTag);

                        byte[] tt3bytes = new byte[lengthTt3];
                        Buffer.BlockCopy(data, 26 + lengthVer, tt3bytes, 0, lengthTt3);

                        byte[] outdata;
                        uint outsize = UnpackData(tt3bytes, out outdata);

                        using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                        {
                            byte compressType = 0;
                            writer.Write(Encoding.ASCII.GetBytes(headerVer));
                            writer.Write(lengthVer);
                            writer.Write(Encoding.ASCII.GetBytes(version));
                            writer.Write(Encoding.ASCII.GetBytes(headerPag));
                            writer.Write(lengthPag);
                            writer.Write(page);
                            writer.Write(Encoding.ASCII.GetBytes(headerTt3));
                            int compressLength = 5 + outdata.Length;
                            writer.Write(compressLength);
                            writer.Write(compressType);
                            writer.Write(outdata.Length);
                            writer.Write(outdata);
                            writer.Write(tagbytes);
                        }   
                    }
                }
                else
                {
                    throw new Exception("Invalid TTM");
                }
            }
            else
            {
                throw new Exception("Invalid TTM");
            }
        }

        private void WriteGeneric(string strOut, byte[] data)
        {
            if(data == null)
            {
                return;
            }
            string strRawDir = m_strOutDir + @"generic\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }

            string fileName = strRawDir + strOut;

            using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(data);
            }
        }

        private void WriteBMP(string strOut, byte[] data)
        {
            string strRawDir = m_strOutDir + @"bmp\";
            if (!Directory.Exists(strRawDir))
            {
                Directory.CreateDirectory(strRawDir);
            }
            int nDataPos = 0;

            string fileName = strRawDir + strOut;
            string strType;
            int nSize;

            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);

            if (strType != "BMP:")
            {
                return;
            }
            nDataPos += 4;

            byte[] byte_packed_size = new byte[4];
            byte_packed_size[3] = 0;
            Buffer.BlockCopy(data, nDataPos, byte_packed_size, 0, 3);
            nDataPos += 3;
            int nPackedSize;
            nPackedSize = BitConverter.ToInt32(byte_packed_size, 0);
            byte flag = data[nDataPos];
            nDataPos++;
            if (flag == 0x80)
            {
                // Just move forward to the next series of bytes, don't skip forward
            }

            strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);

            if (strType != "INF:")
            {
                return;
            }
            nDataPos += 4;
            nSize = BitConverter.ToInt32(data, nDataPos);
            nDataPos += 4;
            byte[] byte_inf = new byte[nSize];
            Buffer.BlockCopy(data, nDataPos, byte_inf, 0, nSize);
            nDataPos += nSize;

            int nTotalSize = 0;

            int nNumInf = BitConverter.ToInt16(byte_inf, 0);
            List<int> nInfParams = new List<int>();
            for (int nIndex = 0; nIndex < nNumInf * 2; nIndex++)
            {
                int nParam1 = BitConverter.ToInt16(byte_inf, 2 + (2 * nIndex));
                nInfParams.Add(nParam1);
            }

            for (int nIndex = 0; nIndex < nNumInf; nIndex++)
            {
                nTotalSize += (nInfParams[nIndex] * nInfParams[nIndex + nNumInf]);
            }

            WriteFile(fileName + ".inf", byte_inf);

            while (true)
            {
                if (nDataPos + 4 > data.Length)
                {
                    break;
                }
                strType = System.Text.Encoding.ASCII.GetString(data, nDataPos, 4);

                if (strType == "VQT:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_vqt = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_vqt, 0, nSize);
                    nDataPos += nSize;
                    WriteFile(fileName + ".vqt", byte_vqt);
                }
                else if (strType == "OFF:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_off = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_off, 0, nSize);
                    nDataPos += nSize;
                    WriteFile(fileName + ".off", byte_off);
                }
                else if (strType == "SCN:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_scn = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_scn, 0, nSize);
                    nDataPos += nSize;
                    WriteFile(fileName + ".scn", byte_scn);
                }
                else if (strType == "BIN:") // packed
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_bin = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_bin, 0, nSize);
                    nDataPos += nSize;
                    byte[] byte_data_out;
                    uint unpacksize = UnpackData(byte_bin, out byte_data_out);
                    if (unpacksize == (nTotalSize / 2))
                    {
                        //CreateBMP(fileName + "_bin", byte_data_out, nInfParams);
                    }
                }
                else if (strType == "VGA:") // packed
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_vga = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_vga, 0, nSize);
                    nDataPos += nSize;
                    byte[] byte_data_out;
                    uint unpacksize = UnpackData(byte_vga, out byte_data_out);
                    if (unpacksize == (nTotalSize / 2))
                    {
                        ///CreateBMP(fileName + "_vga", byte_data_out, nInfParams);
                    }
                }
                else if (strType == "MTX:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_mtx = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_mtx, 0, nSize);
                    nDataPos += nSize;
                    WriteFile(fileName + ".mtx", byte_mtx);
                }
                else if (strType == "SCL:")
                {
                    nDataPos += 4;
                    nSize = BitConverter.ToInt32(data, nDataPos);
                    nDataPos += 4;
                    byte[] byte_scl = new byte[nSize];
                    Buffer.BlockCopy(data, nDataPos, byte_scl, 0, nSize);
                    nDataPos += nSize;
                    WriteFile(fileName + ".scl", byte_scl);
                }
                else
                {
                    // I'm missing something
                }
            }

            /*using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(data);
            }*/
        }

        public bool WriteResource(ResourceReader.ResourceFileData rfd, string strOut)
        {
            m_strOutDir = strOut;
            if (!Directory.Exists(strOut))
            {
                Directory.CreateDirectory(strOut);
            }
            switch (rfd.strExtention.ToUpper())
            {
                case ".RAW":
                    WriteRaw(rfd.strFile, rfd.data);
                    break;
                case ".CDS":
                    WriteCDS(rfd.strFile, rfd.data);
                    break;
                case ".BMP":
                    //WriteBMP(rfd.strFile, rfd.data);
                    break;
                case ".SDS":
                    WriteScene(rfd.strFile, rfd.data);
                    break;
                case ".SNG":
                    WriteSng(rfd.strFile, rfd.data);
                    break;
                case ".ADH":
                    WriteAds(rfd.strFile, rfd.data, "H");
                    break;
                case ".ADL":
                    WriteAds(rfd.strFile, rfd.data, "L");
                    break;
                case ".ADS":
                    WriteAds(rfd.strFile, rfd.data, "S");
                    break;
                case ".TTM":
                    WriteTtm(rfd.strFile, rfd.data);
                    break;
                case ".TDS":
                    WriteTds(rfd.strFile, rfd.data);
                    break;
                case ".DDS":
                    WriteDds(rfd.strFile, rfd.data);
                    break;
                case ".OVL":
                    WriteOvl(rfd.strFile, rfd.data);
                    break;
                default:
                    WriteGeneric(rfd.strFile, rfd.data);
                    break;
            }
            return false;
        }
    }
}
