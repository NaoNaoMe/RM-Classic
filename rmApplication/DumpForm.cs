using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rmApplication
{
	public partial class DumpForm : Form
	{
		private enum DgvRowName : int		// Column name of datagridview
		{
			Size = 0,
			Type
		}

		private SubViewControl SubViewCtrl;

		public DumpForm(SubViewControl tmp)
		{
			SubViewCtrl = tmp;
			InitializeComponent();

			dataGridView1.Rows.Add();

			// Redefined Column Name
			dataGridView1.Columns[0].Name = DgvRowName.Size.ToString();
			dataGridView1.Columns[1].Name = DgvRowName.Type.ToString();

			dataGridView1.Rows[0].Cells[(int)DgvRowName.Size].Value = "1";
			dataGridView1.Rows[0].Cells[(int)DgvRowName.Type].Value = numeralSystem.HEX;


		}


		private void variableTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				if (SubViewCtrl.myComponents.MapList != null)
				{
					string tmpVariable = variableTextBox.Text;

					MapFactor result = SubViewCtrl.myComponents.MapList.Find(item => item.VariableName == tmpVariable);

					if (result != null)
					{
						addressTextBox.Text = result.Address;
						sizeTextBox.Text = result.Size;

					}

				}

			}
		}


		private void requestButton_Click(object sender, EventArgs e)
		{
			string address = addressTextBox.Text;
			string size = sizeTextBox.Text;

			bool flg = SubViewCtrl.commResource_CheckState();

			if (flg == false)
			{
				return;

			}

			dumpTextBox.Text = "";

			SubViewCtrl.myCommProtocol.readDumpData(address, size);

			SubViewCtrl.myCommProtocol.setLogModeStart();
		}


		private void makeButton_Click(object sender, EventArgs e)
		{
			string text = dumpTextBox.Text;

			var max_size = dataGridView1.Columns.Count;

			for (int index = max_size; index > 2; index--)
			{
				dataGridView1.Columns.RemoveAt((index - 1));

			}

			List<string> stArrayData = text.Split('-').ToList();

			var row_size = (dataGridView1.Rows.Count - 1);
			var data_size = stArrayData.Count;

			int total_size = 0;

			foreach (DataGridViewRow item in dataGridView1.Rows)
			{
				if (item.Cells[(int)DgvRowName.Size].Value != null)
				{
					var size = int.Parse(item.Cells[(int)DgvRowName.Size].Value.ToString());
					total_size += size;

				}

			}

			var quotient = data_size / total_size;
			var remainder = data_size - (total_size * quotient);

			if ((quotient != 0) &&
				(remainder == 0))
			{
				int add_num = 0;

				while (quotient != 0)
				{
					DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();

					string column_name = "data";

					column_name = column_name + add_num.ToString();

					add_num++;

					textColumn.DataPropertyName = column_name;
					textColumn.Name = column_name;
					textColumn.HeaderText = column_name;
					dataGridView1.Columns.Add(textColumn);

					foreach (DataGridViewRow item in dataGridView1.Rows)
					{
						if (item.Cells[(int)DgvRowName.Size].Value != null)
						{
							List<string> buff;
							string tmp;
							var size = int.Parse(item.Cells[(int)DgvRowName.Size].Value.ToString());
							var type = item.Cells[(int)DgvRowName.Type].Value.ToString();

							if (type == numeralSystem.ASCII)
							{
								buff = stArrayData.GetRange(0, size);
								stArrayData.RemoveRange(0, size);

								string tmpBinaryText = "";
								foreach(var tmpBinChar in buff)
								{
									tmpBinaryText += tmpBinChar;
								}

								item.Cells[column_name].Value = TypeConvert.FromHexChars(type, size, tmpBinaryText);

							}
							else
							{
								switch (size)
								{
									case 1:
										buff = stArrayData.GetRange(0, size);
										stArrayData.RemoveRange(0, size);

										tmp = buff[0];

										item.Cells[column_name].Value = TypeConvert.FromHexChars(type, size, tmp);

										break;

									case 2:
										buff = stArrayData.GetRange(0, size);
										stArrayData.RemoveRange(0, size);

										if (radioButtonBigEndian.Checked)
										{
											tmp = buff[0] + buff[1];

										}
										else
										{
										tmp = buff[1] + buff[0];

										}

										item.Cells[column_name].Value = TypeConvert.FromHexChars(type, size, tmp);

										break;

									case 4:
										buff = stArrayData.GetRange(0, size);
										stArrayData.RemoveRange(0, size);

										if (radioButtonBigEndian.Checked)
										{
											tmp = buff[0] + buff[1] + buff[2] + buff[3];

										}
										else
										{
										tmp = buff[3] + buff[2] + buff[1] + buff[0];

										}

										item.Cells[column_name].Value = TypeConvert.FromHexChars(type, size, tmp);

										break;

									default:
										break;

								}


							}

						}

					}

					quotient--;

				}

			}

		}


		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;

			if ((e.ColumnIndex >= 0) &&
				(e.RowIndex >= 0))
			{
				if (dgv.Columns[e.ColumnIndex].Name == DgvRowName.Type.ToString())
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
							if (size != "4")
							{
								type = numeralSystem.ASCII;
							}
							else
							{
								type = numeralSystem.FLT;
							}
							break;

						case numeralSystem.FLT:
							type = numeralSystem.ASCII;
							break;

						case numeralSystem.ASCII:
							type = numeralSystem.BIN;
							break;

						default:
							type = numeralSystem.HEX;
							break;

					}

					dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value = size;
					dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value = type;

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
				if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value == null)
				{
					dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value = numeralSystem.HEX;

				}

				if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string) == false)
				{
					int num = 1;
					string str = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();

					if (TypeConvert.IsNumeric(str) == true)
					{
						num = int.Parse(str);
#if false
						if ((num != 1) &&
							(num != 2) &&
							(num != 4))
						{
							num = 1;

						}
#endif
					}
					
					dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value = num;

				}

			}

		}


		private void copyToClipBoardButton_Click(object sender, EventArgs e)
		{
			string header = null;
			string seriesName = "Series";
			string text = null;

			var rowCnt = dataGridView1.RowCount;

			for (int i = 0; i < (rowCnt-1); i++)
			{
				if (string.IsNullOrEmpty(header) == true)
				{
					header = seriesName + i.ToString();

				}
				else
				{
					header = header + "\t" + seriesName + i.ToString();

				}

			}

			header += "\r\n";

			var columnCnt = dataGridView1.ColumnCount;

			List<string> strings = new List<string>();

			for (int i = 2; i < columnCnt; i++)
			{
				foreach (DataGridViewRow item in dataGridView1.Rows)
				{
					if (string.IsNullOrEmpty(item.Cells[i].Value as string) == true)
					{
						text += "\r\n";
						strings.Add(text);
						text = null;
					}
					else
					{
						if (string.IsNullOrEmpty(text) == true)
						{
							text = item.Cells[i].Value.ToString();

						}
						else
						{
							text = text + "\t" + item.Cells[i].Value.ToString();

						}

					}

				}

			}

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.Append(header);

			foreach (var tmp in strings)
			{
				sb.Append(tmp);

			}

			Clipboard.SetText(sb.ToString());

		}



	}
}
