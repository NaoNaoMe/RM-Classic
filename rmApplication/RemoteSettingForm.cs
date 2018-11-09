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
        private bool isRemoteState;

        public RemoteSettingForm(Configuration tmp, bool isRemote)
        {
            config = tmp;
            isRemoteState = isRemote;
            InitializeComponent();

            if (config.ServerAddress != null)
            {
                serverAddressTextBox.Text = config.ServerAddress.ToString();
                serverPortTextBox.Text = config.ServerPort.ToString();
            }

            if (isRemoteState)
            {
                serverAddressTextBox.Enabled = false;
                serverPortTextBox.Enabled = false;
                enableButton.Text = "LOCAL";
            }
            else
            {
                serverAddressTextBox.Enabled = true;
                serverPortTextBox.Enabled = true;
                enableButton.Text = "REMOTE";
            }

        }

        private void enableButton_Click(object sender, EventArgs e)
        {
            if(isRemoteState)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            else
            {
                System.Net.IPAddress address;
                int port;
                if ((System.Net.IPAddress.TryParse(serverAddressTextBox.Text, out address)) &&
                    (int.TryParse(serverPortTextBox.Text, out port)))
                {
                    config.ServerAddress = address;
                    config.ServerPort = port;

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    serverAddressTextBox.Text = config.ServerAddress.ToString();
                    serverPortTextBox.Text = config.ServerPort.ToString();
                }

            }

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            this.Close();
        }
    }
}
