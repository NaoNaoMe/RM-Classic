using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Xml.Serialization;
using System.ComponentModel;


namespace rmApplication
{
	public partial class MainForm : Form
	{
		private OptionForm OptionFormInstance;

		public MainForm()
		{
			InitializeComponent();
		}


		private void MainForm_Load(object sender, EventArgs e)
		{
			subViewControl1.commonInitialRoutine();

			var tmpVSettingFactor = new ViewSetting();

			for (int i = 0; i < 32; i++)
			{
				tmpVSettingFactor.DataSetting.Add(new DataSetting());
			}

			tmpVSettingFactor.DataSetting[0].Group = SubViewControl.GROUP_TEMPORARY_TAG;

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
					string path = ofd.FileName;

					XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));

					ViewSetting deserializedData = new ViewSetting();

					try
					{
						StreamReader reader = new StreamReader(path);
						deserializedData = (ViewSetting)serializer.Deserialize(reader);
						reader.Close();

					}
					catch (Exception ex)
					{
						deserializedData = null;
						MessageBox.Show(ex.Message);

					}

					if (deserializedData != null)
					{
						subViewControl1.loadViewSettingFile(deserializedData);

						string name = subViewControl1.getViewName(path);

						if (name != null)
						{
							this.Text = name + " - " + this.Text;
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
				var tmpVSettingFactor = new ViewSetting();

				foreach (var factor in subViewControl1.myComponents.ViewSettingList)
				{
					foreach (var item in factor.DataSetting)
					{
						tmpVSettingFactor.DataSetting.Add(item);

					}

				}

				try
				{
					FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
					XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));
					serializer.Serialize(fs, tmpVSettingFactor);
					fs.Close();
					
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
			if ( (subViewControl1.myComponents.MapList == null ) ||
				(subViewControl1.myComponents.MapList.Count == 0 ) )
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
				
				if( ret == true )
				{
					try
					{
						System.IO.StreamWriter sw = new System.IO.StreamWriter(
							sfd.FileName,
							false,
							Encoding.GetEncoding("utf-8"));

						foreach (var item in textList)
						{
							sw.WriteLine(item);

						}

						sw.Close();
						
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

		private void settingViewToolStripMenuItem_Click(object sender, EventArgs e)
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
				subViewControl1.customizeDataGridView();
				
			}
		}

		private void changeViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			subViewControl1.changeDataGridViewColumn();

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
				if (OptionFormInstance != null)
				{
					OptionFormInstance.Close();
				}

				OptionFormInstance = new OptionForm(subViewControl1);
				OptionFormInstance.Show();

			}

		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			AboutBox newAboutBox = new AboutBox();
			newAboutBox.ShowDialog();
		}


	}

}
