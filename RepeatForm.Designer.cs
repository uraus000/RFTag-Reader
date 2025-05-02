using System;
namespace RF_Tag_Reader
{
    partial class RepeatForm
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
        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
    {
        this.SplitContainer2 = new System.Windows.Forms.SplitContainer();
        this.RichTextBox3 = new System.Windows.Forms.RichTextBox();
        this.Button5 = new System.Windows.Forms.Button();
        this.Button7 = new System.Windows.Forms.Button();
        this.TextBox8 = new System.Windows.Forms.TextBox();
        this.Button9 = new System.Windows.Forms.Button();
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).BeginInit();
        this.SplitContainer2.Panel1.SuspendLayout();
        this.SplitContainer2.Panel2.SuspendLayout();
        this.SplitContainer2.SuspendLayout();
        this.SuspendLayout();
        //
        // SplitContainer2
        //
        this.SplitContainer2.Panel2.Controls.Add(this.RichTextBox3);
        this.SplitContainer2.Panel1.Controls.Add(this.Button5);
        this.SplitContainer2.Panel1.Controls.Add(this.Button7);
        this.SplitContainer2.Panel1.Controls.Add(this.TextBox8);
        this.SplitContainer2.Panel1.Controls.Add(this.Button9);
        this.SplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
        this.SplitContainer2.Text =  "SplitContainer2";
        this.SplitContainer2.Size = new System.Drawing.Size(690,641);
        this.SplitContainer2.TabIndex = 2;
        this.SplitContainer2.SplitterDistance = 158;
        //
        // RichTextBox3
        //
        this.RichTextBox3.SelectionBackColor = System.Drawing.SystemColors.Window;
        this.RichTextBox3.Text =  "RichTextBox3";
        this.RichTextBox3.SelectionStart = 12;
        this.RichTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
        this.RichTextBox3.Size = new System.Drawing.Size(526,639);
        this.RichTextBox3.TabIndex = 3;
        //
        // Button5
        //
        this.Button5.Text =  "Start";
        this.Button5.Location = new System.Drawing.Point(12,12);
        this.Button5.Size = new System.Drawing.Size(130,50);
        this.Button5.TabIndex = 5;
        //
        // Button7
        //
        this.Button7.Text =  "Stop";
        this.Button7.Location = new System.Drawing.Point(12,72);
        this.Button7.Size = new System.Drawing.Size(130,50);
        this.Button7.TabIndex = 7;
        //
        // TextBox8
        //
        this.TextBox8.Text =  "TextBox8";
        this.TextBox8.Location = new System.Drawing.Point(8,160);
        this.TextBox8.Size = new System.Drawing.Size(144,27);
        this.TextBox8.TabIndex = 8;
        //
        // Button9
        //
        this.Button9.Text =  "Close";
        this.Button9.Location = new System.Drawing.Point(8,540);
        this.Button9.Size = new System.Drawing.Size(139,92);
        this.Button9.TabIndex = 9;
     //
     // form
     //
        this.Size = new System.Drawing.Size(708,688);
        this.Text =  "RepeatForm";
        this.Controls.Add(this.SplitContainer2);
        ((System.ComponentModel.ISupportInitialize)(this.SplitContainer2)).EndInit();
        this.SplitContainer2.Panel1.ResumeLayout(false);
        this.SplitContainer2.Panel2.ResumeLayout(false);
        this.SplitContainer2.ResumeLayout(false);
        this.ResumeLayout(false);
    } 

    #endregion 

    private System.Windows.Forms.SplitContainer SplitContainer2;
    private System.Windows.Forms.RichTextBox RichTextBox3;
    private System.Windows.Forms.Button Button5;
    private System.Windows.Forms.Button Button7;
    private System.Windows.Forms.TextBox TextBox8;
    private System.Windows.Forms.Button Button9;
}

}