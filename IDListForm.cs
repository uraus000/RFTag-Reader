using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace RF_Tag_Reader
{
    public partial class IDListForm : Form
    {
        System.Windows.Forms.Timer ti = new System.Windows.Forms.Timer();
        Main_cls m_Main = Main_cls.Instance;
        public IDListForm()
        {
            InitializeComponent();
            SizeChangedFunc(null,null);
            ListviewInit();

            RefreshBtn.Click += new EventHandler(BtnFunc);
            OpenBtn.Click += new EventHandler(BtnFunc);
            SerialNumList.DoubleClick += new EventHandler(ListviewDoubleClickFunc);
            ListViewUpdate();
            ti.Interval = 100;
            ti.Tick += new EventHandler(UpdateFunc);
            ti.Start();
        }
        private void UpdateFunc(object sender, EventArgs e)
        {
            if(SerialNumList.Items.Count != m_Main.SerialListData.Count)
            {
                ListViewUpdate();
            }
        }

        private void ListviewInit()
        {
             // ListView 설정
            SerialNumList.View = View.Details;
            SerialNumList.FullRowSelect = true;
            SerialNumList.GridLines = true;
            SerialNumList.Columns.Clear();

            // 컬럼 추가
            SerialNumList.Columns.Add("SuppliedDate", 150);
            SerialNumList.Columns.Add("LampSerial", 100);
            SerialNumList.Columns.Add("RFSerial", 100);
            SerialNumList.Columns.Add("RFSerialBinary", 150);
            SerialNumList.Columns.Add("RFType", 100);

            for(int i =0;i<SerialNumList.Columns.Count;i++)
                SerialNumList.Columns[i].TextAlign = HorizontalAlignment.Center;
            
            // 기존 아이템 제거
            SerialNumList.Items.Clear();
        }

        private void ListViewUpdate()
        {
            
            SerialNumList.Items.Clear();
            // SerialListData 순회
            foreach (var item in m_Main.SerialListData)
            {
                string rfSerialHex = BitConverter.ToString(item.RFSerial).Replace("-", " ");

                ListViewItem lvi = new ListViewItem(item.SuppliedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                lvi.SubItems.Add(item.LampSerial);
                lvi.SubItems.Add(rfSerialHex);
                lvi.SubItems.Add(item.RFSerialBinary);
                lvi.SubItems.Add(item.RFType.ToString());
                lvi.Tag = item.LogFilePath;
                SerialNumList.Items.Add(lvi);
            }
        }
        private void ListviewDoubleClickFunc(object sender, EventArgs e)
        {
            bool openflag = false;
            if (SerialNumList.SelectedItems.Count > 0)
            {
                openflag = true;
                try
                {
                    Process.Start("notepad.exe",SerialNumList.SelectedItems[0].Tag.ToString());
                }
                catch(Exception ex)
                {
                    MessageBox.Show("파일을 열 수 없습니다: " + ex.Message);
                }    
                
            }
            if(!openflag)
            {
                MessageBox.Show("파일이 없습니다.");
            }
        }
        private void BtnFunc(object sender, EventArgs e)
        {
            if(sender == RefreshBtn)
            {
                m_Main.LogReadFunc(12);
                ListViewUpdate();
            }
            else if(sender == OpenBtn)
            {
                Process.Start("explorer.exe", m_Main.LogPath);
            }

        }

        private void SizeChangedFunc(object sender, EventArgs e)
        {
            this.SerialNumList.Height = this.Height - 110;
            this.RefreshBtn.Location = new Point(0,this.Height - 110);
            this.RefreshBtn.Size = new Size(this.Width/2-5, 80);
            this.OpenBtn.Location = new Point(this.Width/2,this.Height - 110);
            this.OpenBtn.Size = new Size(this.Width/2-5, 80);
        }
    }
}