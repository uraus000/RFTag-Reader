using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace RF_Tag_Reader
{
    public partial class MainForm : Form
    {
        Main_cls m_Main = Main_cls.Instance;
        private string Version = "RF Tag Read V3.0_241227";
        private DateTime AutoReadTimeStamp = DateTime.Now;
        private bool LampSerialChange = false;
        private IDListForm IDLIST = new IDListForm();

        
        public MainForm()
        {
            InitializeComponent();
            GetSerialPorts();
            
            InitTime.Enabled = true;
            if(SerialPortList.Items.Count > 0)            
                SerialPortList.SelectedIndex = SerialPortList.Items.Count-1;
            UIUpdateTimer.Start();
            FormChangeFunc(null,null);
            IDLIST.Show();
        }
        private void DropdownFunc(object sender, EventArgs e)
        {
            GetSerialPorts();
        }
        private void GetSerialPorts()
        {
            SerialPortList.Items.Clear();
            List<string> portNames = new List<string>();
            string[] port = System.IO.Ports.SerialPort.GetPortNames();

            for(int i =0;i<port.Count();i++)
            {
                SerialPortList.Items.Add(port[i]);
            }
        }
        private void RepeatFunc(object sender, EventArgs e)
        {
            //m_Main.RFTagSuccessWrite(ref richTextBox,10);
            RepeatForm repeatForm = new RepeatForm();
            repeatForm.Show();
        }
        private void LogSaveFunc(object sender, EventArgs e)
        {
            if(sender == LampSerialText)
            {
            }
            else if(sender == LogSaveBtn)
            {
                if(LampSerialText.Text != "")
                {
                    if(LogFunc(LampSerialText.Text,richTextBox.Text))   LogSaveBtn.Enabled = false;
                    MessageBox.Show("저장하였습니다");
                }
                else
                {
                    MessageBox.Show("Lampserial이 잘못기재되어있습니다");
                }
            }
            else if(sender == LogFolderOpenBtn)
            {
                try
                {
                    if(!Directory.Exists(m_Main.LogPath))
                    {
                        Directory.CreateDirectory(m_Main.LogPath);
                    }
                    System.Diagnostics.Process.Start("Explorer.exe",m_Main.LogPath + @"\");
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                
            }
        }

        
        private void DebugModeFunc(object sender, EventArgs e)
        {
            if(Debug1.Visible)
            {
                if(MessageBox.Show("DebugMode를 해제하시겠습니까?","Warning",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Debug1.Checked = false;
                    Debug2.Checked = false;
                    Debug3.Checked = false;
                    Debug4.Checked = false;

                    SerialBit.Visible = false;
                    SerialBitclr.Visible = false;
                    Debug1.Visible = false;
                    Debug2.Visible = false;
                    Debug3.Visible = false;
                    Debug4.Visible = false;
                    SerialBox.Visible = false;
                    SerialBoxLabel.Visible = false;
                    SerialRandom.Visible =false;
                    SerialRandom.Enabled = false;

                    m_Main.debuglist.Debug1 = false;
                    m_Main.debuglist.Debug2 = false;
                    m_Main.debuglist.Debug3 = false;
                    m_Main.debuglist.Debug4 = false;
                    m_Main.SeribitSet = false;
                    m_Main.SerialBitClr = false;
                }
            }
            else
            {
                if(MessageBox.Show("DebugMode를 사용하시겠습니까?","Warning",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Debug1.Checked = false;
                    Debug2.Checked = false;
                    Debug3.Checked = false;
                    Debug4.Checked = false;
                    SerialBit.Checked = false;
                    SerialBitclr.Checked = false;

                    SerialBit.Visible = true;
                    SerialBitclr.Visible = true;
                    Debug1.Visible = true;
                    Debug2.Visible = true;
                    Debug3.Visible = true;
                    Debug4.Visible = true;
                    SerialBox.Visible = true;
                    SerialBoxLabel.Visible = true;
                    SerialRandom.Visible = true;
                    SerialRandom.Enabled = false;

                    m_Main.debuglist.Debug1 = false;
                    m_Main.debuglist.Debug2 = false;
                    m_Main.debuglist.Debug3 = false;
                    m_Main.debuglist.Debug4 = false;
                    m_Main.SeribitSet = false;
                    m_Main.SerialBitClr = false;
                }
            }
        }
        private void DebugFunc(Object sender, EventArgs e)
        {
            if(sender == Debug1)
            {
                m_Main.debuglist.Debug1 = Debug1.Checked;
            }
            else if(sender == Debug2)
            {
                m_Main.debuglist.Debug2 = Debug2.Checked;
            }
            else if(sender == Debug3)
            {
                m_Main.debuglist.Debug3 = Debug3.Checked;
            }
            else if(sender == Debug4)
            {
                m_Main.debuglist.Debug4 = Debug4.Checked;
                SerialBox.Enabled = Debug4.Checked;
                SerialRandom.Enabled = Debug4.Checked;
            }
            else if(sender == SerialRandom)
            {
                Random random = new Random();
                int Upper = 0x00, Lower = 0x00;
                int Result = 0x0000;
                
                Upper = random.Next(0,0xff);
                Lower = random.Next(0,0x1f);

                if(SerialBit.Checked)
                {
                    Lower = Lower | 0x80;
                }

                SerialBox.Text = ((Upper << 8) | Lower).ToString("x4");
                SerialRandom.Enabled = false;
            }
        }
        private void FormChangeFunc(object sender, EventArgs e)
        {
            int w = this.Size.Width;

            richTextBox.Size = new Size((w-320),100);
        }
        private void CheckFunc(object sender, EventArgs e)
        {
            if(sender == SerialBit)
            {
                if(SerialBit.Checked)
                {
                    if(SerialBitclr.Checked)
                    {
                        SerialBitclr.Checked = false;
                    } 
                    m_Main.SeribitSet = true;
                    m_Main.SerialBitClr = false;
                }
                else
                {
                    m_Main.SeribitSet = false;
                }
            }
            else if(sender == SerialBitclr)
            {
                if(SerialBitclr.Checked)
                {
                    if(SerialBit.Checked) 
                    {
                        SerialBit.Checked = false;
                    }
                    m_Main.SeribitSet = false;
                    m_Main.SerialBitClr = true;
                }
                else
                {
                    m_Main.SerialBitClr = false;
                }
            }
        }
        private void UI_Func(object sender, EventArgs e)
        {
            if(m_Main.Serial.IsOpen)
            {
                if(m_Main.debuglist.Debug4 || (sender == InitTime) || (sender == DataWrite))
                {
                    try
                    {
                        if(SerialBox.TextLength != 4)
                        {
                            MessageBox.Show("Serial ID가 잘못 기재되었습니다. 16진수 4자리로 입력하십시오");
                            return;
                        }
                        m_Main.debuglist.SerialID = Convert.ToUInt32(SerialBox.Text,16);
                    }
                    catch(Exception)
                    {
                        MessageBox.Show("Serial ID가 잘못 기재되었습니다.");
                        return;
                    }
                }
                
                if(sender == ReadBtn)
                {
                    bool temp;
                    int temp2;
                    richTextBox.Clear();
                    (temp, temp, Main_cls.RFTagTypeList RFTag, temp2) =  m_Main.ReadTageData(ref richTextBox);
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    richTextBox.ScrollToCaret();
                    if((RFTag == Main_cls.RFTagTypeList.M9WK)||(RFTag == Main_cls.RFTagTypeList.W9WK))
                    {
                        LogSaveBtn.Enabled = true;
                    }
                    else
                    {
                        LogSaveBtn.Enabled = false;
                    }
                }
                else if(sender == InitTime)
                {
                    
                    if(!MessageBoxchk.Checked)
                    {
                        if(MessageBox.Show("초기화를 진행하시겠습니까?","Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)
                        {   return; }
                    }
                    richTextBox.Clear();
                    if(!m_Main.RFTagWrite(ref richTextBox, 0))
                    {
                        MessageBox.Show("쓰기를 실패하였습니다");
                    }
                    else
                    {
                        LogSaveBtn.Enabled = true;
                        SerialRandom.Enabled = true;
                    }
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    richTextBox.ScrollToCaret();
                
                }    
                else if(sender == DataWrite)
                {
                    if(!MessageBoxchk.Checked)
                    {
                        if(MessageBox.Show("데이터를 쓰시겠습니까?","Warning", MessageBoxButtons.YesNo)!=DialogResult.Yes)
                        {   return; }
                    }
                    richTextBox.Clear();
                    if(!m_Main.RFTagWrite(ref richTextBox, Convert.ToInt32(SetTimeValue.Value)))
                    {
                        MessageBox.Show("쓰기를 실패하였습니다");
                    }     
                    else
                    {
                        LogSaveBtn.Enabled = true;
                        SerialRandom.Enabled = true;
                        if(AutoInc.Checked)
                        {
                            if(Convert.ToInt32(SetTimeValue.Value) == SetTimeValue.Maximum)
                            {
                                SetTimeValue.Value = SetTimeValue.Minimum;
                            }
                            else
                            {
                                SetTimeValue.Value += 1;
                            }
                        }
                    }               
                    
                    richTextBox.SelectionStart = richTextBox.Text.Length;
                    richTextBox.ScrollToCaret();
                }
            }
            else
            {
                MessageBox.Show("통신이 연결되지 않았습니다");
            }
        }

       
        private void FormClosingFunc(object sender, EventArgs e)
        {
            UIUpdateTimer.Stop();
            m_Main.SerialDisconnection();
            Application.Exit();
        }
        
        private void UIUpdateTimerFunc(object sender, EventArgs e)
        {
            this.Text = Version + "[" + DateTime.Now.ToString("HH:mm:ss.ff") + "]";
            ConnectionBtn.Enabled = !m_Main.Serial.IsOpen;
            DisConnectionBtn.Enabled = m_Main.Serial.IsOpen;
            ConnectionBtn.BackColor = m_Main.Serial.IsOpen? Color.LightGreen:SystemColors.ControlLight;
            DisConnectionBtn.BackColor = !m_Main.Serial.IsOpen? Color.LightPink:SystemColors.ControlLight;
            
            SerialBaudList.Enabled = !m_Main.Serial.IsOpen;
            SerialPortList.Enabled = !m_Main.Serial.IsOpen;

            if(AutoRead.Checked)
            {
                if(m_Main.Serial.IsOpen)
                {
                    int tick = Convert.ToInt32(AutoReadTime.Value);
                    if((DateTime.Now-AutoReadTimeStamp).TotalMilliseconds > tick)
                    {
                        richTextBox.Clear();
                        m_Main.ReadTageData(ref richTextBox);
                        AutoReadTimeStamp = DateTime.Now;
                    }
                } 
            }
            else
            {
                AutoReadTimeStamp = DateTime.Now;
            }
        }
        private void ConnectionFunc(object sender, EventArgs e)
        {
            if(!m_Main.SerialConnection(SerialPortList.Text, Convert.ToInt16(SerialBaudList.Text)))MessageBox.Show("통신포트열기를 실패했습니다.");
        }
        private void DisConnectionFunc(object Sender, EventArgs e)
        {
            if(!m_Main.SerialDisconnection()) MessageBox.Show("포트닫기를 실패했습니다.");
        }

        public bool LogFunc(string LampSerial, string Description)
        {
            string DirPath = m_Main.LogPath + @"\"+ DateTime.Now.ToString("yyyy_MM");
            string filename = LampSerial + ".log";
            string FullPath = Path.Combine(DirPath, filename);

            if(!Directory.Exists(DirPath)) { Directory.CreateDirectory(DirPath); }
            if(File.Exists(FullPath)) 
            {
                if(MessageBox.Show("Log파일이 존재합니다. 덮어쓰시겠습니까?","Warning",MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return false;
                }
            }
            if (!File.Exists(FullPath))
            {
                FileStream fs = new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter ws = new StreamWriter(fs, System.Text.Encoding.Default);
                StringBuilder sp = new StringBuilder();

                sp.Append(Description);
                ws.WriteLine(sp.ToString());

                ws.Flush();
                ws.Close();
                fs.Close();
            }
            else
            {
                FileStream fs = new FileStream(FullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                StreamWriter ws = new StreamWriter(fs, System.Text.Encoding.Default);
                StringBuilder sp = new StringBuilder();

                sp.Append(Description);
                ws.WriteLine(sp.ToString());

                ws.Flush();
                ws.Close();
                fs.Close();
            }
            m_Main.LogDataSaveFunc(Description, FullPath);
            return true;
        }
    }
}
