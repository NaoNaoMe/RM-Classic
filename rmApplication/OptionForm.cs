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
                {
                    cmbPortName.SelectedIndex = cmbPortName.Items.Count - 1;
                }

            }

            cmbBaudRate.Items.Clear();

            BuadRateItem baud;
            baud = new BuadRateItem();
            baud.Name = "4800bps";
            baud.Buadrate = 4800;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "9600bps";
            baud.Buadrate = 9600;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "19200bps";
            baud.Buadrate = 19200;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "38400bps";
            baud.Buadrate = 38400;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "57600bps";
            baud.Buadrate = 57600;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "115200bps";
            baud.Buadrate = 115200;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "230400bps";
            baud.Buadrate = 230400;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "256000bps";
            baud.Buadrate = 256000;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "460800bps";
            baud.Buadrate = 460800;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "512000bps";
            baud.Buadrate = 512000;
            cmbBaudRate.Items.Add(baud);

            baud = new BuadRateItem();
            baud.Name = "921600bps";
            baud.Buadrate = 921600;
            cmbBaudRate.Items.Add(baud);


            foundIndex = 0;
            foundFlg = false;

            foreach (BuadRateItem item in cmbBaudRate.Items)
            {
                if(item.Buadrate == config.BaudRate)
                {
                    foundFlg = true;
                    break;
                }
                foundIndex++;
            }

            if (foundFlg == true)
                cmbBaudRate.SelectedIndex = foundIndex;
            else
                cmbBaudRate.SelectedIndex = 1;

            localIPTextBox.Text = config.ClientAddress.ToString();
            portTextBox.Text = config.ClientPort.ToString();

            if(config.PassNumber <= UInt32.MaxValue)
            {
                var passwordText = config.PassNumber.ToString("X");
                passwordTextBox.Text = passwordText.PadLeft(8, '0');

            }
            else
            {
                passwordTextBox.Text = "0000FFFF";
            }

            if (config.RmRange == CommInstructions.RmAddr.Byte2)
                addr2byteRadioButton.Checked = true;
            else
                addr4byteRadioButton.Checked = true;

            if (config.CommMode == CommMainCtrl.CommunicationMode.Serial)
                settingTabControl.SelectedTab = serialCommTabPage;
            else
                settingTabControl.SelectedTab = localNetTabPage;

        }

        private void OptionForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Configuration tmpConfig = new rmApplication.Configuration();

            if (cmbPortName.SelectedItem != null)
                tmpConfig.SerialPortName = cmbPortName.SelectedItem.ToString();

            System.Net.IPAddress address;
            if (System.Net.IPAddress.TryParse(localIPTextBox.Text, out address))
                tmpConfig.ClientAddress = address;

            int port;
            if (int.TryParse(portTextBox.Text, out port))
                tmpConfig.ClientPort = port;

            bool isFailed = false;

            if (settingTabControl.SelectedTab == serialCommTabPage)
            {
                tmpConfig.CommMode = CommMainCtrl.CommunicationMode.Serial;

                if (string.IsNullOrEmpty(tmpConfig.SerialPortName))
                    isFailed = true;

            }
            else if (settingTabControl.SelectedTab == localNetTabPage)
            {
                tmpConfig.CommMode = CommMainCtrl.CommunicationMode.LocalNet;

                if(tmpConfig.ClientAddress.Equals(System.Net.IPAddress.Parse("0.0.0.0")))
                    isFailed = true;

            }
            else
            {
                isFailed = true;

            }

            if (cmbBaudRate.SelectedItem != null)
            {
                BuadRateItem baud = (BuadRateItem)cmbBaudRate.SelectedItem;
                tmpConfig.BaudRate = baud.Buadrate;
            }
            else
            {
                isFailed = true;

            }


            UInt32 passnumber;
            if (!UInt32.TryParse(passwordTextBox.Text, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out passnumber))
                isFailed = true;
            else
                tmpConfig.PassNumber = passnumber;

            if (addr2byteRadioButton.Checked)
                tmpConfig.RmRange = CommInstructions.RmAddr.Byte2;
            else
                tmpConfig.RmRange = CommInstructions.RmAddr.Byte4;

            if(!isFailed)
            {
                config.CommMode = tmpConfig.CommMode;
                config.RmRange = tmpConfig.RmRange;
                config.BaudRate = tmpConfig.BaudRate;

                config.SerialPortName = tmpConfig.SerialPortName;

                config.ClientAddress = tmpConfig.ClientAddress;
                config.ClientPort = tmpConfig.ClientPort;

                config.PassNumber = tmpConfig.PassNumber;
                this.Close();
            }
            else
            {
                MessageBox.Show("Some options are invalid.",
                    "Caution",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void portCheckButton_Click(object sender, EventArgs e)
        {
            if (cmbPortName.SelectedItem == null)
            {
                MessageBox.Show("Invalid!!");
                return;
            }

            var port = cmbPortName.SelectedItem.ToString();

            try
            {
                using (System.IO.Ports.SerialPort tmpSerial = new System.IO.Ports.SerialPort())
                {
                    tmpSerial.PortName = port;
                    tmpSerial.Open();
                    MessageBox.Show("Success!!");
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
            System.Net.IPAddress address;
            if (!System.Net.IPAddress.TryParse(localIPTextBox.Text, out address))
            {
                MessageBox.Show("Invalid!!");
                return;
            }

            try
            {
                using (System.Net.NetworkInformation.Ping tmpPing = new System.Net.NetworkInformation.Ping())
                {
                    System.Net.NetworkInformation.PingReply reply = tmpPing.Send(address, 1);

                    if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        MessageBox.Show("Success!!");
                    }
                    else
                    {
                        MessageBox.Show("Failed!!");
                    }

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

        }
    }
}
