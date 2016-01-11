using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO.Ports;

using System.Xml.Serialization;
using System.IO;

namespace rmApplication
{
	public partial class MainForm : Form
	{
		public enum CommMode
		{
			NotDefine,
			Serial,
			NetWork
		}

		public CommMode pbCommunicationMode = CommMode.NotDefine;
		public string pbLocalIP;
		public int pbPortNum;
		public string pbPassword = "0000FFFF";
		public MapInfo.List MapList;

		private enum DgvRowName : int		// Column name of datagirdview1
		{
			Check = 0,
			Size,
			Variant,
			Addrlock,
			Address,
			Offset,
			Name,
			Type,
			ReadText,		// hidden cell
			ReadValue,
			WriteText,		// hidden cell
			WriteValue,
			WrTrg
		}


		private const int COLUMN_NUM = 32;							//表示行数の規定
		private const int SELECT_NUM = 32;							//選択できる最大
		private const int MAX_TOTAL_SIZE = 128;						//ログデータ最大サイズ

		private bool CommActiveFlg;
		private bool CustomizingModeFlg;

		private ViewSetting MainViewSetting;
		private List<DataSetting> WorkingDataSourceForDGV;
		private DataGridViewRow[] CheckedCellData;

		private string ValidMapPath;
		private DateTime ValidMapLastWrittenDate;

		private struct SocketsAsyncParam
		{
			public System.Net.Sockets.TcpClient Client;
			public byte[] ReadBuff;
		}

		private static SocketsAsyncParam SocketsParam;

		private DumpForm DumpFormInstance;



		private void saveLogData( string header, List<string> listString )
		{
			if( listString.Count != 0 )
			{
				try
				{
					string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

					string directoryPath = System.IO.Path.GetDirectoryName(appPath);

					string logPath = directoryPath + @"\" + "log.csv";

					System.IO.StreamWriter sw = new System.IO.StreamWriter(
						logPath,
						false,
						System.Text.Encoding.GetEncoding("utf-8"));

					sw.WriteLine(header);

					foreach (var data in listString)
					{
						sw.WriteLine(data);

					}

					sw.Close();

				}
				catch (Exception ex)
				{

				}
			}
		}


		private void reloadcommLogtextBox(string data)
		{
			if (data != null)
			{
				List<string> lines = new List<string>(commLogtextBox.Lines);

				if (lines.Count > 500)
				{
					lines.RemoveAt(0);
					commLogtextBox.Text = String.Join("\r\n", lines);

				}

				commLogtextBox.AppendText(data + Environment.NewLine);

			}

		}

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Redefined Column Name
			dataGridView1.Columns[0].Name = DgvRowName.Check.ToString();
			dataGridView1.Columns[1].Name = DgvRowName.Size.ToString();
			dataGridView1.Columns[2].Name = DgvRowName.Variant.ToString();
			dataGridView1.Columns[3].Name = DgvRowName.Addrlock.ToString();
			dataGridView1.Columns[4].Name = DgvRowName.Address.ToString();
			dataGridView1.Columns[5].Name = DgvRowName.Offset.ToString();
			dataGridView1.Columns[6].Name = DgvRowName.Name.ToString();
			dataGridView1.Columns[7].Name = DgvRowName.Type.ToString();
			dataGridView1.Columns[8].Name = DgvRowName.ReadText.ToString();
			dataGridView1.Columns[9].Name = DgvRowName.ReadValue.ToString();
			dataGridView1.Columns[10].Name = DgvRowName.WriteText.ToString();
			dataGridView1.Columns[11].Name = DgvRowName.WriteValue.ToString();
			dataGridView1.Columns[12].Name = DgvRowName.WrTrg.ToString();

			//Stop Communication
			CommActiveFlg = false;

			//Not customize DataGridView
			CustomizingModeFlg = false;

			WorkingDataSourceForDGV = new List<DataSetting>();

			MainViewSetting = new ViewSetting();
			MainViewSetting.DataSetting = new List<DataSetting>();

