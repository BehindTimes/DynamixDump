using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DynamixLZW
{
    internal class lzwdecompressor
    {
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

        public bool extract(byte[] file_bytes, string outFile)
        {
            byte[] outData;
            UnpackData(file_bytes, out outData);

            using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open(outFile, FileMode.Create)))
            {
                binWriter.Write(outData);

            }

            return true;
        }

        public bool compress(byte[] file_bytes, string outFile)
        {
            using (BinaryWriter binWriter =
                    new BinaryWriter(File.Open(outFile, FileMode.Create)))
            {
                binWriter.Write(file_bytes.Length);

                build_dictionary(file_bytes, 0, binWriter);

            }

            return true;
        }

        byte code_word_remainder = 0;
        int code_word_shift = 0;

        int debugsize = 4;
        int totalWrite = 0;

        void write_code_word(int code_word, int code_word_size, BinaryWriter binWriter)
        {
            int temp_int = code_word << code_word_shift;
            temp_int += code_word_remainder;
            code_word_shift += code_word_size;
            while (code_word_shift >= 8)
            {
                byte temp_byte = (byte)(temp_int & 0xFF);
                binWriter.Write(temp_byte);
                totalWrite++;
                if(totalWrite > 0x151D)
                {
                    int j = 9;
                }
                //Debug.Write($"{temp_byte:X2}");
                debugsize++;
                if(debugsize % 16 == 0)
                {
                    //Debug.Write("\n");
                }
                else if (debugsize % 2 == 0)
                {
                    //Debug.Write(" ");
                }
                temp_int >>= 8;
                code_word_shift -= 8;
            }
            code_word_remainder = (byte)(temp_int & 0xFF);
        }

        int find_sequence_pos(List<List<byte>> cur_dict, List<byte> temp_list)
        {
            int seq_index = 0;
            foreach (var curList in cur_dict)
            {
                if (curList.SequenceEqual(temp_list))
                {
                    break;
                }
                seq_index++;
            }

            return seq_index;
        }

        int build_dictionary(byte[] file_bytes, int start_pos, BinaryWriter binWriter)
        {
            code_word_remainder = 0;
            code_word_shift = 0;
            int total_bit_size = file_bytes.Length * 8;
            int next_free_codeword = 0x101;
            int codeword_size = 9;
            List<List<byte>> cur_dict = new List<List<byte>>();
            int temp_pos = start_pos;
            bool invalid = false;

            int qqq = 0;

            while (temp_pos < file_bytes.Length)
            {
                qqq++;
                if(qqq > 0xfd)
                {
                    int j = 9;
                }
                int seq_index = 0;
                bool sequence_found = false;
                List<byte> temp_list = new List<byte>();
                temp_list.Add(file_bytes[temp_pos]);
                temp_pos++;
                if (temp_pos >= file_bytes.Length)
                {
                    // Write the code word
                    write_code_word(temp_list[0], codeword_size, binWriter);
                    break; // We've reached the end, so just write this
                }
                temp_list.Add(file_bytes[temp_pos]);
                while (cur_dict.Any(p => p.SequenceEqual(temp_list)))
                {
                    seq_index = find_sequence_pos(cur_dict, temp_list);

                    sequence_found = true;

                    temp_pos++;
                    if (temp_pos >= file_bytes.Length)
                    {
                        // Write the sequence
                        write_code_word(next_free_codeword + seq_index, codeword_size, binWriter);
                        invalid = true;
                        break; // We've reached the end, so just write this
                    }
                    temp_list.Add(file_bytes[temp_pos]);
                }
                
                if (invalid)
                {
                    break;
                }
                if (sequence_found)
                {
                    // Write the sequence
                    write_code_word(next_free_codeword + seq_index, codeword_size, binWriter);

                }
                else
                {
                    // Write the code word
                    write_code_word(temp_list[0], codeword_size, binWriter);
                }

                if (cur_dict.Count < 3840)
                {
                    cur_dict.Add(temp_list);
                    if (cur_dict.Count == 256 || cur_dict.Count == 768 || cur_dict.Count == 1792)
                    {
                        codeword_size++;
                    }
                    /*else if (cur_dict.Count > 3840)
                    {
                        temp_pos -= (temp_list.Count - 1);
                        cur_dict.Clear();
                        //write_code_word(0x100, codeword_size, binWriter);
                        codeword_size = 9;
                        continue;
                    }*/
                }
            }
            //write_code_word(0x101, codeword_size, binWriter);

            if (code_word_shift > 0)
            {
                binWriter.Write(code_word_remainder);
            }

            MessageBox.Show("File written!");
            return -1;
        }

        private uint UnpackData(byte[] byte_data, out byte[] byte_data_out)
        {
            uint nSize = BitConverter.ToUInt32(byte_data, 0);

            byte_data_out = new byte[nSize];

            UnpackLZW(nSize, byte_data, ref byte_data_out);

            return nSize;
        }

        private void UnpackLZW(uint nSize, byte[] byte_data, ref byte[] byte_data_out)
        {
            ResetLZW();

            buf_ptr = 4;  // Skip the method and size

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
                    else
                    {
                        int j = 9;
                    }

                    // reset running code!
                    for (lcv = 0; lcv < m_dict_table[code].len; lcv++)
                    {
                        code_cur[lcv] = m_dict_table[code].str[lcv];
                    }

                    int codepost = (int)code;
                    code_len = (uint)m_dict_table[codepost].len;
                }
            }
        }

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
    }
}
