using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace rmApplication
{
    public partial class MainForm : Form
    {
        private string AssemblyVersion
        {
            get
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                var splited = version.Split('.');
                if (splited.Count() == 4)
                    version = splited[0] + "." + splited[1] + "." + splited[2];

                return version;
            }
        }

        private string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        private SubViewControl subViewCtrl;
        private string pathViewFileName;
        private string pathMapFileName;

        public MainForm(string[] args)
        {
            InitializeComponent();

            this.Text = AssemblyProduct;

            subViewCtrl = new SubViewControl();

            mainPanel.Controls.Add(subViewCtrl);
            subViewCtrl.Dock = DockStyle.Fill;

            if (IsValidRequestCommandLine(args))
            {
                subViewCtrl.RunRemoteServerAsync();
                subViewCtrl.RunMainLogicAsync();
                subViewCtrl.Enabled = false;
            }
            else
            {
                CommMainCtrl.CommunicationMode mode;
                if (Enum.TryParse<CommMainCtrl.CommunicationMode>(Properties.Settings.Default.CommMode, out mode))
                    subViewCtrl.Config.CommMode = mode;

                CommInstructions.RmAddr range;
                if (Enum.TryParse<CommInstructions.RmAddr>(Properties.Settings.Default.RmRange, out range))
                    subViewCtrl.Config.RmRange = range;

                subViewCtrl.Config.SerialPortName = Properties.Settings.Default.SerialPortName;

                string ipAddressText;
                System.Net.IPAddress ipAddress;
                int port;

                ipAddressText = Properties.Settings.Default.ClientAddress;
                port = Properties.Settings.Default.ClientPort;
                if(port != 0 && System.Net.IPAddress.TryParse(ipAddressText, out ipAddress))
                {
                    subViewCtrl.Config.ClientAddress = ipAddress;
                    subViewCtrl.Config.ClientPort = port;
                }


                ipAddressText = Properties.Settings.Default.ServerAddress;
                port = Properties.Settings.Default.ServerPort;
                if (port != 0 && System.Net.IPAddress.TryParse(ipAddressText, out ipAddress))
                {
                    subViewCtrl.Config.ServerAddress = ipAddress;
                    subViewCtrl.Config.ServerPort = port;
                }

                var baudRate = Properties.Settings.Default.BaudRate;
                if (baudRate != 0)
                {
                    subViewCtrl.Config.BaudRate = baudRate;

                    if (Properties.Settings.Default.PassNumber <= UInt32.MaxValue)
                        subViewCtrl.Config.PassNumber = Properties.Settings.Default.PassNumber;

                }

                if (!loadViewFile(Properties.Settings.Default.PathViewFileName))
                {
                    pathViewFileName = null;

                    DefaultViewSettings();

                }
                else
                {
                    pathViewFileName = Properties.Settings.Default.PathViewFileName;

                }


            }

        }


        private bool IsValidRequestCommandLine(string[] args)
        {
            bool isValid = false;

            int index = Array.IndexOf(args, "--remote");

            if (index == -1)
                return isValid;

            if (args.Length >= (index + 3))
            {
                System.Net.IPAddress address;
                int port;
                if ((System.Net.IPAddress.TryParse(args[index + 1], out address)) &&
                    (int.TryParse(args[index + 2], out port)))
                {
                    subViewCtrl.Config.ServerAddress = address;
                    subViewCtrl.Config.ServerPort = port;
                    isValid = true;
                }
                else
                {
                    Console.Write("Invalid parameter");
                }
            }

            return isValid;
        }

        private void DefaultViewSettings()
        {
            var tmpSetting = new ViewSetting();

            for (int i = 0; i < 16; i++)
            {
                tmpSetting.Settings.Add(new DataSetting());
            }

            tmpSetting.Settings[0].Group = "x:Test";

            subViewCtrl.LoadViewSettingFile(tmpSetting);

            var tmp = new ViewFileName();

            this.Text = makeWindowTitle(tmp);

        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            subViewCtrl.ClosingRoutine();

            Properties.Settings.Default.CommMode = subViewCtrl.Config.CommMode.ToString();
            Properties.Settings.Default.RmRange = subViewCtrl.Config.RmRange.ToString();
            Properties.Settings.Default.BaudRate = subViewCtrl.Config.BaudRate;

            Properties.Settings.Default.SerialPortName = subViewCtrl.Config.SerialPortName;

            Properties.Settings.Default.ClientAddress = subViewCtrl.Config.ClientAddress.ToString();
            Properties.Settings.Default.ClientPort = subViewCtrl.Config.ClientPort;

            Properties.Settings.Default.ServerAddress = subViewCtrl.Config.ServerAddress.ToString();
            Properties.Settings.Default.ServerPort = subViewCtrl.Config.ServerPort;

            Properties.Settings.Default.PassNumber = subViewCtrl.Config.PassNumber;

            Properties.Settings.Default.PathViewFileName = pathViewFileName;

            Properties.Settings.Default.Save();

        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsRemote)
            {
                MessageBox.Show("Stop remote server.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }

            if (subViewCtrl.IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }

            if ((subViewCtrl.ViewSettingList != null) &&
                (subViewCtrl.ViewSettingList.Count > 0))
            {
                DialogResult result = MessageBox.Show("The DataGridView is already loaded.\nDo you want to initialize in the DataGridView?",
                                                        "Question",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Exclamation,
                                                        MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                    DefaultViewSettings();

            }


        }


        private void openViewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsRemote)
            {
                MessageBox.Show("Stop remote server.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }

            if (subViewCtrl.IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter =
                "xml File(*.xml)|*.xml|All Files(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "Open View File";
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (loadViewFile(ofd.FileName))
                    pathViewFileName = ofd.FileName;
                else
                    pathViewFileName = null;

            }

            ofd.Dispose();

        }


        private void openMapFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsRemote)
            {
                MessageBox.Show("Stop remote server.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            if (subViewCtrl.IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter =
                "RM Map File(*.rmmap)|*.rmmap|Map File(*.map)|*.map|All Files(*.*)|*.*";
            ofd.FilterIndex = 1;
            ofd.Title = "Open Map File";
            if (!string.IsNullOrEmpty(pathMapFileName))
                ofd.InitialDirectory = System.IO.Path.GetDirectoryName(pathMapFileName);

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (subViewCtrl.LoadMapFile(ofd.FileName))
                {
                    pathMapFileName = ofd.FileName;
                }
                else
                {
                    MessageBox.Show("Can't read map file",
                                        "Caution",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                    pathMapFileName = null;
                }

            }

            ofd.Dispose();

        }

        private void saveViewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            string defaultFileName = string.Empty;
            if (!string.IsNullOrEmpty(pathViewFileName))
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(pathViewFileName);

                ViewFileName tmp = new ViewFileName();
                if (!ViewFileName.GetNameFromOldFormat(fileName, out tmp))
                    ViewFileName.GetName(fileName, out tmp);

                tmp.SoftwareVersion = subViewCtrl.GetTargetVersionName();

                defaultFileName = ViewFileName.MakeFileName(tmp);

            }
            else
            {
                var tmp = new ViewFileName();

                tmp.SettingName = "TestSetting001";
                tmp.SoftwareVersion = subViewCtrl.GetTargetVersionName();

                defaultFileName = ViewFileName.MakeFileName(tmp);

            }

            sfd.Title = "Save View File";
            //sfd.InitialDirectory = @"D:\";
            sfd.FileName = defaultFileName;
            sfd.Filter =
                "xml File(*.xml)|*.xml|All Files(*.*)|*.*";
            sfd.FilterIndex = 1;
            if(!string.IsNullOrEmpty(pathViewFileName))
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(pathViewFileName);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var tmpViewSetting = new ViewSetting();

                foreach (var setting in subViewCtrl.ViewSettingList)
                {
                    foreach (var factor in setting.Settings)
                    {
                        tmpViewSetting.Settings.Add(factor);

                    }

                }

                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));
                        serializer.Serialize(fs, tmpViewSetting);

                    }

                    string fileName = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);

                    ViewFileName tmp = new ViewFileName();
                    ViewFileName.GetName(fileName, out tmp);

                    this.Text = makeWindowTitle(tmp);

                    pathViewFileName = sfd.FileName;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }

            }

            sfd.Dispose();

        }

        private void saveMapFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Save Map File";
            //sfd.InitialDirectory = @"D:\";
            sfd.FileName = "test.rmmap";
            sfd.Filter =
                "RM Map File(*.rmmap)|*.rmmap|All Files(*.*)|*.*";
            sfd.FilterIndex = 1;
            if (!string.IsNullOrEmpty(pathMapFileName))
                sfd.InitialDirectory = System.IO.Path.GetDirectoryName(pathMapFileName);

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                List<string> textList = new List<string>();

                if (RmAddressMap.Convert(textList, subViewCtrl.MapList))
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false, Encoding.GetEncoding("utf-8")))
                        {
                            foreach (var item in textList)
                                sw.WriteLine(item);

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                }

            }

            sfd.Dispose();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsRemote)
            {
                MessageBox.Show("Stop remote server.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            if (subViewCtrl.IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            var form = new OptionForm(subViewCtrl.Config);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();

            subViewCtrl.UpdateInformation();

        }

        private void remoteCtrlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewCtrl.IsRemote)
            {
                MessageBox.Show("Stop remote server.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }

            if (subViewCtrl.IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            var form = new RemoteSettingForm(subViewCtrl.Config);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowDialog();
            
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox newAboutBox = new AboutBox();
            newAboutBox.ShowDialog();
        }

        private bool loadViewFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            bool isSuccess = false;
            ViewSetting deserializedData = new ViewSetting();

            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(path, Encoding.GetEncoding("utf-8")))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));
                    deserializedData = (ViewSetting)serializer.Deserialize(reader);

                }
            }
            catch (Exception)
            {
                deserializedData = null;
            }

            if (deserializedData != null)
            {
                subViewCtrl.LoadViewSettingFile(deserializedData);

                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

                ViewFileName tmp = new ViewFileName();
                if (!ViewFileName.GetNameFromOldFormat(fileName, out tmp))
                    ViewFileName.GetName(fileName, out tmp);

                this.Text = makeWindowTitle(tmp);

                isSuccess = true;

            }

            return isSuccess;
        }

        private string makeWindowTitle(ViewFileName tmp)
        {
            subViewCtrl.SetTargetVersionName(tmp.SoftwareVersion);
            string viewName = tmp.SettingName;

            return viewName + " - " + AssemblyProduct + " " + AssemblyVersion;
        }

    }

}
