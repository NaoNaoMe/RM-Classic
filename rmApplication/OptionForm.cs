using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;

namespace rmApplication
{
	public partial class OptionForm : Form
	{
		private SubViewControl SubViewCtrl;

		private class BuadRateItem : Object
		{
			private string m_name = "";
			private int m_value = 0;

			public string NAME
			{
				set { m_name = value; }
				get { return m_name; }
			}

			public int BAUDRATE
			{
				set { m_value = value; }
				get { return m_value; }
			}

			public override string ToString()
			{
				return m_name;
			}

		}	/* class BuadRateItem */


		public OptionForm(SubViewControl tmp)
		{
			SubViewCtrl = tmp;
			InitializeComponent();

		}

		private void OptionForm_Load(object sender, EventArgs e)
		{
			string[] PortList = SerialPort.GetPortNames();

			cmbPortName.Items.Clear();

			int tmpIndex = 0;
			int foundIndex = 0;
			bool foundFlg = false;

			foreach (var PortName in PortList)
			{
				cmbPortName.Items.Add(PortName);

				if (PortName == SubViewCtrl.myComponents.CommPort)
				{
					foundFlg = true;
					foundIndex = tmpIndex;
				}

				tmpIndex++;

			}

			if (foundFlg == true)
			{
				cmbPortName.SelectedIndex = foundIndex;

			}
			else
			{
				if (cmbPortName.Items.Count > 0)
				{
					cmbPortName.SelectedIndex = cmbPortName.Items.Count - 1;
				}

			}

			foundIndex = 0;
			foundFlg = false;

			cmbBaudRate.Items.Clear();

			BuadRateItem baud;
			baud = new BuadRateItem();
			baud.NAME = "4800bps";
			baud.BAUDRATE = 4800;

			if (baud.BAUDRATE == SubViewCtrl.myComponents.CommBaudRate)
			{
				foundFlg = true;
				foundIndex = 0;
			}

			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "9600bps";
			baud.BAUDRATE = 9600;

			if (baud.BAUDRATE == SubViewCtrl.myComponents.CommBaudRate)
			{
				foundFlg = true;
				foundIndex = 1;
			}

			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "19200bps";
			baud.BAUDRATE = 19200;

			if (baud.BAUDRATE == SubViewCtrl.myComponents.CommBaudRate)
			{
				foundFlg = true;
				foundIndex = 2;
			}

			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "38400bps";
			baud.BAUDRATE = 38400;

			if (baud.BAUDRATE == SubViewCtrl.myComponents.CommBaudRate)
			{
				foundFlg = true;
				foundIndex = 3;
			}

			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "115200bps";
			baud.BAUDRATE = 115200;

			if (baud.BAUDRATE == SubViewCtrl.myComponents.CommBaudRate)
			{
				foundFlg = true;
				foundIndex = 4;
			}

			cmbBaudRate.Items.Add(baud);

			if (foundFlg == true)
			{
				cmbBaudRate.SelectedIndex = foundIndex;

			}
			else
			{
				cmbBaudRate.SelectedIndex = 1;

			}


			localIPTextBox.Text = SubViewCtrl.myComponents.NetIP;
			portTextBox.Text = SubViewCtrl.myComponents.NetPort.ToString();


			passwordTextBox.Text = SubViewCtrl.myComponents.Password;

			var tmp = SubViewCtrl.myCommProtocol.myComponent.SelectByte;

			if (tmp == CommProtocol.Components.RmAddr.Byte4)
			{
				adr4byteRadioButton.Checked = true;
			}
			else
			{
				adr2byteRadioButton.Checked = true;

			}

		}

		private void OptionForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			SubViewCtrl.checkDataGridViewCells();
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			if (tabControl1.SelectedIndex == 0)
			{
				SubViewCtrl.myComponents.CommPort = cmbPortName.SelectedItem.ToString();

				BuadRateItem baud = (BuadRateItem)cmbBaudRate.SelectedItem;
				SubViewCtrl.myComponents.CommBaudRate = baud.BAUDRATE;

				SubViewCtrl.myComponents.CommunicationMode = SubViewControl.Components.CommMode.Serial;

			}
			else if (tabControl1.SelectedIndex == 1)
			{
				SubViewCtrl.myComponents.NetIP = localIPTextBox.Text;
				SubViewCtrl.myComponents.NetPort = int.Parse(portTextBox.Text);
				SubViewCtrl.myComponents.CommunicationMode = SubViewControl.Components.CommMode.NetWork;
				
				// Baudrate must be defined by Uart-Network ConverterUnit.
				BuadRateItem baud = (BuadRateItem)cmbBaudRate.SelectedItem;
				SubViewCtrl.myComponents.CommBaudRate = baud.BAUDRATE;
				
			}

			SubViewCtrl.myComponents.Password = passwordTextBox.Text;

			if (adr2byteRadioButton.Checked)
			{
				SubViewCtrl.myCommProtocol.myComponent.SelectByte = CommProtocol.Components.RmAddr.Byte2;

			}
			else
			{
				SubViewCtrl.myCommProtocol.myComponent.SelectByte = CommProtocol.Components.RmAddr.Byte4;

			}

			this.Close();

		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();

		}

	}
}
