namespace rmApplication
{
    partial class OptionForm
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBoxCommon = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.addr2byteRadioButton = new System.Windows.Forms.RadioButton();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.addr4byteRadioButton = new System.Windows.Forms.RadioButton();
            this.cmbBaudRate = new System.Windows.Forms.ComboBox();
            this.settingTabControl = new System.Windows.Forms.TabControl();
            this.serialCommTabPage = new System.Windows.Forms.TabPage();
            this.portCheckButton = new System.Windows.Forms.Button();
            this.cmbPortName = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.localNetTabPage = new System.Windows.Forms.TabPage();
            this.pingButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.localIPTextBox = new System.Windows.Forms.TextBox();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxCommon.SuspendLayout();
            this.settingTabControl.SuspendLayout();
            this.serialCommTabPage.SuspendLayout();
            this.localNetTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(251, 251);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(92, 36);
            this.cancelButton.TabIndex = 19;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(351, 251);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(92, 36);
            this.okButton.TabIndex = 18;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // groupBoxCommon
            // 
            this.groupBoxCommon.Controls.Add(this.label4);
            this.groupBoxCommon.Controls.Add(this.label5);
            this.groupBoxCommon.Controls.Add(this.addr2byteRadioButton);
            this.groupBoxCommon.Controls.Add(this.passwordTextBox);
            this.groupBoxCommon.Controls.Add(this.addr4byteRadioButton);
            this.groupBoxCommon.Controls.Add(this.cmbBaudRate);
            this.groupBoxCommon.Location = new System.Drawing.Point(12, 150);
            this.groupBoxCommon.Name = "groupBoxCommon";
            this.groupBoxCommon.Size = new System.Drawing.Size(431, 94);
            this.groupBoxCommon.TabIndex = 37;
            this.groupBoxCommon.TabStop = false;
            this.groupBoxCommon.Text = "Common";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(135, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 13);
            this.label4.TabIndex = 33;
            this.label4.Text = "Baud Rate:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(135, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "Password:";
            // 
            // addr2byteRadioButton
            // 
            this.addr2byteRadioButton.AutoSize = true;
            this.addr2byteRadioButton.Location = new System.Drawing.Point(6, 54);
            this.addr2byteRadioButton.Name = "addr2byteRadioButton";
            this.addr2byteRadioButton.Size = new System.Drawing.Size(92, 17);
            this.addr2byteRadioButton.TabIndex = 29;
            this.addr2byteRadioButton.TabStop = true;
            this.addr2byteRadioButton.Text = "Address:2byte";
            this.addr2byteRadioButton.UseVisualStyleBackColor = true;
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Location = new System.Drawing.Point(243, 53);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(162, 20);
            this.passwordTextBox.TabIndex = 27;
            this.passwordTextBox.Text = "00000000";
            this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // addr4byteRadioButton
            // 
            this.addr4byteRadioButton.AutoSize = true;
            this.addr4byteRadioButton.Location = new System.Drawing.Point(6, 23);
            this.addr4byteRadioButton.Name = "addr4byteRadioButton";
            this.addr4byteRadioButton.Size = new System.Drawing.Size(92, 17);
            this.addr4byteRadioButton.TabIndex = 28;
            this.addr4byteRadioButton.TabStop = true;
            this.addr4byteRadioButton.Text = "Address:4byte";
            this.addr4byteRadioButton.UseVisualStyleBackColor = true;
            // 
            // cmbBaudRate
            // 
            this.cmbBaudRate.FormattingEnabled = true;
            this.cmbBaudRate.Location = new System.Drawing.Point(243, 22);
            this.cmbBaudRate.Margin = new System.Windows.Forms.Padding(4);
            this.cmbBaudRate.Name = "cmbBaudRate";
            this.cmbBaudRate.Size = new System.Drawing.Size(162, 21);
            this.cmbBaudRate.TabIndex = 16;
            // 
            // settingTabControl
            // 
            this.settingTabControl.Controls.Add(this.serialCommTabPage);
            this.settingTabControl.Controls.Add(this.localNetTabPage);
            this.settingTabControl.Location = new System.Drawing.Point(12, 12);
            this.settingTabControl.Name = "settingTabControl";
            this.settingTabControl.SelectedIndex = 0;
            this.settingTabControl.Size = new System.Drawing.Size(431, 126);
            this.settingTabControl.TabIndex = 38;
            // 
            // serialCommTabPage
            // 
            this.serialCommTabPage.Controls.Add(this.portCheckButton);
            this.serialCommTabPage.Controls.Add(this.cmbPortName);
            this.serialCommTabPage.Controls.Add(this.label1);
            this.serialCommTabPage.Location = new System.Drawing.Point(4, 22);
            this.serialCommTabPage.Name = "serialCommTabPage";
            this.serialCommTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.serialCommTabPage.Size = new System.Drawing.Size(423, 100);
            this.serialCommTabPage.TabIndex = 2;
            this.serialCommTabPage.Text = "Serial Communication";
            this.serialCommTabPage.UseVisualStyleBackColor = true;
            // 
            // portCheckButton
            // 
            this.portCheckButton.Location = new System.Drawing.Point(326, 32);
            this.portCheckButton.Name = "portCheckButton";
            this.portCheckButton.Size = new System.Drawing.Size(75, 23);
            this.portCheckButton.TabIndex = 34;
            this.portCheckButton.Text = "Check";
            this.portCheckButton.UseVisualStyleBackColor = true;
            this.portCheckButton.Click += new System.EventHandler(this.portCheckButton_Click);
            // 
            // cmbPortName
            // 
            this.cmbPortName.FormattingEnabled = true;
            this.cmbPortName.Location = new System.Drawing.Point(121, 7);
            this.cmbPortName.Margin = new System.Windows.Forms.Padding(4);
            this.cmbPortName.Name = "cmbPortName";
            this.cmbPortName.Size = new System.Drawing.Size(162, 21);
            this.cmbPortName.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Comm Port:";
            // 
            // localNetTabPage
            // 
            this.localNetTabPage.Controls.Add(this.pingButton);
            this.localNetTabPage.Controls.Add(this.label3);
            this.localNetTabPage.Controls.Add(this.localIPTextBox);
            this.localNetTabPage.Controls.Add(this.portTextBox);
            this.localNetTabPage.Controls.Add(this.label2);
            this.localNetTabPage.Location = new System.Drawing.Point(4, 22);
            this.localNetTabPage.Name = "localNetTabPage";
            this.localNetTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.localNetTabPage.Size = new System.Drawing.Size(423, 100);
            this.localNetTabPage.TabIndex = 0;
            this.localNetTabPage.Text = "Local Network";
            this.localNetTabPage.UseVisualStyleBackColor = true;
            // 
            // pingButton
            // 
            this.pingButton.Location = new System.Drawing.Point(326, 32);
            this.pingButton.Name = "pingButton";
            this.pingButton.Size = new System.Drawing.Size(75, 23);
            this.pingButton.TabIndex = 33;
            this.pingButton.Text = "Ping";
            this.pingButton.UseVisualStyleBackColor = true;
            this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "Local Port:";
            // 
            // localIPTextBox
            // 
            this.localIPTextBox.Location = new System.Drawing.Point(112, 6);
            this.localIPTextBox.Name = "localIPTextBox";
            this.localIPTextBox.Size = new System.Drawing.Size(162, 20);
            this.localIPTextBox.TabIndex = 0;
            this.localIPTextBox.Text = "192.168.0.7";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(112, 34);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(162, 20);
            this.portTextBox.TabIndex = 1;
            this.portTextBox.Text = "16383";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Local IP:";
            // 
            // OptionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(458, 297);
            this.ControlBox = false;
            this.Controls.Add(this.settingTabControl);
            this.Controls.Add(this.groupBoxCommon);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "OptionForm";
            this.Text = "OptionForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionForm_FormClosing);
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.groupBoxCommon.ResumeLayout(false);
            this.groupBoxCommon.PerformLayout();
            this.settingTabControl.ResumeLayout(false);
            this.serialCommTabPage.ResumeLayout(false);
            this.serialCommTabPage.PerformLayout();
            this.localNetTabPage.ResumeLayout(false);
            this.localNetTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.GroupBox groupBoxCommon;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.RadioButton addr2byteRadioButton;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.RadioButton addr4byteRadioButton;
        private System.Windows.Forms.ComboBox cmbBaudRate;
        private System.Windows.Forms.TabControl settingTabControl;
        private System.Windows.Forms.TabPage serialCommTabPage;
        private System.Windows.Forms.Button portCheckButton;
        private System.Windows.Forms.ComboBox cmbPortName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage localNetTabPage;
        private System.Windows.Forms.Button pingButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox localIPTextBox;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.Label label2;
    }
}