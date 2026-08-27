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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SubViewControl));
            mainDataGridView = new System.Windows.Forms.DataGridView();
            mainStatusStrip = new System.Windows.Forms.StatusStrip();
            userToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            area1ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            area2ToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            rxPeriodToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            activityToolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            mainToolStrip = new System.Windows.Forms.ToolStrip();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            viewPageComboBox = new System.Windows.Forms.ToolStripComboBox();
            customizeToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            timeStepToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            commToolStripButton = new System.Windows.Forms.ToolStripButton();
            dumpToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            logLengthToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            copyLogToolStripButton = new System.Windows.Forms.ToolStripButton();
            terminalToolStripButton = new System.Windows.Forms.ToolStripButton();
            remoteToolStripButton = new System.Windows.Forms.ToolStripButton();
            mainTimer = new System.Windows.Forms.Timer(components);
            targetVersionViewControl = new NoticeViewControl();
            receivedVersionViewControl = new NoticeViewControl();
            ((System.ComponentModel.ISupportInitialize)mainDataGridView).BeginInit();
            mainStatusStrip.SuspendLayout();
            mainToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // mainDataGridView
            // 
            mainDataGridView.AllowUserToAddRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 128);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            mainDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            mainDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 128);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            mainDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            mainDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            mainDataGridView.Location = new System.Drawing.Point(0, 63);
            mainDataGridView.Margin = new System.Windows.Forms.Padding(4);
            mainDataGridView.Name = "mainDataGridView";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 128);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            mainDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            mainDataGridView.RowTemplate.Height = 21;
            mainDataGridView.Size = new System.Drawing.Size(1283, 391);
            mainDataGridView.TabIndex = 0;
            mainDataGridView.CellContentClick += mainDataGridView_CellContentClick;
            mainDataGridView.CellEndEdit += mainDataGridView_CellEndEdit;
            mainDataGridView.CellValidating += mainDataGridView_CellValidating;
            mainDataGridView.EditingControlShowing += mainDataGridView_EditingControlShowing;
            mainDataGridView.RowPostPaint += mainDataGridView_RowPostPaint;
            mainDataGridView.KeyDown += mainDataGridView_KeyDown;
            // 
            // mainStatusStrip
            // 
            mainStatusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            mainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { userToolStripStatusLabel, area1ToolStripStatusLabel, area2ToolStripStatusLabel, rxPeriodToolStripStatusLabel, activityToolStripProgressBar });
            mainStatusStrip.Location = new System.Drawing.Point(0, 454);
            mainStatusStrip.Name = "mainStatusStrip";
            mainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            mainStatusStrip.Size = new System.Drawing.Size(1283, 27);
            mainStatusStrip.TabIndex = 9;
            mainStatusStrip.Text = "mainStatusStrip";
            // 
            // userToolStripStatusLabel
            // 
            userToolStripStatusLabel.AutoSize = false;
            userToolStripStatusLabel.Name = "userToolStripStatusLabel";
            userToolStripStatusLabel.Size = new System.Drawing.Size(100, 22);
            // 
            // area1ToolStripStatusLabel
            // 
            area1ToolStripStatusLabel.Name = "area1ToolStripStatusLabel";
            area1ToolStripStatusLabel.Size = new System.Drawing.Size(498, 22);
            area1ToolStripStatusLabel.Spring = true;
            area1ToolStripStatusLabel.Text = "-";
            // 
            // area2ToolStripStatusLabel
            // 
            area2ToolStripStatusLabel.Name = "area2ToolStripStatusLabel";
            area2ToolStripStatusLabel.Size = new System.Drawing.Size(498, 22);
            area2ToolStripStatusLabel.Spring = true;
            area2ToolStripStatusLabel.Text = "-";
            area2ToolStripStatusLabel.Click += area2ToolStripStatusLabel_Click;
            // 
            // rxPeriodToolStripStatusLabel
            // 
            rxPeriodToolStripStatusLabel.AutoSize = false;
            rxPeriodToolStripStatusLabel.Name = "rxPeriodToolStripStatusLabel";
            rxPeriodToolStripStatusLabel.Size = new System.Drawing.Size(120, 22);
            rxPeriodToolStripStatusLabel.Text = "-";
            // 
            // activityToolStripProgressBar
            // 
            activityToolStripProgressBar.AutoSize = false;
            activityToolStripProgressBar.Name = "activityToolStripProgressBar";
            activityToolStripProgressBar.Size = new System.Drawing.Size(47, 21);
            // 
            // mainToolStrip
            // 
            mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            mainToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripLabel1, viewPageComboBox, customizeToolStripButton, toolStripLabel2, timeStepToolStripTextBox, commToolStripButton, dumpToolStripButton, toolStripLabel3, logLengthToolStripTextBox, copyLogToolStripButton, terminalToolStripButton, remoteToolStripButton });
            mainToolStrip.Location = new System.Drawing.Point(0, 0);
            mainToolStrip.Name = "mainToolStrip";
            mainToolStrip.Size = new System.Drawing.Size(1283, 27);
            mainToolStrip.TabIndex = 10;
            mainToolStrip.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(36, 24);
            toolStripLabel1.Text = "Page:";
            // 
            // viewPageComboBox
            // 
            viewPageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            viewPageComboBox.Name = "viewPageComboBox";
            viewPageComboBox.Size = new System.Drawing.Size(195, 27);
            viewPageComboBox.SelectedIndexChanged += viewPageComboBox_SelectedIndexChanged;
            // 
            // customizeToolStripButton
            // 
            customizeToolStripButton.AutoSize = false;
            customizeToolStripButton.Image = Properties.Resources.Complete_and_ok_gray;
            customizeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            customizeToolStripButton.Name = "customizeToolStripButton";
            customizeToolStripButton.Size = new System.Drawing.Size(100, 24);
            customizeToolStripButton.Text = "Customize";
            customizeToolStripButton.Click += customizeToolStripButton_Click;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.AutoSize = false;
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new System.Drawing.Size(98, 24);
            toolStripLabel2.Text = "Time Step(ms):";
            // 
            // timeStepToolStripTextBox
            // 
            timeStepToolStripTextBox.AutoSize = false;
            timeStepToolStripTextBox.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            timeStepToolStripTextBox.Name = "timeStepToolStripTextBox";
            timeStepToolStripTextBox.Size = new System.Drawing.Size(58, 23);
            timeStepToolStripTextBox.Leave += timeStepToolStripTextBox_Leave;
            timeStepToolStripTextBox.KeyPress += timeStepToolStripTextBox_KeyPress;
            // 
            // commToolStripButton
            // 
            commToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            commToolStripButton.AutoSize = false;
            commToolStripButton.Image = Properties.Resources.FlagThread_white;
            commToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            commToolStripButton.Name = "commToolStripButton";
            commToolStripButton.Size = new System.Drawing.Size(106, 24);
            commToolStripButton.Text = "Comm Open";
            commToolStripButton.Click += commToolStripButton_Click;
            // 
            // dumpToolStripButton
            // 
            dumpToolStripButton.AutoSize = false;
            dumpToolStripButton.Image = Properties.Resources.VirtualMachine;
            dumpToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            dumpToolStripButton.Name = "dumpToolStripButton";
            dumpToolStripButton.Size = new System.Drawing.Size(65, 24);
            dumpToolStripButton.Text = "Dump";
            dumpToolStripButton.Click += dumpToolStripButton_Click;
            // 
            // toolStripLabel3
            // 
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new System.Drawing.Size(70, 24);
            toolStripLabel3.Text = "Log Length:";
            // 
            // logLengthToolStripTextBox
            // 
            logLengthToolStripTextBox.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            logLengthToolStripTextBox.Name = "logLengthToolStripTextBox";
            logLengthToolStripTextBox.Size = new System.Drawing.Size(58, 27);
            logLengthToolStripTextBox.Leave += logLengthToolStripTextBox_Leave;
            logLengthToolStripTextBox.KeyPress += logLengthToolStripTextBox_KeyPress;
            // 
            // copyLogToolStripButton
            // 
            copyLogToolStripButton.Image = (System.Drawing.Image)resources.GetObject("copyLogToolStripButton.Image");
            copyLogToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            copyLogToolStripButton.Name = "copyLogToolStripButton";
            copyLogToolStripButton.Size = new System.Drawing.Size(82, 24);
            copyLogToolStripButton.Text = "Copy Log";
            copyLogToolStripButton.Click += copyLogToolStripButton_Click;
            // 
            // terminalToolStripButton
            // 
            terminalToolStripButton.Image = Properties.Resources.Monitor_Screen_16xLG;
            terminalToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            terminalToolStripButton.Name = "terminalToolStripButton";
            terminalToolStripButton.Size = new System.Drawing.Size(77, 24);
            terminalToolStripButton.Text = "Terminal";
            terminalToolStripButton.Click += terminalToolStripButton_Click;
            // 
            // remoteToolStripButton
            // 
            remoteToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            remoteToolStripButton.Image = Properties.Resources.Remote;
            remoteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            remoteToolStripButton.Name = "remoteToolStripButton";
            remoteToolStripButton.Size = new System.Drawing.Size(76, 24);
            remoteToolStripButton.Text = "REMOTE";
            remoteToolStripButton.Click += remoteToolStripButton_Click;
            // 
            // mainTimer
            // 
            mainTimer.Tick += mainTimer_Tick;
            // 
            // targetVersionViewControl
            // 
            targetVersionViewControl.Dock = System.Windows.Forms.DockStyle.Top;
            targetVersionViewControl.Label = "Target Version";
            targetVersionViewControl.Location = new System.Drawing.Point(0, 45);
            targetVersionViewControl.Margin = new System.Windows.Forms.Padding(2);
            targetVersionViewControl.Name = "targetVersionViewControl";
            targetVersionViewControl.Size = new System.Drawing.Size(1283, 18);
            targetVersionViewControl.TabIndex = 12;
            targetVersionViewControl.TextBox = "";
            targetVersionViewControl.TextEnabled = false;
            // 
            // receivedVersionViewControl
            // 
            receivedVersionViewControl.Dock = System.Windows.Forms.DockStyle.Top;
            receivedVersionViewControl.Label = "Received Version";
            receivedVersionViewControl.Location = new System.Drawing.Point(0, 27);
            receivedVersionViewControl.Margin = new System.Windows.Forms.Padding(2);
            receivedVersionViewControl.Name = "receivedVersionViewControl";
            receivedVersionViewControl.Size = new System.Drawing.Size(1283, 18);
            receivedVersionViewControl.TabIndex = 11;
            receivedVersionViewControl.TextBox = "";
            receivedVersionViewControl.TextEnabled = false;
            // 
            // SubViewControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(mainDataGridView);
            Controls.Add(targetVersionViewControl);
            Controls.Add(receivedVersionViewControl);
            Controls.Add(mainStatusStrip);
            Controls.Add(mainToolStrip);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "SubViewControl";
            Size = new System.Drawing.Size(1283, 481);
            ((System.ComponentModel.ISupportInitialize)mainDataGridView).EndInit();
            mainStatusStrip.ResumeLayout(false);
            mainStatusStrip.PerformLayout();
            mainToolStrip.ResumeLayout(false);
            mainToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

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
        private System.Windows.Forms.ToolStripButton remoteToolStripButton;
    }
}
