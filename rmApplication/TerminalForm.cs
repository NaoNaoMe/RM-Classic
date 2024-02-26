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

        private class LocalEchoItem : object
        {
            public string Name { set; get; } = "";

            public bool echo { set; get; } = false;

            public override string ToString()
            {
                return Name;
            }

        }   /* class LocalEchoItem */

        private class VerboseItem : object
        {
            public string Name { set; get; } = "";

            public bool verbose { set; get; } = false;

            public override string ToString()
            {
                return Name;
            }

        }   /* class VerboseItem */

        private bool echo;
        private bool verbose;

        public delegate void SendDataFunction(byte[] data);
        public SendDataFunction SendDataFunctionCallback;

        public TerminalForm()
        {
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

            localEchoToolStripComboBox.Items.Clear();

            LocalEchoItem localEchoItem;

            localEchoItem = new LocalEchoItem();
            localEchoItem.Name = "echo off";
            localEchoItem.echo = false;
            localEchoToolStripComboBox.Items.Add(localEchoItem);

            localEchoItem = new LocalEchoItem();
            localEchoItem.Name = "echo on";
            localEchoItem.echo = true;
            localEchoToolStripComboBox.Items.Add(localEchoItem);

            localEchoToolStripComboBox.SelectedIndex = 0;
            echo = false;

            VerboseItem verboseItem;

            verboseItem = new VerboseItem();
            verboseItem.Name = "Verbose off";
            verboseItem.verbose = false;
            verboseToolStripComboBox.Items.Add(verboseItem);

            verboseItem = new VerboseItem();
            verboseItem.Name = "Verbose on";
            verboseItem.verbose = true;
            verboseToolStripComboBox.Items.Add(verboseItem);

            verboseToolStripComboBox.SelectedIndex = 0;
            verbose = false;
        }

        string ConvertLineEndings(string data)
        {
            string tempData = data.Replace("\r\n", "\n");
            tempData = tempData.Replace("\r", "\n");
            return tempData.Replace("\n", "\r\n");
        }

        private void SendText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            string delimiter = "";

            if (lineEndingComboBox.SelectedItem != null)
            {
                LineEndingItem item = (LineEndingItem)lineEndingComboBox.SelectedItem;
                delimiter = item.Delimter;
            }

            text += delimiter;

            var result = System.Text.Encoding.ASCII.GetBytes(text);

            SendDataFunctionCallback?.Invoke(result);

            if(echo)
                UpdateLogtextBox("Tx >>", text);
        }

        private void UpdateLogtextBox(string subText, string text)
        {
            text = ConvertLineEndings(text);
            if (verbose)
            {
                text = text.Replace("\r\n", "");
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                text = time + " " + subText + " " + text;
                logTextBox.AppendText(text + Environment.NewLine);
            }
            else
            {
                logTextBox.AppendText(text);
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            SendText(sendTextBox.Text);
        }

        private void clearToolStripButton_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }

        public void UploadReceivedBytes(byte[] bytes)
        {
            var result = System.Text.Encoding.ASCII.GetString(bytes);

            UpdateLogtextBox("Rx >>", result);
        }

        private void sendTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SendText(sendTextBox.Text);
            }
        }

        private void localEchoToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (localEchoToolStripComboBox.SelectedIndex == 0)
                echo = false;
            else
                echo = true;

        }

        private void verboseToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (verboseToolStripComboBox.SelectedIndex == 0)
                verbose = false;
            else
                verbose = true;
        }
    }
}
