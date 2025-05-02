using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.CodeDom.Compiler;

namespace RF_Tag_Reader
{
    public class Main_cls
    {
#region MainClass Instance
        //--------------------------------------------------------------------------------
        // CVDR_M_CL 클래스 Get으로 캡슐화
        //--------------------------------------------------------------------------------
        static readonly object padlock = new object();
        static Main_cls m_Main_Instance;
        public static Main_cls Instance
        {
            get
            {
                lock (padlock)
                {
                    return m_Main_Instance;
                }
            }
        }
#endregion

        public class DebugCls()
        {
            public bool Debug1;
            public bool Debug2;
            public bool Debug3;
            public bool Debug4;
            public uint SerialID;
        }
        public DebugCls debuglist = new DebugCls();
        public SerialPort Serial = new SerialPort();
        private MainForm Form;

        public string[] RFTapTypeStr = {"RI-TRP-W9WK", "RI-TRP-M9WK","ETC", "None", "Timeout"};
        public enum RFTagTypeList {W9WK, M9WK, ETC, NONE, TIMEOUT}
        public enum RFTagPageList {Page01, Page02, Page03, Page08, Page09, Page10, Page11, Page12}
        private string RxBufStr="";
        public UInt16 UseTime = 0;
        public RFTagTypeList RFTag = new RFTagTypeList();
        private string W9WK_ReadDataStr = "";
        public byte[] W9Wk_ReadData = new byte[10];
        public byte[,] M9WK_PageReadData = new byte[8,5];
        string[] M9WK_PageReadDataStr = new string[8];
        private byte[] PageReadIndex = {0x04, 0x08, 0x0c, 0x20, 0x24, 0x28, 0x2c,0x30};    // {01, 02, 03, 08, 09, 10, 11, 12}
        private byte[] PageWriteIndex = {0x05, 0x09, 0x0d, 0x21, 0x25, 0x29, 0x2d,0x31};    // {01, 02, 03, 08, 09, 10, 11, 12}
        private string[] PageIndexstr = {"Page01","Page02","Page03","Page08","Page09","Page10","Page11","Page12" };
        public bool TestFlag = false;
        public bool SerialBitClr = false, SeribitSet = false;
        public int TotalCnt = 0, SuccessCnt = 0;
        public String LogPath = @"C:\Wonik\RFID_Log";
        public class SerialIDCls()
        {
            public string LogFilePath;
            public DateTime SuppliedDate;
            public string LampSerial = "";
            public byte[] RFSerial = new byte[2];
            public string RFSerialBinary = "0000 0000 0000 0000";
            public RFTagTypeList RFType;
        }
        
        public List<SerialIDCls> SerialListData = new List<SerialIDCls>();

        public Main_cls()
        {
            m_Main_Instance = this;

            LogReadFunc(12);
            Form = new MainForm();
            Form.Show();
        }