			ViewSettingEntity.InitialDataSource(WorkingDataSourceForDGV, COLUMN_NUM);

			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;
			dataGridView1.DataSource = WorkingDataSourceForDGV;

			for (int i = 0; i < dataGridView1.Rows.Count; i++)
			{
				dataGridView1.Rows[i].HeaderCell.Value = i.ToString();
			}

			dataGridView1.RowHeadersWidth = 48;

			editingCtrl(true);

			deleteSpecifiedCells();

			mainTimer.Enabled = true;

			dutVerViewControl.Label =		"DUT     Version:";
			targetVerViewControl.Label =	"Target  Version:";
			settingVerViewControl.Label =	"Setting Version:";

		}


		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (serialPort1.IsOpen == true)
			{
				serialPort1.Close();

			}
			
		}


		private void editingCtrl(bool flg)
		{
			dataGridView1.Columns[(int)DgvRowName.Check].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Size].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Variant].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Addrlock].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Address].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Offset].ReadOnly = flg;
			dataGridView1.Columns[(int)DgvRowName.Name].ReadOnly = flg;

		}


		private void visibleCtrl(bool flg)
		{
			dataGridView1.Columns[(int)DgvRowName.Size].Visible = flg;
			dataGridView1.Columns[(int)DgvRowName.Variant].Visible = flg;
			dataGridView1.Columns[(int)DgvRowName.Addrlock].Visible = flg;
			dataGridView1.Columns[(int)DgvRowName.Address].Visible = flg;
			dataGridView1.Columns[(int)DgvRowName.Offset].Visible = flg;

		}


		private void deleteSpecifiedCells()
		{
			foreach (DataGridViewRow item in dataGridView1.Rows)
			{
				item.Cells[(int)DgvRowName.ReadText].Value = null;
				item.Cells[(int)DgvRowName.ReadValue].Value = null;
				//item.Cells[(int)DgvRowName.WriteText].Value = null;

			}

		}


		private bool checkTimeStampAboutMapFile()
		{
			bool reload_flg = false;

			if ((File.Exists(ValidMapPath)) &&
				(ValidMapPath != null))
			{
				DateTime now = File.GetLastWriteTime(ValidMapPath);

				if (now > ValidMapLastWrittenDate)
				{
					DialogResult result = MessageBox.Show("Map file was updated.\nDo you want to reload Address in Data Grid View?",
															"Question",
															MessageBoxButtons.YesNo,
															MessageBoxIcon.Exclamation,
															MessageBoxDefaultButton.Button2);

					if (result == DialogResult.Yes)
					{
						ValidMapLastWrittenDate = now;
						reload_flg = true;

					}
					else if (result == DialogResult.No)
					{

					}

				}
				else
				{

				}

			}
			else
			{

			}

			return reload_flg;

		}


		private void renewLogSetting()
		{
			bool flg = CommResource_CheckState();

			if( flg == false )
			{
				return;

			}

			CheckedCellData = (from DataGridViewRow x in dataGridView1.Rows where (bool)x.Cells[(int)DgvRowName.Check].Value == true select x).ToArray();

			List<string> listSize = new List<string>();
			List<string> listAddress = new List<string>();
			List<string> listOffset = new List<string>();
			List<string> listType = new List<string>();
			int maxIndex = CheckedCellData.Length;

			bool errFlg = false;

			for (int i = 0; i < maxIndex; i++)
			{
				if ((string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value as string) == true) ||
					(string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Address].Value as string) == true) ||
					(string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Offset].Value as string) == true) ||
					(string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Type].Value as string) == true))
				{
					errFlg = true;
					break;

				}
				else
				{
					listSize.Add(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value.ToString());
					listAddress.Add(CheckedCellData[i].Cells[(int)DgvRowName.Address].Value.ToString());
					listOffset.Add(CheckedCellData[i].Cells[(int)DgvRowName.Offset].Value.ToString());
					listType.Add(CheckedCellData[i].Cells[(int)DgvRowName.Type].Value.ToString());
					
				}
				
				
				
			}
			
			if( maxIndex == 0 )
			{
				MessageBox.Show("No cheked cells.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);
				
			}
			else if( errFlg == true )
			{
				MessageBox.Show("Can't send Address Data.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);
				
			}
			else
			{
				CommProtocol.setLogData(listSize, listType, listAddress, listOffset, maxIndex);

				CommProtocol.pbCommLogBuffer = new List<string>();
				
			}
			

		}


		private void renewTimingSetting(string timingNum)
		{
			bool flg = CommResource_CheckState();

			if (flg == false)
			{
				return;

			}

			Exception ex_text = null;

			string timngVal = TypeConvert.ToHexChars(numeralSystem.UDEC, 2, timingNum, out ex_text);

			CommProtocol.setTiming(timngVal);

		}


		private void writeData(string size, string address, string offset, string writeVal)
		{
			bool flg = CommResource_CheckState();

			if (flg == false)
			{
				return;

			}

			CommProtocol.wirteData(size, address, offset, writeVal);

		}


		private void reviseAddressAndSizeFromMapFile()
		{
			if ((MapList != null) &&
				 (MainViewSetting != null))
			{
				int data_size = MainViewSetting.DataSetting.Count;

				for (int j = 0; j < data_size; j++)
				{
					if ((MainViewSetting.DataSetting[j].Variant != null)&&
						(MainViewSetting.DataSetting[j].AddrLock != true))
					{
						string tmp_variant = MainViewSetting.DataSetting[j].Variant.ToString();

						MapInfo.Factor result = MapList.Factor.Find(item => item.VariableName == tmp_variant);

						if (result != null)
						{
							if ((int.Parse(result.Size) >= 1) &&
								 (int.Parse(result.Size) <= 4))
							{
								MainViewSetting.DataSetting[j].Size = result.Size;

							}

							MainViewSetting.DataSetting[j].Address = result.Address;

						}
						else
						{
							MainViewSetting.DataSetting[j].Check = false;
							MainViewSetting.DataSetting[j].Address = null;

						}

					}

				}

			}

		}


		private void mainTimer_Tick(object sender, EventArgs e)
		{
			while( CommProtocol.pbCommLog.Count != 0)
			{
				reloadcommLogtextBox(CommProtocol.pbCommLog.Dequeue());

			}

			bool flg = CommResource_CheckState();

			if( flg == false )
			{
				dispRxDStatusLabel.BackColor = Color.FromKnownColor(KnownColor.Control);
				dispTxDStatusLabel.BackColor = Color.FromKnownColor(KnownColor.Control);

				return;

			}


			List<byte> rxFrameData = CommProtocol.mainControl();
			
			if ( rxFrameData != null )
			{
				dispRxDStatusLabel.BackColor = Color.Red;

				List<string> listSize = new List<string>();
				int maxIndex = CheckedCellData.Length;

				for (int i = 0; i < maxIndex; i++)
				{
					listSize.Add(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value.ToString());

				}

				rxFrameData.RemoveRange(0, 1);	//remove opcode

				bool validflg;
				List<string> rxData = CommProtocol.interpretRxFrameToHexChars(rxFrameData, listSize, out validflg);

				if(validflg == true)
				{
					for (int i = 0; i < maxIndex; i++)
					{
						string retText = rxData[i];

						CheckedCellData[i].Cells[(int)DgvRowName.ReadText].Value = retText;

						string type = CheckedCellData[i].Cells[(int)DgvRowName.Type].Value.ToString();

						string retValue = TypeConvert.FromHexChars(type, int.Parse(listSize[i]), retText);

						CheckedCellData[i].Cells[(int)DgvRowName.ReadValue].Value = retValue;

					}
				}

			}
			else
			{
				dispRxDStatusLabel.BackColor = Color.FromKnownColor(KnownColor.Control);
				
			}

			List<byte> txBuff = CommProtocol.getTxData();

			if (txBuff != null)
			{
				dispTxDStatusLabel.BackColor = Color.Orange;

				bool ret = false;
				
				if( pbCommunicationMode == CommMode.Serial )
				{
					ret = serialPort1_DataSend(txBuff);
					
				}
				else if( pbCommunicationMode == CommMode.NetWork )
				{
					ret = sockets_DataSend(txBuff);
					
				}

				CommProtocol.setTxCondtion( ret );

			}
			else
			{
				dispTxDStatusLabel.BackColor = Color.FromKnownColor(KnownColor.Control);
				
			}

			dutVerViewControl.TextBox = CommProtocol.pbDutVersion;

			//dump Data
			if(CommProtocol.pbDumpData != null)
			{
				string tmp = DumpFormInstance.dumpTextBox.Text;
				
				if( tmp != "" )
				{
					tmp += "-";
					
				}
				
				tmp += CommProtocol.pbDumpData;
				DumpFormInstance.dumpTextBox.Text = tmp;
				CommProtocol.pbDumpData = null;

			}

		}


		private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if ( CommActiveFlg == false)
			{
				return;

			}

			if (serialPort1.IsOpen == false)
			{
				return;

			}

			try
			{
				int size = serialPort1.BytesToRead;

				byte[] rcvbuff = new byte[size];

				serialPort1.Read(rcvbuff, 0, size);

				List<byte> rcvlist = rcvbuff.ToList();

				CommProtocol.decode(rcvlist);

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);

			}

		}


		private bool serialPort1_DataSend( List<byte> frame )
		{
			bool flg = false;
			
			List<byte> txBuff = new List<byte>(frame);
			

			if (serialPort1.IsOpen == false)
			{

			}
			else
			{
				try
				{
					byte[] tmp = CommProtocol.encode(txBuff);

					int count = Convert.ToSByte(tmp	.Length);

					serialPort1.Write(tmp, 0, count);

					flg = true;
					
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);

				}
				
			}

			return flg;
		}


		private bool serialPort1_SelectState(bool req_flg)
		{
			bool ret = false;

			if(req_flg == true)
			{
				if (serialPort1.IsOpen == false)
				{
					try
					{
						serialPort1.Open();

						ret = true;

					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);

					}

					this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);

				}


			}
			else
			{
				if (serialPort1.IsOpen == true)
				{
					serialPort1.Close();

					ret = true;

					this.serialPort1.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);

				}

			}

			return ret;
		}


		private bool serialPort1_CheckState()
		{
			bool ret = false;

			if (serialPort1.IsOpen == true)
			{
				ret = true;

			}

			return ret;
		}


		private void sockets_DataReceived(IAsyncResult ar)
		{
			if ( CommActiveFlg == false)
			{
				return;

			}

			SocketsAsyncParam ap = (SocketsAsyncParam)ar.AsyncState;

			System.Net.Sockets.NetworkStream stream = ap.Client.GetStream();

			int size = stream.EndRead(ar);

			List<byte> rcvlist = new List<byte>();

			for (int i = 0; i < size; i++ )
			{
				rcvlist.Add(ap.ReadBuff[i]);

			}

			CommProtocol.decode(rcvlist);

			stream.BeginRead(ap.ReadBuff, 0, ap.ReadBuff.Length, new AsyncCallback(sockets_DataReceived), ap);

		}


		private bool sockets_DataSend( List<byte> frame )
		{
			bool flg = false;
			
			System.Net.Sockets.NetworkStream stream = SocketsParam.Client.GetStream();

			List<byte> txBuff = new List<byte>(frame);

			byte[] tmp = CommProtocol.encode(txBuff);

			stream.Write(tmp, 0, tmp.Length);

			flg = true;

			return flg;
		}


		private bool sockets_SelectState(bool req_flg)
		{
			bool ret = false;

			if(req_flg == true)
			{
				try
				{
					SocketsParam.Client = new System.Net.Sockets.TcpClient();
					SocketsParam.ReadBuff = new byte[16];
					SocketsParam.Client.Connect(pbLocalIP, pbPortNum);

					System.Net.Sockets.NetworkStream stream = SocketsParam.Client.GetStream();

					stream.BeginRead(SocketsParam.ReadBuff, 0, SocketsParam.ReadBuff.Length, new AsyncCallback(sockets_DataReceived), SocketsParam);

					stream = SocketsParam.Client.GetStream();
					ret = true;

				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);

				}

			}
			else
			{
				if (SocketsParam.Client != null)
				{
					SocketsParam.Client.Close();

				}

			}

			return ret;
		}


		private bool sockets_CheckState()
		{
			bool ret = false;
			
			if( ( pbCommunicationMode == CommMode.NetWork ) &&
				( CommActiveFlg == true ) &&
				( pbLocalIP != null ) &&
				( pbPortNum != 0 ) )
			{
				ret = true;

			}

			return ret;
		}


		public bool CommResource_CheckState()
		{
			bool ret = false;
			
			switch( pbCommunicationMode )
			{
				case CommMode.NotDefine:
				
				break;
				
				case CommMode.Serial:
					ret = serialPort1_CheckState();
				
				break;
				
				case CommMode.NetWork:
					ret = sockets_CheckState();
				
				break;
				
			}
			
			return ret;
		}


		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;

			if ( (e.ColumnIndex >= 0) &&
				(e.RowIndex >= 0) )
			{
				if (dgv.Columns[e.ColumnIndex].Name == DgvRowName.WrTrg.ToString())
				{
					if ((string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string) == true) ||
						(string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value as string) == true) ||
						(string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value as string) == true))
					{
						MessageBox.Show("Size, Type or WriteValue might be empty.",
											"Caution",
											MessageBoxButtons.OK,
											MessageBoxIcon.Warning);
						return;
						
					}
					
					string size = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();
					string type = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value.ToString();
					string writeValue = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value.ToString();
					
					Exception ex_text = null;

					string writeText = TypeConvert.ToHexChars(type, int.Parse(size), writeValue, out ex_text);

					if (ex_text != null)
					{
						MessageBox.Show(ex_text.Message);

					}
					else if (writeText == null)
					{
						MessageBox.Show("Write data is invalid.",
											"Caution",
											MessageBoxButtons.OK,
											MessageBoxIcon.Warning);

					}
					else
					{
						dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value = writeText;

						bool flg = CommResource_CheckState();

						if (flg == false)
						{

						}
						else if ((string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value as string) == true) ||
								(string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value as string) == true))
						{
							MessageBox.Show("Address or offset data might be empty.",
												"Caution",
												MessageBoxButtons.OK,
												MessageBoxIcon.Warning);
						}
						else
						{
							string address = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value.ToString();
							string offset = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value.ToString();

							writeData(size, address, offset, writeText);
							
						}
						
					}

				}
				else if (dgv.Columns[e.ColumnIndex].Name == DgvRowName.Type.ToString())
				{
					string type = null;

					if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value != null)
					{
						type = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value.ToString();

					}
					else
					{
						type = numeralSystem.BIN;

					}

					string readText = null;

					if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value != null)
					{
						readText = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value.ToString();

					}

					string writeText = null;

					if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value != null)
					{
						writeText = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value.ToString();

					}

					string size = null;

					if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value != null)
					{
						size = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();

					}
					else
					{
						size = "1";

					}

					switch (type)
					{
						case numeralSystem.BIN:
							type = numeralSystem.UDEC;
							break;

						case numeralSystem.UDEC:
							type = numeralSystem.DEC;
							break;

						case numeralSystem.DEC:
							type = numeralSystem.HEX;
							break;

						case numeralSystem.HEX:
							if( size != "4" )
							{
								type = numeralSystem.BIN;
							}
							else
							{
								type = numeralSystem.FLT;
							}
							break;

						case numeralSystem.FLT:
							type = numeralSystem.BIN;
							break;

						default:
							type = numeralSystem.HEX;
							break;

					}

					string readVal = TypeConvert.FromHexChars(type, int.Parse(size), readText);
					string writeVal = TypeConvert.FromHexChars(type, int.Parse(size), writeText);

					dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value = type;

					if (readVal != null)
					{
						dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadValue].Value = readVal;

					}

					if (writeVal != null)
					{
						dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value = writeVal;

					}

				}
				else
				{

				}

			}

		}


		private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;

			if ((e.ColumnIndex >= 0) &&
				(e.RowIndex >= 0))
			{
				if (dgv.Columns[e.ColumnIndex].Name == DgvRowName.Variant.ToString())
				{
					if ( ( MapList != null ) &&
						(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Variant].Value != null))
					{
						String tmp_variant = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Variant].Value.ToString();

						MapInfo.Factor result = MapList.Factor.Find(item => item.VariableName == tmp_variant);

						if (result != null)
						{
							if ( (int.Parse(result.Size) >= 1) &&
								 (int.Parse(result.Size) <= 4) )
							{
								dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value = result.Size;

							}

							dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value = result.Address;

						}

					}

				}
				else if (dgv.Columns[e.ColumnIndex].Name == DgvRowName.Check.ToString())
				{
					int cnt = 0;
					int total_size = 0;

					foreach (DataGridViewRow item in dataGridView1.Rows)
					{
						//if checkbox is checked
						if (Convert.ToBoolean((item.Cells[(int)DgvRowName.Check] as DataGridViewCheckBoxCell).FormattedValue))
						{
							if ((string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Size].Value as string) == true) ||
								(string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Address].Value as string) == true) ||
								(string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Offset].Value as string) == true) ||
								(string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Type].Value as string) == true))
							{
								dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = false;

							}
							else
							{
								int size = int.Parse(item.Cells[(int)DgvRowName.Size].Value.ToString());
								total_size += size;
								cnt++;

							}


						}

					}

					if (total_size > MAX_TOTAL_SIZE)
					{
						dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = false;
					}

					if (cnt >= SELECT_NUM)
					{
						dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = false;
					}


				}
			}
		}


		private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
		{
			if( ( e.KeyCode == Keys.Delete ) &&
				( CustomizingModeFlg == true ) )
			{
				foreach (DataGridViewCell c in dataGridView1.SelectedCells)
				{
					dataGridView1[c.ColumnIndex, c.RowIndex].Value = null;
				}

			}

		}


		private void opclCommButton_Click(object sender, EventArgs e)
		{
			const string commopen =		"Comm Open ";
			const string commclose =	"Comm Close";

			if (CustomizingModeFlg == true)
			{
				MessageBox.Show("Quit custmizing mode.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);

			}
			else
			{
				if (CommActiveFlg == false)
				{
					CommProtocol.startStopWatch();

					bool reload_flg = checkTimeStampAboutMapFile();

					if (reload_flg == true)
					{
						reviseAddressAndSizeFromMapFile();
						this.dataGridView1.Refresh();

					}

					bool ret = false;
					
					if( pbCommunicationMode == CommMode.Serial )
					{
						ret = serialPort1_SelectState(true);
						
					}
					else if( pbCommunicationMode == CommMode.NetWork )
					{
						ret = sockets_SelectState(true);
						
					}

					if (ret == true)
					{
						CommProtocol.initial();

						CommActiveFlg = true;

						opclCommButton.Image = Properties.Resources.FlagThread_red;
						opclCommButton.Text = commopen;

						CommProtocol.readVersion(pbPassword);

						renewLogSetting();

						CommProtocol.setLogModeStart();

					}
				}
				else
				{
					CommProtocol.stopStopWatch();

					bool ret = false;

					CommActiveFlg = false;

					opclCommButton.Image = Properties.Resources.FlagThread_white;
					opclCommButton.Text = commclose;

					if (pbCommunicationMode == CommMode.Serial)
					{
						ret = serialPort1_SelectState(false);
						
					}
					else if( pbCommunicationMode == CommMode.NetWork )
					{
						ret = sockets_SelectState(false);
						
					}

					//Revise Timing
					timingValTextBox.Text = "500";

					if (ret == true)
					{
						CommProtocol.clear();

					}

					if (CommProtocol.pbLogFlg == true)
					{
						boolDataLogButton.PerformClick();
						
					}


				}

			}

		}


		private void pageValComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (MainViewSetting != null)
			{
				int index = int.Parse(pageValComboBox.SelectedItem.ToString());

				bool flg = ViewSettingEntity.ReloadDataSource(MainViewSetting.DataSetting, WorkingDataSourceForDGV, index, COLUMN_NUM);

				if (flg == true)
				{
					this.dataGridView1.DataSource = WorkingDataSourceForDGV;

					for (int i = 0; i < dataGridView1.Rows.Count; i++)
					{
						dataGridView1.Rows[i].HeaderCell.Value = i.ToString();
					}

					dataGridView1.RowHeadersWidth = 48;

					renewLogSetting();

					CommProtocol.setLogModeStart();

					deleteSpecifiedCells();

					this.dataGridView1.Refresh();

				}

			}
		}


		private void timingValTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Enter)
			{
				string timingNum = timingValTextBox.Text.ToString();
				renewTimingSetting(timingNum);

			}

		}


		private void dispVariantsToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void settingCommToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CommActiveFlg == true)
			{
				MessageBox.Show("Stop communication.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);

			}
			else
			{
				CommSettingForm frm = new CommSettingForm(this);
				frm.Show();

			}

		}

		private void settingViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CommActiveFlg == true)
			{
				MessageBox.Show("Stop communication.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);

			}
			else
			{
				if (CustomizingModeFlg == false)
				{
					if (dataGridView1.Columns[(int)DgvRowName.Size].Visible == false)
					{
						visibleCtrl(true);

					}

					editingCtrl(false);

					bool reload_flg = checkTimeStampAboutMapFile();

					if (reload_flg == true)
					{
						reviseAddressAndSizeFromMapFile();

						this.dataGridView1.Refresh();

					}

					CustomizingModeFlg = true;

					dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Orange;

				}
				else
				{
					CustomizingModeFlg = false;

					editingCtrl(true);

					dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.LightBlue;

				}

			}

		}

		private void changeViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dataGridView1.Columns[(int)DgvRowName.Size].Visible == false)
			{
				visibleCtrl(true);

			}
			else
			{
				visibleCtrl(false);

			}

		}

		private void openViewFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CommActiveFlg == true)
			{
				MessageBox.Show("Stop communication.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);

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

					try
					{
						StreamReader reader = new StreamReader(path);
						MainViewSetting = (ViewSetting)serializer.Deserialize(reader);
						reader.Close();

						if (MainViewSetting != null)
						{
							// Important.  pageValComboBox_SelectedIndexChanged will occur, if call "pageValComboBox.Items.Clear()".
							pageValComboBox.Items.Clear();

							for (int i = 0; i < (MainViewSetting.DataSetting.Count / COLUMN_NUM); i++)
							{
								pageValComboBox.Items.Add(i.ToString());

							}

							pageValComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

							// Important.  pageValComboBox_SelectedIndexChanged will occur, if pageValComboBox.SelectedIndex is changed.
							pageValComboBox.SelectedIndex = 0;

							string patnName = Path.GetFileName(path);
							int firstCharacter = patnName.IndexOf("_TgV");
							int secondCharacter = patnName.IndexOf("_StV");

							if ((firstCharacter > 0) &&
								(secondCharacter > 0) &&
								(secondCharacter > firstCharacter))
							{
								string text;
								int sindex;
								int length;

								sindex = firstCharacter + 4;
								length = secondCharacter - sindex;
								text = patnName.Substring(sindex, length);
								targetVerViewControl.TextBox = text;

								sindex = secondCharacter + 4;
								length = patnName.Length - sindex - 4;	// remove ".xml"
								text = patnName.Substring(sindex, length);
								settingVerViewControl.TextBox = text;

							}

							if (MapList != null)
							{
								MessageBox.Show("Purge map file info",
													"Caution",
													MessageBoxButtons.OK,
													MessageBoxIcon.Warning);

								MapList = new MapInfo.List();
								ValidMapLastWrittenDate = DateTime.Today;

							}


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

				ofd.Dispose();

			}

		}

		private void openMapFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (CommActiveFlg == true)
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
					MapList = new MapInfo.List();
					MapList.Factor = new List<MapInfo.Factor>();

					string path = ofd.FileName;

					DateTime date;

					if (File.Exists(path))
					{
						date = File.GetLastWriteTime(path);

					}
					else
					{
						date = DateTime.Now;

					}

					bool ret = false;

					if (ret == false)
					{
						ret = ReadElfMap.Interpret(path, MapList);

					}

					if (ret == false)
					{
						ret = IarMap.Interpret(path, MapList);

					}

					if (ret == false)
					{
						ret = KeilMap.Interpret(path, MapList);

					}

					if (ret == false)
					{
						ret = RmAddressMap.Interpret(path, MapList);

					}

					if (ret == true)
					{
						ValidMapPath = path;
						ValidMapLastWrittenDate = date;

						reviseAddressAndSizeFromMapFile();

						this.dataGridView1.Refresh();

					}
					else
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

			sfd.Title = "Save File";
			//sfd.InitialDirectory = @"D:\";
			sfd.FileName = "test.xml";
			sfd.Filter =
				"ViewSetting File(*.xml)|*.xml|All Files(*.*)|*.*";
			sfd.FilterIndex = 1;
			sfd.RestoreDirectory = true;
			sfd.ShowHelp = true;
			sfd.CreatePrompt = true;

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				XmlSerializer serializer = new XmlSerializer(typeof(ViewSetting));

				FileStream fs = new FileStream(sfd.FileName, FileMode.Create);

				if (MainViewSetting.DataSetting.Count == 0)
				{
					ViewSettingEntity.InitialDataSource(MainViewSetting.DataSetting, COLUMN_NUM);
					ViewSettingEntity.ReloadDataSource(WorkingDataSourceForDGV, MainViewSetting.DataSetting, 0, COLUMN_NUM);

				}

				serializer.Serialize(fs, MainViewSetting);

				fs.Close();

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
			if (CommActiveFlg == true)
			{
				MessageBox.Show("Stop communication.",
									"Caution",
									MessageBoxButtons.OK,
									MessageBoxIcon.Warning);

			}
			else
			{
				OptionForm frm = new OptionForm(this);
				frm.Show();

			}

		}

		private void boolDataLogButton_Click(object sender, EventArgs e)
		{
			const string logging = "Logging";
			const string stoplog = "Stop Log";

			if (CommProtocol.pbLogFlg == true)
			{
				boolDataLogButton.Image = Properties.Resources.Complete_and_ok_gray;
				boolDataLogButton.Text = stoplog;

				CommProtocol.pbLogFlg = false;

				if( CheckedCellData != null )
				{
					// add Variant Name as log.csv header
					string header = "";
					
					foreach( var name in CheckedCellData )
					{
						header = header + "," + name.Cells[(int)DgvRowName.Name].Value;
					}

					saveLogData(header, CommProtocol.pbCommLogBuffer);
					
				}


			}
			else
			{
				boolDataLogButton.Image = Properties.Resources.Complete_and_ok_green;
				boolDataLogButton.Text = logging;

				CommProtocol.pbLogFlg = true;

			}


		}

		private void dumpEntryButton_Click(object sender, EventArgs e)
		{
			DumpFormInstance = new DumpForm(this);
			DumpFormInstance.Show();

		}
	}

}
