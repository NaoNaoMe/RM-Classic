namespace rmApplication
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubViewControl));
            this.mainDataGridView = new System.Windows.Forms.DataGridView();
            this.mainStatusStrip = new System.Windows.Forms.StatusStrip();
            this.userToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.area1ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.area2ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.activityToolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.viewPageComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.customizeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.timeStepToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.commToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.dumpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.logLengthToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.copyLogToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.terminalToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mainTimer = new System.Windows.Forms.Timer(this.components);
            this.rxPeriodToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.targetVersionViewControl = new rmApplication.NoticeViewControl();
            this.receivedVersionViewControl = new rmApplication.NoticeViewControl();
            ((System.ComponentModel.ISupportInitialize)(this.mainDataGridView)).BeginInit();
            this.mainStatusStrip.SuspendLayout();
            this.mainToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainDataGridView
            // 
            this.mainDataGridView.AllowUserToAddRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mainDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.mainDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.mainDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.mainDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainDataGridView.Location = new System.Drawing.Point(0, 63);
            this.mainDataGridView.Name = "mainDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mainDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.mainDataGridView.RowTemplate.Height = 21;
            this.mainDataGridView.Size = new System.Drawing.Size(1100, 297);
            this.mainDataGridView.TabIndex = 0;
            this.mainDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainDataGridView_CellContentClick);
            this.mainDataGridView.CellValidated += new System.Windows.Forms.DataGridViewCellEventHandler(this.mainDataGridView_CellValidated);
            this.mainDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.mainDataGridView_CellValidating);
            this.mainDataGridView.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.mainDataGridView_EditingControlShowing);
            this.mainDataGridView.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.mainDataGridView_RowPostPaint);
            this.mainDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.mainDataGridView_KeyDown);
            // 
            // mainStatusStrip
            // 
            this.mainStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userToolStripStatusLabel,
            this.area1ToolStripStatusLabel,
            this.area2ToolStripStatusLabel,
            this.rxPeriodToolStripStatusLabel,
            this.activityToolStripProgressBar});
            this.mainStatusStrip.Location = new System.Drawing.Point(0, 360);
            this.mainStatusStrip.Name = "mainStatusStrip";
            this.mainStatusStrip.Size = new System.Drawing.Size(1100, 25);
            this.mainStatusStrip.TabIndex = 9;
            this.mainStatusStrip.Text = "mainStatusStrip";
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
            this.area1ToolStripStatusLabel.Size = new System.Drawing.Size(411, 20);
            this.area1ToolStripStatusLabel.Spring = true;
            this.area1ToolStripStatusLabel.Text = "-";
            // 
            // area2ToolStripStatusLabel
            // 
            this.area2ToolStripStatusLabel.Name = "area2ToolStripStatusLabel";
            this.area2ToolStripStatusLabel.Size = new System.Drawing.Size(411, 20);
            this.area2ToolStripStatusLabel.Spring = true;
            this.area2ToolStripStatusLabel.Text = "-";
            this.area2ToolStripStatusLabel.Click += new System.EventHandler(this.area2ToolStripStatusLabel_Click);
            // 
            // activityToolStripProgressBar
            // 
            this.activityToolStripProgressBar.AutoSize = false;
            this.activityToolStripProgressBar.Name = "activityToolStripProgressBar";
            this.activityToolStripProgressBar.Size = new System.Drawing.Size(40, 19);
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.viewPageComboBox,
            this.customizeToolStripButton,
            this.toolStripLabel2,
            this.timeStepToolStripTextBox,
            this.commToolStripButton,
            this.dumpToolStripButton,
            this.toolStripLabel3,
            this.logLengthToolStripTextBox,
            this.copyLogToolStripButton,
            this.terminalToolStripButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(1100, 27);
            this.mainToolStrip.TabIndex = 10;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(41, 24);
            this.toolStripLabel1.Text = "Page:";
            // 
            // viewPageComboBox
            // 
            this.viewPageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.viewPageComboBox.Name = "viewPageComboBox";
            this.viewPageComboBox.Size = new System.Drawing.Size(180, 27);
            this.viewPageComboBox.SelectedIndexChanged += new System.EventHandler(this.viewPageComboBox_SelectedIndexChanged);
            // 
            // customizeToolStripButton
            // 
            this.customizeToolStripButton.AutoSize = false;
            this.customizeToolStripButton.Image = global::rmApplication.Properties.Resources.Complete_and_ok_gray;
            this.customizeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.customizeToolStripButton.Name = "customizeToolStripButton";
            this.customizeToolStripButton.Size = new System.Drawing.Size(100, 24);
            this.customizeToolStripButton.Text = "Customize";
            this.customizeToolStripButton.Click += new System.EventHandler(this.customizeToolStripButton_Click);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.AutoSize = false;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(98, 24);
            this.toolStripLabel2.Text = "Time Step(ms):";
            // 
            // timeStepToolStripTextBox
            // 
            this.timeStepToolStripTextBox.AutoSize = false;
            this.timeStepToolStripTextBox.Name = "timeStepToolStripTextBox";
            this.timeStepToolStripTextBox.Size = new System.Drawing.Size(50, 27);
            this.timeStepToolStripTextBox.Leave += new System.EventHandler(this.timeStepToolStripTextBox_Leave);
            this.timeStepToolStripTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.timeStepToolStripTextBox_KeyPress);
            // 
            // commToolStripButton
            // 
            this.commToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.commToolStripButton.AutoSize = false;
            this.commToolStripButton.Image = global::rmApplication.Properties.Resources.FlagThread_white;
            this.commToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.commToolStripButton.Name = "commToolStripButton";
            this.commToolStripButton.Size = new System.Drawing.Size(106, 24);
            this.commToolStripButton.Text = "Comm Open";
            this.commToolStripButton.Click += new System.EventHandler(this.commToolStripButton_Click);
            // 
            // dumpToolStripButton
            // 
            this.dumpToolStripButton.AutoSize = false;
            this.dumpToolStripButton.Image = global::rmApplication.Properties.Resources.VirtualMachine;
            this.dumpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.dumpToolStripButton.Name = "dumpToolStripButton";
            this.dumpToolStripButton.Size = new System.Drawing.Size(65, 24);
            this.dumpToolStripButton.Text = "Dump";
            this.dumpToolStripButton.Click += new System.EventHandler(this.dumpToolStripButton_Click);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(78, 24);
            this.toolStripLabel3.Text = "Log Length:";
            // 
            // logLengthToolStripTextBox
            // 
            this.logLengthToolStripTextBox.Name = "logLengthToolStripTextBox";
            this.logLengthToolStripTextBox.Size = new System.Drawing.Size(50, 27);
            this.logLengthToolStripTextBox.Leave += new System.EventHandler(this.logLengthToolStripTextBox_Leave);
            this.logLengthToolStripTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.logLengthToolStripTextBox_KeyPress);
            // 
            // copyLogToolStripButton
            // 
            this.copyLogToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("copyLogToolStripButton.Image")));
            this.copyLogToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyLogToolStripButton.Name = "copyLogToolStripButton";
            this.copyLogToolStripButton.Size = new System.Drawing.Size(86, 24);
            this.copyLogToolStripButton.Text = "Copy Log";
            this.copyLogToolStripButton.Click += new System.EventHandler(this.copyLogToolStripButton_Click);
            // 
            // terminalToolStripButton
            // 
            this.terminalToolStripButton.Image = global::rmApplication.Properties.Resources.Monitor_Screen_16xLG;
            this.terminalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.terminalToolStripButton.Name = "terminalToolStripButton";
            this.terminalToolStripButton.Size = new System.Drawing.Size(83, 24);
            this.terminalToolStripButton.Text = "Terminal";
            this.terminalToolStripButton.Click += new System.EventHandler(this.terminalToolStripButton_Click);
            // 
            // mainTimer
            // 
            this.mainTimer.Tick += new System.EventHandler(this.mainTimer_Tick);
            // 
            // rxPeriodToolStripStatusLabel
            // 
            this.rxPeriodToolStripStatusLabel.AutoSize = false;
            this.rxPeriodToolStripStatusLabel.Name = "rxPeriodToolStripStatusLabel";
            this.rxPeriodToolStripStatusLabel.Size = new System.Drawing.Size(120, 20);
            this.rxPeriodToolStripStatusLabel.Text = "-";
            // 
            // targetVersionViewControl
            // 
            this.targetVersionViewControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.targetVersionViewControl.Label = "Target Version";
            this.targetVersionViewControl.Location = new System.Drawing.Point(0, 45);
            this.targetVersionViewControl.Margin = new System.Windows.Forms.Padding(2);
            this.targetVersionViewControl.Name = "targetVersionViewControl";
            this.targetVersionViewControl.Size = new System.Drawing.Size(1100, 18);
            this.targetVersionViewControl.TabIndex = 12;
            this.targetVersionViewControl.TextBox = "";
            this.targetVersionViewControl.TextEnabled = false;
            // 
            // receivedVersionViewControl
            // 
            this.receivedVersionViewControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.receivedVersionViewControl.Label = "Received Version";
            this.receivedVersionViewControl.Location = new System.Drawing.Point(0, 27);
            this.receivedVersionViewControl.Margin = new System.Windows.Forms.Padding(2);
            this.receivedVersionViewControl.Name = "receivedVersionViewControl";
            this.receivedVersionViewControl.Size = new System.Drawing.Size(1100, 18);
            this.receivedVersionViewControl.TabIndex = 11;
            this.receivedVersionViewControl.TextBox = "";
            this.receivedVersionViewControl.TextEnabled = false;
            // 
            // SubViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainDataGridView);
            this.Controls.Add(this.targetVersionViewControl);
            this.Controls.Add(this.receivedVersionViewControl);
            this.Controls.Add(this.mainStatusStrip);
            this.Controls.Add(this.mainToolStrip);
            this.Name = "SubViewControl";
            this.Size = new System.Drawing.Size(1100, 385);
            ((System.ComponentModel.ISupportInitialize)(this.mainDataGridView)).EndInit();
            this.mainStatusStrip.ResumeLayout(false);
            this.mainStatusStrip.PerformLayout();
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView mainDataGridView;
        private NoticeViewControl targetVersionViewControl;
        private NoticeViewControl receivedVersionViewControl;
        private System.Windows.Forms.StatusStrip mainStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel userToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel area1ToolStripStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel area2ToolStripStatusLabel;
        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox viewPageComboBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox timeStepToolStripTextBox;
        private System.Windows.Forms.ToolStripButton commToolStripButton;
        private System.Windows.Forms.ToolStripButton dumpToolStripButton;
        private System.Windows.Forms.Timer mainTimer;
        private System.Windows.Forms.ToolStripButton customizeToolStripButton;
        private System.Windows.Forms.ToolStripProgressBar activityToolStripProgressBar;
        private System.Windows.Forms.ToolStripButton copyLogToolStripButton;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripTextBox logLengthToolStripTextBox;
        private System.Windows.Forms.ToolStripStatusLabel rxPeriodToolStripStatusLabel;
        private System.Windows.Forms.ToolStripButton terminalToolStripButton;
    }
}
