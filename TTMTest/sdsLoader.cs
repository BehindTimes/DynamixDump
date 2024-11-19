using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TTMTest
{
    internal class sdsLoader
    {
        private XmlDocument? m_doc;
        private float m_version;

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
                if(!strFile.EndsWith("SDS"))
                {
                    continue;
                }
                string strXml = strFile + ".xml";
                m_doc = new XmlDocument();

                XmlDeclaration xmlDeclaration = m_doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement? root = m_doc.DocumentElement;

                m_doc.InsertBefore(xmlDeclaration, root);

                XmlElement body = m_doc.CreateElement(string.Empty, "SDS", string.Empty);
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
            byte[] sceneFile = File.ReadAllBytes(strFile);

            // We're assuming the file's been uncompressed
            int nDataPos = 0;
            nDataPos += 0x0d;

            uint magicNum = BitConverter.ToUInt32(sceneFile, nDataPos);
            nDataPos += 4;

            string strVersion = readString(sceneFile, ref nDataPos);
            m_version = float.Parse(strVersion, CultureInfo.InvariantCulture.NumberFormat);

            XmlElement magicNumberElement = m_doc.CreateElement("MagicNumber");
            XmlText text = m_doc.CreateTextNode(magicNum.ToString());
            magicNumberElement.AppendChild(text);
            body.AppendChild(magicNumberElement);

            XmlElement versionElement = m_doc.CreateElement("Version");
            XmlText textVer = m_doc.CreateTextNode(strVersion);
            versionElement.AppendChild(textVer);
            body.AppendChild(versionElement);

            UInt16 _num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            XmlElement sceneNumElement = m_doc.CreateElement("sceneNum");
            XmlText sceneNumText = m_doc.CreateTextNode(_num.ToString());
            sceneNumElement.AppendChild(sceneNumText);
            body.AppendChild(sceneNumElement);

            XmlElement enterOpsElement = m_doc.CreateElement("enter_scene_ops");
            readOpList(ref enterOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(enterOpsElement);

            XmlElement leaveOpsElement = m_doc.CreateElement("leave_scene_ops");
            readOpList(ref leaveOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(leaveOpsElement);

            if(isVersionOver(1.206f))
            {
                XmlElement preTickOpsElement = m_doc.CreateElement("pre_tick_ops");
                readOpList(ref preTickOpsElement, sceneFile, ref nDataPos);
                body.AppendChild(preTickOpsElement);
            }

            XmlElement postTickOpsElement = m_doc.CreateElement("post_tick_ops");
            readOpList(ref postTickOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(postTickOpsElement);

            UInt16 _field6_0x14 = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            XmlElement field6_0x14Element = m_doc.CreateElement("field6_0x14");
            XmlText field6_0x14text = m_doc.CreateTextNode(_field6_0x14.ToString());
            field6_0x14Element.AppendChild(field6_0x14text);
            body.AppendChild(field6_0x14Element);

            string strAds = readString(sceneFile, ref nDataPos);
            XmlElement adsElement = m_doc.CreateElement("ADS_File");
            XmlText adstext = m_doc.CreateTextNode(strAds);
            adsElement.AppendChild(adstext);
            body.AppendChild(adsElement);

            XmlElement hotAreasElement = m_doc.CreateElement("hot_areas");
            readHotAreaList(ref hotAreasElement, sceneFile, ref nDataPos);
            body.AppendChild(hotAreasElement);

            XmlElement objectInteractions1Element = m_doc.CreateElement("object_interactions1");
            readObjInteractionList(ref objectInteractions1Element, sceneFile, ref nDataPos);
            body.AppendChild(objectInteractions1Element);

            if (isVersionOver(1.205f))
            {
                XmlElement objectInteractions2Element = m_doc.CreateElement("object_interactions2");
                readObjInteractionList(ref objectInteractions2Element, sceneFile, ref nDataPos);
                body.AppendChild(objectInteractions2Element);
            }

            if (!isVersionOver(1.214f))
            {
                XmlElement dialogsElement = m_doc.CreateElement("dialogs");
                readDialogList(ref dialogsElement, sceneFile, ref nDataPos);
                body.AppendChild(dialogsElement);
            }

            if (isVersionOver(1.203f))
            {
                XmlElement triggersElement = m_doc.CreateElement("triggers");
                readTriggerList(ref triggersElement, sceneFile, ref nDataPos);
                body.AppendChild(triggersElement);
            }

            if (isVersionOver(1.223f))
            {
                XmlElement triggersElement = m_doc.CreateElement("conditionalScene");
                readConditionalSceneOpList(ref triggersElement, sceneFile, ref nDataPos);
                body.AppendChild(triggersElement);
            }

            return true;
        }

        private void readConditionalSceneOpList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            // Not implemented in Rise of the Dragon
        }

        private void readTriggerList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if (m_doc == null)
            {
                return;
            }

            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;

            for (int index = 0; index < num; ++index)
            {
                XmlElement triggerElement = m_doc.CreateElement("trigger");

                UInt16 triggernum = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement numElement = m_doc.CreateElement("num");
                XmlText numText = m_doc.CreateTextNode(triggernum.ToString());
                numElement.AppendChild(numText);
                triggerElement.AppendChild(numElement);

                if (isVersionOver(1.219f))
                {
                    XmlElement checkTimesElement = m_doc.CreateElement("checkTimes");
                    XmlText checkTimesText = m_doc.CreateTextNode(checkTimesElement.ToString());
                    checkTimesElement.AppendChild(checkTimesText);
                    triggerElement.AppendChild(checkTimesElement);
                }

                XmlElement conditionsElement = m_doc.CreateElement("conditions");
                readConditionList(ref conditionsElement, sceneFile, ref nDataPos);
                triggerElement.AppendChild(conditionsElement);

                XmlElement opsElement = m_doc.CreateElement("ops");
                readOpList(ref opsElement, sceneFile, ref nDataPos);
                triggerElement.AppendChild(opsElement);

                body.AppendChild(triggerElement);
            }
        }

        // Where the modifications to add voice go
        private void readDialogList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if (m_doc == null)
            {
                return;
            }

            UInt16 nitems = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;

            for (int index = 0; index < nitems; ++index)
            {
                XmlElement dialogElement = m_doc.CreateElement("dialog");

                UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 rectX = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 rectY = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 rectWidth = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 rectHeight = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 bgColor = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 fontColor = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement numElement = m_doc.CreateElement("num");
                XmlText numtext = m_doc.CreateTextNode(num.ToString());
                numElement.AppendChild(numtext);
                dialogElement.AppendChild(numElement);

                XmlElement rectElement = m_doc.CreateElement("Rect");

                XmlElement xElement = m_doc.CreateElement("X");
                XmlText xtext = m_doc.CreateTextNode(rectX.ToString());
                xElement.AppendChild(xtext);
                rectElement.AppendChild(xElement);

                XmlElement yElement = m_doc.CreateElement("Y");
                XmlText ytext = m_doc.CreateTextNode(rectY.ToString());
                yElement.AppendChild(ytext);
                rectElement.AppendChild(yElement);

                XmlElement widthElement = m_doc.CreateElement("Width");
                XmlText widthtext = m_doc.CreateTextNode(rectWidth.ToString());
                widthElement.AppendChild(widthtext);
                rectElement.AppendChild(widthElement);

                XmlElement heightElement = m_doc.CreateElement("Height");
                XmlText heighttext = m_doc.CreateTextNode(rectHeight.ToString());
                heightElement.AppendChild(heighttext);
                rectElement.AppendChild(heightElement);

                dialogElement.AppendChild(rectElement);

                UInt16 selectionBgCol = 0, selectonFontCol = 0;

                XmlElement bgColElement = m_doc.CreateElement("bgColor");
                XmlText bgColtext = m_doc.CreateTextNode(bgColor.ToString());
                bgColElement.AppendChild(bgColtext);
                dialogElement.AppendChild(bgColElement);

                XmlElement fontColElement = m_doc.CreateElement("fontColor");
                XmlText fontColtext = m_doc.CreateTextNode(fontColor.ToString());
                fontColElement.AppendChild(fontColtext);
                dialogElement.AppendChild(fontColElement);

                if (isVersionOver(1.209f))
                {
                    selectionBgCol = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    selectonFontCol = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;

                    XmlElement selectionBgColElement = m_doc.CreateElement("selectionBgCol");
                    XmlText selectionBgColtext = m_doc.CreateTextNode(selectionBgCol.ToString());
                    selectionBgColElement.AppendChild(selectionBgColtext);
                    dialogElement.AppendChild(selectionBgColElement);

                    XmlElement selectonFontColElement = m_doc.CreateElement("selectonFontCol");
                    XmlText selectonFontColtext = m_doc.CreateTextNode(selectonFontCol.ToString());
                    selectonFontColElement.AppendChild(selectonFontColtext);
                    dialogElement.AppendChild(selectonFontColElement);
                }

                UInt16 fontSize = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement fontSizeElement = m_doc.CreateElement("fontSize");
                XmlText fontSizetext = m_doc.CreateTextNode(fontSize.ToString());
                fontSizeElement.AppendChild(fontSizetext);
                dialogElement.AppendChild(fontSizeElement);

                uint flags = 0;

                if (!isVersionOver(1.210f))
                {
                    flags = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                }
                else
                {
                    flags = BitConverter.ToUInt32(sceneFile, nDataPos);
                    nDataPos += 4;
                }
                getDialogFlags(ref dialogElement, flags);

                UInt16 frametype = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                XmlElement frameTypeElement = m_doc.CreateElement("frameType");
                XmlText frameTypetext = m_doc.CreateTextNode(getFrameType(frametype));
                frameTypeElement.AppendChild(frameTypetext);
                dialogElement.AppendChild(frameTypeElement);

                UInt16 dialogtime = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                XmlElement timeElement = m_doc.CreateElement("dialog_time");
                XmlText timetext = m_doc.CreateTextNode(dialogtime.ToString());
                timeElement.AppendChild(timetext);
                dialogElement.AppendChild(timeElement);

                if (isVersionOver(1.215f))
                {
                    UInt16 nextDialogFileNum = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    XmlElement nextDialogFileNumElement = m_doc.CreateElement("nextDialogFileNum");
                    XmlText nextDialogFileNumtext = m_doc.CreateTextNode(nextDialogFileNum.ToString());
                    nextDialogFileNumElement.AppendChild(nextDialogFileNumtext);
                    dialogElement.AppendChild(nextDialogFileNumElement);
                }
                if (isVersionOver(1.207f))
                {
                    UInt16 nextDialogFileNum = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    XmlElement nextDialogFileNumElement = m_doc.CreateElement("nextDialogDlgNum");
                    XmlText nextDialogFileNumtext = m_doc.CreateTextNode(nextDialogFileNum.ToString());
                    nextDialogFileNumElement.AppendChild(nextDialogFileNumtext);
                    dialogElement.AppendChild(nextDialogFileNumElement);
                }

                if (isVersionOver(1.216f))
                {
                    UInt16 unknown1 = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    XmlElement unknown1Element = m_doc.CreateElement("unknown1");
                    XmlText unknown1Numtext = m_doc.CreateTextNode(unknown1.ToString());
                    unknown1Element.AppendChild(unknown1Numtext);
                    dialogElement.AppendChild(unknown1Element);

                    UInt16 unknown2 = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    XmlElement unknown2Element = m_doc.CreateElement("unknown2");
                    XmlText unknown2Numtext = m_doc.CreateTextNode(unknown2.ToString());
                    unknown2Element.AppendChild(unknown2Numtext);
                    dialogElement.AppendChild(unknown2Element);
                }

                UInt16 dialogLen = BitConverter.ToUInt16(sceneFile, nDataPos);
                /*if(dialogLen == 0x2c)
                {
                    int j = 9;
                }*/
                nDataPos += 2;

                if(dialogLen > 0)
                {
                    Encoding cp437 = Encoding.GetEncoding("iso-8859-1");
                    string strType = cp437.GetString(sceneFile, nDataPos, dialogLen);
                    strType = strType.Replace("\0", string.Empty);
                    nDataPos += dialogLen;
                    XmlElement dialogTextElement = m_doc.CreateElement("dialogText");
                    XmlText dialogTexttext = m_doc.CreateTextNode(strType);
                    dialogTextElement.AppendChild(dialogTexttext);
                    dialogElement.AppendChild(dialogTextElement);
                }
                else
                {
                    XmlElement dialogTextElement = m_doc.CreateElement("dialogText");
                    dialogElement.AppendChild(dialogTextElement);
                }
                XmlElement dialogActionsElement = m_doc.CreateElement("dialogActions");
                readDialogActionList(ref dialogActionsElement, sceneFile, ref nDataPos);
                dialogElement.AppendChild(dialogActionsElement);

                body.AppendChild(dialogElement);
            }
        }

        private void readDialogActionList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if (m_doc == null)
            {
                return;
            }

            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;

            for (int index = 0; index < num; ++index)
            {
                XmlElement dialogActionElement = m_doc.CreateElement("dialogAction");

                UInt16 start = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 end = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                XmlElement startElement = m_doc.CreateElement("start");
                XmlText startText = m_doc.CreateTextNode(start.ToString());
                startElement.AppendChild(startText);
                dialogActionElement.AppendChild(startElement);
                XmlElement endElement = m_doc.CreateElement("end");
                XmlText endText = m_doc.CreateTextNode(end.ToString());
                endElement.AppendChild(endText);
                dialogActionElement.AppendChild(endElement);

                XmlElement opsElement = m_doc.CreateElement("opList");
                readOpList(ref opsElement, sceneFile, ref nDataPos);
                dialogActionElement.AppendChild(opsElement);

                body.AppendChild(dialogActionElement);
            }
        }

        private void readObjInteractionList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if(m_doc  == null)
            {
                return;
            }
            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            for (int index = 0; index < num; ++index)
            {
                XmlElement objectInteractionElement = m_doc.CreateElement("object_interactions");
                UInt16 dropped, target, target1 = 0;

                if (!isVersionOver(1.205f))
                {
                    target1 = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    dropped = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    target = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;

                    XmlElement target1Element = m_doc.CreateElement("target1");
                    XmlText target1text = m_doc.CreateTextNode(target1.ToString());
                    target1Element.AppendChild(target1text);
                    objectInteractionElement.AppendChild(target1Element);

                    XmlElement droppedElement = m_doc.CreateElement("dropped");
                    XmlText droppedtext = m_doc.CreateTextNode(dropped.ToString());
                    droppedElement.AppendChild(droppedtext);
                    objectInteractionElement.AppendChild(droppedElement);

                    XmlElement targetElement = m_doc.CreateElement("target");
                    XmlText targettext = m_doc.CreateTextNode(target.ToString());
                    targetElement.AppendChild(targettext);
                    objectInteractionElement.AppendChild(targetElement);
                }
                else
                {
                    dropped = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    target = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;

                    XmlElement droppedElement = m_doc.CreateElement("dropped");
                    XmlText droppedtext = m_doc.CreateTextNode(dropped.ToString());
                    droppedElement.AppendChild(droppedtext);
                    objectInteractionElement.AppendChild(droppedElement);

                    XmlElement targetElement = m_doc.CreateElement("target");
                    XmlText targettext = m_doc.CreateTextNode(target.ToString());
                    targetElement.AppendChild(targettext);
                    objectInteractionElement.AppendChild(targetElement);
                }

                XmlElement opsElement = m_doc.CreateElement("opList");
                readOpList(ref opsElement, sceneFile, ref nDataPos);
                objectInteractionElement.AppendChild(opsElement);

                body.AppendChild(objectInteractionElement);
            }
        }

        private void readHotAreaList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if (m_doc == null)
            {
                return;
            }
            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            for(int index = 0; index < num; ++index)
            {
                XmlElement hotAreaElement = m_doc.CreateElement("hot_area");
                readHotArea(ref hotAreaElement, sceneFile, ref nDataPos);
                body.AppendChild(hotAreaElement);
            }
        }

        private void readHotArea(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if (m_doc == null)
            {
                return;
            }

            UInt16 rectX = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 rectY = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 rectWidth = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 rectHeight = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 cursorNum = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            UInt16 otherCursorNum = 0;
            UInt16 objInteractionListFlag = 0;
            if (isVersionOver(1.217f))
            {
                otherCursorNum = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
            }
            if (isVersionOver(1.218f))
            {
                objInteractionListFlag = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                if(objInteractionListFlag > 0)
                {
                    // Not implemented as Rise of the Dragon doesn't need it
                }
            }

            XmlElement rectElement = m_doc.CreateElement("Rect");

            XmlElement xElement = m_doc.CreateElement("X");
            XmlText xtext = m_doc.CreateTextNode(rectX.ToString());
            xElement.AppendChild(xtext);
            rectElement.AppendChild(xElement);

            XmlElement yElement = m_doc.CreateElement("Y");
            XmlText ytext = m_doc.CreateTextNode(rectY.ToString());
            yElement.AppendChild(ytext);
            rectElement.AppendChild(yElement);

            XmlElement widthElement = m_doc.CreateElement("Width");
            XmlText widthtext = m_doc.CreateTextNode(rectWidth.ToString());
            widthElement.AppendChild(widthtext);
            rectElement.AppendChild(widthElement);

            XmlElement heightElement = m_doc.CreateElement("Height");
            XmlText heighttext = m_doc.CreateTextNode(rectHeight.ToString());
            heightElement.AppendChild(heighttext);
            rectElement.AppendChild(heightElement);

            body.AppendChild(rectElement);

            XmlElement hotAreaNumElement = m_doc.CreateElement("hotAreaNum");
            XmlText hotAreaNumText = m_doc.CreateTextNode(num.ToString());
            hotAreaNumElement.AppendChild(hotAreaNumText);
            body.AppendChild(hotAreaNumElement);

            XmlElement cursorNumElement = m_doc.CreateElement("cursorNum");
            XmlText cursorNumText = m_doc.CreateTextNode(cursorNum.ToString());
            cursorNumElement.AppendChild(cursorNumText);
            body.AppendChild(cursorNumElement);

            XmlElement conditionsElement = m_doc.CreateElement("enable_conditions");
            readConditionList(ref conditionsElement, sceneFile, ref nDataPos);
            body.AppendChild(conditionsElement);

            XmlElement onRClickOpsElement = m_doc.CreateElement("onRClickOps");
            readOpList(ref onRClickOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(onRClickOpsElement);

            XmlElement onLDownOpsElement = m_doc.CreateElement("onLDownOps");
            readOpList(ref onLDownOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(onLDownOpsElement);

            XmlElement onLClickOpsElement = m_doc.CreateElement("onLClickOps");
            readOpList(ref onLClickOpsElement, sceneFile, ref nDataPos);
            body.AppendChild(onLClickOpsElement);
        }

        private string readString(byte[] sceneFile, ref int nDataPos)
        {
            string ret = "";
            while (sceneFile[nDataPos] != 0)
            {
                ret += (char)sceneFile[nDataPos];
                nDataPos++;
            }
            nDataPos++;

            return ret;
        }

        private bool isVersionOver(float version)
        {
            return m_version >= version;
        }

        private void readOpList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if(m_doc == null)
            {
                return;
            }
            UInt16 nitems = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            for(int index = 0; index < nitems; ++index)
            {
                XmlElement opsElement = m_doc.CreateElement("scene_ops");
                XmlElement conditionsElement = m_doc.CreateElement("conditions");
                readConditionList(ref conditionsElement, sceneFile, ref nDataPos);
                opsElement.AppendChild(conditionsElement);

                UInt16 opCode = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement opElement = m_doc.CreateElement("opCode");
                XmlText opText = m_doc.CreateTextNode(opCode.ToString());
                opElement.AppendChild(opText);
                opElement.SetAttribute("Description", getSceneOpCode(opCode));
                opsElement.AppendChild(opElement);

                UInt16 nvals = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement argsElement = m_doc.CreateElement("args");
                for (int argIndex = 0; argIndex < nvals / 2; ++argIndex)
                {
                    UInt16 arg = BitConverter.ToUInt16(sceneFile, nDataPos);
                    nDataPos += 2;
                    XmlElement argElement = m_doc.CreateElement("arg");
                    XmlText argText = m_doc.CreateTextNode(arg.ToString());
                    argElement.AppendChild(argText);
                    argsElement.AppendChild(argElement);
                }
                opsElement.AppendChild(argsElement);

                body.AppendChild(opsElement);
            }
        }

        private string getFrameType(int op)
        {
            switch (op)
            {
                case 1:
                    return "kDlgFramePlain";
                case 2:
                    return "kDlgFrameBorder";
                case 3:
                    return "kDlgFrameThought";
                case 4:
                    return "kDlgFrameRounded";
                default:
                    return "Unknown";
            }
        }

        private string getSceneOpCode(int op)
        {
            switch(op)
            {
                case 0:
                    return "kSceneOpNone";
                case 1:
                    return "kSceneOpChangeScene";
                case 2:
                    return "kSceneOpNoop";
                case 3:
                    return "kSceneOpGlobal";
                case 4:
                    return "kSceneOpSegmentStateOps";
                case 5:
                    return "kSceneOpSetItemAttr";
                case 6:
                    return "kSceneOpSetDragItem";
                case 7:
                    return "kSceneOpOpenInventory";
                case 8:
                    return "kSceneOpShowDlg";
                case 9:
                    return "kSceneOpShowInvButton";
                case 10:
                    return "kSceneOpHideInvButton";
                case 11:
                    return "kSceneOpEnableTrigger";
                case 12:
                    return "kSceneOpChangeSceneToStored";
                case 13:
                    return "kSceneOpAddFlagToDragItem";
                case 14:
                    return "kSceneOpOpenInventoryZoom";
                case 15:
                    return "kSceneOpMoveItemsBetweenScenes";
                case 16:
                    return "kSceneOpShowClock";
                case 17:
                    return "kSceneOpHideClock";
                case 18:
                    return "kSceneOpShowMouse";
                case 19:
                    return "kSceneOpHideMouse";
                case 20:
                    return "kSceneOpLoadTalkDataAndSetFlags";
                case 21:
                    return "kSceneOpDrawVisibleTalkHeads";
                case 22:
                    return "kSceneOpLoadTalkData";
                case 24:
                    return "kSceneOpLoadDDSData";
                case 25:
                    return "kSceneOpFreeDDSData";
                case 26:
                    return "kSceneOpFreeTalkData";
                case 100:
                    return "kSceneOpPasscode";
                case 101:
                    return "kSceneOpMeanwhile";
                case 102:
                    return "kSceneOpOpenGameOverMenu";
                case 103:
                    return "kSceneOpTiredDialog";
                case 104:
                    return "kSceneOpArcadeTick";
                case 105:
                    return "kSceneOpDrawDragonCountdown1";
                case 106:
                    return "kSceneOpDrawDragonCountdown2";
                case 107:
                    return "kSceneOpOpenPlaySkipIntroMenu";
                case 108:
                    return "kSceneOpOpenBetterSaveGameMenu";
                default:
                    return "Unknown";
            }
        }

        private void readConditionList(ref XmlElement body, byte[] sceneFile, ref int nDataPos)
        {
            if(m_doc == null)
            {
                return;
            }
            
            UInt16 num = BitConverter.ToUInt16(sceneFile, nDataPos);
            nDataPos += 2;
            for (int index = 0; index < num; ++index)
            {
                XmlElement conditionElement = m_doc.CreateElement("condition");
                UInt16 cnum = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 cond = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;
                UInt16 val = BitConverter.ToUInt16(sceneFile, nDataPos);
                nDataPos += 2;

                XmlElement cnumElement = m_doc.CreateElement("cnum");
                XmlText cnumText = m_doc.CreateTextNode(cnum.ToString());
                cnumElement.AppendChild(cnumText);
                conditionElement.AppendChild(cnumElement);

                XmlElement condElement = m_doc.CreateElement("conditions");
                getSceneCondition(ref condElement, cond);
                conditionElement.AppendChild(condElement);

                XmlElement valElement = m_doc.CreateElement("val");
                XmlText valText = m_doc.CreateTextNode(val.ToString());
                valElement.AppendChild(valText);
                conditionElement.AppendChild(valElement);

                body.AppendChild(conditionElement);
            }
        }

        private void getDialogFlags(ref XmlElement body, uint op)
        {
            if (m_doc == null)
            {
                return;
            }
            XmlElement flagsElement = m_doc.CreateElement("dialog_flags");

            if (op == 0)
            {
                XmlElement condElement = m_doc.CreateElement("dialog_flag");
                XmlText condText = m_doc.CreateTextNode("kDlgFlagNone");
                condElement.AppendChild(condText);
                flagsElement.AppendChild(condElement);
            }
            else
            {
                if ((op & 0x01) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagFlatBg");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x02) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLeftJust");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x04) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo4");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x08) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo8");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x10) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo10");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x20) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo20");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x40) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo40");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x80) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagLo80");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }

                if ((op & 0x010000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHiFinished");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x020000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHi2");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x040000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHi4");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x080000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagRedrawSelectedActionChanged");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x100000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHi10");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x200000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHi20");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x400000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagHi40");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
                if ((op & 0x800000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagVisible");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }

                if ((op & 0x1000000) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("dialog_flag");
                    XmlText condText = m_doc.CreateTextNode("kDlgFlagOpening");
                    condElement.AppendChild(condText);
                    flagsElement.AppendChild(condElement);
                }
            }

            body.AppendChild(flagsElement);
        }

        private void getSceneCondition(ref XmlElement body, int op)
        {
            if(m_doc == null)
            {
                return;
            }
            if(op == 0)
            {
                XmlElement condElement = m_doc.CreateElement("condition");
                XmlText condText = m_doc.CreateTextNode("kSceneCondNone");
                condElement.AppendChild(condText);
                body.AppendChild(condElement);
            }
            else
            {
                if ((op & 0x01) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondLessThan");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x02) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondEqual");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x04) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondNegate");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x08) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondAbsVal");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x10) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondOr");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x20) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondNeedItemSceneNum");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x40) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondNeedItemQuality");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
                if ((op & 0x80) > 0)
                {
                    XmlElement condElement = m_doc.CreateElement("condition");
                    XmlText condText = m_doc.CreateTextNode("kSceneCondSceneState");
                    condElement.AppendChild(condText);
                    body.AppendChild(condElement);
                }
            }
        }

        private XmlNode? FindNode(XmlNodeList list, string nodeName)
        {
            if (list.Count > 0)
            {
                foreach (XmlNode node in list)
                {
                    if (node.Name.Equals(nodeName)) return node;
                }
            }
            return null;
        }

        public void save(string strDir)
        {
            bool bValid = true;
            if (!strDir.EndsWith("\\"))
            {
                strDir += "\\";
            }

            string strFile = strDir + "S12.SAV";
            string strXml = strDir + "S12.SDS.xml";
            m_doc = new XmlDocument();
            m_doc.Load(strXml);
            if (m_doc.DocumentElement != null)
            {
                List<byte> listHeader = new List<byte>();
                List<byte> listVersion = new List<byte>();
                List<byte> listMagicNumber = new List<byte>();
                List<byte> listSceneNum = new List<byte>();
                List<byte> listEnterSceneOps = new List<byte>();
                List<byte> listLeaveSceneOps = new List<byte>();
                List<byte> listPreTickOps = new List<byte>();
                List<byte> listPostTickOps = new List<byte>();
                List<byte> listfield6_0x14Nodes = new List<byte>();
                List<byte> listADS_File = new List<byte>();
                List<byte> listHotAreas = new List<byte>();
                List<byte> listobject_interactions1 = new List<byte>();
                List<byte> listobject_interactions2 = new List<byte>();
                List<byte> listDialogs = new List<byte>();
                List<byte> listTriggers = new List<byte>();

                XmlElement root = m_doc.DocumentElement;
                XmlNodeList MagicNumberNodes = root.GetElementsByTagName("MagicNumber");
                XmlNodeList versionNodes = root.GetElementsByTagName("Version");
                XmlNodeList SceneNumNodes = root.GetElementsByTagName("sceneNum");
                XmlNodeList enter_scene_opsNodes = root.GetElementsByTagName("enter_scene_ops");
                XmlNodeList leave_scene_opsNodes = root.GetElementsByTagName("leave_scene_ops");
                XmlNodeList pre_tick_opsNodes = root.GetElementsByTagName("pre_tick_ops");
                XmlNodeList post_tick_opsNodes = root.GetElementsByTagName("post_tick_ops");
                XmlNodeList field6_0x14Nodes = root.GetElementsByTagName("field6_0x14");
                XmlNodeList ADS_FileNodes = root.GetElementsByTagName("ADS_File");
                XmlNodeList hot_areasNodes = root.GetElementsByTagName("hot_areas");
                XmlNodeList object_interactions1Nodes = root.GetElementsByTagName("object_interactions1");
                XmlNodeList object_interactions2Nodes = root.GetElementsByTagName("object_interactions2");
                XmlNodeList dialogsNodes = root.GetElementsByTagName("dialogs");
                XmlNodeList triggersNodes = root.GetElementsByTagName("triggers");

                if (MagicNumberNodes.Count == 1 &&
                    versionNodes.Count == 1 &&
                    SceneNumNodes.Count == 1 &&
                    enter_scene_opsNodes.Count == 1 &&
                    leave_scene_opsNodes.Count == 1 &&
                    pre_tick_opsNodes.Count == 1 &&
                    post_tick_opsNodes.Count == 1 &&
                    field6_0x14Nodes.Count == 1 &&
                    ADS_FileNodes.Count == 1 &&
                    hot_areasNodes.Count == 1 &&
                    object_interactions1Nodes.Count == 1 &&
                    object_interactions2Nodes.Count == 1 &&
                    dialogsNodes.Count == 1 &&
                    triggersNodes.Count == 1)
                {
                    XmlNode? elementMagicNumber = MagicNumberNodes.Item(0);
                    if (elementMagicNumber != null)
                    {
                        XmlNode curNode = elementMagicNumber;
                        if (!ProcessUint32(curNode, ref listMagicNumber))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elemenVersion = versionNodes.Item(0);
                    if (elemenVersion != null)
                    {
                        XmlNode curNode = elemenVersion;
                        float version;
                        if (!ProcessVersionNode(curNode, ref listVersion, out version))
                        {
                            bValid = false;
                        }
                        m_version = version;
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elemenSceneNum = SceneNumNodes.Item(0);
                    if (elemenSceneNum != null)
                    {
                        XmlNode curNode = elemenSceneNum;
                        if (!ProcessUint16(curNode, ref listSceneNum))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elemententer_scene_opsNodes = enter_scene_opsNodes.Item(0);
                    if (elemententer_scene_opsNodes != null)
                    {
                        XmlNode curNode = elemententer_scene_opsNodes;
                        if (curNode == null)
                        {
                            bValid = false;
                        }
                        else
                        {
                            if (!ProcessOpListNode(curNode, ref listEnterSceneOps))
                            {
                                bValid = false;
                            }
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elementleave_scene_opsNodes = leave_scene_opsNodes.Item(0);
                    if (elementleave_scene_opsNodes != null)
                    {
                        XmlNode curNode = elementleave_scene_opsNodes;
                        if (curNode == null)
                        {
                            bValid = false;
                        }
                        else
                        {
                            if (!ProcessOpListNode(curNode, ref listLeaveSceneOps))
                            {
                                bValid = false;
                            }
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    if (isVersionOver(1.206f))
                    {
                        XmlNode? elementlistPreTickOps = pre_tick_opsNodes.Item(0);
                        if (elementlistPreTickOps != null)
                        {
                            XmlNode curNode = elementlistPreTickOps;
                            if (curNode == null)
                            {
                                bValid = false;
                            }
                            else
                            {
                                if (!ProcessOpListNode(curNode, ref listPreTickOps))
                                {
                                    bValid = false;
                                }
                            }
                        }
                        else
                        {
                            bValid = false;
                        }
                    }
                    XmlNode? elementlistPostTickOps = post_tick_opsNodes.Item(0);
                    if (elementlistPostTickOps != null)
                    {
                        XmlNode curNode = elementlistPostTickOps;
                        if (curNode == null)
                        {
                            bValid = false;
                        }
                        else
                        {
                            if (!ProcessOpListNode(curNode, ref listPostTickOps))
                            {
                                bValid = false;
                            }
                        }
                    }
                    else
                    {
                        bValid = false;
                    }

                    XmlNode? elementfield6_0x14Nodes = field6_0x14Nodes.Item(0);
                    if (elementfield6_0x14Nodes != null)
                    {
                        XmlNode curNode = elementfield6_0x14Nodes;
                        if (!ProcessUint16(curNode, ref listfield6_0x14Nodes))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }

                    XmlNode? elementADS_FileNodes = ADS_FileNodes.Item(0);
                    if (elementADS_FileNodes != null)
                    {
                        XmlNode curNode = elementADS_FileNodes;
                        if (!ProcessString(curNode, ref listADS_File, false))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }

                    XmlNode? elementhot_areasNodes = hot_areasNodes.Item(0);
                    if (elementhot_areasNodes != null)
                    {
                        XmlNode curNode = elementhot_areasNodes;
                        if (!ProcessHotAreasNode(curNode, ref listHotAreas))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }
                    XmlNode? elementobject_interactions1Nodes = object_interactions1Nodes.Item(0);
                    if (elementobject_interactions1Nodes != null)
                    {
                        XmlNode curNode = elementobject_interactions1Nodes;
                        if (!ProcessInteractionsListNode(curNode, ref listobject_interactions1))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        bValid = false;
                    }

                    if (isVersionOver(1.205f))
                    {
                        XmlNode? elementobject_interactions2Nodes = object_interactions2Nodes.Item(0);
                        if (elementobject_interactions2Nodes != null)
                        {
                            XmlNode curNode = elementobject_interactions2Nodes;
                            if (!ProcessInteractionsListNode(curNode, ref listobject_interactions2))
                            {
                                bValid = false;
                            }
                        }
                        else
                        {
                            bValid = false;
                        }
                    }
                    if (!isVersionOver(1.214f))
                    {
                        XmlNode? elementdialogsNodes = dialogsNodes.Item(0);
                        if (elementdialogsNodes != null)
                        {
                            XmlNode curNode = elementdialogsNodes;
                            if (!ProcessDialogsNode(curNode, ref listDialogs))
                            {
                                bValid = false;
                            }
                        }
                        else
                        {
                            bValid = false;
                        }
                    }
                    if (isVersionOver(1.203f))
                    {
                        XmlNode? elementtriggersNodes = triggersNodes.Item(0);
                        if (elementtriggersNodes != null)
                        {
                            XmlNode curNode = elementtriggersNodes;
                            if (!ProcessTriggersNode(curNode, ref listTriggers))
                            {
                                bValid = false;
                            }
                        }
                        else
                        {
                            bValid = false;
                        }
                    }
                    if (isVersionOver(1.223f))
                    {
                        // Not implemented, conditional scene ops
                    }

                    int totalfilesize = 0;
                    totalfilesize += listVersion.Count;
                    totalfilesize += listMagicNumber.Count;
                    totalfilesize += listSceneNum.Count;
                    totalfilesize += listEnterSceneOps.Count;
                    totalfilesize += listLeaveSceneOps.Count;
                    totalfilesize += listPreTickOps.Count;
                    totalfilesize += listPostTickOps.Count;
                    totalfilesize += listfield6_0x14Nodes.Count;
                    totalfilesize += listADS_File.Count;
                    totalfilesize += listHotAreas.Count;
                    totalfilesize += listobject_interactions1.Count;
                    totalfilesize += listobject_interactions2.Count;
                    totalfilesize += listDialogs.Count;
                    totalfilesize += listTriggers.Count;

                    if (!ProcessHeaderNode(ref listHeader, (UInt32)totalfilesize))
                    {
                        bValid = false;
                    }

                    if (bValid)
                    {
                        using (BinaryWriter writer = new BinaryWriter(File.Open(strFile, FileMode.Create)))
                        {
                            writer.Write(listHeader.ToArray());
                            writer.Write(listMagicNumber.ToArray());
                            writer.Write(listVersion.ToArray());
                            writer.Write(listSceneNum.ToArray());
                            writer.Write(listEnterSceneOps.ToArray());
                            writer.Write(listLeaveSceneOps.ToArray());
                            writer.Write(listPreTickOps.ToArray());
                            writer.Write(listPostTickOps.ToArray());
                            writer.Write(listfield6_0x14Nodes.ToArray());
                            writer.Write(listADS_File.ToArray());
                            writer.Write(listHotAreas.ToArray());
                            writer.Write(listobject_interactions1.ToArray());
                            writer.Write(listobject_interactions2.ToArray());
                            writer.Write(listDialogs.ToArray());
                            writer.Write(listTriggers.ToArray());
                        }
                    }
                }
            }
        }

        private bool ProcessTriggersNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listTriggers = new List<byte>();

            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            byte[] bytes = BitConverter.GetBytes(nitems);
            listTriggers.AddRange(bytes);

            for (int index = 0; index < nitems; ++index)
            {
                if (!ProcessTriggerNode(root.ChildNodes[index], ref listTriggers))
                {
                    return false;
                }
            }

            curList.AddRange(listTriggers);

            return retVal;
        }

        private bool ProcessTriggerNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listTrigger = new List<byte>();

            XmlNode? numNodes = FindNode(root.ChildNodes, "num");
            XmlNode? conditionsNodes = FindNode(root.ChildNodes, "conditions");
            XmlNode? opsNodes = FindNode(root.ChildNodes, "ops");

            if (!ProcessUint16(numNodes, ref listTrigger))
            {
                return false;
            }
            if (!ProcessConditionList(conditionsNodes, ref listTrigger))
            {
                return false;
            }
            if (!ProcessOpListNode(opsNodes, ref listTrigger))
            {
                return false;
            }

            curList.AddRange(listTrigger);

            return retVal;
        }

        private bool ProcessDialogsNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listDialogs = new List<byte>();

            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            byte[] bytes = BitConverter.GetBytes(nitems);
            listDialogs.AddRange(bytes);

            for (int index = 0; index < nitems; ++index)
            {
                if(!ProcessDialogNode(root.ChildNodes[index], ref listDialogs))
                {
                    return false;
                }
            }
            curList.AddRange(listDialogs);

            return retVal;
        }

        private bool ProcessDialogNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listDialog = new List<byte>();

            XmlNode? numNodes = FindNode(root.ChildNodes, "num");
            XmlNode? RectNodes = FindNode(root.ChildNodes, "Rect");
            XmlNode? bgColNodes = FindNode(root.ChildNodes, "bgColor");
            XmlNode? fontColNodes = FindNode(root.ChildNodes, "fontColor");
            XmlNode? selectionBgColNodes = FindNode(root.ChildNodes, "selectionBgCol");
            XmlNode? selectonFontColNodes = FindNode(root.ChildNodes, "selectonFontCol");
            XmlNode? fontSizeNodes = FindNode(root.ChildNodes, "fontSize");
            XmlNode? dialog_flagsNodes = FindNode(root.ChildNodes, "dialog_flags");
            XmlNode? frameTypeNodes = FindNode(root.ChildNodes, "frameType");
            XmlNode? dialog_timeNodes = FindNode(root.ChildNodes, "dialog_time");
            XmlNode? nextDialogDlgNumNodes = FindNode(root.ChildNodes, "nextDialogDlgNum");
            XmlNode? dialogTextNodes = FindNode(root.ChildNodes, "dialogText");
            XmlNode? dialogActionsNodes = FindNode(root.ChildNodes, "dialogActions");

            if (!ProcessUint16(numNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessRect(RectNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessUint16(bgColNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessUint16(fontColNodes, ref listDialog))
            {
                return false;
            }
            if (isVersionOver(1.209f))
            {
                if (!ProcessUint16(selectionBgColNodes, ref listDialog))
                {
                    return false;
                }
                if (!ProcessUint16(selectonFontColNodes, ref listDialog))
                {
                    return false;
                }
            }
            if (!ProcessUint16(fontSizeNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessDialogFlags(dialog_flagsNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessFrameType(frameTypeNodes, ref listDialog))
            {
                return false;
            }
            if (!ProcessUint16(dialog_timeNodes, ref listDialog))
            {
                return false;
            }
            if (isVersionOver(1.215f))
            {
                // Not implemented: _nextDialogFileNum
            }
            if (isVersionOver(1.207f))
            {
                if (!ProcessUint16(nextDialogDlgNumNodes, ref listDialog))
                {
                    return false;
                }
            }
            if (isVersionOver(1.216f))
            {
                // Not implemented: unknowns 1 & 2
            }
            if (!ProcessString(dialogTextNodes, ref listDialog, true))
            {
                return false;
            }
            if (!ProcessDialogActionsList(dialogActionsNodes, ref listDialog))
            {
                return false;
            }

            curList.AddRange(listDialog);

            return retVal;
        }

        private bool ProcessDialogActionsList(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listActionList = new List<byte>();

            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            byte[] bytes = BitConverter.GetBytes(nitems);
            listActionList.AddRange(bytes);

            for (int index = 0; index < nitems; ++index)
            {
                if (!ProcessDialogActionList(root.ChildNodes[index], ref listActionList))
                {
                    return false;
                }
            }

            curList.AddRange(listActionList);

            return retVal;
        }

        private bool ProcessDialogActionList(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            XmlNode? startNodes = FindNode(root.ChildNodes, "start");
            XmlNode? endNodes = FindNode(root.ChildNodes, "end");
            XmlNode? opListNodes = FindNode(root.ChildNodes, "opList");

            if (!ProcessUint16(startNodes, ref curList))
            {
                return false;
            }
            if (!ProcessUint16(endNodes, ref curList))
            {
                return false;
            }
            if (!ProcessOpListNode(opListNodes, ref curList))
            {
                return false;
            }

            return retVal;
        }

        private bool ProcessString(XmlNode? root, ref List<byte> curList, bool includeSize)
        {
            if (root == null)
            {
                return false;
            }
            if (includeSize)
            {
                UInt16 tempval = (UInt16)(root.InnerText.Length + 1);

                byte[] bytes = BitConverter.GetBytes(tempval);
                curList.AddRange(bytes);
            }
            Encoding cp437 = Encoding.GetEncoding("iso-8859-1");
            byte[] valuebytes = cp437.GetBytes(root.InnerText);
            curList.AddRange(valuebytes);
            curList.Add(0);
            return true;
        }

        private bool ProcessFrameType(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }
            UInt16 tempval = getDialogFrameType(root.InnerText);
            byte[] bytes = BitConverter.GetBytes(tempval);
            curList.AddRange(bytes);

            return retVal;
        }

        private UInt16 getDialogFrameType(string strText)
        {
            if (strText == "kDlgFramePlain")
            {
                return 1;
            }
            else if (strText == "kDlgFrameBorder")
            {
                return 2;
            }
            else if (strText == "kDlgFrameThought")
            {
                return 3;
            }
            else if (strText == "kDlgFrameRounded")
            {
                return 4;
            }

            return 0;
        }

        private bool ProcessDialogFlags(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;
            UInt32 dialogflags = 0;

            if (root == null)
            {
                return false;
            }
            for (int index = 0; index < root.ChildNodes.Count; ++index)
            {
                XmlNode? curNode = root.ChildNodes[index];
                if (curNode == null)
                {
                    return false;
                }
                UInt32 tempval = getDialogFlagVal(curNode.InnerText);
                dialogflags |= tempval;
            }

            if (!isVersionOver(1.209f))
            {
                UInt16 nitems = (UInt16)dialogflags;
                byte[] bytes = BitConverter.GetBytes(nitems);
                curList.AddRange(bytes);
            }
            else
            {
                byte[] bytes = BitConverter.GetBytes(dialogflags);
                curList.AddRange(bytes);
            }

            return retVal;
        }

        private UInt32 getDialogFlagVal(string strText)
        {
            if (strText == "kDlgFlagNone")
            {
                return 0;
            }
            else if (strText == "kDlgFlagFlatBg")
            {
                return 1;
            }
            else if (strText == "kDlgFlagLeftJust")
            {
                return 2;
            }
            else if (strText == "kDlgFlagLo4")
            {
                return 4;
            }
            else if (strText == "kDlgFlagLo8")
            {
                return 8;
            }
            else if (strText == "kDlgFlagLo40")
            {
                return 0x40;
            }
            else if (strText == "kDlgFlagLo80")
            {
                return 0x80;
            }
            else if (strText == "kDlgFlagHiFinished")
            {
                return 0x10000;
            }
            else if (strText == "kDlgFlagHi2")
            {
                return 0x20000;
            }
            else if (strText == "kDlgFlagHi4")
            {
                return 0x40000;
            }
            else if (strText == "kDlgFlagRedrawSelectedActionChanged")
            {
                return 0x80000;
            }
            else if (strText == "kDlgFlagHi10")
            {
                return 0x100000;
            }
            else if (strText == "kDlgFlagHi20")
            {
                return 0x200000;
            }
            else if (strText == "kDlgFlagHi40")
            {
                return 0x400000;
            }
            else if (strText == "kDlgFlagVisible")
            {
                return 0x800000;
            }
            else if (strText == "kDlgFlagOpening")
            {
                return 0x1000000;
            }
            return 0;
        }

        private bool ProcessInteractionListNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listInteractionList = new List<byte>();

            XmlNode? targetNodes = FindNode(root.ChildNodes, "target");
            XmlNode? droppedNodes = FindNode(root.ChildNodes, "dropped");
            XmlNode? target1Nodes = FindNode(root.ChildNodes, "target1");
            XmlNode? opListNodes = FindNode(root.ChildNodes, "opList");

            if (!isVersionOver(1.205f))
            {
                if (!ProcessUint16(target1Nodes, ref listInteractionList))
                {
                    return false;
                }
                if (!ProcessUint16(droppedNodes, ref listInteractionList))
                {
                    return false;
                }
                if (!ProcessUint16(targetNodes, ref listInteractionList))
                {
                    return false;
                }
            }
            else
            {
                if (!ProcessUint16(droppedNodes, ref listInteractionList))
                {
                    return false;
                }
                if (!ProcessUint16(targetNodes, ref listInteractionList))
                {
                    return false;
                }
            }

            if (!ProcessOpListNode(opListNodes, ref listInteractionList))
            {
                return false;
            }

            curList.AddRange(listInteractionList);

            return retVal;
        }

        private bool ProcessInteractionsListNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listInteractionsList = new List<byte>();

            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            byte[] bytes = BitConverter.GetBytes(nitems);
            listInteractionsList.AddRange(bytes);

            for (int index = 0; index < nitems; ++index)
            {
                if (!ProcessInteractionListNode(root.ChildNodes[index], ref listInteractionsList))
                {
                    return false;
                }
            }

            curList.AddRange(listInteractionsList);

            return retVal;
        }

            private bool ProcessHotAreasNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }
            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            List<byte> listHotAreas = new List<byte>();
            byte[] bytes = BitConverter.GetBytes(nitems);
            listHotAreas.AddRange(bytes);

            for (int index = 0; index < nitems; ++index)
            {
                if (!ProcessHotAreaNode(root.ChildNodes[index], ref listHotAreas))
                {
                    return false;
                }
            }
            curList.AddRange(listHotAreas);

            return retVal;
        }

        private bool ProcessHotAreaNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }
            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            if(nitems != 7)
            {
                return false;
            }
            List<byte> listHotArea = new List<byte>();

            XmlNode? RectNodes = FindNode(root.ChildNodes, "Rect");
            XmlNode? hotAreaNumNodes = FindNode(root.ChildNodes, "hotAreaNum");
            XmlNode? cursorNumNodes = FindNode(root.ChildNodes, "cursorNum");
            XmlNode? enable_conditionsNodes = FindNode(root.ChildNodes, "enable_conditions");
            XmlNode? onRClickOpsNodes = FindNode(root.ChildNodes, "onRClickOps");
            XmlNode? onLDownOpsNodes = FindNode(root.ChildNodes, "onLDownOps");
            XmlNode? onLClickOpsNodes = FindNode(root.ChildNodes, "onLClickOps");

            if (!ProcessRect(RectNodes, ref listHotArea))
            {
                return false;
            }
            if (!ProcessUint16(hotAreaNumNodes, ref listHotArea))
            {
                return false;
            }
            if (!ProcessUint16(cursorNumNodes, ref listHotArea))
            {
                return false;
            }
            if (!ProcessConditionList(enable_conditionsNodes, ref listHotArea))
            {
                return false;
            }
            if(!ProcessOpListNode(onRClickOpsNodes, ref listHotArea))
            {
                return false;
            }
            if (!ProcessOpListNode(onLDownOpsNodes, ref listHotArea))
            {
                return false;
            }
            if (!ProcessOpListNode(onLClickOpsNodes, ref listHotArea))
            {
                return false;
            }

            curList.AddRange(listHotArea);

            return retVal;
        }

        private bool ProcessRect(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }
            if(root.ChildNodes.Count != 4)
            {
                return false;
            }
            for (int index = 0; index < root.ChildNodes.Count; ++index)
            {
                if (!ProcessUint16(root.ChildNodes[index], ref curList))
                {
                    return false;
                }
            }

            return retVal;
        }

        private bool ProcessOpListNode(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            List<byte> listOps = new List<byte>();
            byte[] bytes = BitConverter.GetBytes(nitems);
            listOps.AddRange(bytes);
            for(int index = 0; index < nitems; ++index)
            {
                XmlNode? curNode = root.ChildNodes[index];
                if(curNode == null)
                {
                    return false;
                }
                if (curNode.ChildNodes.Count != 3)
                {
                    return false;
                }
                retVal = retVal && ProcessConditionList(curNode.ChildNodes[0], ref listOps);
                retVal = retVal && ProcessUint16(curNode.ChildNodes[1], ref listOps);
                retVal = retVal && ProcessArgsList(curNode.ChildNodes[2], ref listOps);
            }
            curList.AddRange(listOps);

            return retVal;
        }

        private bool ProcessArgsList(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }

            List<byte> listArgs = new List<byte>();
            UInt16 nVal = (UInt16)root.ChildNodes.Count;
            byte[] bytes = BitConverter.GetBytes((UInt16)(nVal * 2));
            listArgs.AddRange(bytes);
            for(int index = 0; index < nVal; ++index)
            {
                retVal = retVal && ProcessUint16(root.ChildNodes[index], ref listArgs);
            }

            curList.AddRange(listArgs);
            return retVal;
        }

        private bool ProcessConditionList(XmlNode? root, ref List<byte> curList)
        {
            bool retVal = true;

            if (root == null)
            {
                return false;
            }
            UInt16 nitems = (UInt16)root.ChildNodes.Count;
            List<byte> listConditions = new List<byte>();
            byte[] bytes = BitConverter.GetBytes(nitems);
            listConditions.AddRange(bytes);

            for(int index = 0; index < nitems; ++index)
            {
                XmlNode? curNode = root.ChildNodes[index];
                if (curNode == null)
                {
                    return false;
                }
                if (curNode.ChildNodes.Count != 3)
                {
                    return false;
                }
                retVal = retVal && ProcessUint16(curNode.ChildNodes[0], ref listConditions);
                retVal = retVal && ProcessGetSceneConditions(curNode.ChildNodes[1], ref listConditions);
                retVal = retVal && ProcessUint16(curNode.ChildNodes[2], ref listConditions);
            }

            curList.AddRange(listConditions);

            return retVal;
        }

        private bool ProcessHeaderNode(ref List<byte> curList, UInt32 filesize)
        {
            byte[] headerbytes = Encoding.ASCII.GetBytes("SDS:");
            curList.AddRange(headerbytes);

            // Temporarily add in size.  We can come back to this later
            UInt32 size1 = filesize + 5;
            byte[] size1bytes = BitConverter.GetBytes(size1);
            curList.AddRange(size1bytes);
            curList.Add(0); // compression
            UInt32 size2 = filesize;
            byte[] size2bytes = BitConverter.GetBytes(size2);
            curList.AddRange(size2bytes);
            return true;
        }

        private bool ProcessGetSceneConditions(XmlNode? root, ref List<byte> curList)
        {
            if (root == null)
            {
                return false;
            }
            UInt16 num = 0;
            for(int index = 0; index < root.ChildNodes.Count; ++index)
            {
                XmlNode? curNode = root.ChildNodes[index];
                if (curNode == null)
                {
                    return false;
                }
                UInt16 tempval =  getSceneVal(curNode.InnerText);
                if(tempval > 0x80)
                {
                    return false;
                }
                num |= tempval;
            }
            List<byte> listConditions = new List<byte>();
            byte[] bytes = BitConverter.GetBytes(num);
            curList.AddRange(bytes);

            return true;
        }

        private UInt16 getSceneVal(string strText)
        {
            if (strText == "kSceneCondNone")
            {
                return 0;
            }
            else if (strText == "kSceneCondLessThan")
            {
                return 1;
            }
            else if(strText == "kSceneCondEqual")
            {
                return 2;
            }
            else if(strText == "kSceneCondNegate")
            {
                return 4;
            }
            else if(strText == "kSceneCondAbsVal")
            {
                return 8;
            }
            else if(strText == "kSceneCondOr")
            {
                return 0x10;
            }
            else if(strText == "kSceneCondNeedItemSceneNum")
            {
                return 0x20;
            }
            else if(strText == "kSceneCondNeedItemQuality")
            {
                return 0x40;
            }
            else if(strText == "kSceneCondSceneState")
            {
                return 0x80;
            }
            return 0;
        }

        private bool ProcessUint32(XmlNode? root, ref List<byte> curList)
        {
            if (root == null)
            {
                return false;
            }
            List<byte> listMagicNumber = new List<byte>();
            if (root.InnerText.Length <= 0)
            {
                return false;
            }
            UInt32 expectedNum;
            if (UInt32.TryParse(root.InnerText, out expectedNum))
            {
                byte[] bytes = BitConverter.GetBytes(expectedNum);
                listMagicNumber.AddRange(bytes);
            }
            else
            {
                return false;
            }

            curList.AddRange(listMagicNumber);

            return true;
        }

        private bool ProcessUint16(XmlNode? root, ref List<byte> curList)
        {
            if(root == null)
            {
                return false;
            }
            List<byte> listMagicNumber = new List<byte>();
            if (root.InnerText.Length <= 0)
            {
                return false;
            }
            UInt16 expectedNum;
            if (UInt16.TryParse(root.InnerText, out expectedNum))
            {
                byte[] bytes = BitConverter.GetBytes(expectedNum);
                listMagicNumber.AddRange(bytes);
            }
            else
            {
                return false;
            }

            curList.AddRange(listMagicNumber);

            return true;
        }

        private bool ProcessVersionNode(XmlNode root, ref List<byte> curList, out float version)
        {
            version = 0;
            List<byte> listVersion = new List<byte>();
            if (root.InnerText.Length <= 0)
            {
                return false;
            }
            byte[] versionbytes = Encoding.ASCII.GetBytes(root.InnerText);
            version = float.Parse(root.InnerText, CultureInfo.InvariantCulture.NumberFormat);
            listVersion.AddRange(versionbytes);
            listVersion.Add(0);
            curList.AddRange(listVersion);
            return true;
        }
    }
}
