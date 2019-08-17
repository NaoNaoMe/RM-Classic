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
    public partial class TerminalForm : Form
    {
        private class LineEndingItem : object
        {
            public string Name { set; get; } = "";

            public string Delimter { set; get; } = "";

            public override string ToString()
            {
                return Name;
            }

        }   /* class LineEndingItem */

        private SubViewControl subViewCtrl;

        public TerminalForm(SubViewControl tmp)
        {
            subViewCtrl = tmp;

            InitializeComponent();

            lineEndingComboBox.Items.Clear();

            LineEndingItem item;

            item = new LineEndingItem();
            item.Name = "No line ending";
            item.Delimter = "";
            lineEndingComboBox.Items.Add(item);

            item = new LineEndingItem();
            item.Name = "New line";
            item.Delimter = "\n";
            lineEndingComboBox.Items.Add(item);

            item = new LineEndingItem();
            item.Name = "Carriage return";
            item.Delimter = "\r";
            lineEndingComboBox.Items.Add(item);

            item = new LineEndingItem();
            item.Name = "Both NL & CR";
            item.Delimter = "\r\n";
            lineEndingComboBox.Items.Add(item);

            lineEndingComboBox.SelectedIndex = 1;

        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsCommunicationActive == false)
                return;

            var text = sendTextBox.Text;
            string delimiter = "";

            if (lineEndingComboBox.SelectedItem != null)
            {
                LineEndingItem item = (LineEndingItem)lineEndingComboBox.SelectedItem;
                delimiter = item.Delimter;
            }

            var result = System.Text.Encoding.ASCII.GetBytes(text + delimiter);

            subViewCtrl.Logic.SendText(result);
        }

        private void clearToolStripButton_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }

        public void UploadReceivedBytes(List<byte> bytes)
        {
            var result = System.Text.Encoding.ASCII.GetString(bytes.ToArray());

            logTextBox.AppendText(result);

        }

        private void sendTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (subViewCtrl.IsCommunicationActive == false)
                    return;

                var text = sendTextBox.Text;
                string delimiter = "";

                if (lineEndingComboBox.SelectedItem != null)
                {
                    LineEndingItem item = (LineEndingItem)lineEndingComboBox.SelectedItem;
                    delimiter = item.Delimter;
                }

                var result = System.Text.Encoding.ASCII.GetBytes(text + delimiter);

                subViewCtrl.Logic.SendText(result);

            }
        }
    }
}
