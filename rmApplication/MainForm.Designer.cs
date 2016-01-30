namespace rmApplication
{
	partial class MainForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
			this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
			this.userToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.area1ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.area2ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.dispTxDStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.dispRxDStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openViewFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openMapFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveViewFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.customizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.changeViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.settingCommToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.indexToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.searchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainTimer = new System.Windows.Forms.Timer(this.components);
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.checkColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.variantColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.addrLockColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.addressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.offsetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.typeColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.readTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.readValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeTextColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.writeButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
			this.commLogtextBox = new System.Windows.Forms.TextBox();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
			this.pageValComboBox = new System.Windows.Forms.ToolStripComboBox();
			this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
			this.timingValTextBox = new System.Windows.Forms.ToolStripTextBox();
			this.opclCommButton = new System.Windows.Forms.ToolStripButton();
			this.mainToolStrip = new System.Windows.Forms.ToolStrip();
			this.boolDataLogButton = new System.Windows.Forms.ToolStripButton();
			this.dumpEntryButton = new System.Windows.Forms.ToolStripButton();
			this.settingVerViewControl = new rmApplication.VersionViewControl();
			this.targetVerViewControl = new rmApplication.VersionViewControl();
			this.dutVerViewControl = new rmApplication.VersionViewControl();
			this.saveMapFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mainStatusStrip.SuspendLayout();
			this.mainMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.mainToolStrip.SuspendLayout();
			this.SuspendLayout();
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
			this.mainStatusStrip.Location = new System.Drawing.Point(0, 315);
			this.mainStatusStrip.Name = "mainStatusStrip";
			this.mainStatusStrip.Size = new System.Drawing.Size(604, 25);
			this.mainStatusStrip.TabIndex = 1;
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
			this.area1ToolStripStatusLabel.Size = new System.Drawing.Size(222, 20);
			this.area1ToolStripStatusLabel.Spring = true;
			this.area1ToolStripStatusLabel.Text = "Empty";
			// 
			// area2ToolStripStatusLabel
			// 
			this.area2ToolStripStatusLabel.Name = "area2ToolStripStatusLabel";
			this.area2ToolStripStatusLabel.Size = new System.Drawing.Size(222, 20);
			this.area2ToolStripStatusLabel.Spring = true;
			this.area2ToolStripStatusLabel.Text = "Empty";
			// 
			// dispTxDStatusLabel
			// 
			this.dispTxDStatusLabel.BackColor = System.Drawing.SystemColors.Control;
			this.dispTxDStatusLabel.Name = "dispTxDStatusLabel";
			this.dispTxDStatusLabel.Size = new System.Drawing.Size(22, 20);
			this.dispTxDStatusLabel.Text = "Tx";
			// 
			// dispRxDStatusLabel
			// 
			this.dispRxDStatusLabel.Name = "dispRxDStatusLabel";
			this.dispRxDStatusLabel.Size = new System.Drawing.Size(23, 20);
			this.dispRxDStatusLabel.Text = "Rx";
			// 
			// mainMenuStrip
			// 
			this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
			this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.mainMenuStrip.Name = "mainMenuStrip";
			this.mainMenuStrip.Size = new System.Drawing.Size(604, 26);
			this.mainMenuStrip.TabIndex = 3;
			this.mainMenuStrip.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator6,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator7,
            this.printToolStripMenuItem,
            this.printPreviewToolStripMenuItem,
            this.toolStripSeparator8,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(40, 22);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// newToolStripMenuItem
			// 
			this.newToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("newToolStripMenuItem.Image")));
			this.newToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.newToolStripMenuItem.Name = "newToolStripMenuItem";
			this.newToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.newToolStripMenuItem.Text = "&New";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openViewFileToolStripMenuItem,
            this.openMapFileToolStripMenuItem});
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.openToolStripMenuItem.Text = "&Open";
			// 
			// openViewFileToolStripMenuItem
			// 
			this.openViewFileToolStripMenuItem.Name = "openViewFileToolStripMenuItem";
			this.openViewFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.openViewFileToolStripMenuItem.Text = "&View File";
			this.openViewFileToolStripMenuItem.Click += new System.EventHandler(this.openViewFileToolStripMenuItem_Click);
			// 
			// openMapFileToolStripMenuItem
			// 
			this.openMapFileToolStripMenuItem.Name = "openMapFileToolStripMenuItem";
			this.openMapFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.openMapFileToolStripMenuItem.Text = "&Map File";
			this.openMapFileToolStripMenuItem.Click += new System.EventHandler(this.openMapFileToolStripMenuItem_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(154, 6);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
			this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.saveToolStripMenuItem.Text = "&Save";
			// 
			// saveAsToolStripMenuItem
			// 
			this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveViewFileToolStripMenuItem,
            this.saveMapFileToolStripMenuItem});
			this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
			this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.saveAsToolStripMenuItem.Text = "Save &As";
			// 
			// saveViewFileToolStripMenuItem
			// 
			this.saveViewFileToolStripMenuItem.Name = "saveViewFileToolStripMenuItem";
			this.saveViewFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveViewFileToolStripMenuItem.Text = "&View File";
			this.saveViewFileToolStripMenuItem.Click += new System.EventHandler(this.saveViewFileToolStripMenuItem_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(154, 6);
			// 
			// printToolStripMenuItem
			// 
			this.printToolStripMenuItem.Enabled = false;
			this.printToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("printToolStripMenuItem.Image")));
			this.printToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.printToolStripMenuItem.Name = "printToolStripMenuItem";
			this.printToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.printToolStripMenuItem.Text = "&Print";
			// 
			// printPreviewToolStripMenuItem
			// 
			this.printPreviewToolStripMenuItem.Enabled = false;
			this.printPreviewToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("printPreviewToolStripMenuItem.Image")));
			this.printPreviewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
			this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.printPreviewToolStripMenuItem.Text = "Print Pre&view";
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(154, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(157, 26);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// editToolStripMenuItem
			// 
			this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator9,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator10,
            this.selectAllToolStripMenuItem});
			this.editToolStripMenuItem.Enabled = false;
			this.editToolStripMenuItem.Name = "editToolStripMenuItem";
			this.editToolStripMenuItem.Size = new System.Drawing.Size(42, 22);
			this.editToolStripMenuItem.Text = "&Edit";
			// 
			// undoToolStripMenuItem
			// 
			this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
			this.undoToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.undoToolStripMenuItem.Text = "&Undo";
			// 
			// redoToolStripMenuItem
			// 
			this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
			this.redoToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.redoToolStripMenuItem.Text = "&Redo";
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(127, 6);
			// 
			// cutToolStripMenuItem
			// 
			this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
			this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
			this.cutToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.cutToolStripMenuItem.Text = "Cu&t";
			// 
			// copyToolStripMenuItem
			// 
			this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
			this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
			this.copyToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.copyToolStripMenuItem.Text = "&Copy";
			// 
			// pasteToolStripMenuItem
			// 
			this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
			this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
			this.pasteToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.pasteToolStripMenuItem.Text = "&Paste";
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(127, 6);
			// 
			// selectAllToolStripMenuItem
			// 
			this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
			this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.selectAllToolStripMenuItem.Text = "Select &All";
			// 
			// toolsToolStripMenuItem
			// 
			this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.customizeToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.settingCommToolStripMenuItem});
			this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			this.toolsToolStripMenuItem.Size = new System.Drawing.Size(50, 22);
			this.toolsToolStripMenuItem.Text = "&Tools";
			// 
			// customizeToolStripMenuItem
			// 
			this.customizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingViewToolStripMenuItem,
            this.changeViewToolStripMenuItem});
			this.customizeToolStripMenuItem.Name = "customizeToolStripMenuItem";
			this.customizeToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.customizeToolStripMenuItem.Text = "&Customize";
			// 
			// settingViewToolStripMenuItem
			// 
			this.settingViewToolStripMenuItem.Name = "settingViewToolStripMenuItem";
			this.settingViewToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.settingViewToolStripMenuItem.Text = "&Setting View";
			this.settingViewToolStripMenuItem.Click += new System.EventHandler(this.settingViewToolStripMenuItem_Click);
			// 
			// changeViewToolStripMenuItem
			// 
			this.changeViewToolStripMenuItem.Name = "changeViewToolStripMenuItem";
			this.changeViewToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
			this.changeViewToolStripMenuItem.Text = "&Change View";
			this.changeViewToolStripMenuItem.Click += new System.EventHandler(this.changeViewToolStripMenuItem_Click);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.optionsToolStripMenuItem.Text = "&Options";
			this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
			// 
			// settingCommToolStripMenuItem
			// 
			this.settingCommToolStripMenuItem.Name = "settingCommToolStripMenuItem";
			this.settingCommToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
			this.settingCommToolStripMenuItem.Text = "&Setting Comm";
			this.settingCommToolStripMenuItem.Click += new System.EventHandler(this.settingCommToolStripMenuItem_Click);
			// 
			// helpToolStripMenuItem
			// 
			this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentsToolStripMenuItem,
            this.indexToolStripMenuItem,
            this.searchToolStripMenuItem,
            this.toolStripSeparator11,
            this.aboutToolStripMenuItem});
			this.helpToolStripMenuItem.Enabled = false;
			this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			this.helpToolStripMenuItem.Size = new System.Drawing.Size(46, 22);
			this.helpToolStripMenuItem.Text = "&Help";
			// 
			// contentsToolStripMenuItem
			// 
			this.contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
			this.contentsToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.contentsToolStripMenuItem.Text = "&Contents";
			// 
			// indexToolStripMenuItem
			// 
			this.indexToolStripMenuItem.Name = "indexToolStripMenuItem";
			this.indexToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.indexToolStripMenuItem.Text = "&Index";
			// 
			// searchToolStripMenuItem
			// 
			this.searchToolStripMenuItem.Name = "searchToolStripMenuItem";
			this.searchToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.searchToolStripMenuItem.Text = "&Search";
			// 
			// toolStripSeparator11
			// 
			this.toolStripSeparator11.Name = "toolStripSeparator11";
			this.toolStripSeparator11.Size = new System.Drawing.Size(125, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
			this.aboutToolStripMenuItem.Text = "&About...";
			// 
			// mainTimer
			// 
			this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
			// 
			// dataGridView1
			// 
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.checkColumn,
            this.sizeColumn,
            this.variantColumn,
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
			this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGridView1.Location = new System.Drawing.Point(0, 0);
			this.dataGridView1.Name = "dataGridView1";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView1.RowTemplate.Height = 21;
			this.dataGridView1.Size = new System.Drawing.Size(604, 121);
			this.dataGridView1.TabIndex = 0;
			this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
			this.dataGridView1.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEndEdit);
			this.dataGridView1.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView1_EditingControlShowing);
			this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
			// 
			// checkColumn
			// 
			this.checkColumn.DataPropertyName = "Check";
			this.checkColumn.Frozen = true;
			this.checkColumn.HeaderText = "CK";
			this.checkColumn.Name = "checkColumn";
			this.checkColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.checkColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.checkColumn.Width = 42;
			// 
			// sizeColumn
			// 
			this.sizeColumn.DataPropertyName = "Size";
			this.sizeColumn.HeaderText = "Size";
			this.sizeColumn.Name = "sizeColumn";
			this.sizeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.sizeColumn.Width = 42;
			// 
			// variantColumn
			// 
			this.variantColumn.DataPropertyName = "Variant";
			this.variantColumn.HeaderText = "Variant";
			this.variantColumn.Name = "variantColumn";
			this.variantColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// addrLockColumn
			// 
			this.addrLockColumn.DataPropertyName = "AddrLock";
			this.addrLockColumn.HeaderText = "AddrLock";
			this.addrLockColumn.Name = "addrLockColumn";
			this.addrLockColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			this.addrLockColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.addrLockColumn.Width = 42;
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
			this.offsetColumn.Width = 62;
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
			this.typeColumn.HeaderText = "Type";
			this.typeColumn.Name = "typeColumn";
			this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			this.typeColumn.Width = 42;
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
			// commLogtextBox
			// 
			this.commLogtextBox.BackColor = System.Drawing.SystemColors.Window;
			this.commLogtextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commLogtextBox.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.commLogtextBox.Location = new System.Drawing.Point(0, 0);
			this.commLogtextBox.Margin = new System.Windows.Forms.Padding(2);
			this.commLogtextBox.Multiline = true;
			this.commLogtextBox.Name = "commLogtextBox";
			this.commLogtextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.commLogtextBox.Size = new System.Drawing.Size(604, 83);
			this.commLogtextBox.TabIndex = 0;
			this.commLogtextBox.WordWrap = false;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 107);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.commLogtextBox);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.dataGridView1);
			this.splitContainer1.Size = new System.Drawing.Size(604, 208);
			this.splitContainer1.SplitterDistance = 83;
			this.splitContainer1.TabIndex = 8;
			// 
			// toolStripLabel1
			// 
			this.toolStripLabel1.Name = "toolStripLabel1";
			this.toolStripLabel1.Size = new System.Drawing.Size(41, 24);
			this.toolStripLabel1.Text = "Page:";
			// 
			// pageValComboBox
			// 
			this.pageValComboBox.Name = "pageValComboBox";
			this.pageValComboBox.Size = new System.Drawing.Size(75, 27);
			this.pageValComboBox.SelectedIndexChanged += new System.EventHandler(this.pageValComboBox_SelectedIndexChanged);
			// 
			// toolStripLabel2
			// 
			this.toolStripLabel2.Name = "toolStripLabel2";
			this.toolStripLabel2.Size = new System.Drawing.Size(53, 24);
			this.toolStripLabel2.Text = "Timing:";
			// 
			// timingValTextBox
			// 
			this.timingValTextBox.Name = "timingValTextBox";
			this.timingValTextBox.Size = new System.Drawing.Size(60, 27);
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
			// mainToolStrip
			// 
			this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
			this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.pageValComboBox,
            this.toolStripLabel2,
            this.timingValTextBox,
            this.opclCommButton,
            this.boolDataLogButton,
            this.dumpEntryButton});
			this.mainToolStrip.Location = new System.Drawing.Point(0, 26);
			this.mainToolStrip.Name = "mainToolStrip";
			this.mainToolStrip.Size = new System.Drawing.Size(604, 27);
			this.mainToolStrip.TabIndex = 2;
			this.mainToolStrip.Text = "toolStrip1";
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
			this.dumpEntryButton.Size = new System.Drawing.Size(65, 24);
			this.dumpEntryButton.Text = "dump";
			this.dumpEntryButton.Click += new System.EventHandler(this.dumpEntryButton_Click);
			// 
			// settingVerViewControl
			// 
			this.settingVerViewControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.settingVerViewControl.Label = "_StV";
			this.settingVerViewControl.Location = new System.Drawing.Point(0, 89);
			this.settingVerViewControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.settingVerViewControl.Name = "settingVerViewControl";
			this.settingVerViewControl.Size = new System.Drawing.Size(604, 18);
			this.settingVerViewControl.TabIndex = 7;
			this.settingVerViewControl.TextBox = "";
			// 
			// targetVerViewControl
			// 
			this.targetVerViewControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.targetVerViewControl.Label = "_TgV";
			this.targetVerViewControl.Location = new System.Drawing.Point(0, 71);
			this.targetVerViewControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.targetVerViewControl.Name = "targetVerViewControl";
			this.targetVerViewControl.Size = new System.Drawing.Size(604, 18);
			this.targetVerViewControl.TabIndex = 6;
			this.targetVerViewControl.TextBox = "";
			// 
			// dutVerViewControl
			// 
			this.dutVerViewControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.dutVerViewControl.Label = "From DUT";
			this.dutVerViewControl.Location = new System.Drawing.Point(0, 53);
			this.dutVerViewControl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.dutVerViewControl.Name = "dutVerViewControl";
			this.dutVerViewControl.Size = new System.Drawing.Size(604, 18);
			this.dutVerViewControl.TabIndex = 5;
			this.dutVerViewControl.TextBox = "";
			// 
			// saveMapFileToolStripMenuItem
			// 
			this.saveMapFileToolStripMenuItem.Name = "saveMapFileToolStripMenuItem";
			this.saveMapFileToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
			this.saveMapFileToolStripMenuItem.Text = "&Map File";
			this.saveMapFileToolStripMenuItem.Click += new System.EventHandler(this.saveMapFileToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(604, 340);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.settingVerViewControl);
			this.Controls.Add(this.targetVerViewControl);
			this.Controls.Add(this.dutVerViewControl);
			this.Controls.Add(this.mainStatusStrip);
			this.Controls.Add(this.mainToolStrip);
			this.Controls.Add(this.mainMenuStrip);
			this.Name = "MainForm";
			this.Text = "RM Classic";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.mainStatusStrip.ResumeLayout(false);
			this.mainStatusStrip.PerformLayout();
			this.mainMenuStrip.ResumeLayout(false);
			this.mainMenuStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.mainToolStrip.ResumeLayout(false);
			this.mainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.StatusStrip mainStatusStrip;
		public System.IO.Ports.SerialPort serialPort1;
		private System.Windows.Forms.ToolStripStatusLabel userToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel area1ToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel area2ToolStripStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel dispTxDStatusLabel;
		private System.Windows.Forms.ToolStripStatusLabel dispRxDStatusLabel;
		private System.Windows.Forms.MenuStrip mainMenuStrip;
		private System.Windows.Forms.Timer mainTimer;
		private VersionViewControl dutVerViewControl;
		private VersionViewControl targetVerViewControl;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openViewFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openMapFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem customizeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingViewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changeViewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingCommToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem contentsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem indexToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem searchToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveViewFileToolStripMenuItem;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.TextBox commLogtextBox;
		private VersionViewControl settingVerViewControl;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStripLabel toolStripLabel1;
		private System.Windows.Forms.ToolStripComboBox pageValComboBox;
		private System.Windows.Forms.ToolStripLabel toolStripLabel2;
		private System.Windows.Forms.ToolStripTextBox timingValTextBox;
		private System.Windows.Forms.ToolStripButton opclCommButton;
		private System.Windows.Forms.ToolStrip mainToolStrip;
		private System.Windows.Forms.ToolStripButton boolDataLogButton;
		private System.Windows.Forms.ToolStripButton dumpEntryButton;
		private System.Windows.Forms.DataGridViewCheckBoxColumn checkColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn variantColumn;
		private System.Windows.Forms.DataGridViewCheckBoxColumn addrLockColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn addressColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn offsetColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
		private System.Windows.Forms.DataGridViewButtonColumn typeColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn readTextColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn readValueColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn writeTextColumn;
		private System.Windows.Forms.DataGridViewTextBoxColumn writeValueColumn;
		private System.Windows.Forms.DataGridViewButtonColumn writeButtonColumn;
		private System.Windows.Forms.ToolStripMenuItem saveMapFileToolStripMenuItem;
	}
}

