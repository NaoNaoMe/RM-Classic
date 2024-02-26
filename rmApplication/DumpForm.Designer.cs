namespace rmApplication
{
    partial class DumpForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.sizeTextBox = new System.Windows.Forms.TextBox();
            this.requestButton = new System.Windows.Forms.Button();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.label4 = new System.Windows.Forms.Label();
            this.incrementalValueTextBox = new System.Windows.Forms.TextBox();
            this.enableOffsetCheckBox = new System.Windows.Forms.CheckBox();
            this.endiannessGroupBox = new System.Windows.Forms.GroupBox();
            this.tms320c28xEndianRadioButton = new System.Windows.Forms.RadioButton();
            this.bigEndianRadioButton = new System.Windows.Forms.RadioButton();
            this.littleEndianRadioButton = new System.Windows.Forms.RadioButton();
            this.copyToClipBoardButton = new System.Windows.Forms.Button();
            this.symbolTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.makeButton = new System.Windows.Forms.Button();
            this.subSplitContainer = new System.Windows.Forms.SplitContainer();
            this.dumpDataGridView = new System.Windows.Forms.DataGridView();
            this.mainHexBox = new Be.Windows.Forms.HexBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.endiannessGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.subSplitContainer)).BeginInit();
            this.subSplitContainer.Panel1.SuspendLayout();
            this.subSplitContainer.Panel2.SuspendLayout();
            this.subSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dumpDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Address";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "Size";
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(83, 41);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(141, 19);
            this.addressTextBox.TabIndex = 4;
            this.addressTextBox.Text = "0x20000000";
            // 
            // sizeTextBox
            // 
            this.sizeTextBox.Location = new System.Drawing.Point(83, 72);
            this.sizeTextBox.Name = "sizeTextBox";
            this.sizeTextBox.Size = new System.Drawing.Size(141, 19);
            this.sizeTextBox.TabIndex = 5;
            this.sizeTextBox.Text = "1";
            // 
            // requestButton
            // 
            this.requestButton.Location = new System.Drawing.Point(230, 15);
            this.requestButton.Name = "requestButton";
            this.requestButton.Size = new System.Drawing.Size(109, 29);
            this.requestButton.TabIndex = 6;
            this.requestButton.Text = "Request";
            this.requestButton.UseVisualStyleBackColor = true;
            this.requestButton.Click += new System.EventHandler(this.requestButton_Click);
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.label4);
            this.splitContainerMain.Panel1.Controls.Add(this.incrementalValueTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.enableOffsetCheckBox);
            this.splitContainerMain.Panel1.Controls.Add(this.endiannessGroupBox);
            this.splitContainerMain.Panel1.Controls.Add(this.copyToClipBoardButton);
            this.splitContainerMain.Panel1.Controls.Add(this.symbolTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.label3);
            this.splitContainerMain.Panel1.Controls.Add(this.makeButton);
            this.splitContainerMain.Panel1.Controls.Add(this.requestButton);
            this.splitContainerMain.Panel1.Controls.Add(this.label1);
            this.splitContainerMain.Panel1.Controls.Add(this.addressTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.sizeTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.label2);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.subSplitContainer);
            this.splitContainerMain.Size = new System.Drawing.Size(466, 471);
            this.splitContainerMain.SplitterDistance = 170;
            this.splitContainerMain.SplitterWidth = 5;
            this.splitContainerMain.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(291, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 12);
            this.label4.TabIndex = 38;
            this.label4.Text = "Incremental value for series";
            // 
            // incrementalValueTextBox
            // 
            this.incrementalValueTextBox.Location = new System.Drawing.Point(291, 121);
            this.incrementalValueTextBox.Name = "incrementalValueTextBox";
            this.incrementalValueTextBox.Size = new System.Drawing.Size(100, 19);
            this.incrementalValueTextBox.TabIndex = 37;
            this.incrementalValueTextBox.Text = "1";
            this.incrementalValueTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.incrementalValueTextBox_KeyPress);
            this.incrementalValueTextBox.Leave += new System.EventHandler(this.incrementalValueTextBox_Leave);
            // 
            // enableOffsetCheckBox
            // 
            this.enableOffsetCheckBox.AutoSize = true;
            this.enableOffsetCheckBox.Checked = true;
            this.enableOffsetCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableOffsetCheckBox.Location = new System.Drawing.Point(348, 22);
            this.enableOffsetCheckBox.Name = "enableOffsetCheckBox";
            this.enableOffsetCheckBox.Size = new System.Drawing.Size(94, 16);
            this.enableOffsetCheckBox.TabIndex = 36;
            this.enableOffsetCheckBox.Text = "Enable Offset";
            this.enableOffsetCheckBox.UseVisualStyleBackColor = true;
            // 
            // endiannessGroupBox
            // 
            this.endiannessGroupBox.Controls.Add(this.tms320c28xEndianRadioButton);
            this.endiannessGroupBox.Controls.Add(this.bigEndianRadioButton);
            this.endiannessGroupBox.Controls.Add(this.littleEndianRadioButton);
            this.endiannessGroupBox.Location = new System.Drawing.Point(17, 97);
            this.endiannessGroupBox.Name = "endiannessGroupBox";
            this.endiannessGroupBox.Size = new System.Drawing.Size(268, 55);
            this.endiannessGroupBox.TabIndex = 35;
            this.endiannessGroupBox.TabStop = false;
            this.endiannessGroupBox.Text = "Endianness";
            // 
            // tms320c28xEndianRadioButton
            // 
            this.tms320c28xEndianRadioButton.AutoSize = true;
            this.tms320c28xEndianRadioButton.Location = new System.Drawing.Point(108, 20);
            this.tms320c28xEndianRadioButton.Name = "tms320c28xEndianRadioButton";
            this.tms320c28xEndianRadioButton.Size = new System.Drawing.Size(90, 16);
            this.tms320c28xEndianRadioButton.TabIndex = 2;
            this.tms320c28xEndianRadioButton.Text = "TMS320C28x";
            this.tms320c28xEndianRadioButton.UseVisualStyleBackColor = true;
            // 
            // bigEndianRadioButton
            // 
            this.bigEndianRadioButton.AutoSize = true;
            this.bigEndianRadioButton.Location = new System.Drawing.Point(62, 20);
            this.bigEndianRadioButton.Name = "bigEndianRadioButton";
            this.bigEndianRadioButton.Size = new System.Drawing.Size(40, 16);
            this.bigEndianRadioButton.TabIndex = 1;
            this.bigEndianRadioButton.Text = "Big";
            this.bigEndianRadioButton.UseVisualStyleBackColor = true;
            // 
            // littleEndianRadioButton
            // 
            this.littleEndianRadioButton.AutoSize = true;
            this.littleEndianRadioButton.Checked = true;
            this.littleEndianRadioButton.Location = new System.Drawing.Point(7, 20);
            this.littleEndianRadioButton.Name = "littleEndianRadioButton";
            this.littleEndianRadioButton.Size = new System.Drawing.Size(49, 16);
            this.littleEndianRadioButton.TabIndex = 0;
            this.littleEndianRadioButton.TabStop = true;
            this.littleEndianRadioButton.Text = "Little";
            this.littleEndianRadioButton.UseVisualStyleBackColor = true;
            // 
            // copyToClipBoardButton
            // 
            this.copyToClipBoardButton.Location = new System.Drawing.Point(348, 51);
            this.copyToClipBoardButton.Margin = new System.Windows.Forms.Padding(4);
            this.copyToClipBoardButton.Name = "copyToClipBoardButton";
            this.copyToClipBoardButton.Size = new System.Drawing.Size(109, 29);
            this.copyToClipBoardButton.TabIndex = 10;
            this.copyToClipBoardButton.Text = "Copy";
            this.copyToClipBoardButton.UseVisualStyleBackColor = true;
            this.copyToClipBoardButton.Click += new System.EventHandler(this.copyToClipBoardButton_Click);
            // 
            // symbolTextBox
            // 
            this.symbolTextBox.Location = new System.Drawing.Point(83, 11);
            this.symbolTextBox.Name = "symbolTextBox";
            this.symbolTextBox.Size = new System.Drawing.Size(141, 19);
            this.symbolTextBox.TabIndex = 9;
            this.symbolTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.symbolTextBox_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Symbol";
            // 
            // makeButton
            // 
            this.makeButton.Location = new System.Drawing.Point(231, 51);
            this.makeButton.Margin = new System.Windows.Forms.Padding(4);
            this.makeButton.Name = "makeButton";
            this.makeButton.Size = new System.Drawing.Size(109, 29);
            this.makeButton.TabIndex = 7;
            this.makeButton.Text = "Make";
            this.makeButton.UseVisualStyleBackColor = true;
            this.makeButton.Click += new System.EventHandler(this.makeButton_Click);
            // 
            // subSplitContainer
            // 
            this.subSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.subSplitContainer.Name = "subSplitContainer";
            // 
            // subSplitContainer.Panel1
            // 
            this.subSplitContainer.Panel1.Controls.Add(this.dumpDataGridView);
            // 
            // subSplitContainer.Panel2
            // 
            this.subSplitContainer.Panel2.Controls.Add(this.mainHexBox);
            this.subSplitContainer.Size = new System.Drawing.Size(466, 296);
            this.subSplitContainer.SplitterDistance = 155;
            this.subSplitContainer.TabIndex = 0;
            // 
            // dumpDataGridView
            // 
            this.dumpDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dumpDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dumpDataGridView.Location = new System.Drawing.Point(0, 0);
            this.dumpDataGridView.Margin = new System.Windows.Forms.Padding(4);
            this.dumpDataGridView.Name = "dumpDataGridView";
            this.dumpDataGridView.RowTemplate.Height = 21;
            this.dumpDataGridView.Size = new System.Drawing.Size(155, 296);
            this.dumpDataGridView.TabIndex = 0;
            this.dumpDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dumpDataGridView_CellContentClick);
            this.dumpDataGridView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dumpDataGridView_CellEndEdit);
            this.dumpDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dumpDataGridView_RowPostPaint);
            // 
            // mainHexBox
            // 
            this.mainHexBox.ColumnInfoVisible = true;
            this.mainHexBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainHexBox.Font = new System.Drawing.Font("MS Gothic", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.mainHexBox.HexCasing = Be.Windows.Forms.HexCasing.Lower;
            this.mainHexBox.LineInfoVisible = true;
            this.mainHexBox.Location = new System.Drawing.Point(0, 0);
            this.mainHexBox.Name = "mainHexBox";
            this.mainHexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.mainHexBox.Size = new System.Drawing.Size(307, 296);
            this.mainHexBox.StringViewVisible = true;
            this.mainHexBox.TabIndex = 0;
            this.mainHexBox.UseFixedBytesPerLine = true;
            this.mainHexBox.VScrollBarVisible = true;
            // 
            // DumpForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(466, 471);
            this.Controls.Add(this.splitContainerMain);
            this.Name = "DumpForm";
            this.Text = "DumpForm";
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel1.PerformLayout();
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.endiannessGroupBox.ResumeLayout(false);
            this.endiannessGroupBox.PerformLayout();
            this.subSplitContainer.Panel1.ResumeLayout(false);
            this.subSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.subSplitContainer)).EndInit();
            this.subSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dumpDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.TextBox sizeTextBox;
        private System.Windows.Forms.Button requestButton;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.DataGridView dumpDataGridView;
        private System.Windows.Forms.Button makeButton;
        private System.Windows.Forms.TextBox symbolTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button copyToClipBoardButton;
        private System.Windows.Forms.SplitContainer subSplitContainer;
        private Be.Windows.Forms.HexBox mainHexBox;
        private System.Windows.Forms.GroupBox endiannessGroupBox;
        private System.Windows.Forms.RadioButton tms320c28xEndianRadioButton;
        private System.Windows.Forms.RadioButton bigEndianRadioButton;
        private System.Windows.Forms.RadioButton littleEndianRadioButton;
        private System.Windows.Forms.CheckBox enableOffsetCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox incrementalValueTextBox;
    }
}