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
        private Configuration config;

        private class BuadRateItem : Object
        {
            private string name = "";
            private int buadrate = 0;
            public string Name
            {
                set { name = value; }
                get { return name; }
            }
            public int Buadrate
            {
                set { buadrate = value; }
                get { return buadrate; }
            }
            public override string ToString()
            {
                return name;
            }
        }   /* class BuadRateItem */

        public OptionForm(Configuration tmp)
        {
            config = tmp;
            InitializeComponent();

            passwordTextBox.KeyPress += textBox_KeyPress;
            localIPTextBox.KeyPress += textBox_KeyPress;
            portTextBox.KeyPress += textBox_KeyPress;
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                e.Handled = true;
        }

        private void OptionForm_Load(object sender, EventArgs e)
        {
            string[] PortList = SerialPort.GetPortNames();
            int tmpIndex = 0;
            int foundIndex = 0;
            bool foundFlg = false;
            cmbPortName.Items.Clear();
            foreach (var PortName in PortList)
            {
                cmbPortName.Items.Add(PortName);
                if (PortName == config.SerialPortName)
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
                    cmbPortName.SelectedIndex = cmbPortName.Items.Count - 1;
            }

            cmbBaudRate.Items.Clear();
            BuadRateItem baud;
            baud = new BuadRateItem(); baud.Name = "4800bps"; baud.Buadrate = 4800; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "9600bps"; baud.Buadrate = 9600; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "19200bps"; baud.Buadrate = 19200; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "38400bps"; baud.Buadrate = 38400; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "57600bps"; baud.Buadrate = 57600; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "115200bps"; baud.Buadrate = 115200; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "230400bps"; baud.Buadrate = 230400; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "256000bps"; baud.Buadrate = 256000; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "460800bps"; baud.Buadrate = 460800; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "512000bps"; baud.Buadrate = 512000; cmbBaudRate.Items.Add(baud);
            baud = new BuadRateItem(); baud.Name = "921600bps"; baud.Buadrate = 921600; cmbBaudRate.Items.Add(baud);

            foundIndex = 0;
            foundFlg = false;
            foreach (BuadRateItem item in cmbBaudRate.Items)
            {
                if (item.Buadrate == config.BaudRate)
                {
                    foundFlg = true;
                    break;
                }
                foundIndex++;
            }
            if (foundFlg == true)
                cmbBaudRate.SelectedIndex = foundIndex;
            else
                cmbBaudRate.SelectedIndex = 5;

            localIPTextBox.Text = config.ClientAddress.ToString();
            portTextBox.Text = config.ClientPort.ToString();

            passwordTextBox.Text = config.PassNumber.ToString("X").PadLeft(8, '0');

            if (config.RmRange == CommInstructions.RmAddr.Byte2)
                addr2byteRadioButton.Checked = true;
            else
                addr4byteRadioButton.Checked = true;

            if (config.CommMode == CommMainCtrl.CommunicationMode.Serial)
                settingTabControl.SelectedTab = serialCommTabPage;
            else
                settingTabControl.SelectedTab = localNetTabPage;

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            // Password (common, 32bit hex)
            if (!UInt32.TryParse(passwordTextBox.Text,
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out UInt32 passNumber))
            {
                MessageBox.Show("Invalid password format. Enter an 8-digit hex value. (e.g. 0000FFFF)",
                    "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                passwordTextBox.Focus();
                return;
            }

            // Baud rate
            if (cmbBaudRate.SelectedItem == null)
            {
                MessageBox.Show("Select a baud rate.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbBaudRate.Focus();
                return;
            }
            BuadRateItem selectedBaud = (BuadRateItem)cmbBaudRate.SelectedItem;

            // Mode-specific
            CommMainCtrl.CommunicationMode mode;
            string serialPortName = "";
            System.Net.IPAddress clientAddress = null;
            int clientPort = 0;

            if (settingTabControl.SelectedTab == serialCommTabPage)
            {
                mode = CommMainCtrl.CommunicationMode.Serial;
                if (cmbPortName.SelectedItem == null)
                {
                    MessageBox.Show("Select a serial port.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbPortName.Focus();
                    return;
                }
                serialPortName = cmbPortName.SelectedItem.ToString();
            }
            else if (settingTabControl.SelectedTab == localNetTabPage)
            {
                mode = CommMainCtrl.CommunicationMode.LocalNet;
                if (!System.Net.IPAddress.TryParse(localIPTextBox.Text, out clientAddress) ||
                    clientAddress.Equals(System.Net.IPAddress.Parse("0.0.0.0")))
                {
                    MessageBox.Show("Invalid IP address format.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    localIPTextBox.Focus();
                    return;
                }
                if (!int.TryParse(portTextBox.Text, out clientPort) || clientPort < 1 || clientPort > 65535)
                {
                    MessageBox.Show("Invalid port number. (1 - 65535)", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    portTextBox.Focus();
                    return;
                }
            }
            else
            {
                return;
            }

            // Apply
            config.CommMode = mode;
            config.PassNumber = passNumber;
            config.BaudRate = selectedBaud.Buadrate;
            config.RmRange = addr2byteRadioButton.Checked ? CommInstructions.RmAddr.Byte2 : CommInstructions.RmAddr.Byte4;
            config.SerialPortName = serialPortName;
            if (clientAddress != null)
            {
                config.ClientAddress = clientAddress;
                config.ClientPort = clientPort;
            }
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void portCheckButton_Click(object sender, EventArgs e)
        {
            if (cmbPortName.SelectedItem == null)
            {
                MessageBox.Show("Select a serial port.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var port = cmbPortName.SelectedItem.ToString();
            try
            {
                using (System.IO.Ports.SerialPort tmpSerial = new System.IO.Ports.SerialPort())
                {
                    tmpSerial.PortName = port;
                    tmpSerial.Open();
                    MessageBox.Show(port + " is available.");
                    tmpSerial.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pingButton_Click(object sender, EventArgs e)
        {
            if (!System.Net.IPAddress.TryParse(localIPTextBox.Text, out System.Net.IPAddress address))
            {
                MessageBox.Show("Invalid IP address format.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                localIPTextBox.Focus();
                return;
            }
            try
            {
                using (System.Net.NetworkInformation.Ping tmpPing = new System.Net.NetworkInformation.Ping())
                {
                    var reply = tmpPing.Send(address, 1);
                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                        MessageBox.Show("Ping success.");
                    else
                        MessageBox.Show("Ping failed.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
