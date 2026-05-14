using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace rmApplication
{
    public partial class RemoteSettingForm : Form
    {
        private Configuration config;

        public RemoteSettingForm(Configuration tmp)
        {
            config = tmp;
            InitializeComponent();

            if (config.ServerAddress != null)
            {
                serverAddressTextBox.Text = config.ServerAddress.ToString();
                serverPortTextBox.Text = config.ServerPort.ToString();
            }

            serverAddressTextBox.KeyPress += textbox_KeyPress;
            serverPortTextBox.KeyPress += textbox_KeyPress;
        }

        private void textbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                e.Handled = true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!System.Net.IPAddress.TryParse(serverAddressTextBox.Text, out System.Net.IPAddress address))
            {
                MessageBox.Show("Invalid IP address format.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                serverAddressTextBox.Focus();
                return;
            }

            if (!int.TryParse(serverPortTextBox.Text, out int port))
            {
                MessageBox.Show("Invalid port format.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                serverPortTextBox.Focus();
                return;
            }

            config.ServerAddress = address;
            config.ServerPort = port;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            this.Close();
        }
    }
}
