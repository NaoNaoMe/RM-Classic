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

        }

        private void okButton_Click(object sender, EventArgs e)
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

                MessageBox.Show("Invalid parameters.",
                    "Caution",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            this.Close();
        }
    }
}
