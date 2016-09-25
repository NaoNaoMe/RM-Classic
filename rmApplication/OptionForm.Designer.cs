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
			this.cmbBaudRate = new System.Windows.Forms.ComboBox();
			this.cmbPortName = new System.Windows.Forms.ComboBox();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.localIPTextBox = new System.Windows.Forms.TextBox();
			this.adr2byteRadioButton = new System.Windows.Forms.RadioButton();
			this.adr4byteRadioButton = new System.Windows.Forms.RadioButton();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBoxCommunication = new System.Windows.Forms.GroupBox();
			this.radioButtonSerialP = new System.Windows.Forms.RadioButton();
			this.radioButtonLocalN = new System.Windows.Forms.RadioButton();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBoxCommon = new System.Windows.Forms.GroupBox();
			this.groupBoxCommunication.SuspendLayout();
			this.groupBoxCommon.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(251, 292);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(92, 36);
			this.cancelButton.TabIndex = 19;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(351, 292);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(92, 36);
			this.okButton.TabIndex = 18;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cmbBaudRate
			// 
			this.cmbBaudRate.FormattingEnabled = true;
			this.cmbBaudRate.Location = new System.Drawing.Point(243, 22);
			this.cmbBaudRate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmbBaudRate.Name = "cmbBaudRate";
			this.cmbBaudRate.Size = new System.Drawing.Size(162, 24);
			this.cmbBaudRate.TabIndex = 16;
			// 
			// cmbPortName
			// 
			this.cmbPortName.FormattingEnabled = true;
			this.cmbPortName.Location = new System.Drawing.Point(243, 22);
			this.cmbPortName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmbPortName.Name = "cmbPortName";
			this.cmbPortName.Size = new System.Drawing.Size(162, 24);
			this.cmbPortName.TabIndex = 15;
			// 
			// portTextBox
			// 
			this.portTextBox.Location = new System.Drawing.Point(243, 81);
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(162, 22);
			this.portTextBox.TabIndex = 1;
			this.portTextBox.Text = "16383";
			// 
			// localIPTextBox
			// 
			this.localIPTextBox.Location = new System.Drawing.Point(243, 53);
			this.localIPTextBox.Name = "localIPTextBox";
			this.localIPTextBox.Size = new System.Drawing.Size(162, 22);
			this.localIPTextBox.TabIndex = 0;
			this.localIPTextBox.Text = "192.168.0.7";
			// 
			// adr2byteRadioButton
			// 
			this.adr2byteRadioButton.AutoSize = true;
			this.adr2byteRadioButton.Location = new System.Drawing.Point(6, 102);
			this.adr2byteRadioButton.Name = "adr2byteRadioButton";
			this.adr2byteRadioButton.Size = new System.Drawing.Size(120, 21);
			this.adr2byteRadioButton.TabIndex = 29;
			this.adr2byteRadioButton.TabStop = true;
			this.adr2byteRadioButton.Text = "Address:2byte";
			this.adr2byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// adr4byteRadioButton
			// 
			this.adr4byteRadioButton.AutoSize = true;
			this.adr4byteRadioButton.Location = new System.Drawing.Point(6, 75);
			this.adr4byteRadioButton.Name = "adr4byteRadioButton";
			this.adr4byteRadioButton.Size = new System.Drawing.Size(120, 21);
			this.adr4byteRadioButton.TabIndex = 28;
			this.adr4byteRadioButton.TabStop = true;
			this.adr4byteRadioButton.Text = "Address:4byte";
			this.adr4byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Location = new System.Drawing.Point(244, 53);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = new System.Drawing.Size(162, 22);
			this.passwordTextBox.TabIndex = 27;
			this.passwordTextBox.Text = "00000000";
			this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(135, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 17);
			this.label1.TabIndex = 26;
			this.label1.Text = "Comm Port:";
			// 
			// groupBoxCommunication
			// 
			this.groupBoxCommunication.Controls.Add(this.radioButtonLocalN);
			this.groupBoxCommunication.Controls.Add(this.radioButtonSerialP);
			this.groupBoxCommunication.Controls.Add(this.label3);
			this.groupBoxCommunication.Controls.Add(this.portTextBox);
			this.groupBoxCommunication.Controls.Add(this.label1);
			this.groupBoxCommunication.Controls.Add(this.label2);
			this.groupBoxCommunication.Controls.Add(this.cmbPortName);
			this.groupBoxCommunication.Controls.Add(this.localIPTextBox);
			this.groupBoxCommunication.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBoxCommunication.Location = new System.Drawing.Point(12, 12);
			this.groupBoxCommunication.Name = "groupBoxCommunication";
			this.groupBoxCommunication.Size = new System.Drawing.Size(431, 122);
			this.groupBoxCommunication.TabIndex = 30;
			this.groupBoxCommunication.TabStop = false;
			this.groupBoxCommunication.Text = "Communication";
			// 
			// radioButtonSerialP
			// 
			this.radioButtonSerialP.AutoSize = true;
			this.radioButtonSerialP.Checked = true;
			this.radioButtonSerialP.Location = new System.Drawing.Point(6, 45);
			this.radioButtonSerialP.Name = "radioButtonSerialP";
			this.radioButtonSerialP.Size = new System.Drawing.Size(95, 21);
			this.radioButtonSerialP.TabIndex = 0;
			this.radioButtonSerialP.TabStop = true;
			this.radioButtonSerialP.Text = "Serial Port";
			this.radioButtonSerialP.UseVisualStyleBackColor = true;
			// 
			// radioButtonLocalN
			// 
			this.radioButtonLocalN.AutoSize = true;
			this.radioButtonLocalN.Location = new System.Drawing.Point(6, 72);
			this.radioButtonLocalN.Name = "radioButtonLocalN";
			this.radioButtonLocalN.Size = new System.Drawing.Size(118, 21);
			this.radioButtonLocalN.TabIndex = 1;
			this.radioButtonLocalN.Text = "Local Network";
			this.radioButtonLocalN.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(140, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(62, 17);
			this.label2.TabIndex = 31;
			this.label2.Text = "Local IP:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(140, 84);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(76, 17);
			this.label3.TabIndex = 32;
			this.label3.Text = "Local Port:";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(135, 25);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 17);
			this.label4.TabIndex = 33;
			this.label4.Text = "Baud Rate:";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(135, 53);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(73, 17);
			this.label5.TabIndex = 34;
			this.label5.Text = "Password:";
			// 
			// groupBoxCommon
			// 
			this.groupBoxCommon.Controls.Add(this.label4);
			this.groupBoxCommon.Controls.Add(this.label5);
			this.groupBoxCommon.Controls.Add(this.adr2byteRadioButton);
			this.groupBoxCommon.Controls.Add(this.passwordTextBox);
			this.groupBoxCommon.Controls.Add(this.adr4byteRadioButton);
			this.groupBoxCommon.Controls.Add(this.cmbBaudRate);
			this.groupBoxCommon.Location = new System.Drawing.Point(12, 150);
			this.groupBoxCommon.Name = "groupBoxCommon";
			this.groupBoxCommon.Size = new System.Drawing.Size(431, 135);
			this.groupBoxCommon.TabIndex = 35;
			this.groupBoxCommon.TabStop = false;
			this.groupBoxCommon.Text = "Common";
			// 
			// OptionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.ClientSize = new System.Drawing.Size(454, 337);
			this.ControlBox = false;
			this.Controls.Add(this.groupBoxCommon);
			this.Controls.Add(this.groupBoxCommunication);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "OptionForm";
			this.Text = "OptionForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionForm_FormClosing);
			this.Load += new System.EventHandler(this.OptionForm_Load);
			this.groupBoxCommunication.ResumeLayout(false);
			this.groupBoxCommunication.PerformLayout();
			this.groupBoxCommon.ResumeLayout(false);
			this.groupBoxCommon.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ComboBox cmbBaudRate;
		private System.Windows.Forms.ComboBox cmbPortName;
		private System.Windows.Forms.TextBox portTextBox;
		private System.Windows.Forms.TextBox localIPTextBox;
		private System.Windows.Forms.RadioButton adr2byteRadioButton;
		private System.Windows.Forms.RadioButton adr4byteRadioButton;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBoxCommunication;
		private System.Windows.Forms.RadioButton radioButtonLocalN;
		private System.Windows.Forms.RadioButton radioButtonSerialP;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox groupBoxCommon;
	}
}