        public (bool, string) LogDataSaveFunc(string Description, string FilePath)
        {
            SerialIDCls tempcls = new SerialIDCls();
            tempcls.LogFilePath = Path.GetFileName(FilePath); 
            tempcls.LampSerial = Path.GetFileNameWithoutExtension(FilePath);
            tempcls.SuppliedDate = File.GetCreationTime(FilePath);
            string[] TextLine = Description.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            bool readfail = true;
            bool readfail2 = true;
            
            foreach (string line in TextLine)
            {
                // RF Tag Type 찾기
                if(readfail)
                {
                    if (line.Contains("RF Tag Type"))
                    {
                        int start = line.IndexOf('[') + 1;
                        int end = line.IndexOf(']');
                        if (start > 0 && end > start)
                        {
                            string tagType = line.Substring(start, end - start);
                            if(tagType == "RI-TRP-W9WK")    tempcls.RFType = RFTagTypeList.W9WK;
                            else if(tagType == "RI-TRP-M9WK")   tempcls.RFType = RFTagTypeList.M9WK;
                            else    tempcls.RFType = RFTagTypeList.NONE;
                            readfail = false;
                        }
                    }
                }

                if(readfail2)
                {
                    if(tempcls.RFType == RFTagTypeList.M9WK)
                    {
                        if (line.StartsWith("Page08"))      // Page08 찾기
                        {
                            int colonIndex = line.IndexOf(':');
                            if (colonIndex != -1)
                            {
                                try
                                {
                                    string hexPart = line.Substring(colonIndex + 1).Trim();
                                    string[] page08Bytes = hexPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                    tempcls.RFSerial[0] = byte.Parse(page08Bytes[6],System.Globalization.NumberStyles.HexNumber);
                                    tempcls.RFSerial[1] = byte.Parse(page08Bytes[5],System.Globalization.NumberStyles.HexNumber);
                                    readfail2 = false;
                                }
                                catch   {   }
                            }
                        }
                    }
                    else if(tempcls.RFType == RFTagTypeList.W9WK)
                    {
                        if (line.Contains("[RX]"))
                        {
                            int colonIndex = line.IndexOf(':');
                            if (colonIndex != -1)
                            {
                                string hexPart = line.Substring(colonIndex + 1).Trim();
                                string[] bytes = hexPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                tempcls.RFSerial[0] = byte.Parse(bytes[4],System.Globalization.NumberStyles.HexNumber);
                                tempcls.RFSerial[1] = byte.Parse(bytes[3],System.Globalization.NumberStyles.HexNumber);
                                readfail2 = false;
                            }
                        }
                    }
                }
            }
            if(!readfail && !readfail2)
            {
                tempcls.RFSerialBinary = ByteArrayToBinaryString(tempcls.RFSerial);
                SerialListData.Add(tempcls);
            }
            return (true, "");
        }
        public (bool, string) LogReadFunc(int ReadMonth)
        {
            SerialListData.Clear();

            if(!Directory.Exists(LogPath))  {   return (false, "경로없음"); }
                        
            for(int i =0;i<ReadMonth;i++)
            {
                string subFolderName = LogPath + @"\" + DateTime.Now.AddMonths(i+1-ReadMonth).ToString("yyyy_MM");
                if(Directory.Exists(subFolderName))
                {   
                    string[] files = Directory.GetFiles(subFolderName, "*.log").OrderBy(f => File.GetLastWriteTime(f)).ToArray();

                    for(int j=0;j<files.Length;j++)
                    {
                        SerialIDCls tempcls = new SerialIDCls();
                        tempcls.LogFilePath = Path.GetFullPath(files[j]); 
                        tempcls.LampSerial = Path.GetFileNameWithoutExtension(files[j]);
                        tempcls.SuppliedDate = File.GetCreationTime(files[j]);
                        string[] TextLine = File.ReadAllLines(files[j]);
                        bool readfail = true;
                        bool readfail2 = true;
                        
                        foreach (string line in TextLine)
                        {
                            // RF Tag Type 찾기
                            if(readfail)
                            {
                                if (line.Contains("RF Tag Type"))
                                {
                                    int start = line.IndexOf('[') + 1;
                                    int end = line.IndexOf(']');
                                    if (start > 0 && end > start)
                                    {
                                        string tagType = line.Substring(start, end - start);
                                        if(tagType == "RI-TRP-W9WK")    tempcls.RFType = RFTagTypeList.W9WK;
                                        else if(tagType == "RI-TRP-M9WK")   tempcls.RFType = RFTagTypeList.M9WK;
                                        else    tempcls.RFType = RFTagTypeList.NONE;
                                        readfail = false;
                                    }
                                }
                            }

                            if(readfail2)
                            {
                                if(tempcls.RFType == RFTagTypeList.M9WK)
                                {
                                    if (line.StartsWith("Page08"))      // Page08 찾기
                                    {
                                        int colonIndex = line.IndexOf(':');
                                        if (colonIndex != -1)
                                        {
                                            try
                                            {
                                                string hexPart = line.Substring(colonIndex + 1).Trim();
                                                string[] page08Bytes = hexPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                                tempcls.RFSerial[0] = byte.Parse(page08Bytes[6],System.Globalization.NumberStyles.HexNumber);
                                                tempcls.RFSerial[1] = byte.Parse(page08Bytes[5],System.Globalization.NumberStyles.HexNumber);
                                                readfail2 = false;
                                            }
                                            catch   {   }
                                        }
                                    }
                                }
                                else if(tempcls.RFType == RFTagTypeList.W9WK)
                                {
                                    if (line.Contains("[RX]"))
                                    {
                                        int colonIndex = line.IndexOf(':');
                                        if (colonIndex != -1)
                                        {
                                            string hexPart = line.Substring(colonIndex + 1).Trim();
                                            string[] bytes = hexPart.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                            tempcls.RFSerial[0] = byte.Parse(bytes[4],System.Globalization.NumberStyles.HexNumber);
                                            tempcls.RFSerial[1] = byte.Parse(bytes[3],System.Globalization.NumberStyles.HexNumber);
                                            readfail2 = false;
                                        }
                                    }
                                }
                            }
                       }
                       if(!readfail && !readfail2)
                       {
                            tempcls.RFSerialBinary = ByteArrayToBinaryString(tempcls.RFSerial);
                            SerialListData.Add(tempcls);
                       }
                    }
                }
            }

            return (true, "읽기성공");
        }
        public string ByteArrayToHexString(byte[] byteArray, byte Length)
        {
            StringBuilder hex = new StringBuilder(Length * 2);
            for(int i =0;i<Length;i++)
            {
                hex.AppendFormat("{0:X2} ", byteArray[i]);
            }
            // foreach (byte b in byteArray)
            // {
            //     hex.AppendFormat("{0:X2}\t", b);
            // }
            return hex.ToString();
        }
        public string ToBinaryString(byte value)
        {
            string binary = Convert.ToString(value, 2).PadLeft(8, '0'); // 8자리 2진수
            return binary.Insert(4, " "); // 중간에 공백 추가
        }
        public string ByteArrayToBinaryString(byte[] data)
        {
            if (data.Length != 2) return "0000 0000 0000 0000";
            return ToBinaryString(data[0]) + " " + ToBinaryString(data[1]);
        }
        public bool W9WKWriteData(byte[] writeData)
        {
            byte[] TxBuf = new byte[100];
            byte[] RxBuf = new byte[100];
            byte TxIndex =0, RxIndex =0;
            byte Temp;
            bool Readwait = false;
            bool Result = false;
            DateTime ReadTimeout = DateTime.Now;

            // writeData[2] = (byte)(Settime & 0x00ff);
            // writeData[3] = (byte)((Settime & 0xff00)>>8); 

            TxIndex = 0;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0x0D;
            TxBuf[TxIndex++] = 0x80;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0x15;
            TxBuf[TxIndex++] = 0xBB;
            TxBuf[TxIndex++] = 0xEB;
            TxBuf[TxIndex++] = writeData[7];
            TxBuf[TxIndex++] = writeData[6];
            TxBuf[TxIndex++] = writeData[5];
            TxBuf[TxIndex++] = writeData[4];
            TxBuf[TxIndex++] = writeData[3];
            TxBuf[TxIndex++] = writeData[2];
            TxBuf[TxIndex++] = writeData[1];
            TxBuf[TxIndex++] = writeData[0];
            Temp = XORBCC(TxBuf, TxIndex);
            TxBuf[TxIndex++] = Temp;

            RxIndex = 0;
            Serial.DiscardInBuffer();
            Serial.Write(TxBuf,0,TxIndex);
            Readwait = true;
            ReadTimeout = DateTime.Now;

            Thread.Sleep(10);
            while(Readwait)
            {
                while(Serial.BytesToRead > 0)   RxBuf[RxIndex++] = (byte)Serial.ReadByte();
                if(RxIndex > 2)
                {
                    if(RxBuf[RxIndex-1] == XORBCC(RxBuf,(byte)(RxIndex-1)))
                    {
                        Readwait = false;
                        if(RxIndex >4)  Result = true;
                        RxBufStr = ByteArrayToHexString(RxBuf, RxIndex);
                    }
                }
                if((DateTime.Now - ReadTimeout).TotalMilliseconds > 500) 
                {
                    Readwait = false;
                    RxBufStr = ByteArrayToHexString(RxBuf, RxIndex);
                }
                Thread.Sleep(1);
            }
            return Result;
        }
        public bool RFTagWrite(ref RichTextBox richtextbox, int SettingTime)
        {
            bool ReadError, ErrorFlag;
            RFTagTypeList RFType;
            int UseTime;
            (ReadError,ErrorFlag, RFType, UseTime) = ReadTageData(ref richtextbox);
            if((ReadError)||((RFType != RFTagTypeList.W9WK)&&(RFType != RFTagTypeList.M9WK)))
            {
                richtextbox.BackColor = Color.FromArgb(255,240,240);
                richtextbox.Text += "Data Read Fail!!!";
                return false;
            } 
            int[] startingIdx = new int[4];
            byte[] M9WK_Buff1 = {0x64, 0x00,0x64,0x00};
            byte[] M9WK_Buff2 = {0x00, 0x00, 0x64};
            byte[] W9WK_Buf = {0x00, 0x64, 0x00, 0x64};
            byte[] writebyte = new byte[8];
            richtextbox.BackColor = Color.FromArgb(255,255,255);

            switch(RFType)
            {
                case RFTagTypeList.W9WK:
                    richtextbox.Text += "\r\nTime " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")+" [DataWrite]\r\n";

                    if(debuglist.Debug4)
                    {
                        writebyte[0] = (byte)((debuglist.SerialID & 0xff00)>>8);
                        writebyte[1] = (byte)((debuglist.SerialID & 0x00ff)>>0);

                        richtextbox.Text += "Serial ID = 0x" + (W9Wk_ReadData[0]<<8|W9Wk_ReadData[1]).ToString("x4") + " --> 0x" + debuglist.SerialID.ToString("x4") + "\r\n";
                    }
                    else
                    {
                        writebyte[0] = W9Wk_ReadData[0];
                        if(SeribitSet)
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1] | 0x80);
                        }
                        else if(SerialBitClr)
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1] & 0x3f);
                        }
                        else
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1]);
                        }
                    }
                    writebyte[2] = (byte)(SettingTime & 0x00ff);
                    writebyte[3] = (byte)((SettingTime & 0xff00)>>8);
                    richtextbox.Text += "Setting Time = " + SettingTime.ToString() + "Hour\r\n";
                    if(writebyte[1] != (byte)(W9Wk_ReadData[1]))
                    {
                        richtextbox.Text += "Data 1 0x" + W9Wk_ReadData[1].ToString("x2") + "--> 0x" + writebyte[1].ToString("x2") + "\r\n";
                    }
                    

                    for(int i =0;i<W9WK_Buf.Length;i++)
                    {
                        writebyte[7-i] = W9WK_Buf[i];
                        if(W9Wk_ReadData[7-i] != W9WK_Buf[i])
                        {
                            richtextbox.Text += "Data "+ (i+5).ToString() + " 0x" + W9Wk_ReadData[7-i].ToString("X2") +" --> 0x" + W9WK_Buf[i].ToString("X2") + "\r\n";
                        }
                    }
                    
                    if(!W9WKWriteData(writebyte))
                    {
                         richtextbox.Text += "데이터 쓰기 실패!!";
                         richtextbox.BackColor = Color.FromArgb(255,240,240);
                         return false;
                    }
                    richtextbox.Text += "\r\nWrite Complete!!\r\n\r\n"; 

                    (ReadError,ErrorFlag, RFType, UseTime) =ReadTageData(ref richtextbox);  
                    richtextbox.Text += "\r\n";             
                    if(ReadError) 
                    {
                        richtextbox.Text += "Read Fail!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else if(ErrorFlag)
                    {
                        richtextbox.Text += "Write Error!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    } 
                    else if(UseTime != SettingTime) 
                    {
                        richtextbox.Text +=  "장입에러!!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else
                    {
                        richtextbox.Text += "Write Success\r\n";
                        richtextbox.BackColor = Color.FromArgb(240,255,240);
                        return true;
                    } 
                break;

                case RFTagTypeList.M9WK:
                    richtextbox.Text += "\r\nTime " + DateTime.Now.ToString("YYYY/MM/dd HH:mm:ss.ff")+" [DataWrite]\r\n";

                    byte writeidx = 0;
                    
                    writebyte[writeidx++] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,0]);
                    writebyte[writeidx++] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1]);
                    writebyte[writeidx++] = (byte)(SettingTime & 0x00ff);
                    writebyte[writeidx++] = (byte)((SettingTime & 0xff00)>>8);
                    writebyte[writeidx++] = 0x88;

                    if(debuglist.Debug4)
                    {
                        writebyte[0] = (byte)((debuglist.SerialID & 0xff00)>>8);
                        writebyte[1] = (byte)((debuglist.SerialID & 0x00ff)>>0);

                        richtextbox.Text += "Serial ID = 0x" + (M9WK_PageReadData[(int)RFTagPageList.Page08,0]<<8|M9WK_PageReadData[(int)RFTagPageList.Page08,1]).ToString("x4") + " --> 0x" + debuglist.SerialID.ToString("x4") + "\r\n";
                    }
                    else
                    {
                        if(SeribitSet)
                        {
                            writebyte[1] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1] | 0x80);
                        }
                        else if(SerialBitClr)
                        {
                            writebyte[1] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1] & 0x3f);
                        }
                        else
                        {
                            writebyte[1] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1]);
                        }
                    }
                    if(debuglist.Debug1)
                    {
                        writebyte[4] = 0x32;
                    }
                    

                    richtextbox.Text += "Setting Time = " + SettingTime.ToString() + "Hour\r\n";

                    if(writebyte[1] != (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1]))
                    {
                        if(!debuglist.Debug4)
                            richtextbox.Text += "Page08 Data 1 0x" + M9WK_PageReadData[(int)RFTagPageList.Page08,1].ToString("x2") + "--> 0x" + writebyte[1].ToString("x2") + "\r\n";
                    }
                    if(M9WK_PageReadData[(int)RFTagPageList.Page08,4] != writebyte[4])
                    {
                        richtextbox.Text += "Page08 Data 5 0x" + M9WK_PageReadData[(int)RFTagPageList.Page08,4].ToString("x2") + "--> 0x" + writebyte[4].ToString("x2") + "\r\n";
                    }

                    if(!M9WK_PageWrite(RFTagPageList.Page08,writebyte))
                    {
                         richtextbox.Text += "Page 08 쓰기 실패!!";
                         richtextbox.BackColor = Color.FromArgb(255,240,240);
                         return false;
                    }
                    writeidx =0;
                    writebyte[writeidx++] = M9WK_Buff1[0];
                    writebyte[writeidx++] = M9WK_Buff1[1];
                    writebyte[writeidx++] = M9WK_Buff1[2];
                    writebyte[writeidx++] = M9WK_Buff1[3];
                    writebyte[writeidx++] = 0x32;
                    if(debuglist.Debug2){   writebyte[4] = 0x88;    }
                    for(int i =0;i<M9WK_Buff1.Count();i++)
                    {
                        if(M9WK_PageReadData[(int)RFTagPageList.Page09,i] != M9WK_Buff1[i])
                        {
                            richtextbox.Text += "Page09 Data "+ (i+1).ToString() + " 0x" + M9WK_PageReadData[(int)RFTagPageList.Page09,i].ToString("X2") +" --> 0x" + M9WK_Buff1[i].ToString("X2") + "\r\n";
                        }
                    }
                    if(M9WK_PageReadData[(int)RFTagPageList.Page09,4] != 0x32)
                    {
                        richtextbox.Text += "Page09 Data 5 0x" + M9WK_PageReadData[(int)RFTagPageList.Page09,4].ToString("X2") +" --> 0x32\r\n";
                    }
                     if(!M9WK_PageWrite(RFTagPageList.Page09,writebyte))
                    {
                         richtextbox.Text += "Page09 쓰기 실패!!";
                         richtextbox.BackColor = Color.FromArgb(255,240,240);
                         return false;
                    }

                    writeidx =0;
                    writebyte[writeidx++] = M9WK_Buff2[0];
                    writebyte[writeidx++] = M9WK_Buff2[1];
                    writebyte[writeidx++] = M9WK_Buff2[2];
                    writebyte[writeidx++] = SerialIDCal(M9WK_PageReadData[(int)RFTagPageList.Page03,2]);
                    //writebyte[writeidx++] = 0x47;
                    writebyte[writeidx++] = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page03,1] ^ 0x65);      // 연산
                    if(debuglist.Debug3)    {   writebyte[4] = 0x48;    }

                    for(int i =0;i<M9WK_Buff2.Count();i++)
                    {
                        if(M9WK_PageReadData[(int)RFTagPageList.Page10,i] != M9WK_Buff2[i])
                        {
                            richtextbox.Text += "Page10 Data "+ (i+1).ToString() + " 0x" + M9WK_PageReadData[(int)RFTagPageList.Page10,i].ToString("X2") +" --> 0x" + M9WK_Buff2[i].ToString("X2") + "\r\n";
                        }
                    }

                    if(M9WK_PageReadData[(int)RFTagPageList.Page10,3] != SerialIDCal(M9WK_PageReadData[(int)RFTagPageList.Page03,2]))
                    {
                        richtextbox.Text += "Page10 Data 4 0x" + M9WK_PageReadData[(int)RFTagPageList.Page10,3].ToString("X2") +" --> 0x" + SerialIDCal(M9WK_PageReadData[(int)RFTagPageList.Page03,2]).ToString("x2") + "\r\n";
                    }

                    if(M9WK_PageReadData[(int)RFTagPageList.Page10,4] != 0x47)
                    {
                        richtextbox.Text += "Page10 Data 5 0x" + M9WK_PageReadData[(int)RFTagPageList.Page10,4].ToString("X2") +" --> 0x47\r\n";
                    }
                     if(!M9WK_PageWrite(RFTagPageList.Page10,writebyte))
                    {
                         richtextbox.Text += "Page10 쓰기 실패!!";
                         richtextbox.BackColor = Color.FromArgb(255,240,240);
                         return false;
                    }

                    richtextbox.Text += "\r\nWrite Complete!!\r\n\r\n"; 
                    
                    (ReadError,ErrorFlag, RFType, UseTime) =ReadTageData(ref richtextbox);  
                    richtextbox.Text += "\r\n";             
                    if(ReadError) 
                    {
                        richtextbox.Text += "Read Fail!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else if(ErrorFlag)
                    {
                        richtextbox.Text += "Write Error!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    } 
                    else if(UseTime != SettingTime) 
                    {
                        richtextbox.Text +=  "장입에러!!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else
                    {
                        richtextbox.Text += "Write Success\r\n";
                        richtextbox.BackColor = Color.FromArgb(240,255,240);
                        return true;
                    } 
                     
                break;
                default:
                    richtextbox.Clear();
                    richtextbox.Text += "Error";
                    richtextbox.BackColor = Color.FromArgb(255,240,240);
                    return false;
                break;
            }

            richtextbox.BackColor = (ErrorFlag)?Color.FromArgb(255,240,240):Color.FromArgb(240,255,240);
            
            return ReadError;
        }

        public bool RFTagSuccessWrite(ref RichTextBox richtextbox, int SettingTime)
        {
            bool ReadError, ErrorFlag;
            RFTagTypeList RFType;
            int UseTime;
            (ReadError,ErrorFlag, RFType, UseTime) = ReadTageData(ref richtextbox);
            if((ReadError)||((RFType != RFTagTypeList.W9WK)&&(RFType != RFTagTypeList.M9WK)))
            {
                richtextbox.BackColor = Color.FromArgb(255,240,240);
                richtextbox.Text += "Data Read Fail!!!";
                return false;
            } 
            int[] startingIdx = new int[4];
            byte[] M9WK_Buff1 = {0x64, 0x00,0x64,0x00};
            byte[] M9WK_Buff2 = {0x00, 0x00, 0x64};
            byte[] W9WK_Buf = {0x00, 0x64, 0x00, 0x64};
            byte[] writebyte = new byte[8];
            richtextbox.BackColor = Color.FromArgb(255,255,255);

            switch(RFType)
            {
                /*case RFTapTypeList.W9WK:
                    richtextbox.Text += "\r\nTime " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")+" [DataWrite]\r\n";

                    if(debuglist.Debug4)
                    {
                        writebyte[0] = (byte)((debuglist.SerialID & 0xff00)>>8);
                        writebyte[1] = (byte)((debuglist.SerialID & 0x00ff)>>0);

                        richtextbox.Text += "Serial ID = 0x" + (W9Wk_ReadData[0]<<8|W9Wk_ReadData[1]).ToString("x4") + " --> 0x" + debuglist.SerialID.ToString("x4") + "\r\n";
                    }
                    else
                    {
                        writebyte[0] = W9Wk_ReadData[0];
                        if(SeribitSet)
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1] | 0x80);
                        }
                        else if(SerialBitClr)
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1] & 0x3f);
                        }
                        else
                        {
                            writebyte[1] = (byte)(W9Wk_ReadData[1]);
                        }
                    }
                    writebyte[2] = (byte)(SettingTime & 0x00ff);
                    writebyte[3] = (byte)((SettingTime & 0xff00)>>8);
                    richtextbox.Text += "Setting Time = " + SettingTime.ToString() + "Hour\r\n";
                    if(writebyte[1] != (byte)(W9Wk_ReadData[1]))
                    {
                        richtextbox.Text += "Data 1 0x" + W9Wk_ReadData[1].ToString("x2") + "--> 0x" + writebyte[1].ToString("x2") + "\r\n";
                    }
                    

                    for(int i =0;i<W9WK_Buf.Length;i++)
                    {
                        writebyte[7-i] = W9WK_Buf[i];
                        if(W9Wk_ReadData[7-i] != W9WK_Buf[i])
                        {
                            richtextbox.Text += "Data "+ (i+5).ToString() + " 0x" + W9Wk_ReadData[7-i].ToString("X2") +" --> 0x" + W9WK_Buf[i].ToString("X2") + "\r\n";
                        }
                    }
                    
                    if(!W9WKWriteData(writebyte))
                    {
                         richtextbox.Text += "데이터 쓰기 실패!!";
                         richtextbox.BackColor = Color.FromArgb(255,240,240);
                         return false;
                    }
                    richtextbox.Text += "\r\nWrite Complete!!\r\n\r\n"; 

                    (ReadError,ErrorFlag, RFType, UseTime) =ReadTageData(ref richtextbox);  
                    richtextbox.Text += "\r\n";             
                    if(ReadError) 
                    {
                        richtextbox.Text += "Read Fail!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else if(ErrorFlag)
                    {
                        richtextbox.Text += "Write Error!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    } 
                    else if(UseTime != SettingTime) 
                    {
                        richtextbox.Text +=  "장입에러!!!";
                        richtextbox.BackColor = Color.FromArgb(255,240,240);
                        return false;
                    }
                    else
                    {
                        richtextbox.Text += "Write Success\r\n";
                        richtextbox.BackColor = Color.FromArgb(240,255,240);
                        return true;
                    } 
                break;
*/
                case RFTagTypeList.M9WK:
                    DateTime WriteTimeStamp = DateTime.Now;
                    int DebugTime = 0x0000;
                    byte SerialID_0 = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,0]);
                    byte SerialID_1 = (byte)(M9WK_PageReadData[(int)RFTagPageList.Page08,1]);
                    richtextbox.Text += "\r\nTime " + DateTime.Now.ToString("YYYY/MM/dd HH:mm:ss.ff")+" [DataWrite]\r\n";
                    TotalCnt = 0;
                    SuccessCnt = 0;
                    for(int i =0;i<50;i++)
                    {
                        bool FailFlag = false;
                        byte writeidx = 0;
                        
                        writebyte[writeidx++] = SerialID_0;
                        writebyte[writeidx++] = SerialID_1;
                        writebyte[writeidx++] = (byte)(DebugTime & 0x00ff);
                        writebyte[writeidx++] = (byte)((DebugTime & 0xff00)>>8);
                        writebyte[writeidx++] = 0x88;

                        if(!M9WK_PageWrite(RFTagPageList.Page08,writebyte))
                        {
                            richtextbox.Text += "쓰기 실패!! [" + DebugTime.ToString() + "]\r\n";
                            richtextbox.BackColor = Color.FromArgb(255,240,240);
                            FailFlag = true;
                        }
                        Thread.Sleep(50);
                        /*if(ReadRFTagPage(RFTagPageList.Page08, RFTapTypeList.M9WK) != RFTapTypeList.M9WK)
                        {
                            richtextbox.Text += "읽기 실패!! [" + DebugTime.ToString() + "]\r\n";
                            richtextbox.BackColor = Color.FromArgb(255,240,240);
                            FailFlag = true;
                        }*/
                        ReadRFTagPage(RFTagPageList.Page08, RFTagTypeList.M9WK);
                        richtextbox.Text += M9WK_PageReadDataStr[(int)RFTagPageList.Page08] + "\r\n";
                        if((byte)M9WK_PageReadData[(int)RFTagPageList.Page08,0] != SerialID_0)  FailFlag = true;
                        if((byte)M9WK_PageReadData[(int)RFTagPageList.Page08,1] != SerialID_1)  FailFlag = true;
                        if((byte)M9WK_PageReadData[(int)RFTagPageList.Page08,2] != (byte)(DebugTime & 0x00ff))  FailFlag = true;
                        if((byte)M9WK_PageReadData[(int)RFTagPageList.Page08,3] != (byte)((DebugTime & 0xff00)>>8))  FailFlag = true;
                        if((byte)M9WK_PageReadData[(int)RFTagPageList.Page08,4] != 0x88)  FailFlag = true;

                        TotalCnt ++;
                        if(!FailFlag)
                        {
                            SuccessCnt ++;
                        }
                        DebugTime = DebugTime + 257;
                        Thread.Sleep(50);
                    }

                    richtextbox.Text += String.Format("반복 Test 완료 {0}/{1},{2}\r\n",TotalCnt,SuccessCnt,((double)(SuccessCnt/TotalCnt) * 100).ToString("0.00"));
                    richtextbox.BackColor = Color.FromArgb(255,240,240);
                    
                break;
                default:
                    richtextbox.Clear();
                    richtextbox.Text += "Error";
                    richtextbox.BackColor = Color.FromArgb(255,240,240);
                    return false;
                break;
            }

            richtextbox.BackColor = (ErrorFlag)?Color.FromArgb(255,240,240):Color.FromArgb(240,255,240);
            
            return ReadError;
        }

        private byte SerialIDCal(byte data)
        {
            byte temp = 0;
            byte upper = (byte)((data & 0xf0)>>4);
            byte lower = (byte)((data & 0x0f)>>0);
            upper = (byte)(~(upper ^ 0x0d));
            lower = (byte)(lower + 0x0a);
            temp = (byte)((((upper & 0x0f)<<4) | (lower & 0x0f)) + 0x00);
            return temp;    
        }
        public bool M9WK_PageWrite(RFTagPageList pageList, byte[] WriteData)
        {
             byte[] TxBuf = new byte[100];
            byte[] RxBuf = new byte[100];
            byte TxIndex =0, RxIndex =0, writeidx = 0, tempIdx = 0;
            byte Temp;
            bool Readwait = false;
            bool Result = false;
            DateTime ReadTimeout = DateTime.Now;

            byte[] TempBuf = new byte[6];
            TempBuf[tempIdx++] = PageWriteIndex[(int)pageList];
            TempBuf[tempIdx++] = WriteData[writeidx++];
            TempBuf[tempIdx++] = WriteData[writeidx++];
            TempBuf[tempIdx++] = WriteData[writeidx++];
            TempBuf[tempIdx++] = WriteData[writeidx++];
            TempBuf[tempIdx++] = WriteData[writeidx++];
            
            tempIdx = 0;
            TxIndex = 0;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0x15;
            TxBuf[TxIndex++] = 0xE8;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0x32;
            TxBuf[TxIndex++] = 0x0F;
            TxBuf[TxIndex++] = 0xAA;
            TxBuf[TxIndex++] = 0x00;
            TxBuf[TxIndex++] = 0x4A;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0xE0;
            TxBuf[TxIndex++] = 0x01; 
            TxBuf[TxIndex++] = 0x08;
            TxBuf[TxIndex++] = 0x02;
            TxBuf[TxIndex++] = 0x08;
            TxBuf[TxIndex++] = TempBuf[tempIdx++];
            TxBuf[TxIndex++] = TempBuf[tempIdx++];    
            TxBuf[TxIndex++] = TempBuf[tempIdx++];
            TxBuf[TxIndex++] = TempBuf[tempIdx++];
            TxBuf[TxIndex++] = TempBuf[tempIdx++];
            TxBuf[TxIndex++] = TempBuf[tempIdx++];
            
            TxBuf[TxIndex++] = CRC16CCITT(TempBuf, 6)[0];      
            TxBuf[TxIndex++] = CRC16CCITT(TempBuf, 6)[1];
            Temp = XORBCC(TxBuf, TxIndex);
            TxBuf[TxIndex++] = Temp;

            RxIndex = 0;
            Serial.DiscardInBuffer();
            Serial.Write(TxBuf,0,TxIndex);
            Readwait = true;
            ReadTimeout = DateTime.Now;

            Thread.Sleep(10);
            while(Readwait)
            {
                while(Serial.BytesToRead > 0)   RxBuf[RxIndex++] = (byte)Serial.ReadByte();
                if(RxIndex > 2)
                {
                    if(RxBuf[RxIndex-1] == XORBCC(RxBuf,(byte)(RxIndex-1)))
                    {
                        Readwait = false;
                        if(RxIndex >4)  Result = true;
                        RxBufStr = ByteArrayToHexString(RxBuf, RxIndex);
                    }
                }
                if((DateTime.Now - ReadTimeout).TotalMilliseconds > 500) 
                {
                    Readwait = false;
                    RxBufStr = ByteArrayToHexString(RxBuf, RxIndex);
                }
                Thread.Sleep(1);
            }
            return Result;
        }
        private RFTagTypeList ReadRFTagPage(RFTagPageList pageList, RFTagTypeList rftype)       // RFTag Type 확인
        {
            byte[] TxBuf = new byte[100];
            byte[] RxBuf = new byte[100];
            byte[] CRC_CheckBuf = new byte [10];
            byte TxIndex =0, RxIndex =0;
            byte Temp;
            bool Readwait = false;
            DateTime ReadTimeout = DateTime.Now;

            TxIndex = 0;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = 0x04;
            TxBuf[TxIndex++] = 0x48;
            TxBuf[TxIndex++] = 0x32;
            TxBuf[TxIndex++] = 0x01;
            TxBuf[TxIndex++] = PageReadIndex[(int)pageList];
            Temp = XORBCC(TxBuf, TxIndex);
            TxBuf[TxIndex++] = Temp;

            RxIndex = 0;
            Serial.DiscardInBuffer();
            Serial.Write(TxBuf,0,TxIndex);
            Readwait = true;
            ReadTimeout = DateTime.Now;

            Thread.Sleep(10);
            while(Readwait)
            {
                while(Serial.BytesToRead > 0)   RxBuf[RxIndex++] = (byte)Serial.ReadByte();
                if(RxIndex > 2)
                {
                    if(RxBuf[RxIndex-1] == XORBCC(RxBuf,(byte)(RxIndex-1)))
                    {
                        Readwait = false;
                        if(RxIndex == 18)
                        {
                            if(rftype == RFTagTypeList.NONE)    RFTag = RFTagTypeList.M9WK;
                            for(int a =0;a<5;a++)
                            {
                                M9WK_PageReadData[(int)pageList,a] = RxBuf[5+a];
                            }
                            for(int i =0;i<7;i++)
                            {
                                CRC_CheckBuf[i] = RxBuf[4+i];
                            }
                            if((CRC16CCITT(CRC_CheckBuf,7)[0] != RxBuf[11])||(CRC16CCITT(CRC_CheckBuf,7)[1] != RxBuf[12]))
                            {
                               // rftype = RFTapTypeList.NONE;
                                return RFTagTypeList.NONE;
                            }
                            M9WK_PageReadDataStr[(int)pageList] = ByteArrayToHexString(RxBuf, RxIndex);
                        } 
                        else if(RxIndex == 12) 
                        {
                            if(rftype == RFTagTypeList.NONE)    {    RFTag = RFTagTypeList.W9WK;    }
                            else if(rftype !=  RFTagTypeList.W9WK){  rftype = RFTagTypeList.NONE;}
                            
                            for(int i =0;i<8;i++)   W9Wk_ReadData[i] = RxBuf[3+i];
                            W9WK_ReadDataStr = ByteArrayToHexString(RxBuf, RxIndex);
                        }
                        else if(RxIndex == 4) 
                        {
                            RFTag = RFTagTypeList.NONE; 
                        }
                        else
                        {
                            RFTag = RFTagTypeList.ETC; 
                        } 
                    }
                }
                if((DateTime.Now - ReadTimeout).TotalMilliseconds > 500) 
                {
                    Readwait = false;
                    RFTag = RFTagTypeList.TIMEOUT;
                }
                Thread.Sleep(1);
            }
            return RFTag;
        }

        public (bool, bool, RFTagTypeList, int) ReadTageData(ref RichTextBox richtextbox)
        {
            for(int i =0;i<M9WK_PageReadData.GetLength(0);i++)
            {
                for(int j=0;j<M9WK_PageReadData.GetLength(1);j++)
                {
                    M9WK_PageReadData[i,j] = 0x00;
                    M9WK_PageReadDataStr[i] = "";
                }
            }
            for(int i=0;i<W9Wk_ReadData.GetLength(0);i++)
            {
                W9Wk_ReadData[i]= 0x00;
            }
            W9WK_ReadDataStr = "";

            RFTagTypeList RFType = ReadRFTagPage(RFTagPageList.Page08, RFTagTypeList.NONE);
            string Result = "Time " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ff")+" [DataRead]\r\n";
            int[] startingIdx = new int[4];
            int index;
            bool ErrorFlag = false;
            byte[] M9WK_Buf1 = {0x64, 0x00,0x64, 0x00};
            byte[] M9WK_Buf2 = {0x00, 0x00, 0x64};
            byte[] W9WK_Buf = {0x64, 0x00, 0x64, 0x00};
            bool ReadError = false;

            richtextbox.BackColor = Color.FromArgb(255,255,255);

            switch(RFType)
            {
                case RFTagTypeList.NONE :
                case RFTagTypeList.TIMEOUT :
                case RFTagTypeList.ETC:
                    Result += "RF Tag Type = [" + RFTapTypeStr[(int)RFType] + "]\r\n";
                    richtextbox.Clear();
                    richtextbox.Text = Result;
                    ErrorFlag = true;
                    break;

                case RFTagTypeList.W9WK:
                    UseTime = (ushort)((W9Wk_ReadData[3]<<8) |(W9Wk_ReadData[2]));
                    Result += "RF Tag Type = [" + RFTapTypeStr[(int)RFTag] + "]\r\n";
                    Result += "RF Tag ReadTime = " + UseTime.ToString() + " Hour\r\n";
                    Result += "RF Tag Serial = ";
                    index = richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += W9Wk_ReadData[1].ToString("b8") +"\r\n\r\n";
                    Result += "[RX] : ";
                    startingIdx[0] = Result.Replace("\n","").Length; 
                    Result += W9WK_ReadDataStr + "\r\n";
                    
                    richtextbox.Text += Result;

                    for(int i =0;i<W9WK_Buf.Length;i++)
                    {
                        if(W9Wk_ReadData[4+i] != W9WK_Buf[i])
                        {
                            TextBoxColorChange(ref richtextbox,startingIdx[0] + (7+i)*3, 3,Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                            ErrorFlag = true;
                        }
                    }
                    TextBoxColorChange(ref richtextbox,startingIdx[0] + 5*3, 5,Color.Blue, new Font(richtextbox.Font,FontStyle.Bold));
                    TextBoxColorChange(ref richtextbox,index, 3, Color.Blue, new Font(richtextbox.Font,FontStyle.Bold)); // 시리얼표시 // 시간표시
                break;

                case RFTagTypeList.M9WK:
                    
                    if(ReadRFTagPage(RFTagPageList.Page03, RFType) != RFTagTypeList.M9WK) ReadError = true;
                    if(ReadRFTagPage(RFTagPageList.Page09, RFType) != RFTagTypeList.M9WK) ReadError = true;
                    if(ReadRFTagPage(RFTagPageList.Page10, RFType) != RFTagTypeList.M9WK) ReadError = true;
                    if(ReadRFTagPage(RFTagPageList.Page11, RFType) != RFTagTypeList.M9WK);// ReadError = true;
                    if(ReadRFTagPage(RFTagPageList.Page12, RFType) != RFTagTypeList.M9WK);// ReadError = true;
                    
                   // if((M9WK_PageReadData[(int)RFTagPageList.Page08,1]&0xe0) != 0x80)   ErrorFlag = true;
                    if(M9WK_PageReadData[(int)RFTagPageList.Page08,4]!=0x88)   ErrorFlag = true;
                    if(M9WK_PageReadData[(int)RFTagPageList.Page09,4]!=0x32)   ErrorFlag = true;
                    if(M9WK_PageReadData[(int)RFTagPageList.Page10,4]!=0x47)   ErrorFlag = true;
                    if(SerialIDCal(M9WK_PageReadData[(int)RFTagPageList.Page03,2]) != M9WK_PageReadData[(int)RFTagPageList.Page10,3])   ErrorFlag = true;
                    
                    for(int i =0;i<M9WK_Buf1.Length;i++)
                    {
                        if(M9WK_Buf1[i] != M9WK_PageReadData[(int)RFTagPageList.Page09, i])  ErrorFlag = true;
                    }
                    
                    for(int i =0;i<M9WK_Buf2.Length;i++)
                    {
                        if(M9WK_Buf2[i] != M9WK_PageReadData[(int)RFTagPageList.Page10, i]) ErrorFlag = true;
                    }
                    
                    UseTime = (ushort)((M9WK_PageReadData[(int)RFTagPageList.Page08, 3]<<8)|(M9WK_PageReadData[(int)RFTagPageList.Page08, 2]));
                    
                    Result += "RF Tag Type = [" + RFTapTypeStr[(int)RFTag] + "]\r\n";
                    Result += "RF Tag ReadTime = " + UseTime.ToString() + " Hour\r\n";
                    Result += "RF Tag Serial = " ; 
                    index = richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += M9WK_PageReadData[(int)RFTagPageList.Page08, 1].ToString("b8") +"\r\n\r\n";
                    

                    Result += PageIndexstr[(int)RFTagPageList.Page03] + " [RX] : "; 
                    startingIdx[0] = richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += M9WK_PageReadDataStr[(int)RFTagPageList.Page03] + "\r\n";
                    
                    Result += PageIndexstr[(int)RFTagPageList.Page08] + " [RX] : ";
                    startingIdx[1] =  richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += M9WK_PageReadDataStr[(int)RFTagPageList.Page08] + "\r\n";
                    
                    Result += PageIndexstr[(int)RFTagPageList.Page09] + " [RX] : ";
                    startingIdx[2] =  richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += M9WK_PageReadDataStr[(int)RFTagPageList.Page09] + "\r\n";

                    Result += PageIndexstr[(int)RFTagPageList.Page10] + " [RX] : ";
                    startingIdx[3] =  richtextbox.Text.Length + Result.Replace("\n","").Length;
                    Result += M9WK_PageReadDataStr[(int)RFTagPageList.Page10] + "\r\n";

                    Result += PageIndexstr[(int)RFTagPageList.Page11] + " [RX] : " + M9WK_PageReadDataStr[(int)RFTagPageList.Page11] + "\r\n"; 
                    Result += PageIndexstr[(int)RFTagPageList.Page12] + " [RX] : " + M9WK_PageReadDataStr[(int)RFTagPageList.Page12] + "\r\n";
                    
                    //richtextbox.Clear();
                    richtextbox.Text += Result;

                    TextBoxColorChange(ref richtextbox,index, 3, Color.Blue, new Font(richtextbox.Font,FontStyle.Bold)); // 시리얼표시
                    
                    TextBoxColorChange(ref richtextbox,startingIdx[1] + 7*3, 5,Color.Blue, new Font(richtextbox.Font,FontStyle.Bold)); // 시간표시
                    //TextBoxColorChange(ref richtextbox,startingIdx[1] + 6*3, 3,((M9WK_PageReadData[(int)RFTagPageList.Page08,1]&0xe0) == 0x80)?Color.Green:Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    TextBoxColorChange(ref richtextbox,startingIdx[1] + 9*3, 3,(M9WK_PageReadData[(int)RFTagPageList.Page08,4]==0x88)?Color.Green:Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    TextBoxColorChange(ref richtextbox,startingIdx[2] + 9*3, 3,(M9WK_PageReadData[(int)RFTagPageList.Page09,4]==0x32)?Color.Green:Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    //TextBoxColorChange(ref richtextbox,startingIdx[3] + 9*3, 3,(M9WK_PageReadData[(int)RFTagPageList.Page10,4]==0x47)?Color.Green:Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    TextBoxColorChange(ref richtextbox,startingIdx[3] + 9*3, 3,(M9WK_PageReadData[(int)RFTagPageList.Page10,4]==(M9WK_PageReadData[(int)RFTagPageList.Page03,1]^0x65))?Color.Green:Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    if(SerialIDCal(M9WK_PageReadData[(int)RFTagPageList.Page03,2]) != M9WK_PageReadData[(int)RFTagPageList.Page10,3])
                    {
                        TextBoxColorChange(ref richtextbox,startingIdx[3] + 8*3, 3,Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    }
                    else
                    {
                        TextBoxColorChange(ref richtextbox,startingIdx[3] + 8*3, 3,Color.Green, new Font(richtextbox.Font,FontStyle.Bold)); 
                    }
                    
                    for(int i =0;i<M9WK_Buf1.Length;i++)
                    {
                        if(M9WK_Buf1[i] != M9WK_PageReadData[(int)RFTagPageList.Page09, i])  TextBoxColorChange(ref richtextbox,startingIdx[2] + (5+i)*3, 3,Color.Red, new Font(richtextbox.Font,FontStyle.Bold));
                    }
                    for(int i =0;i<M9WK_Buf2.Length;i++)
                    {
                        if(M9WK_Buf2[i] != M9WK_PageReadData[(int)RFTagPageList.Page10, i]) TextBoxColorChange(ref richtextbox,startingIdx[3] + (5+i)*3, 3,Color.Red, new Font(richtextbox.Font,FontStyle.Bold)); 
                    }
                break;
                default:
                    Result += "Error";
                    ErrorFlag = true;
                    richtextbox.Clear();
                    richtextbox.Text = Result;
                break;
            }

            richtextbox.BackColor = (ErrorFlag)?Color.FromArgb(255,240,240):Color.FromArgb(240,255,240);
            
            return (ReadError, ErrorFlag, RFType, UseTime);
        }

        private void TextBoxColorChange(ref RichTextBox richText, int StartIndex, int length, Color color, Font font)
        {
            richText.Select(StartIndex, length);
            richText.SelectionFont = font;
            richText.SelectionColor = color;
        }
       
        public bool SerialDisconnection()
        {
            try
            {
                if(Serial.IsOpen)   
                {
                    Serial.Close();
                }
                else return false;
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool SerialConnection(string ComPort, int Baud)
        {
            try
            {
                if(Serial.IsOpen)
                {
                    Serial.Close();
                }
                Serial.PortName = ComPort;
                Serial.BaudRate = Baud;
                Serial.Parity = Parity.None;
                Serial.DataBits = 8;
                Serial.StopBits = StopBits.One;

                Serial.Open();
            }
            catch(Exception ex)
            {
                return false;    
            }
            return true;
        }

        private byte XORBCC(byte[] data, byte Length)
        {
            byte sum = data[1];
            for(int i =2;i<Length;i++)
                sum ^= data[i];
            return sum;
        }
        private  byte[] CRC16CCITT(byte[] bytes, int length)     // 0x3791 Start
        {
            ushort polynomial = 0x8408;
            ushort initialValue = 0x3791;
            ushort crc = initialValue;

            for(int id =0;id<length;id++)
            {
                crc ^= bytes[id];
                for (int i = 0; i < 8; ++i)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc = (ushort)((crc >> 1) ^ polynomial);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return BitConverter.GetBytes(crc);
        }
    }
}