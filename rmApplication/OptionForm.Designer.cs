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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.portTextBox = new System.Windows.Forms.TextBox();
			this.localIPTextBox = new System.Windows.Forms.TextBox();
			this.adr2byteRadioButton = new System.Windows.Forms.RadioButton();
			this.adr4byteRadioButton = new System.Windows.Forms.RadioButton();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(141, 202);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(69, 27);
			this.cancelButton.TabIndex = 19;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(17, 202);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(69, 27);
			this.okButton.TabIndex = 18;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cmbBaudRate
			// 
			this.cmbBaudRate.FormattingEnabled = true;
			this.cmbBaudRate.Location = new System.Drawing.Point(5, 45);
			this.cmbBaudRate.Name = "cmbBaudRate";
			this.cmbBaudRate.Size = new System.Drawing.Size(194, 20);
			this.cmbBaudRate.TabIndex = 16;
			// 
			// cmbPortName
			// 
			this.cmbPortName.FormattingEnabled = true;
			this.cmbPortName.Location = new System.Drawing.Point(5, 6);
			this.cmbPortName.Name = "cmbPortName";
			this.cmbPortName.Size = new System.Drawing.Size(194, 20);
			this.cmbPortName.TabIndex = 15;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(9, 10);
			this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(209, 98);
			this.tabControl1.TabIndex = 20;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.cmbPortName);
			this.tabPage1.Controls.Add(this.cmbBaudRate);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tabPage1.Size = new System.Drawing.Size(201, 72);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Serial";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.portTextBox);
			this.tabPage2.Controls.Add(this.localIPTextBox);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.tabPage2.Size = new System.Drawing.Size(201, 72);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Network";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// portTextBox
			// 
			this.portTextBox.Location = new System.Drawing.Point(15, 42);
			this.portTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(173, 19);
			this.portTextBox.TabIndex = 1;
			this.portTextBox.Text = "16383";
			// 
			// localIPTextBox
			// 
			this.localIPTextBox.Location = new System.Drawing.Point(15, 12);
			this.localIPTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.localIPTextBox.Name = "localIPTextBox";
			this.localIPTextBox.Size = new System.Drawing.Size(174, 19);
			this.localIPTextBox.TabIndex = 0;
			this.localIPTextBox.Text = "192.168.0.7";
			// 
			// adr2byteRadioButton
			// 
			this.adr2byteRadioButton.AutoSize = true;
			this.adr2byteRadioButton.Location = new System.Drawing.Point(73, 169);
			this.adr2byteRadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.adr2byteRadioButton.Name = "adr2byteRadioButton";
			this.adr2byteRadioButton.Size = new System.Drawing.Size(95, 16);
			this.adr2byteRadioButton.TabIndex = 29;
			this.adr2byteRadioButton.TabStop = true;
			this.adr2byteRadioButton.Text = "Address:2byte";
			this.adr2byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// adr4byteRadioButton
			// 
			this.adr4byteRadioButton.AutoSize = true;
			this.adr4byteRadioButton.Location = new System.Drawing.Point(73, 150);
			this.adr4byteRadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.adr4byteRadioButton.Name = "adr4byteRadioButton";
			this.adr4byteRadioButton.Size = new System.Drawing.Size(95, 16);
			this.adr4byteRadioButton.TabIndex = 28;
			this.adr4byteRadioButton.TabStop = true;
			this.adr4byteRadioButton.Text = "Address:4byte";
			this.adr4byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Location = new System.Drawing.Point(73, 122);
			this.passwordTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = new System.Drawing.Size(138, 19);
			this.passwordTextBox.TabIndex = 27;
			this.passwordTextBox.Text = "00000000";
			this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 124);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 12);
			this.label1.TabIndex = 26;
			this.label1.Text = "Password:";
			// 
			// OptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(244, 270);
			this.ControlBox = false;
			this.Controls.Add(this.adr2byteRadioButton);
			this.Controls.Add(this.adr4byteRadioButton);
			this.Controls.Add(this.passwordTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "OptionForm";
			this.Text = "OptionForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionForm_FormClosing);
			this.Load += new System.EventHandler(this.OptionForm_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ComboBox cmbBaudRate;
		private System.Windows.Forms.ComboBox cmbPortName;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TextBox portTextBox;
		private System.Windows.Forms.TextBox localIPTextBox;
		private System.Windows.Forms.RadioButton adr2byteRadioButton;
		private System.Windows.Forms.RadioButton adr4byteRadioButton;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.Label label1;
	}
}