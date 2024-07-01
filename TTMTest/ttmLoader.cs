using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Formats.Tar;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TTMTest
{
    internal class ttmLoader
    {
        private XmlDocument? m_doc;

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
                string strXml = strFile + ".xml";
                m_doc = new XmlDocument();

                XmlDeclaration xmlDeclaration = m_doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement? root = m_doc.DocumentElement;

                m_doc.InsertBefore(xmlDeclaration, root);

                XmlElement body = m_doc.CreateElement(string.Empty, "TTM", string.Empty);
                m_doc.AppendChild(body);

                if (processFile(strFile, body))
                {
                    m_doc.Save(strXml);
                }
            }
        }

        private bool processFile(string strFile, XmlElement body)
        {
            if (!File.Exists(strFile))
            {
                return false;
            }
            if (m_doc == null)
            {
                return false;
            }
            byte[] ttmFile = File.ReadAllBytes(strFile);

            int nDataPos = 0;
            string strType = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, 4);
            nDataPos += 4;
            if (strType != "VER:")
            {
                return false;
            }
            int verSize = BitConverter.ToInt32(ttmFile, nDataPos);
            nDataPos += 4;
            string strVersion = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, verSize);
            nDataPos += verSize;
            XmlElement versionElement = m_doc.CreateElement("Version");
            strVersion = strVersion.Remove(strVersion.Length - 1);
            XmlText text = m_doc.CreateTextNode(strVersion);
            versionElement.AppendChild(text);
            body.AppendChild(versionElement);
            Console.WriteLine("Version = {0}", strVersion);
            string strPage = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, 4);
            if (strPage != "PAG:")
            {
                return false;
            }
            nDataPos += 4;
            int pageSize = BitConverter.ToInt32(ttmFile, nDataPos);
            nDataPos += 4;
            byte[] byte_page = new byte[pageSize];
            Buffer.BlockCopy(ttmFile, nDataPos, byte_page, 0, pageSize);
            nDataPos += pageSize;
            string strPageData = "";
            for (int index = 0; index < byte_page.Length; ++index)
            {
                if (index > 0)
                {
                    strPageData += ",";
                }
                strPageData += byte_page[index].ToString();
            }
            XmlElement pageElement = m_doc.CreateElement("Page");
            if (strPageData.Length > 0)
            {
                XmlText textPage = m_doc.CreateTextNode(strPageData);
                pageElement.AppendChild(textPage);
            }
            
            body.AppendChild(pageElement);
            strType = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, 4);

            if (strType != "TT3:")
            {
                return false;
            }
            nDataPos += 4;
            int tt3size = BitConverter.ToInt32(ttmFile, nDataPos);
            nDataPos += 4;
            byte compressionType = ttmFile[nDataPos];
            nDataPos += 1;
            int decompressSize = BitConverter.ToInt32(ttmFile, nDataPos);
            nDataPos += 4;
            byte[] ttmData = new byte[decompressSize];
            Buffer.BlockCopy(ttmFile, nDataPos, ttmData, 0, decompressSize);
            XmlElement tt3Element = m_doc.CreateElement("TT3");
            body.AppendChild(tt3Element);
            ProcessTTM(ttmData, tt3Element);
            nDataPos += decompressSize;

            string strTti = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, 4);
            if (strTti != "TTI:")
            {
                return false;
            }
            nDataPos += 4;
            uint ttisize = BitConverter.ToUInt32(ttmFile, nDataPos);
            nDataPos += 4;
            ttisize ^= 0x80000000;
            string strTag = System.Text.Encoding.ASCII.GetString(ttmFile, nDataPos, 4);
            if (strTag != "TAG:")
            {
                return false;
            }
            nDataPos += 4;
            int tagsize = BitConverter.ToInt32(ttmFile, nDataPos);
            nDataPos += 4;

            XmlElement tagElement = m_doc.CreateElement("TAGS");
            short numTags = BitConverter.ToInt16(ttmFile, nDataPos);
            nDataPos += 2;
            byte[] tagData = new byte[tagsize - 2];
            Buffer.BlockCopy(ttmFile, nDataPos, tagData, 0, tagsize - 2);
            tagElement.SetAttribute("Number", numTags.ToString());
            body.AppendChild(tagElement);
            ProcessTags(tagData, tagElement);
            return true;
        }

        string ttmOpName(uint op)
        {
            switch (op)
            {
                case 0x0000: return "FINISH";
                case 0x0020: return "SAVE(free?) BACKGROUND";
                case 0x0070: return "FREE PALETTE";
                case 0x0080: return "FREE SHAPE / DRAW BACKGROUND??";
                case 0x0090: return "FREE FONT";
                case 0x00B0: return "NULLOP";
                case 0x0110: return "PURGE";
                case 0x0400: return "PALETTE RESET ?";
                case 0x0510: return "UNKNOWN 0x0510";
                case 0x0ff0: return "FINISH FRAME / DRAW";
                case 0x1020: return "SET DELAY";
                case 0x1030: return "SET BRUSH";
                case 0x1050: return "SELECT BMP";
                case 0x1060: return "SELECT PAL";
                case 0x1070: return "SELECT FONT";
                case 0x1090: return "SELECT SONG";
                case 0x10a0: return "SET SCENE";
                case 0x1100: // fall through
                case 0x1110: return "SET SCENE";
                case 0x1120: return "SET GETPUT NUM";
                case 0x1200: return "GOTO";
                case 0x1300: return "PLAY SFX";
                case 0x2000: return "SET DRAW COLORS";
                case 0x2010: return "SET FRAME";
                case 0x2020: return "SET RANDOM DELAY";
                case 0x2300: return "PAL SET BLOCK SWAP 0";
                case 0x2310: return "PAL SET BLOCK SWAP 1";
                case 0x2320: return "PAL SET BLOCK SWAP 2";
                case 0x2400: return "PAL DO BLOCK SWAP";
                case 0x4000: return "SET CLIP WINDOW";
                case 0x4110: return "FADE OUT";
                case 0x4120: return "FADE IN";
                case 0x4200: return "STORE AREA";
                case 0x4210: return "SAVE GETPUT REGION";
                case 0xa000: return "DRAW PIXEL";
                case 0xa010: return "WIPE 10?";
                case 0xa020: return "WIPE 20?";
                case 0xa030: return "WIPE OUT-TO-IN";
                case 0xa040: return "WIPE INTERLACED";
                case 0xa050: return "WIPE LEFT-TO-RIGHT";
                case 0xa060: return "WIPE RIGHT-TO-LEFT";
                case 0xa070: return "WIPE TOP-TO-BOTTOM";
                case 0xa080: return "WIPE BOTTOM-TO-TOP";
                case 0xa090: return "WIPE IN-TO-OUT";
                case 0xa0a0: return "DRAW LINE";
                case 0xa100: return "DRAW FILLED RECT";
                case 0xa110: return "DRAW EMPTY RECT";
                case 0xa200: return "DRAW STRING 0";
                case 0xa210: return "DRAW STRING 1";
                case 0xa220: return "DRAW STRING 2";
                case 0xa230: return "DRAW STRING 3";
                case 0xa240: return "DRAW STRING 4";
                case 0xa250: return "DRAW STRING 5";
                case 0xa260: return "DRAW STRING 6";
                case 0xa270: return "DRAW STRING 7";
                case 0xa280: return "DRAW STRING 8";
                case 0xa290: return "DRAW STRING 9";
                case 0xa500: return "DRAW BMP";
                case 0xa520: return "DRAW SPRITE FLIP";
                case 0xa530: return "DRAW BMP4";
                case 0xa600: return "DRAW GETPUT";
                case 0xb000: return "INIT CREDITS SCROLL";
                case 0xb010: return "DRAW CREDITS SCROLL";
                case 0xf010: return "LOAD SCR";
                case 0xf020: return "LOAD BMP";
                case 0xf040: return "LOAD FONT";
                case 0xf050: return "LOAD PAL";
                case 0xf060: return "LOAD SONG";
                case 0xf100: return "SET STRING 0";
                case 0xf110: return "SET STRING 1";
                case 0xf120: return "SET STRING 2";
                case 0xf130: return "SET STRING 3";
                case 0xf140: return "SET STRING 4";
                case 0xf150: return "SET STRING 5";
                case 0xf160: return "SET STRING 6";
                case 0xf170: return "SET STRING 7";
                case 0xf180: return "SET STRING 8";
                case 0xf190: return "SET STRING 9";
                case 0x0220: return "STOP CURRENT MUSIC";

                case 0x00C0: return "FREE BACKGROUND";
                case 0x0230: return "reset current music?";
                case 0x1310: return "STOP SFX";
                case 0xa300: return "DRAW some string";
                case 0xa400: return "DRAW FILLED CIRCLE";
                case 0xa420: return "DRAW EMPTY CIRCLE";
                case 0xa510: return "DRAW SPRITE1";
                case 0xb600: return "DRAW SCREEN";
                case 0xc020: return "LOAD_SAMPLE";
                case 0xc030: return "SELECT_SAMPLE";
                case 0xc040: return "DESELECT_SAMPLE";
                case 0xc050: return "PLAY_SAMPLE";
                case 0xc060: return "STOP_SAMPLE";

                default: return "UNKNOWN!!";
            }
        }

        void ProcessTags(byte[] data, XmlElement body)
        {
            if(m_doc == null) return;
            int pos = 0;
            int tagNum = 0;
            while(pos < data.Length)
            {
                short tagId = BitConverter.ToInt16(data, pos);
                pos += 2;
                string strId = "";

                byte curByte;

                while(true)
                {
                    curByte = data[pos];
                    pos++;
                    if(curByte != 0)
                    {
                        strId += (char)curByte;
                    }
                    else
                    {
                        break;
                    }
                }

                XmlElement tagElement = m_doc.CreateElement("TAG");
                tagElement.SetAttribute("Number", tagNum.ToString());
                tagElement.SetAttribute("ID", tagId.ToString());
                XmlText textPage = m_doc.CreateTextNode(strId);
                tagElement.AppendChild(textPage);
                body.AppendChild(tagElement);
                tagNum++;
            }
        }

        void ProcessTTM(byte[] data, XmlElement body)
        {
            if(m_doc == null) { return; }
            for (int nDataPos = 0; nDataPos < data.Length; nDataPos += 2)
            {
                uint code = BitConverter.ToUInt16(data, nDataPos);
                nDataPos += 2;
                uint op = code & 0xFFF0;
                uint count = code & 0x000F;
                string strOpName = ttmOpName(op);
               // Console.WriteLine("OP: {0:X4} ({1})", op, strOpName);

                byte[] tempbytes = new byte[2];
                ushort[] ivals = new ushort[8];
                string tempStr = "";
                if (count > 8 && count != 0x0f)
                {
                    Console.WriteLine("Error, invalid op code!");
                    return;
                }
                if (count == 0x0F)
                {
                    do
                    {
                        tempbytes[0] = data[nDataPos];
                        tempbytes[1] = data[nDataPos + 1];
                        if (tempbytes[0] != 0)
                        {
                            tempStr += (char)tempbytes[0];
                        }
                        if (tempbytes[1] != 0)
                        {
                            tempStr += (char)tempbytes[1];
                        }
                        nDataPos += 2;
                    }
                    while (tempbytes[0] != 0 && tempbytes[1] != 0);
                    Console.WriteLine("OP: {0:X4} ({1} {2})", op, strOpName, tempStr);
                    string xmlString = string.Format("OP_{0:X4}", op);
                    XmlElement tt3Element = m_doc.CreateElement(xmlString);
                    tt3Element.SetAttribute("Description", strOpName);
                    XmlText text = m_doc.CreateTextNode(tempStr);
                    tt3Element.AppendChild(text);
                    body.AppendChild(tt3Element);
                }
                else
                {
                    string opStr = string.Format("OP: {0:X4} ({1})", op, strOpName);
                    opStr += ")";
                    string opData = "(";

                    string xmlString = string.Format("OP_{0:X4}", op);
                    XmlElement tt3Element = m_doc.CreateElement(xmlString);
                    tt3Element.SetAttribute("Description", strOpName);

                    for (int i = 0; i < count; i++)
                    {
                        XmlElement valueElement = m_doc.CreateElement("Value");

                        ivals[i] = BitConverter.ToUInt16(data, nDataPos);
                        nDataPos += 2;

                        if(i > 0)
                        {
                            opData += " ";
                        }
                        opData += ivals[i].ToString();

                        XmlText valuetext = m_doc.CreateTextNode(ivals[i].ToString());
                        valueElement.AppendChild(valuetext);
                        tt3Element.AppendChild(valueElement);
                    }
                    opData += ")";
                    Console.WriteLine("{0} Data: {1}", opStr, opData);

                    body.AppendChild(tt3Element);
                }
                nDataPos -= 2;
            }
        }

        public void save(string strDir)
        {
            bool bValid = true;
            if (!strDir.EndsWith("\\"))
            {
                strDir += "\\";
            }
            //string strFile = strDir + "DHTHREAT.SAV";
            //string strXml = strDir + "DHTHREAT.TTM.xml";
            string strFile = strDir + "TITLE2.SAV";
            string strXml = strDir + "TITLE2.TTM.xml";
            m_doc = new XmlDocument();
            m_doc.Load(strXml);
            if(m_doc.DocumentElement != null )
            {
                List<byte> listVersion = new List<byte>();
                List<byte> listPage = new List<byte>();
                List<byte> listTags = new List<byte>();
                List<byte> listTt3 = new List<byte>();

                XmlElement root = m_doc.DocumentElement;
                XmlNodeList versionNodes = root.GetElementsByTagName("Version");
                XmlNodeList pageNodes = root.GetElementsByTagName("Page");
                XmlNodeList tagsNodes = root.GetElementsByTagName("TAGS");
                XmlNodeList tt3Nodes = root.GetElementsByTagName("TT3");

                if(versionNodes.Count == 1 &&
                    pageNodes.Count == 1 &&
                    tagsNodes.Count == 1 &&
                    tt3Nodes.Count == 1)
                {
                    XmlNode? elementVersion = versionNodes.Item(0);
                    if (elementVersion != null)
                    {
                        XmlNode curNode = elementVersion;
                        if(!ProcessVersionNode(curNode, ref listVersion))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elementPage = pageNodes.Item(0);
                    if (elementPage != null)
                    {
                        XmlNode curNode = elementPage;
                        if(!ProcessPageNode(curNode, ref listPage))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elementTT3 = tagsNodes.Item(0);
                    if (elementTT3 != null)
                    {
                        XmlNode curNode = elementTT3;
                        if(!ProcessTagsNode(curNode, ref listTags))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elementTags = tt3Nodes.Item(0);
                    if (elementTags != null)
                    {
                        XmlNode curNode = elementTags;
                        if(!ProcessTT3Node(curNode, ref listTt3))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    if(bValid)
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open(strFile, FileMode.Create)))
                        {
                            writer.Write(listVersion.ToArray());
                            writer.Write(listPage.ToArray());
                            writer.Write(listTt3.ToArray());
                            writer.Write(listTags.ToArray());
                        }
                    }
                }
            }
        }

        public bool ProcessVersionNode(XmlNode root, ref List<byte> curList)
        {
            List<byte> listVersion = new List<byte>();
            if(root.InnerText.Length <= 0)
            {
                return false;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(root.InnerText);
            listVersion.AddRange(bytes);
            listVersion.Add(0);

            byte[] headerbytes = Encoding.ASCII.GetBytes("VER:");
            byte[] sizebytes = BitConverter.GetBytes(listVersion.Count);
            curList.AddRange(headerbytes);
            curList.AddRange(sizebytes);
            curList.AddRange(listVersion);
            return true;
        }

        public bool ProcessPageNode(XmlNode root, ref List<byte> curList)
        {
            List<byte> listPage = new List<byte>();
            if (root.InnerText.Length <= 0)
            {
                return false;
            }
            string[] curBytes = root.InnerText.Split(",");
            byte[] headerbytes = Encoding.ASCII.GetBytes("PAG:");
            byte[] sizebytes = BitConverter.GetBytes(curBytes.Length);
            curList.AddRange(headerbytes);
            curList.AddRange(sizebytes);
            for(int index = 0; index < curBytes.Length; ++index)
            {
                if(Int32.TryParse(curBytes[index], out int value))
                {
                    byte[] valuebytes = BitConverter.GetBytes(value);
                    curList.Add(valuebytes[0]);
                }
                else
                {
                    return false;
                }
                
            }
            return true;
        }

        public bool ProcessTagsNode(XmlNode root, ref List<byte> curList)
        {
            short expectedNum = 0;
            List<byte> listTags = new List<byte>();
            byte[] headerbytes = Encoding.ASCII.GetBytes("TTI:");
            curList.AddRange(headerbytes);
            if(root.Attributes == null)
            {
                return false;
            }
            XmlAttribute? curAttribute = root.Attributes["Number"];
            if(curAttribute != null)
            {
                string strNumber = curAttribute.Value;
                if(Int16.TryParse(strNumber, out expectedNum))
                {
                    byte[] sizebytes = BitConverter.GetBytes(expectedNum);
                    listTags.AddRange(sizebytes);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            if(expectedNum!= root.ChildNodes.Count)
            {
                return false;
            }
            for(int index = 0; index < root.ChildNodes.Count; ++index)
            {
                XmlNode? tempNode = root.ChildNodes[index];
                if(tempNode != null)
                {
                    if(tempNode.Attributes == null)
                    {
                        return false;
                    }
                    curAttribute = tempNode.Attributes["ID"];
                    if (curAttribute != null)
                    {
                        string strNumber = curAttribute.Value;
                        if (Int16.TryParse(strNumber, out expectedNum))
                        {
                            byte[] sizebytes = BitConverter.GetBytes(expectedNum);
                            listTags.AddRange(sizebytes);
                            if(tempNode.InnerText.Length > 0)
                            {
                                byte[] valuebytes = Encoding.ASCII.GetBytes(tempNode.InnerText);
                                listTags.AddRange(valuebytes);
                            }
                            listTags.Add(0);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            uint totalSize = (uint)(listTags.Count + 8);
            totalSize |= 0x80000000;
            byte[] totalBytes = BitConverter.GetBytes(totalSize);
            curList.AddRange(totalBytes);
            headerbytes = Encoding.ASCII.GetBytes("TAG:");
            curList.AddRange(headerbytes);
            totalBytes = BitConverter.GetBytes(listTags.Count);
            curList.AddRange(totalBytes);
            curList.AddRange(listTags.ToArray());
            return true;
        }

        public bool ProcessTT3Node(XmlNode root, ref List<byte> curList)
        {
            List<byte> listOps = new List<byte>();

            byte[] headerbytes = Encoding.ASCII.GetBytes("TT3:");
            curList.AddRange(headerbytes);

            // Technically it can be zero, but there's really no point if it is
            if(root.ChildNodes.Count <= 0)
            {
                return false;
            }

            for(int index = 0; index < root.ChildNodes.Count; ++index)
            {
                XmlNode? tempNode = root.ChildNodes[index];
                if(tempNode != null)
                {
                    if (tempNode.Name.Length != 7)
                    {
                        return false;
                    }
                    string strOp = tempNode.Name.Replace("OP_", "");
                    if (strOp.Length != 4)
                    {
                        return false;
                    }
                    ushort opVal;
                    if(UInt16.TryParse(strOp, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out opVal))
                    {
                        if(opVal % 0x10 != 0)
                        {
                            return false;
                        }
                        ushort realoutval = opVal;
                        if(tempNode.ChildNodes.Count > 0)
                        {
                            if (tempNode.ChildNodes.Count > 1)
                            {
                                realoutval += (ushort)tempNode.ChildNodes.Count;
                                byte[] opByteArray = BitConverter.GetBytes(realoutval);
                                listOps.AddRange(opByteArray);
                                for(int i = 0; i < tempNode.ChildNodes.Count; ++i)
                                {
                                    XmlNode? childNode = tempNode.ChildNodes[i];
                                    if(childNode != null)
                                    {
                                        if (childNode.Name != "Value")
                                        {
                                            return false;
                                        }
                                        if (childNode.InnerText.Length > 0)
                                        {
                                            if (UInt16.TryParse(childNode.InnerText, out ushort result))
                                            {
                                                opByteArray = BitConverter.GetBytes(result);
                                                listOps.AddRange(opByteArray);
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                XmlNode? childNode = tempNode.ChildNodes[0];
                                if(childNode != null)
                                {
                                    if (childNode.ChildNodes.Count > 0)
                                    {
                                        if(childNode.Name != "Value")
                                        {
                                            return false;
                                        }
                                        realoutval += (ushort)tempNode.ChildNodes.Count;
                                        byte[] opByteArray = BitConverter.GetBytes(realoutval);
                                        listOps.AddRange(opByteArray);

                                        if (childNode.InnerText.Length > 0)
                                        {
                                            if (UInt16.TryParse(childNode.InnerText, out ushort result))
                                            {
                                                opByteArray = BitConverter.GetBytes(result);
                                                listOps.AddRange(opByteArray);
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        if (childNode.InnerText.Length > 0)
                                        {
                                            realoutval += 0xF;
                                            byte[] opByteArray = BitConverter.GetBytes(realoutval);
                                            listOps.AddRange(opByteArray);
                                            byte[] valuebytes = Encoding.ASCII.GetBytes(tempNode.InnerText);
                                            listOps.AddRange(valuebytes);
                                            listOps.Add(0);
                                            if (valuebytes.Length % 2 == 0)
                                            {
                                                listOps.Add(0); // The parser reads by uint16s, so make sure it's even when including null character
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            byte[] opByteArray = BitConverter.GetBytes(realoutval);
                            listOps.AddRange(opByteArray);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            int fullsize = listOps.Count + 5;
            byte[] sizeArray = BitConverter.GetBytes(fullsize);
            curList.AddRange(sizeArray);
            curList.Add(0); // Going uncompressed
            sizeArray = BitConverter.GetBytes(listOps.Count);
            curList.AddRange(sizeArray);
            curList.AddRange(listOps.ToArray());
            return true;
        }
    }
}
