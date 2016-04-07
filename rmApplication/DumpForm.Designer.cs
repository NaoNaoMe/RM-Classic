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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.makeButton = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.typeColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.label3 = new System.Windows.Forms.Label();
			this.variableTextBox = new System.Windows.Forms.TextBox();
			this.copyToClipBoardButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(11, 34);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "Address";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(11, 57);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "Size";
			// 
			// dumpTextBox
			// 
			this.dumpTextBox.Location = new System.Drawing.Point(13, 77);
			this.dumpTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.dumpTextBox.Multiline = true;
			this.dumpTextBox.Name = "dumpTextBox";
			this.dumpTextBox.Size = new System.Drawing.Size(234, 50);
			this.dumpTextBox.TabIndex = 3;
			this.dumpTextBox.Text = "00-AA-23-DD";
			// 
			// addressTextBox
			// 
			this.addressTextBox.Location = new System.Drawing.Point(62, 31);
			this.addressTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.addressTextBox.Name = "addressTextBox";
			this.addressTextBox.Size = new System.Drawing.Size(107, 19);
			this.addressTextBox.TabIndex = 4;
			this.addressTextBox.Text = "20000000";
			// 
			// sizeTextBox
			// 
			this.sizeTextBox.Location = new System.Drawing.Point(62, 54);
			this.sizeTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.sizeTextBox.Name = "sizeTextBox";
			this.sizeTextBox.Size = new System.Drawing.Size(107, 19);
			this.sizeTextBox.TabIndex = 5;
			this.sizeTextBox.Text = "1";
			// 
			// requestButton
			// 
			this.requestButton.Location = new System.Drawing.Point(252, 51);
			this.requestButton.Margin = new System.Windows.Forms.Padding(2);
			this.requestButton.Name = "requestButton";
			this.requestButton.Size = new System.Drawing.Size(82, 22);
			this.requestButton.TabIndex = 6;
			this.requestButton.Text = "Request";
			this.requestButton.UseVisualStyleBackColor = true;
			this.requestButton.Click += new System.EventHandler(this.requestButton_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.copyToClipBoardButton);
			this.splitContainer1.Panel1.Controls.Add(this.variableTextBox);
			this.splitContainer1.Panel1.Controls.Add(this.label3);
			this.splitContainer1.Panel1.Controls.Add(this.makeButton);
			this.splitContainer1.Panel1.Controls.Add(this.dumpTextBox);
			this.splitContainer1.Panel1.Controls.Add(this.requestButton);
			this.splitContainer1.Panel1.Controls.Add(this.label1);
			this.splitContainer1.Panel1.Controls.Add(this.addressTextBox);
			this.splitContainer1.Panel1.Controls.Add(this.sizeTextBox);
			this.splitContainer1.Panel1.Controls.Add(this.label2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.dataGridView1);
			this.splitContainer1.Size = new System.Drawing.Size(346, 304);
			this.splitContainer1.SplitterDistance = 138;
			this.splitContainer1.TabIndex = 7;
			// 
			// makeButton
			// 
			this.makeButton.Location = new System.Drawing.Point(252, 78);
			this.makeButton.Name = "makeButton";
			this.makeButton.Size = new System.Drawing.Size(82, 22);
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
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.RowTemplate.Height = 21;
			this.dataGridView1.Size = new System.Drawing.Size(346, 162);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
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
			this.typeColumn.HeaderText = "Type";
			this.typeColumn.Name = "typeColumn";
			this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.typeColumn.Width = 42;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(11, 11);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(42, 12);
			this.label3.TabIndex = 8;
			this.label3.Text = "Variable";
			// 
			// variableTextBox
			// 
			this.variableTextBox.Location = new System.Drawing.Point(62, 8);
			this.variableTextBox.Margin = new System.Windows.Forms.Padding(2);
			this.variableTextBox.Name = "variableTextBox";
			this.variableTextBox.Size = new System.Drawing.Size(107, 19);
			this.variableTextBox.TabIndex = 9;
			this.variableTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.variableTextBox_KeyDown);
			// 
			// copyToClipBoardButton
			// 
			this.copyToClipBoardButton.Location = new System.Drawing.Point(252, 106);
			this.copyToClipBoardButton.Name = "copyToClipBoardButton";
			this.copyToClipBoardButton.Size = new System.Drawing.Size(82, 22);
			this.copyToClipBoardButton.TabIndex = 10;
			this.copyToClipBoardButton.Text = "Copy";
			this.copyToClipBoardButton.UseVisualStyleBackColor = true;
			this.copyToClipBoardButton.Click += new System.EventHandler(this.copyToClipBoardButton_Click);
			// 
			// DumpForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(346, 304);
			this.Controls.Add(this.splitContainer1);
			this.Margin = new System.Windows.Forms.Padding(2);
			this.Name = "DumpForm";
			this.Text = "DumpForm";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
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
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
		private System.Windows.Forms.DataGridViewButtonColumn typeColumn;
		private System.Windows.Forms.Button makeButton;
		private System.Windows.Forms.TextBox variableTextBox;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button copyToClipBoardButton;
	}
}