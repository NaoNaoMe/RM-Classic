﻿namespace rmApplication
{
	partial class SubViewControl
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.commLogtextBox = new System.Windows.Forms.TextBox();
			this.WarningViewControl = new rmApplication.VersionViewControl();
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.groupColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.checkColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.variableColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addrLockColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.addressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.offsetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
			this.readTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.readValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.userToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.area1ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.area2ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.dispTxDStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.dispRxDStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.mainToolStrip = new System.Windows.Forms.ToolStrip();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.pageValComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.customizeToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.timingValTextBox = new System.Windows.Forms.ToolStripTextBox();
			this.opclCommButton = new System.Windows.Forms.ToolStripButton();
			this.boolDataLogButton = new System.Windows.Forms.ToolStripButton();
			this.dumpEntryButton = new System.Windows.Forms.ToolStripButton();
			this.serialPort = new System.IO.Ports.SerialPort(this.components);
			this.mainTimer = new System.Windows.Forms.Timer(this.components);
			this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.targetVerViewControl = new rmApplication.VersionViewControl();
			this.dutVerViewControl = new rmApplication.VersionViewControl();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			this.mainStatusStrip.SuspendLayout();
			this.mainToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 76);
			this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.commLogtextBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.WarningViewControl);
			this.splitContainer1.Panel2.Controls.Add(this.dataGridView);
			this.splitContainer1.Size = new System.Drawing.Size(993, 412);
			this.splitContainer1.SplitterDistance = 83;
			this.splitContainer1.SplitterWidth = 5;
			this.splitContainer1.TabIndex = 14;
			// 
			// commLogtextBox
			// 
			this.commLogtextBox.BackColor = System.Drawing.SystemColors.Window;
			this.commLogtextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commLogtextBox.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.commLogtextBox.Location = new System.Drawing.Point(0, 0);
			this.commLogtextBox.Multiline = true;
			this.commLogtextBox.Name = "commLogtextBox";
			this.commLogtextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.commLogtextBox.Size = new System.Drawing.Size(993, 83);
			this.commLogtextBox.TabIndex = 0;
			this.commLogtextBox.WordWrap = false;
			// 
			// WarningViewControl
			// 
			this.WarningViewControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.WarningViewControl.Label = "Warning";
			this.WarningViewControl.Location = new System.Drawing.Point(0, 300);
			this.WarningViewControl.Name = "WarningViewControl";
			this.WarningViewControl.Size = new System.Drawing.Size(993, 24);
			this.WarningViewControl.TabIndex = 1;
			this.WarningViewControl.TextBox = "";
			this.WarningViewControl.TextEnabled = false;
			// 
			// dataGridView
			// 
			this.dataGridView.AllowUserToAddRows = false;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.groupColumn,
            this.checkColumn,
            this.sizeColumn,
            this.variableColumn,
            this.addrLockColumn,
            this.addressColumn,
            this.offsetColumn,
            this.nameColumn,
            this.typeColumn,
            this.readTextColumn,
            this.readValueColumn,
            this.writeTextColumn,
            this.writeValueColumn,
            this.writeButtonColumn});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView.Location = new System.Drawing.Point(0, 0);
			this.dataGridView.Margin = new System.Windows.Forms.Padding(4);
			this.dataGridView.Name = "dataGridView";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView.RowTemplate.Height = 21;
			this.dataGridView.Size = new System.Drawing.Size(993, 324);
			this.dataGridView.TabIndex = 0;
			this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellContentClick);
			this.dataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dataGridView_CellValidating);
			this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellValueChanged);
			this.dataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView_EditingControlShowing);
			this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
			this.dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dataGridView_MouseDown);
			// 
			// groupColumn
			// 
			this.groupColumn.DataPropertyName = "Group";
			this.groupColumn.HeaderText = "Group";
			this.groupColumn.Name = "groupColumn";
			// 
			// checkColumn
			// 
			this.checkColumn.DataPropertyName = "Check";
			this.checkColumn.HeaderText = "CK";
			this.checkColumn.Name = "checkColumn";
			this.checkColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.checkColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.checkColumn.Width = 38;
			// 
			// sizeColumn
			// 
			this.sizeColumn.DataPropertyName = "Size";
			this.sizeColumn.HeaderText = "Size";
			this.sizeColumn.Name = "sizeColumn";
			this.sizeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.sizeColumn.Width = 58;
			// 
			// variableColumn
			// 
			this.variableColumn.DataPropertyName = "Variable";
			this.variableColumn.HeaderText = "Variable";
			this.variableColumn.Name = "variableColumn";
			this.variableColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// addrLockColumn
			// 
			this.addrLockColumn.DataPropertyName = "AddrLock";
			this.addrLockColumn.HeaderText = "AddrLock";
			this.addrLockColumn.Name = "addrLockColumn";
			this.addrLockColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.addrLockColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.addrLockColumn.Width = 58;
			// 
			// addressColumn
			// 
			this.addressColumn.DataPropertyName = "Address";
			this.addressColumn.HeaderText = "Address";
			this.addressColumn.Name = "addressColumn";
			this.addressColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// offsetColumn
			// 
			this.offsetColumn.DataPropertyName = "Offset";
			this.offsetColumn.HeaderText = "Offset";
			this.offsetColumn.Name = "offsetColumn";
			this.offsetColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.offsetColumn.Width = 58;
			// 
			// nameColumn
			// 
			this.nameColumn.DataPropertyName = "Name";
			this.nameColumn.HeaderText = "Name";
			this.nameColumn.Name = "nameColumn";
			// 
			// typeColumn
			// 
			this.typeColumn.DataPropertyName = "Type";
			this.typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
			this.typeColumn.HeaderText = "Type";
			this.typeColumn.Name = "typeColumn";
			this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.typeColumn.Width = 48;
			// 
			// readTextColumn
			// 
			this.readTextColumn.HeaderText = "ReadText";
			this.readTextColumn.Name = "readTextColumn";
			this.readTextColumn.Visible = false;
			// 
			// readValueColumn
			// 
			this.readValueColumn.HeaderText = "Read";
			this.readValueColumn.Name = "readValueColumn";
			// 
			// writeTextColumn
			// 
			this.writeTextColumn.DataPropertyName = "WriteText";
			this.writeTextColumn.HeaderText = "WriteText";
			this.writeTextColumn.Name = "writeTextColumn";
			this.writeTextColumn.Visible = false;
			// 
			// writeValueColumn
			// 
			this.writeValueColumn.DataPropertyName = "WriteValue";
			this.writeValueColumn.HeaderText = "Write";
			this.writeValueColumn.Name = "writeValueColumn";
			// 
			// writeButtonColumn
			// 
			this.writeButtonColumn.HeaderText = "WR";
			this.writeButtonColumn.Name = "writeButtonColumn";
			this.writeButtonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.writeButtonColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.writeButtonColumn.Text = "TRG";
			this.writeButtonColumn.Width = 42;
			// 
			// mainStatusStrip
			// 
			this.mainStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userToolStripStatusLabel,
            this.area1ToolStripStatusLabel,
            this.area2ToolStripStatusLabel,
            this.dispTxDStatusLabel,
            this.dispRxDStatusLabel});
			this.mainStatusStrip.Location = new System.Drawing.Point(0, 488);
			this.mainStatusStrip.Name = "mainStatusStrip";
			this.mainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
			this.mainStatusStrip.Size = new System.Drawing.Size(993, 25);
			this.mainStatusStrip.TabIndex = 9;
			this.mainStatusStrip.Text = "statusStrip1";
			// 
			// userToolStripStatusLabel
			// 
			this.userToolStripStatusLabel.AutoSize = false;
			this.userToolStripStatusLabel.Name = "userToolStripStatusLabel";
			this.userToolStripStatusLabel.Size = new System.Drawing.Size(100, 20);
			// 
			// area1ToolStripStatusLabel
			// 
			this.area1ToolStripStatusLabel.Name = "area1ToolStripStatusLabel";
			this.area1ToolStripStatusLabel.Size = new System.Drawing.Size(412, 20);
			this.area1ToolStripStatusLabel.Spring = true;
			this.area1ToolStripStatusLabel.Text = "Empty";
			// 
			// area2ToolStripStatusLabel
			// 
			this.area2ToolStripStatusLabel.Name = "area2ToolStripStatusLabel";
			this.area2ToolStripStatusLabel.Size = new System.Drawing.Size(412, 20);
			this.area2ToolStripStatusLabel.Spring = true;
			this.area2ToolStripStatusLabel.Text = "-";
			// 
			// dispTxDStatusLabel
			// 
			this.dispTxDStatusLabel.BackColor = System.Drawing.SystemColors.Control;
			this.dispTxDStatusLabel.Name = "dispTxDStatusLabel";
			this.dispTxDStatusLabel.Size = new System.Drawing.Size(23, 20);
			this.dispTxDStatusLabel.Text = "Tx";
			// 
			// dispRxDStatusLabel
			// 
			this.dispRxDStatusLabel.Name = "dispRxDStatusLabel";
			this.dispRxDStatusLabel.Size = new System.Drawing.Size(25, 20);
			this.dispRxDStatusLabel.Text = "Rx";
			// 
			// mainToolStrip
			// 
			this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.pageValComboBox,
            this.customizeToolStripButton,
            this.toolStripLabel2,
            this.timingValTextBox,
            this.opclCommButton,
            this.boolDataLogButton,
            this.dumpEntryButton});
			this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
			this.mainToolStrip.Name = "mainToolStrip";
			this.mainToolStrip.Size = new System.Drawing.Size(993, 28);
			this.mainToolStrip.TabIndex = 10;
			this.mainToolStrip.Text = "toolStrip1";
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(44, 25);
			this.toolStripLabel1.Text = "Page:";
			// 
			// pageValComboBox
			// 
			this.pageValComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.pageValComboBox.Name = "pageValComboBox";
			this.pageValComboBox.Size = new System.Drawing.Size(239, 28);
			this.pageValComboBox.SelectedIndexChanged += new System.EventHandler(this.pageValComboBox_SelectedIndexChanged);
			// 
			// customizeToolStripButton
			// 
			this.customizeToolStripButton.Image = global::rmApplication.Properties.Resources.Complete_and_ok_gray;
			this.customizeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.customizeToolStripButton.Name = "customizeToolStripButton";
			this.customizeToolStripButton.Size = new System.Drawing.Size(102, 25);
			this.customizeToolStripButton.Text = "Customize";
			this.customizeToolStripButton.Click += new System.EventHandler(this.customizeToolStripButton_Click);
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(87, 25);
			this.toolStripLabel2.Text = "Timing(ms):";
			// 
			// timingValTextBox
			// 
			this.timingValTextBox.Name = "timingValTextBox";
			this.timingValTextBox.Size = new System.Drawing.Size(79, 28);
			this.timingValTextBox.Text = "500";
			this.timingValTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.timingValTextBox_KeyPress);
			// 
			// opclCommButton
			// 
			this.opclCommButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.opclCommButton.AutoSize = false;
			this.opclCommButton.Image = global::rmApplication.Properties.Resources.FlagThread_white;
			this.opclCommButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.opclCommButton.Name = "opclCommButton";
			this.opclCommButton.Size = new System.Drawing.Size(106, 24);
			this.opclCommButton.Text = "Comm Close";
			this.opclCommButton.Click += new System.EventHandler(this.opclCommButton_Click);
			// 
			// boolDataLogButton
			// 
			this.boolDataLogButton.AutoSize = false;
			this.boolDataLogButton.Image = global::rmApplication.Properties.Resources.Complete_and_ok_gray;
			this.boolDataLogButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.boolDataLogButton.Name = "boolDataLogButton";
			this.boolDataLogButton.Size = new System.Drawing.Size(84, 24);
			this.boolDataLogButton.Text = "Stop Log";
			this.boolDataLogButton.Click += new System.EventHandler(this.boolDataLogButton_Click);
			// 
			// dumpEntryButton
			// 
			this.dumpEntryButton.Image = global::rmApplication.Properties.Resources.VirtualMachine;
			this.dumpEntryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.dumpEntryButton.Name = "dumpEntryButton";
			this.dumpEntryButton.Size = new System.Drawing.Size(72, 25);
			this.dumpEntryButton.Text = "dump";
			this.dumpEntryButton.Click += new System.EventHandler(this.dumpEntryButton_Click);
			// 
			// mainTimer
			// 
			this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
			// 
			// contextMenuStrip
			// 
			this.contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.contextMenuStrip.Name = "contextMenuStrip";
			this.contextMenuStrip.Size = new System.Drawing.Size(61, 4);
			this.contextMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStrip_ItemClicked);
			// 
			// targetVerViewControl
			// 
			this.targetVerViewControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.targetVerViewControl.Label = "Target Version";
			this.targetVerViewControl.Location = new System.Drawing.Point(0, 52);
			this.targetVerViewControl.Name = "targetVerViewControl";
			this.targetVerViewControl.Size = new System.Drawing.Size(993, 24);
			this.targetVerViewControl.TabIndex = 12;
			this.targetVerViewControl.TextBox = "";
			this.targetVerViewControl.TextEnabled = false;
			// 
			// dutVerViewControl
			// 
			this.dutVerViewControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.dutVerViewControl.Label = "DUT Version";
			this.dutVerViewControl.Location = new System.Drawing.Point(0, 28);
			this.dutVerViewControl.Name = "dutVerViewControl";
			this.dutVerViewControl.Size = new System.Drawing.Size(993, 24);
			this.dutVerViewControl.TabIndex = 11;
			this.dutVerViewControl.TextBox = "";
			this.dutVerViewControl.TextEnabled = false;
			// 
			// SubViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.targetVerViewControl);
			this.Controls.Add(this.dutVerViewControl);
			this.Controls.Add(this.mainStatusStrip);
			this.Controls.Add(this.mainToolStrip);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "SubViewControl";
			this.Size = new System.Drawing.Size(993, 513);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			this.mainStatusStrip.ResumeLayout(false);
			this.mainStatusStrip.PerformLayout();
			this.mainToolStrip.ResumeLayout(false);
			this.mainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox commLogtextBox;
		private System.Windows.Forms.DataGridView dataGridView;
		private VersionViewControl targetVerViewControl;
		private VersionViewControl dutVerViewControl;
		private System.Windows.Forms.StatusStrip mainStatusStrip;
		private System.Windows.Forms.ToolStripStatusLabel userToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel area1ToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel area2ToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel dispTxDStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel dispRxDStatusLabel;
		private System.Windows.Forms.ToolStrip mainToolStrip;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripComboBox pageValComboBox;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripTextBox timingValTextBox;
		private System.Windows.Forms.ToolStripButton opclCommButton;
		private System.Windows.Forms.ToolStripButton boolDataLogButton;
		private System.Windows.Forms.ToolStripButton dumpEntryButton;
		private System.Windows.Forms.Timer mainTimer;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
		private System.IO.Ports.SerialPort serialPort;
		private VersionViewControl WarningViewControl;
		private System.Windows.Forms.ToolStripButton customizeToolStripButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn groupColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn checkColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn variableColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn addrLockColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addressColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn offsetColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewComboBoxColumn typeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn readTextColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn readValueColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn writeTextColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn writeValueColumn;
		private System.Windows.Forms.DataGridViewButtonColumn writeButtonColumn;
	}
}
