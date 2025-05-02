using System;
namespace RF_Tag_Reader
{
    partial class IDListForm
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
        private ListView SerialNumList = new ListView();
        private Button RefreshBtn = new Button();
        private Button OpenBtn = new Button();
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 550);
            this.Text = "Serial List";
            this.ControlBox = false;
            this.SizeChanged += new EventHandler(SizeChangedFunc);
            this.MinimumSize = new Size(650,550);
            
            this.Controls.Add(SerialNumList);
            this.SerialNumList.Location = new Point(0,0);
            this.SerialNumList.Dock = DockStyle.Top;
            this.SerialNumList.Size = new Size(700,700);
            this.SerialNumList.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            this.Controls.Add(RefreshBtn);
            this.RefreshBtn.Location = new Point(0,700);
            this.RefreshBtn.Size = new Size(100,50);
            //this.RefreshBtn.Click += new EventHandler(ConnectionFunc);
            this.RefreshBtn.Text = "Refresh";

            this.Controls.Add(OpenBtn);
            this.OpenBtn.Location = new Point(250,700);
            this.OpenBtn.Text = "Log열기";
            this.OpenBtn.Size = new Size(100,50);
           // this.OpenBtn.Click += new EventHandler(UI_Func);
        }

        #endregion
    }
}