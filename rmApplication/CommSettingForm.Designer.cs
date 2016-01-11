namespace rmApplication
{
	partial class CommSettingForm
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
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(188, 142);
			this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(92, 44);
			this.cancelButton.TabIndex = 19;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(23, 142);
			this.okButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(92, 44);
			this.okButton.TabIndex = 18;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cmbBaudRate
			// 
			this.cmbBaudRate.FormattingEnabled = true;
			this.cmbBaudRate.Location = new System.Drawing.Point(7, 56);
			this.cmbBaudRate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmbBaudRate.Name = "cmbBaudRate";
			this.cmbBaudRate.Size = new System.Drawing.Size(257, 23);
			this.cmbBaudRate.TabIndex = 16;
			// 
			// cmbPortName
			// 
			this.cmbPortName.FormattingEnabled = true;
			this.cmbPortName.Location = new System.Drawing.Point(7, 8);
			this.cmbPortName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.cmbPortName.Name = "cmbPortName";
			this.cmbPortName.Size = new System.Drawing.Size(257, 23);
			this.cmbPortName.TabIndex = 15;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(279, 122);
			this.tabControl1.TabIndex = 20;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.cmbPortName);
			this.tabPage1.Controls.Add(this.cmbBaudRate);
			this.tabPage1.Location = new System.Drawing.Point(4, 25);
			this.tabPage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage1.Size = new System.Drawing.Size(271, 93);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Serial";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.portTextBox);
			this.tabPage2.Controls.Add(this.localIPTextBox);
			this.tabPage2.Location = new System.Drawing.Point(4, 25);
			this.tabPage2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.tabPage2.Size = new System.Drawing.Size(271, 93);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Network";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// portTextBox
			// 
			this.portTextBox.Location = new System.Drawing.Point(20, 52);
			this.portTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.portTextBox.Name = "portTextBox";
			this.portTextBox.Size = new System.Drawing.Size(229, 22);
			this.portTextBox.TabIndex = 1;
			this.portTextBox.Text = "16383";
			// 
			// localIPTextBox
			// 
			this.localIPTextBox.Location = new System.Drawing.Point(20, 15);
			this.localIPTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.localIPTextBox.Name = "localIPTextBox";
			this.localIPTextBox.Size = new System.Drawing.Size(231, 22);
			this.localIPTextBox.TabIndex = 0;
			this.localIPTextBox.Text = "192.168.0.7";
			// 
			// CommSettingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(300, 210);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			this.Name = "CommSettingForm";
			this.Text = "CommSettingForm";
			this.Load += new System.EventHandler(this.CommSettingForm_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

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
	}
}