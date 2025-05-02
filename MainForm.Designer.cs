using System;
namespace RF_Tag_Reader
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private ComboBox SerialPortList = new ComboBox();
        private ComboBox SerialBaudList = new ComboBox();
        private Button ConnectionBtn = new Button();
        private Button DisConnectionBtn = new Button();
        private Button TestBtn = new Button();
        private System.Windows.Forms.Timer UIUpdateTimer = new System.Windows.Forms.Timer();

        private Button ReadBtn = new Button();
        private Button InitTime = new Button();
        private Button DataWrite = new Button();
        private Button DebugModeBtn = new Button();
        private Button LogSaveBtn = new Button();
        private Button LogFolderOpenBtn = new Button();
        private Button RepeatBtn = new Button();
        private TextBox LampSerialText = new TextBox();
        private RichTextBox richTextBox = new RichTextBox();

        private Button CRC16Test = new Button();
        private RichTextBox CRC16Data = new RichTextBox();
        private RichTextBox CRC16Result = new RichTextBox();

        private NumericUpDown SetTimeValue = new NumericUpDown();
        private Label label1 = new Label();
        private Label label2 = new Label();
        private CheckBox AutoInc = new CheckBox();
        private CheckBox AutoRead = new CheckBox();
        private CheckBox SerialBit = new CheckBox();
        private CheckBox SerialBitclr = new CheckBox();
        private CheckBox MessageBoxchk = new CheckBox();
        private NumericUpDown AutoReadTime = new NumericUpDown();

        private CheckBox Debug1 = new CheckBox();
        private CheckBox Debug2 = new CheckBox();
        private CheckBox Debug3 = new CheckBox();
        private CheckBox Debug4 = new CheckBox();
        private TextBox SerialBox = new TextBox();
        private Label SerialBoxLabel = new Label();
        private Button SerialRandom = new Button();
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            

            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 300);
            this.Text = "Form1";
            this.FormClosing += new FormClosingEventHandler(FormClosingFunc);
            this.Controls.Add(SerialPortList);
            this.SerialPortList.Location = new Point(0,0);
            this.SerialPortList.DropDown += new EventHandler(DropdownFunc);
            this.SizeChanged += new EventHandler(FormChangeFunc);
            this.MinimumSize = new Size(500,300);
            this.Size = new Size(800,500);

            this.Controls.Add(SerialBaudList);
            this.SerialBaudList.Location = new Point(0,30);
            this.SerialBaudList.Items.Add("4800");
            this.SerialBaudList.Items.Add("9600");
            this.SerialBaudList.Items.Add("19200");
            this.SerialBaudList.Items.Add("115200");
            this.SerialBaudList.Text = "9600";

            this.Controls.Add(ConnectionBtn);
            this.Controls.Add(DisConnectionBtn);
            this.ConnectionBtn.Location = new Point(120,0);
            this.ConnectionBtn.Click += new EventHandler(ConnectionFunc);
            this.DisConnectionBtn.Click += new EventHandler(DisConnectionFunc);
            this.DisConnectionBtn.Location = new Point(120,30);
            this.ConnectionBtn.Text = "연결";
            this.DisConnectionBtn.Text = "끊기";

            UIUpdateTimer.Interval = 10;
            UIUpdateTimer.Tick += new EventHandler(UIUpdateTimerFunc);

            this.Controls.Add(ReadBtn);
            this.ReadBtn.Location = new Point(200,0);
            this.ReadBtn.Text = "데이터 읽기";
            this.ReadBtn.Size = new Size(100,50);
            this.ReadBtn.Click += new EventHandler(UI_Func);

            this.Controls.Add(InitTime);
            this.InitTime.Location = new Point(200,50);
            this.InitTime.Text = "시간 초기화";
            this.InitTime.Size = new Size(100,50);
            this.InitTime.Click += new EventHandler(UI_Func);

            this.Controls.Add(DataWrite);
            this.DataWrite.Location = new Point(200,100);
            this.DataWrite.Text = "임의 데이터\r\n쓰기";
            this.DataWrite.Size = new Size(100,50);
            this.DataWrite.Click += new EventHandler(UI_Func);

            this.Controls.Add(DebugModeBtn);
            this.DebugModeBtn.Location = new Point(200,150);
            this.DebugModeBtn.Text = "Debug Mode";
            this.DebugModeBtn.Size = new Size(100,50);
            this.DebugModeBtn.Click += new EventHandler(DebugModeFunc);
            this.DebugModeBtn.Visible = true;

            this.Controls.Add(LogSaveBtn);
            this.LogSaveBtn.Location = new Point(200,200);
            this.LogSaveBtn.Size = new Size(100,50);
            this.LogSaveBtn.Text = "Log Save";
            this.LogSaveBtn.Click += new EventHandler(LogSaveFunc);
            this.LogSaveBtn.Visible = true;
            this.LogSaveBtn.Enabled = false;

            this.Controls.Add(LogFolderOpenBtn);
            this.LogFolderOpenBtn.Location = new Point(200,250);
            this.LogFolderOpenBtn.Size = new Size(100,50);
            this.LogFolderOpenBtn.Text = "Log Floder Open";
            this.LogFolderOpenBtn.Click += new EventHandler(LogSaveFunc);
            this.LogFolderOpenBtn.Visible = true;
            this.LogFolderOpenBtn.Enabled = true;

            this.Controls.Add(RepeatBtn);
            this.RepeatBtn.Location = new Point(200,300);
            this.RepeatBtn.Size = new Size(100,50);
            this.RepeatBtn.Text = "반복테스트";
            this.RepeatBtn.Click += new EventHandler(RepeatFunc);
            this.RepeatBtn.Visible = true;
            this.RepeatBtn.Enabled = true;


            this.Controls.Add(label2);
            this.label2.Text = "Lamp Serial";
            this.label2.Location = new Point(0,65);
            this.label2.Visible = true;

            this.Controls.Add(LampSerialText);
            this.LampSerialText.Location = new Point(0,90);
            this.LampSerialText.Size = new Size(200,100);
            this.LampSerialText.Visible = true;
            this.LampSerialText.Text = "";
            this.LampSerialText.TextChanged += new EventHandler(LogSaveFunc);
            
            this.Controls.Add(CRC16Test);
            this.CRC16Test.Location = new Point(200,200);
            this.CRC16Test.Text = "CRC16CITTI Test";
            this.CRC16Test.Size = new Size(100,50);
            this.CRC16Test.Click += new EventHandler(UI_Func);
            this.CRC16Test.Visible = false;

            this.Controls.Add(CRC16Data);
            this.CRC16Data.Location = new Point(0,350);
            this.CRC16Data.Size = new Size(600,50);
            this.CRC16Data.Visible = false;

            this.Controls.Add(CRC16Result);
            this.CRC16Result.Location = new Point(0,400);
            this.CRC16Result.Size = new Size(100,50);
            this.CRC16Result.Visible = false;

            this.Controls.Add(richTextBox);
            this.richTextBox.Location = new Point(300,0);
            this.richTextBox.Size = new Size(1200, 300);
            this.richTextBox.Dock = DockStyle.Right;
            this.richTextBox.ScrollBars = RichTextBoxScrollBars.Both;
            this.richTextBox.WordWrap = false;

            this.Controls.Add(SetTimeValue);
            this.SetTimeValue.Location = new Point(70,130);
            this.SetTimeValue.Size = new Size(100,50);
            this.SetTimeValue.Maximum = 9999;
            this.SetTimeValue.Minimum = 0;
            this.SetTimeValue.Value = 0;

            this.Controls.Add(label1);
            this.label1.Text = "Set Data";
            this.label1.Location = new Point(10,130);

            this.Controls.Add(AutoInc);
            this.AutoInc.Text = "자동 증가";
            this.AutoInc.Location = new Point(10,155);
            this.AutoInc.Size = new Size(80,30);
            this.AutoInc.Checked = false;
            this.AutoInc.Visible = true;

            this.Controls.Add(AutoRead);
            this.AutoRead.Text = "자동 읽기";
            this.AutoRead.Location = new Point(10,180);
            this.AutoRead.Size = new Size(80,30);
            this.AutoRead.Checked = false;
            this.AutoRead.Visible = true;

            this.Controls.Add(AutoReadTime);
            this.AutoReadTime.Location = new Point(100,180);
            this.AutoReadTime.Size = new Size(80,30);
            this.AutoReadTime.Maximum = 5000;
            this.AutoReadTime.Minimum = 500;
            this.AutoReadTime.Value = 700;
            this.AutoReadTime.Visible = true;

            this.Controls.Add(MessageBoxchk);
            this.MessageBoxchk.Text = "메세지박스 사용안함";
            this.MessageBoxchk.Location = new Point(10,205);
            this.MessageBoxchk.Size = new Size(150,30);
            this.MessageBoxchk.Checked = false;
            this.MessageBoxchk.Visible = true;

            this.Controls.Add(SerialBit);
            this.SerialBit.Text = "시리얼비트 추가";
            this.SerialBit.Location = new Point(10,300);
            this.SerialBit.Size = new Size(120,30);
            this.SerialBit.Checked = false;
            this.SerialBit.Visible = false;
            this.SerialBit.CheckStateChanged += new EventHandler(CheckFunc);

            this.Controls.Add(SerialBitclr);
            this.SerialBitclr.Text = "시리얼비트 해제";
            this.SerialBitclr.Location = new Point(150,300);
            this.SerialBitclr.Size = new Size(120,30);
            this.SerialBitclr.Checked = false;
            this.SerialBitclr.Visible = false;
            this.SerialBitclr.CheckStateChanged += new EventHandler(CheckFunc);

            this.Controls.Add(Debug1);
            this.Debug1.Text = "Page8 데이터 디버그";
            this.Debug1.Location = new Point(10,325);
            this.Debug1.Size = new Size(150,30);
            this.Debug1.Checked = false;
            this.Debug1.Visible = false;
            this.Debug1.CheckStateChanged += new EventHandler(DebugFunc);

            this.Controls.Add(Debug2);
            this.Debug2.Text = "Page9 데이터 디버그";
            this.Debug2.Location = new Point(10,350);
            this.Debug2.Size = new Size(150,30);
            this.Debug2.Checked = false;
            this.Debug2.Visible = false;
            this.Debug2.CheckStateChanged += new EventHandler(DebugFunc);

            this.Controls.Add(Debug3);
            this.Debug3.Text = "Page10 데이터 디버그";
            this.Debug3.Location = new Point(10,375);
            this.Debug3.Size = new Size(150,30);
            this.Debug3.Checked = false;
            this.Debug3.Visible = false;
            this.Debug3.CheckStateChanged += new EventHandler(DebugFunc);

            this.Controls.Add(Debug4);
            this.Debug4.Text = "Serial 변경";
            this.Debug4.Location = new Point(10,400);
            this.Debug4.Size = new Size(150,30);
            this.Debug4.Checked = false;
            this.Debug4.Visible = false;
            this.Debug4.CheckStateChanged += new EventHandler(DebugFunc);

            this.Controls.Add(SerialBox);
            this.SerialBox.Location = new Point(180,375);
            this.SerialBox.Size = new Size(100,30);
            this.SerialBox.Visible = false;
            this.SerialBox.Text = "1234";
            this.SerialBox.Enabled = false;

            this.Controls.Add(SerialRandom);
            this.SerialRandom.Text = "Serial 램덤적용";
            this.SerialRandom.Location = new Point(180,400);
            this.SerialRandom.Size = new Size(100,30);
            this.SerialRandom.Visible = false;
            this.SerialRandom.Enabled = false;
            this.SerialRandom.Click += new EventHandler(DebugFunc);

            this.Controls.Add(SerialBoxLabel);
            this.SerialBoxLabel.Location = new Point(180,340);
            this.SerialBoxLabel.Size = new Size(120,60);
            this.SerialBoxLabel.Text = "Serial ID\r\n(16진수 4자리)";
            this.SerialBoxLabel.Visible = false;
        }

        #endregion
    }
}