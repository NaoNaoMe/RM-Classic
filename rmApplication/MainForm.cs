using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace rmApplication
{
    public partial class MainForm : Form
    {
        private const string WINDOW_TITLE = "RM Classic";
        private const string GROUP_INITIAL_TAG = "x:Test";

        private SubViewControl subViewControl1;

        public MainForm()
        {
            InitializeComponent();
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            subViewControl1 = new SubViewControl();

            mainPanel.Controls.Add(subViewControl1);
            subViewControl1.Dock = DockStyle.Fill;

            var tmpVSettingFactor = new ViewSetting();

            for (int i = 0; i < 16; i++)
            {
                tmpVSettingFactor.DataSetting.Add(new DataSetting());
            }

            tmpVSettingFactor.DataSetting[0].Group = GROUP_INITIAL_TAG;

            subViewControl1.loadViewSettingFile(tmpVSettingFactor);

        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            subViewControl1.commonClosingRoutine();

        }


        private void openViewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewControl1.myComponents.CommActiveFlg == true)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;

            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.FileName = "test.xml";
                // ofd.InitialDirectory = @"D:\";
                ofd.Filter =
                    "ViewSetting File(*.xml)|*.xml|All Files(*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.Title = "Open View File";
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string pathName = ofd.FileName;
                    ViewSetting deserializedData = new ViewSetting();

                    try
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(pathName, Encoding.GetEncoding("utf-8")))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));
                            deserializedData = (ViewSetting)serializer.Deserialize(reader);

                        }
                    }
                    catch (Exception ex)
                    {
                        deserializedData = null;
                        MessageBox.Show(ex.Message);

                    }

                    if (deserializedData != null)
                    {
                        subViewControl1.loadViewSettingFile(deserializedData);

                        string fileName = System.IO.Path.GetFileNameWithoutExtension(pathName);
                        string viewName = subViewControl1.getViewName(fileName);

                        if (viewName != null)
                        {
                            this.Text = viewName + " - " + WINDOW_TITLE;
                        }

                    }

                }

                ofd.Dispose();

            }

        }

        private void openMapFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewControl1.myComponents.CommActiveFlg == true)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();

                ofd.FileName = "test.map";
                //ofd.InitialDirectory = @"D:\";
                ofd.Filter =
                    "Map File(*.map)|*.map|All Files(*.*)|*.*";
                ofd.FilterIndex = 1;
                ofd.Title = "Open Map File";
                ofd.RestoreDirectory = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    if (subViewControl1.loadMapFile(ofd.FileName) == false)
                    {
                        MessageBox.Show("Can't read map file",
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                    }

                }
                else
                {

                }

                ofd.Dispose();

            }

        }


        private void saveViewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            string defaultFileName = subViewControl1.getViewSettingFileName();

            sfd.Title = "Save View File";
            //sfd.InitialDirectory = @"D:\";
            sfd.FileName = defaultFileName;
            sfd.Filter =
                "ViewSetting File(*.xml)|*.xml|All Files(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.ShowHelp = true;
            sfd.CreatePrompt = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var tmpViewSetting = new ViewSetting();

                foreach (var factor in subViewControl1.myComponents.ViewSettingList)
                {
                    foreach (var item in factor.DataSetting)
                    {
                        tmpViewSetting.DataSetting.Add(item);

                    }

                }

                ViewSettingMisc.replaceEmptyWithNull(ref tmpViewSetting);

                try
                {
                    using (System.IO.FileStream fs = new System.IO.FileStream(sfd.FileName, System.IO.FileMode.Create))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));
                        serializer.Serialize(fs, tmpViewSetting);

                    }

                    string fileName = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
                    string viewName = subViewControl1.getViewName(fileName);

                    if (viewName != null)
                    {
                        this.Text = viewName + " - " + WINDOW_TITLE;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }

            }
            else
            {

            }

            sfd.Dispose();

        }

        private void saveMapFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((subViewControl1.myComponents.MapList == null) ||
                (subViewControl1.myComponents.MapList.Count == 0))
            {
                return;

            }

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Title = "Save Map File";
            //sfd.InitialDirectory = @"D:\";
            sfd.FileName = "test.map";
            sfd.Filter =
                "Map File(*.map)|*.map|All Files(*.*)|*.*";
            sfd.FilterIndex = 1;
            sfd.RestoreDirectory = true;
            sfd.ShowHelp = true;
            sfd.CreatePrompt = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                List<string> textList = new List<string>();

                bool ret = RmAddressMap.Convert(textList, subViewControl1.myComponents.MapList);

                if (ret == true)
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false, Encoding.GetEncoding("utf-8")))
                        {
                            foreach (var item in textList)
                            {
                                sw.WriteLine(item);

                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                }

            }
            else
            {

            }

            sfd.Dispose();

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();

        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (subViewControl1.myComponents.CommActiveFlg == true)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

            }
            else
            {
                OptionForm otpnForm = new OptionForm(subViewControl1);
                otpnForm.ShowDialog();

            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox newAboutBox = new AboutBox();
            newAboutBox.ShowDialog();
        }


    }

}
