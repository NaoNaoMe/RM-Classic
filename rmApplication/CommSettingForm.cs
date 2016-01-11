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
	public partial class CommSettingForm : Form
	{
		MainForm mf;

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


		public CommSettingForm(MainForm f1)
		{
			mf = f1;
			InitializeComponent();
		}

		private void CommSettingForm_Load(object sender, EventArgs e)
		{
			string[] PortList = SerialPort.GetPortNames();

			cmbPortName.Items.Clear();

			foreach (string PortName in PortList)
			{
				cmbPortName.Items.Add(PortName);
			}
			if (cmbPortName.Items.Count > 0)
			{
				cmbPortName.SelectedIndex = 0;
			}

			cmbBaudRate.Items.Clear();

			BuadRateItem baud;
			baud = new BuadRateItem();
			baud.NAME = "4800bps";
			baud.BAUDRATE = 4800;
			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "9600bps";
			baud.BAUDRATE = 9600;
			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "19200bps";
			baud.BAUDRATE = 19200;
			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "38400bps";
			baud.BAUDRATE = 38400;
			cmbBaudRate.Items.Add(baud);

			baud = new BuadRateItem();
			baud.NAME = "115200bps";
			baud.BAUDRATE = 115200;
			cmbBaudRate.Items.Add(baud);
			cmbBaudRate.SelectedIndex = 1;

		}

		private void okButton_Click(object sender, EventArgs e)
		{
			if (tabControl1.SelectedIndex == 0)
			{
				mf.serialPort1.PortName = cmbPortName.SelectedItem.ToString();

				BuadRateItem baud = (BuadRateItem)cmbBaudRate.SelectedItem;
				mf.serialPort1.BaudRate = baud.BAUDRATE;

				mf.serialPort1.DataBits = 8;
				mf.serialPort1.Parity = Parity.None;
				mf.serialPort1.StopBits = StopBits.One;
				mf.serialPort1.Handshake = Handshake.None;

				mf.serialPort1.Encoding = Encoding.ASCII;

				mf.pbCommunicationMode = rmApplication.MainForm.CommMode.Serial;

			}
			else if (tabControl1.SelectedIndex == 1)
			{
				mf.pbLocalIP = localIPTextBox.Text;
				mf.pbPortNum = int.Parse(portTextBox.Text);
				mf.pbCommunicationMode = rmApplication.MainForm.CommMode.NetWork;

			}

			this.Close();

		}

		private void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();

		}
	}
}
