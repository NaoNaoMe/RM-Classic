using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rmApplication
{
	public partial class OptionForm : Form
	{
		MainForm mf;

		public OptionForm(MainForm f1)
		{
			mf = f1;
			InitializeComponent();
			passwordTextBox.Text = mf.pbPassword;

			var tmp = CommProtocol.getCommAddress();

			if (tmp == CommProtocol.RmAddr.Byte4)
			{
				adr4byteRadioButton.Checked = true;
			}
			else
			{
				adr2byteRadioButton.Checked = true;

			}

		}

		private void okButton_Click(object sender, EventArgs e)
		{
			mf.pbPassword = passwordTextBox.Text.ToString();

			if(adr2byteRadioButton.Checked)
			{
				CommProtocol.setCommAddress2byte();

			}
			else
			{
				CommProtocol.setCommAddress4byte();

			}

			this.Close();

		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();

		}

	}
}
