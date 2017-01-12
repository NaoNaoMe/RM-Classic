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
            this.dumpTextBox = new System.Windows.Forms.TextBox();
            this.addressTextBox = new System.Windows.Forms.TextBox();
            this.sizeTextBox = new System.Windows.Forms.TextBox();
            this.requestButton = new System.Windows.Forms.Button();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.radioButtonLittleEndian = new System.Windows.Forms.RadioButton();
            this.radioButtonBigEndian = new System.Windows.Forms.RadioButton();
            this.copyToClipBoardButton = new System.Windows.Forms.Button();
            this.variableTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.makeButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            // dumpTextBox
            // 
            this.dumpTextBox.Location = new System.Drawing.Point(17, 103);
            this.dumpTextBox.Multiline = true;
            this.dumpTextBox.Name = "dumpTextBox";
            this.dumpTextBox.Size = new System.Drawing.Size(311, 65);
            this.dumpTextBox.TabIndex = 3;
            this.dumpTextBox.Text = "00-AA-23-DD";
            // 
            // addressTextBox
            // 
            this.addressTextBox.Location = new System.Drawing.Point(83, 41);
            this.addressTextBox.Name = "addressTextBox";
            this.addressTextBox.Size = new System.Drawing.Size(141, 19);
            this.addressTextBox.TabIndex = 4;
            this.addressTextBox.Text = "20000000";
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
            this.requestButton.Location = new System.Drawing.Point(337, 15);
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
            this.splitContainerMain.Panel1.Controls.Add(this.radioButtonLittleEndian);
            this.splitContainerMain.Panel1.Controls.Add(this.radioButtonBigEndian);
            this.splitContainerMain.Panel1.Controls.Add(this.copyToClipBoardButton);
            this.splitContainerMain.Panel1.Controls.Add(this.variableTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.label3);
            this.splitContainerMain.Panel1.Controls.Add(this.makeButton);
            this.splitContainerMain.Panel1.Controls.Add(this.dumpTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.requestButton);
            this.splitContainerMain.Panel1.Controls.Add(this.label1);
            this.splitContainerMain.Panel1.Controls.Add(this.addressTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.sizeTextBox);
            this.splitContainerMain.Panel1.Controls.Add(this.label2);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.dataGridView1);
            this.splitContainerMain.Size = new System.Drawing.Size(461, 405);
            this.splitContainerMain.SplitterDistance = 177;
            this.splitContainerMain.SplitterWidth = 5;
            this.splitContainerMain.TabIndex = 7;
            // 
            // radioButtonLittleEndian
            // 
            this.radioButtonLittleEndian.AutoSize = true;
            this.radioButtonLittleEndian.Location = new System.Drawing.Point(336, 80);
            this.radioButtonLittleEndian.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonLittleEndian.Name = "radioButtonLittleEndian";
            this.radioButtonLittleEndian.Size = new System.Drawing.Size(87, 16);
            this.radioButtonLittleEndian.TabIndex = 12;
            this.radioButtonLittleEndian.Text = "Little Endian";
            this.radioButtonLittleEndian.UseVisualStyleBackColor = true;
            // 
            // radioButtonBigEndian
            // 
            this.radioButtonBigEndian.AutoSize = true;
            this.radioButtonBigEndian.Checked = true;
            this.radioButtonBigEndian.Location = new System.Drawing.Point(337, 51);
            this.radioButtonBigEndian.Margin = new System.Windows.Forms.Padding(4);
            this.radioButtonBigEndian.Name = "radioButtonBigEndian";
            this.radioButtonBigEndian.Size = new System.Drawing.Size(78, 16);
            this.radioButtonBigEndian.TabIndex = 11;
            this.radioButtonBigEndian.TabStop = true;
            this.radioButtonBigEndian.Text = "Big Endian";
            this.radioButtonBigEndian.UseVisualStyleBackColor = true;
            // 
            // copyToClipBoardButton
            // 
            this.copyToClipBoardButton.Location = new System.Drawing.Point(336, 141);
            this.copyToClipBoardButton.Margin = new System.Windows.Forms.Padding(4);
            this.copyToClipBoardButton.Name = "copyToClipBoardButton";
            this.copyToClipBoardButton.Size = new System.Drawing.Size(109, 29);
            this.copyToClipBoardButton.TabIndex = 10;
            this.copyToClipBoardButton.Text = "Copy";
            this.copyToClipBoardButton.UseVisualStyleBackColor = true;
            this.copyToClipBoardButton.Click += new System.EventHandler(this.copyToClipBoardButton_Click);
            // 
            // variableTextBox
            // 
            this.variableTextBox.Location = new System.Drawing.Point(83, 11);
            this.variableTextBox.Name = "variableTextBox";
            this.variableTextBox.Size = new System.Drawing.Size(141, 19);
            this.variableTextBox.TabIndex = 9;
            this.variableTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.variableTextBox_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "Variable";
            // 
            // makeButton
            // 
            this.makeButton.Location = new System.Drawing.Point(336, 104);
            this.makeButton.Margin = new System.Windows.Forms.Padding(4);
            this.makeButton.Name = "makeButton";
            this.makeButton.Size = new System.Drawing.Size(109, 29);
            this.makeButton.TabIndex = 7;
            this.makeButton.Text = "Make";
            this.makeButton.UseVisualStyleBackColor = true;
            this.makeButton.Click += new System.EventHandler(this.makeButton_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sizeColumn,
            this.typeColumn});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(461, 223);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
            // 
            // sizeColumn
            // 
            this.sizeColumn.DataPropertyName = "Size";
            this.sizeColumn.HeaderText = "Size";
            this.sizeColumn.Name = "sizeColumn";
            this.sizeColumn.Width = 42;
            // 
            // typeColumn
            // 
            this.typeColumn.DataPropertyName = "Type";
            this.typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.Name = "typeColumn";
            this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.typeColumn.Width = 48;
            // 
            // DumpForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(461, 405);
            this.Controls.Add(this.splitContainerMain);
            this.Name = "DumpForm";
            this.Text = "DumpForm";
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel1.PerformLayout();
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox addressTextBox;
        private System.Windows.Forms.TextBox sizeTextBox;
        private System.Windows.Forms.Button requestButton;
        public System.Windows.Forms.TextBox dumpTextBox;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button makeButton;
        private System.Windows.Forms.TextBox variableTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button copyToClipBoardButton;
        private System.Windows.Forms.RadioButton radioButtonLittleEndian;
        private System.Windows.Forms.RadioButton radioButtonBigEndian;
        private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn typeColumn;
    }
}