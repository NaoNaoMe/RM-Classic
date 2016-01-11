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
			this.label1 = new System.Windows.Forms.Label();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.adr4byteRadioButton = new System.Windows.Forms.RadioButton();
			this.adr2byteRadioButton = new System.Windows.Forms.RadioButton();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(133, 123);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(69, 35);
			this.cancelButton.TabIndex = 21;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(10, 123);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(69, 35);
			this.okButton.TabIndex = 20;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 30);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 12);
			this.label1.TabIndex = 22;
			this.label1.Text = "Password:";
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Location = new System.Drawing.Point(66, 28);
			this.passwordTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.Size = new System.Drawing.Size(138, 19);
			this.passwordTextBox.TabIndex = 23;
			this.passwordTextBox.Text = "00000000";
			this.passwordTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// adr4byteRadioButton
			// 
			this.adr4byteRadioButton.AutoSize = true;
			this.adr4byteRadioButton.Location = new System.Drawing.Point(66, 62);
			this.adr4byteRadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.adr4byteRadioButton.Name = "adr4byteRadioButton";
			this.adr4byteRadioButton.Size = new System.Drawing.Size(95, 16);
			this.adr4byteRadioButton.TabIndex = 24;
			this.adr4byteRadioButton.TabStop = true;
			this.adr4byteRadioButton.Text = "Address:4byte";
			this.adr4byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// adr2byteRadioButton
			// 
			this.adr2byteRadioButton.AutoSize = true;
			this.adr2byteRadioButton.Location = new System.Drawing.Point(66, 82);
			this.adr2byteRadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.adr2byteRadioButton.Name = "adr2byteRadioButton";
			this.adr2byteRadioButton.Size = new System.Drawing.Size(95, 16);
			this.adr2byteRadioButton.TabIndex = 25;
			this.adr2byteRadioButton.TabStop = true;
			this.adr2byteRadioButton.Text = "Address:2byte";
			this.adr2byteRadioButton.UseVisualStyleBackColor = true;
			// 
			// OptionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(212, 169);
			this.Controls.Add(this.adr2byteRadioButton);
			this.Controls.Add(this.adr4byteRadioButton);
			this.Controls.Add(this.passwordTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
			this.Name = "OptionForm";
			this.Text = "optionForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox passwordTextBox;
		private System.Windows.Forms.RadioButton adr4byteRadioButton;
		private System.Windows.Forms.RadioButton adr2byteRadioButton;
	}
}