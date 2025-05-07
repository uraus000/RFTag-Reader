using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Accessibility;

namespace RF_Tag_Reader
{
    public partial class RepeatForm : Form
    {
        System.Windows.Forms.Timer ti = new System.Windows.Forms.Timer();
        Main_cls m_Main = Main_cls.Instance;

        private DateTime Stamp;
        private Thread RepeatThread;
        public bool RepeatRunFlag = false;
        private String MessageString = "";
        private Color richBackColor = Color.FromArgb(255,255,255);

        uint[] dummyData = {0x0000, 0xffff, 0x0000, 0xffff, 0xf0f0, 0x0f0f, 0xf0f0, 0x0f0f, 
                            0x0101, 0x0202, 0x0404, 0x0808, 0x1010, 0x2020, 0x4040, 0x8080,
                            0x0101, 0x0202, 0x0404, 0x0808, 0x1010, 0x2020, 0x4040, 0x8080,
                            0x0101, 0x0202, 0x0404, 0x0808, 0x1010, 0x2020, 0x4040, 0x8080,
                            0x0101, 0x0202, 0x0404, 0x0808, 0x1010, 0x2020, 0x4040, 0x8080,
                            0xaaaa, 0x5555, 0xaaaa, 0x5555, 
                            0xffff};
        public RepeatForm()
        {
            InitializeComponent();
            Button9.Click += new EventHandler(BtnFunc);
            Button5.Click += new EventHandler(BtnFunc);
            Button7.Click += new EventHandler(BtnFunc);
            ti.Interval = 10;
            ti.Tick += new EventHandler(UpdateFunc);
            ti.Start();
            this.FormClosing += new FormClosingEventHandler(formclosingFunc);
            this.SizeChanged += new EventHandler(FormSizeFunc);
            RichTextBox3.Text = "";
            TextBox8.Text = "";

            FormSizeFunc(null,null);
        }
        private void formclosingFunc(object sender, EventArgs e)
        {
            ti.Stop();
            ti.Dispose();
        }
        private void RepeatThreadFunc()
        {
            Stamp = DateTime.Now;
            int dummyDataIndex = 0;
            int dummyFailCnt = 0;
            Main_cls.RFTagTypeList RFTag = Main_cls.RFTagTypeList.NONE;
            byte[] ReadData = new byte[10];
            bool temp;
            int temp2;
            bool WriteSuccess;
            MessageString = "";
            RepeatRunFlag = true;
            RichTextBox richTextBox = new RichTextBox();
            (temp, temp, RFTag, temp2) =  m_Main.ReadTageData(ref richTextBox);
            MessageString = richTextBox.Text;
            MessageString += "\r\n";
            if(RFTag == Main_cls.RFTagTypeList.M9WK)
            {
                for(int i =0;i<m_Main.M9WK_PageReadData.GetLength(1);i++){    ReadData[i] = m_Main.M9WK_PageReadData[(byte)Main_cls.RFTagPageList.Page08,i];  }
                dummyData[dummyData.Length-1] = (uint)(ReadData[2] | (ReadData[3] <<8));        // 기존 시간 저장

                for(int i =0;i<dummyData.Length;i++)
                {
                    if(!RepeatRunFlag) return;
                    ReadData[2] = (byte)(dummyData[i] & 0x00ff);
                    ReadData[3] = (byte)((dummyData[i] & 0xff00)>>8);
                    MessageString += String.Format("({0}/{1}) Write [", (i+1),(dummyData.Length)) + m_Main.ByteArrayToHexString(ReadData, (byte)m_Main.M9WK_PageReadData.GetLength(1)) + "] ";

                    if(!m_Main.M9WK_PageWrite(Main_cls.RFTagPageList.Page08,ReadData))
                    { 
                        dummyFailCnt++; 
                        MessageString += " 쓰기실패!!!\r\n";
                        WriteSuccess = false;
                    }  // 쓰기실패시 
                    else{WriteSuccess = true;}
                    
                    Thread.Sleep(5);
                    if(WriteSuccess)
                    {
                        (temp, temp, Main_cls.RFTagTypeList ReadTag, temp2) =  m_Main.ReadTageData(ref richTextBox);
                        if(ReadTag != RFTag)
                        {
                            dummyFailCnt ++;
                            MessageString += "읽기실패(Tag종류이상!!)\r\n";
                        }
                        else
                        {
                            byte[] tempreaddata = new byte[m_Main.M9WK_PageReadData.GetLength(1)];
                            for(int k =0;k<m_Main.M9WK_PageReadData.GetLength(1);k++)
                            {
                                tempreaddata[k] = m_Main.M9WK_PageReadData[(byte)(Main_cls.RFTagPageList.Page08),k];
                            }
                            MessageString += "==> Read [" + m_Main.ByteArrayToHexString(tempreaddata,(byte)m_Main.M9WK_PageReadData.GetLength(1)) + "]";
                            bool datavail = true;
                            for(int k =0;k<m_Main.M9WK_PageReadData.GetLength(1);k++)
                            {
                                if(m_Main.M9WK_PageReadData[(byte)(Main_cls.RFTagPageList.Page08),k] != ReadData[k])
                                {
                                    datavail = false;
                                }
                                
                            }
                            if(!datavail) 
                            {
                                MessageString += "데이터 이상!!!\r\n";
                                dummyFailCnt ++;
                            }
                            else
                            {
                                MessageString += "Success!!!\r\n";
                            }
                        }
                    }
                    Thread.Sleep(5);
                }

                MessageString += "\r\n";
                MessageString += String.Format("Total {0} / Success {1}  ==> {2}% \r\n", dummyData.Length,(dummyData.Length - dummyFailCnt), (double)(dummyData.Length - dummyFailCnt)/(double)(dummyData.Length)*100 );
                richBackColor = (dummyData.Length == (dummyData.Length - dummyFailCnt))?Color.FromArgb(240,255,240):Color.FromArgb(255,240,240);
            }
            if(RFTag == Main_cls.RFTagTypeList.W9WK)
            {
                for(int i =0;i<m_Main.W9Wk_ReadData.Length;i++){    ReadData[i] = m_Main.W9Wk_ReadData[i];  }
                dummyData[dummyData.Length-1] = (uint)(ReadData[2] | (ReadData[3] <<8));        // 기존 시간 저장
                for(int i =0;i<dummyData.Length;i++)
                {
                    if(!RepeatRunFlag) return;
                    ReadData[2] = (byte)(dummyData[i] & 0x00ff);
                    ReadData[3] = (byte)((dummyData[i] & 0xff00)>>8);
                    MessageString += String.Format("({0}/{1}) Write [", (i+1),(dummyData.Length)) + m_Main.ByteArrayToHexString(ReadData, 8) + "] ";

                    if(!m_Main.W9WKWriteData(ReadData))
                    { 
                        dummyFailCnt++; 
                        MessageString += " 쓰기실패!!!\r\n";
                        WriteSuccess = false;
                    }  // 쓰기실패시 
                    else{WriteSuccess = true;}
                    
                    Thread.Sleep(5);
                    if(WriteSuccess)
                    {
                        (temp, temp, Main_cls.RFTagTypeList ReadTag, temp2) =  m_Main.ReadTageData(ref richTextBox);
                        if(ReadTag != RFTag)
                        {
                            dummyFailCnt ++;
                            MessageString += "읽기실패(Tag종류이상!!)\r\n";
                        }
                        else
                        {
                            MessageString += "==> Read [" + m_Main.ByteArrayToHexString(m_Main.W9Wk_ReadData,8) + "]";
                            bool datavail = true;
                            for(int k =0;k<8;k++)
                            {
                                if(m_Main.W9Wk_ReadData[k] != ReadData[k])
                                {
                                    datavail = false;
                                }
                                
                            }
                            if(!datavail) 
                            {
                                MessageString += "데이터 이상!!!\r\n";
                                dummyFailCnt ++;
                            }
                            else
                            {
                                MessageString += "Success!!!\r\n";
                            }
                        }
                    }
                    Thread.Sleep(5);
                }

                MessageString += "\r\n";
                MessageString += String.Format("Total {0} / Success {1}  ==> {2}% \r\n", dummyData.Length,(dummyData.Length - dummyFailCnt), (double)(dummyData.Length - dummyFailCnt)/(double)(dummyData.Length)*100 );
                richBackColor = (dummyData.Length == (dummyData.Length - dummyFailCnt))?Color.FromArgb(240,255,240):Color.FromArgb(255,240,240);
            }
            
            RepeatRunFlag = false;
        }
        private void BtnFunc(object Sender, EventArgs e)
        {
            if(Sender == Button9)
            {
                if(MessageBox.Show("창을 닫으시겠습니까?", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ti.Stop();
                    this.Close();
                }
            }
            if(Sender == Button5)
            {
                if(!m_Main.Serial.IsOpen)
                {
                    MessageBox.Show("통신연결이 되지 않았습니다");
                    return;
                }
                BackColor = Color.FromArgb(255,255,255);
                RepeatThread = new Thread(RepeatThreadFunc);
                RepeatThread.Start();
            }
            if(Sender == Button7)
            {
                BackColor = Color.FromArgb(255,255,255);
                RepeatRunFlag = false;
            }
        }
        
        private void RepeatStart()
        {
            //if(RepeatThread
        }
        private void UpdateFunc(object sender, EventArgs e)
        {
            Button5.Enabled = !RepeatRunFlag;
            if(RepeatRunFlag)
            {
                if(this.Cursor != Cursors.WaitCursor)
                    this.Cursor = Cursors.WaitCursor;
                TextBox8.Text = ((TimeSpan)(Stamp - DateTime.Now)).ToString(@"mm\:ss\.fff") + "Sec";
            }
            else
            {
                if(this.Cursor != Cursors.Default)
                    this.Cursor = Cursors.Default;
            }
        
            if(RichTextBox3.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length != MessageString.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length)
            {
                RichTextBox3.Text = MessageString;
                RichTextBox3.SelectionStart = RichTextBox3.Text.Length;
                RichTextBox3.ScrollToCaret();
            }
            RichTextBox3.BackColor = richBackColor;
            RichTextBox3.Cursor = this.Cursor;
        }
        private void FormSizeFunc(Object Sender, EventArgs e)
        {
            SplitContainer2.Width = this.Size.Width-210;
            Button5.Size = new Size(180,80);
            Button5.Location = new Point(0,0);
            Button7.Size = new Size(180,80);
            Button7.Location = new Point(0,90);
            TextBox8.Size= new Size(180,20);
            TextBox8.Location = new Point(0,180);
            Button9.Size = new Size(180,80);
            Button9.Location = new Point(0,this.Height - 120);
        }
    }
}