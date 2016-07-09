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
		private enum FixedColumns : int		// Column name of datagridview
		{
			Size = 0,
			Type,
			DataStart
		}

		public const string ROW_TERMINATOR = "-";

		private SubViewControl SubViewCtrl;

		public DumpForm(SubViewControl tmp)
		{
			SubViewCtrl = tmp;
			InitializeComponent();

			dataGridView1.Rows.Add();

			// Redefined Column Name
			dataGridView1.Columns[0].Name = FixedColumns.Size.ToString();
			dataGridView1.Columns[1].Name = FixedColumns.Type.ToString();

			DataTable typeTable = new DataTable("typeTable");
			typeTable.Columns.Add("Display", typeof(string));
			typeTable.Rows.Add(numeralSystem.HEX);
			typeTable.Rows.Add(numeralSystem.UDEC);
			typeTable.Rows.Add(numeralSystem.DEC);
			typeTable.Rows.Add(numeralSystem.BIN);
			typeTable.Rows.Add(numeralSystem.FLT);
			typeTable.Rows.Add(numeralSystem.ASCII);

			(dataGridView1.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).ValueType = typeof(string);
			(dataGridView1.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).ValueMember = "Display";
			(dataGridView1.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).DisplayMember = "Display";
			(dataGridView1.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).DataSource = typeTable;

			dataGridView1.Rows[0].Cells[(int)FixedColumns.Size].Value = "1";
			dataGridView1.Rows[0].Cells[(int)FixedColumns.Type].Value = numeralSystem.HEX;


		}

		private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView dgv = (DataGridView)sender;

			if ((e.ColumnIndex >= 0) &&
				(e.RowIndex >= 0))
			{
				if (dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Type].Value == null)
				{
					dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Type].Value = numeralSystem.HEX;

				}

				if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value as string) == false)
				{
					bool validFlg = false;
					int num = 1;
					string str = dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value.ToString();

					if (TypeConvert.IsNumeric(str) == true)
					{
						num = int.Parse(str);
						validFlg = true;
					}
					
					if( validFlg == false )
					{
						dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].ErrorText = "Invalid Value.";

					}
					else
					{
						dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].ErrorText = null;

					}

				}

			}

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

			if (SubViewCtrl.myComponents.CommActiveFlg == false)
			{
				return;

			}

			dumpTextBox.Text = "";

			SubViewCtrl.myCommProtocol.readDumpData(address, size);

			SubViewCtrl.myCommProtocol.setLogModeStart();
		}


		private void makeButton_Click(object sender, EventArgs e)
		{
			string originalText = dumpTextBox.Text;
			var dividedText = originalText.Split('-').ToList();

			int factorSize = 0;

			List<int> listNumSize = new List<int>();
			List<string> listTextType = new List<string>();

			for (int index = dataGridView1.Columns.Count; index > 2; index--)
			{
				dataGridView1.Columns.RemoveAt((index - 1));

			}

			foreach (DataGridViewRow item in dataGridView1.Rows)
			{
				if (item.Cells[(int)FixedColumns.Size].Value != null)
				{
					int tmpIntSize = int.Parse(item.Cells[(int)FixedColumns.Size].Value.ToString());
					factorSize += tmpIntSize;
					listNumSize.Add(tmpIntSize);
					listTextType.Add(item.Cells[(int)FixedColumns.Type].Value.ToString());

				}

			}

			if(factorSize <= 0)
			{
				return;
			}

			var textSize = dividedText.Count;
			var quotient = textSize / factorSize;
			var remainder = textSize - (factorSize * quotient);

			if ((quotient != 0) &&
				(remainder == 0))
			{
				string[][] arrayTextBuffer = new string[listNumSize.Count][];

				for (int i = 0; i < listNumSize.Count; i++)
				{
					arrayTextBuffer[i] = new string[quotient + (int)FixedColumns.DataStart];
					arrayTextBuffer[i][(int)FixedColumns.Size] = listNumSize[i].ToString();
					arrayTextBuffer[i][(int)FixedColumns.Type] = listTextType[i];

				}

				{
					int j = (int)FixedColumns.DataStart;
					while (dividedText.Count != 0)
					{
						int i = 0;
						foreach (var item in listNumSize)
						{
							string typeData = arrayTextBuffer[i][(int)FixedColumns.Type];

							List<string> listTmp = dividedText.GetRange(0, item);

							string textTmp = "";
							if (typeData != numeralSystem.ASCII)
							{
								if (radioButtonLittleEndian.Checked)
								{
									foreach (var tmp in listTmp.Reverse<string>())
									{
										textTmp += tmp;
									}

								}
								else
								{
									foreach (var tmp in listTmp)
									{
										textTmp += tmp;
									}
								}
							}
							else
							{
								foreach (var tmp in listTmp)
								{
									textTmp += tmp;
								}
							}

							textTmp = TypeConvert.FromHexChars(typeData, item, textTmp);
							arrayTextBuffer[i][j] = textTmp;
							dividedText.RemoveRange(0, item);
							i++;
						}
						j++;
					}

				}

				int number = 0;

				for(int index = (int)FixedColumns.DataStart; index < arrayTextBuffer[0].Count(); index++)
				{
					DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();

					string columnName = "data" + number.ToString();
					number++;

					textColumn.DataPropertyName = columnName;
					textColumn.Name = columnName;
					textColumn.HeaderText = columnName;
					dataGridView1.Columns.Add(textColumn);

				}

				int columnIndex = 0;

				foreach(DataGridViewRow item in dataGridView1.Rows)
				{
					if(columnIndex >= arrayTextBuffer.Count())
					{
						break;
					}

					for(int index =0; index < item.Cells.Count; index++)
					{
						item.Cells[index].Value = arrayTextBuffer[columnIndex][index];
					}

					columnIndex++;

				}

			}

		}

		private void copyToClipBoardButton_Click(object sender, EventArgs e)
		{
			string delimiter = "\t";
			string seriesName = "Series";

			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			string header = null;
			for (int i = 0; i < (dataGridView1.RowCount-1); i++)
			{
				if (string.IsNullOrEmpty(header) == true)
				{
					header = seriesName + i.ToString();

				}
				else
				{
					header = header + delimiter + seriesName + i.ToString();

				}

			}

			sb.AppendLine(header);

			string text = null;
			for (int i = (int)FixedColumns.DataStart; i < dataGridView1.ColumnCount; i++)
			{
				foreach (DataGridViewRow item in dataGridView1.Rows)
				{
					if (string.IsNullOrEmpty(item.Cells[i].Value as string) == true)
					{
						sb.AppendLine(text);
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
							text = text + delimiter + item.Cells[i].Value.ToString();

						}

					}

				}

			}

			Clipboard.SetText(sb.ToString());

		}

		public void PutDumpData(string text)
		{
			string tmp = dumpTextBox.Text;

			if (tmp != "")
			{
				tmp += "-";

			}

			tmp += text;
			dumpTextBox.Text = tmp;

		}

	}
		
}
