using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Be.Windows.Forms;

namespace rmApplication
{
    public partial class DumpForm : Form
    {
        private class DumpConfig
        {
            public int Size;
            public UserType Type;
            public List<string> Values;

            public DumpConfig()
            {
                Size = 1;
                Type = UserType.Hex;
                Values = new List<string>();
            }
        }

        private enum FixedColumns : int     // Column name of datagridview
        {
            Size = 0,
            Type,
            DataStart
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn typeColumn;

        private SubViewControl subViewCtrl;
        private AutoCompleteStringCollection autoCompleteSourceForSymbol;

        private int hexboxAddress;
        private int hexboxOffsetAddress;

        public DumpForm(SubViewControl tmp)
        {
            subViewCtrl = tmp;
            InitializeComponent();

            this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();

            this.dumpDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sizeColumn,
            this.typeColumn});

            // 
            // sizeColumn
            // 
            //this.sizeColumn.DataPropertyName = "Size";
            this.sizeColumn.HeaderText = "Size";
            this.sizeColumn.Name = FixedColumns.Size.ToString();
            this.sizeColumn.Width = 42;
            // 
            // typeColumn
            // 
            //this.typeColumn.DataPropertyName = "Type";
            this.typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.typeColumn.HeaderText = "Type";
            this.typeColumn.Name = FixedColumns.Type.ToString();
            this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.typeColumn.Width = 48;

            dumpDataGridView.Rows.Add();

            System.Data.DataTable typeTable = new System.Data.DataTable("typeTable");
            typeTable.Columns.Add("Display", typeof(string));
            typeTable.Rows.Add(UserType.Hex);
            typeTable.Rows.Add(UserType.UsD);
            typeTable.Rows.Add(UserType.Dec);
            typeTable.Rows.Add(UserType.FLT);
            typeTable.Rows.Add(UserType.DBL);

            (dumpDataGridView.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).ValueType = typeof(string);
            (dumpDataGridView.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).ValueMember = "Display";
            (dumpDataGridView.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).DisplayMember = "Display";
            (dumpDataGridView.Columns[(int)FixedColumns.Type] as DataGridViewComboBoxColumn).DataSource = typeTable;

            dumpDataGridView.Rows[0].Cells[(int)FixedColumns.Size].Value = "1";
            dumpDataGridView.Rows[0].Cells[(int)FixedColumns.Type].Value = UserType.Hex.ToString();

            var test = new List<byte>();
            test.Add(0xaa);
            test.Add(0x08);
            test.Add(0x00);
            test.Add(0xbb);

            hexboxAddress = 0;
            hexboxOffsetAddress = 0;

            mainHexBox.ByteProvider = new DynamicByteProvider(test);
            mainHexBox.LineInfoOffset = hexboxOffsetAddress;

            if ((subViewCtrl.MapList != null) &&
                (subViewCtrl.MapList.Count > 0))
            {
                symbolTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                symbolTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();

                foreach (var factor in subViewCtrl.MapList)
                {
                    autoCompleteSourceForSymbol.Add(factor.Symbol);

                }

                symbolTextBox.AutoCompleteCustomSource = autoCompleteSourceForSymbol;
            }

        }

        private void dumpDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;
            }

            if (dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Type].Value == null)
            {
                dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Type].Value = UserType.Hex.ToString();

            }

            if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value as string) == false)
            {
                int num = 1;
                if (!int.TryParse(dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value.ToString(), out num))
                {
                    dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value = 1;
                }
                else
                {
                    if ((num != 1) &&
                        (num != 2) &&
                        (num != 4) &&
                        (num != 8))
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)FixedColumns.Size].Value = 1;
                    }

                }


            }

        }

        private void dumpDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //https://stackoverflow.com/questions/9581626/show-row-number-in-row-header-of-a-datagridview

            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,

                LineAlignment = StringAlignment.Center
            };
            //get the size of the string
            Size textSize = TextRenderer.MeasureText(rowIdx, this.Font);
            //if header width lower then string width then resize
            if (grid.RowHeadersWidth < textSize.Width + 40)
            {
                grid.RowHeadersWidth = textSize.Width + 40;
            }
            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        private void symbolTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (subViewCtrl.MapList != null)
                {
                    string tmpSymbol = symbolTextBox.Text;

                    var result = subViewCtrl.MapList.Find(item => item.Symbol == tmpSymbol);

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
            string addressText = addressTextBox.Text;
            string sizeText = sizeTextBox.Text;

            int address = 0;
            int size = 0;

            bool isSuccess = false;

            if (subViewCtrl.IsCommunicationActive == false)
                return;

            var empty = new List<byte>();
            mainHexBox.ByteProvider = new DynamicByteProvider(empty);

            if (!string.IsNullOrEmpty(addressText))
            {
                if (addressText.Length >= 2)
                {
                    var header = addressText.Substring(0, 2);

                    if (header == "0x")
                    {
                        addressText = addressText.Remove(0, 2);

                        if (int.TryParse(addressText, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out address))
                        {
                            if (tms320c28xEndianRadioButton.Checked)
                                address = address * 2;

                            hexboxAddress = address;

                            isSuccess = true;
                        }

                    }

                }
            }

            if (!isSuccess)
                return;

            isSuccess = false;

            if (!string.IsNullOrEmpty(sizeText))
            {
                if (int.TryParse(sizeText, out size))
                    isSuccess = true;
            }

            if (!isSuccess)
                return;

            var param = new BusinessLogic.DataParameter();
            param.Address = (uint)address;
            param.Size = (uint)size;
            subViewCtrl.Logic.DumpConfigParameter = param;

            subViewCtrl.Logic.ClearWaitingTasks();
            subViewCtrl.Logic.EnqueueTask(BusinessLogic.CommunicationTasks.Dump);
            subViewCtrl.Logic.EnqueueTask(BusinessLogic.CommunicationTasks.Logging);
            subViewCtrl.Logic.CancelCurrentTask();

            subViewCtrl.RefreshLogData();

        }


        private void makeButton_Click(object sender, EventArgs e)
        {
            if (mainHexBox.ByteProvider == null)
                return;

            if (mainHexBox.ByteProvider.Length == 0)
                return;

            var offsetSize = hexboxAddress - hexboxOffsetAddress;
            Queue<byte> image = new Queue<byte>();
            for (int index = offsetSize; index < mainHexBox.ByteProvider.Length; index++)
                image.Enqueue(mainHexBox.ByteProvider.ReadByte(index));

            for (int index = dumpDataGridView.Columns.Count; index > 2; index--)
                dumpDataGridView.Columns.RemoveAt((index - 1));

            List<DumpConfig> configList = new List<DumpConfig>();

            bool isFailed = false;
            int clusterSize = 0;
            foreach (DataGridViewRow item in dumpDataGridView.Rows)
            {
                if (item.Cells[(int)FixedColumns.Size].Value != null)
                {
                    var config = new DumpConfig();
                    if (!int.TryParse(item.Cells[(int)FixedColumns.Size].Value.ToString(), out config.Size))
                        isFailed = true;

                    if (!Enum.TryParse<UserType>(item.Cells[(int)FixedColumns.Type].Value.ToString(), out config.Type))
                        isFailed = true;

                    clusterSize += config.Size;

                    configList.Add(config);

                }

            }

            if (isFailed)
                return;

            if (configList.Count <= 0)
                return;

            var quotient = image.Count / clusterSize;
            var remainder = image.Count - (clusterSize * quotient);

            if ((quotient == 0) ||
                (remainder != 0))
            {
                return;
            }
            else
            {
                var maxDataColumnSize = quotient;

                while(image.Count != 0)
                {
                    foreach (var config in configList)
                    {
                        List<byte> byteList = new List<byte>();

                        for (int i = 0; i < config.Size; i++)
                            byteList.Add(image.Dequeue());

                        if(tms320c28xEndianRadioButton.Checked)
                        {
                            byte tmp;
                            switch (config.Size)
                            {
                                case 1:
                                    break;
                                case 2:
                                    tmp = byteList[0];
                                    byteList[0] = byteList[1];
                                    byteList[1] = tmp;

                                    break;
                                case 4:
                                    tmp = byteList[0];
                                    byteList[0] = byteList[1];
                                    byteList[1] = tmp;

                                    tmp = byteList[2];
                                    byteList[2] = byteList[3];
                                    byteList[3] = tmp;

                                    break;
                                case 8:
                                    tmp = byteList[0];
                                    byteList[0] = byteList[1];
                                    byteList[1] = tmp;

                                    tmp = byteList[2];
                                    byteList[2] = byteList[3];
                                    byteList[3] = tmp;

                                    tmp = byteList[4];
                                    byteList[4] = byteList[5];
                                    byteList[5] = tmp;

                                    tmp = byteList[6];
                                    byteList[6] = byteList[7];
                                    byteList[7] = tmp;

                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if (bigEndianRadioButton.Checked)
                                byteList.Reverse();

                        }

                        ulong rowData = 0;
                        int digit = 0;
                        foreach(var tmp in byteList)
                        {
                            rowData += (ulong)(tmp * Math.Pow(2, (digit * 8)));
                            digit++;
                        }

                        string value;
                        UserString.TryParse(config.Type, config.Size, rowData, out value);
                        config.Values.Add(value);

                    }
                }

                for (int index = 0; index < maxDataColumnSize; index++)
                {
                    DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                    textColumn.HeaderText = "data" + index.ToString();
                    dumpDataGridView.Columns.Add(textColumn);

                }

                int rowIndex = 0;
                foreach (DataGridViewRow item in dumpDataGridView.Rows)
                {
                    if (rowIndex >= configList.Count)
                        break;

                    for (int columnIndex = 0; columnIndex < maxDataColumnSize; columnIndex++)
                    {
                        item.Cells[columnIndex + (int)FixedColumns.DataStart].Value = configList[rowIndex].Values[columnIndex];
                    }

                    rowIndex++;

                }

            }

        }

        private void copyToClipBoardButton_Click(object sender, EventArgs e)
        {
            string delimiter = "\t";
            string seriesName = "Series";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string header = null;
            for (int i = 0; i < (dumpDataGridView.RowCount - 1); i++)
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
            for (int i = (int)FixedColumns.DataStart; i < dumpDataGridView.ColumnCount; i++)
            {
                foreach (DataGridViewRow item in dumpDataGridView.Rows)
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

        public void UploadHexbox(List<byte> bytes)
        {
            if(enableOffsetCheckBox.Checked)
            {
                var remainder = hexboxAddress % 16;
                hexboxOffsetAddress = hexboxAddress - remainder;

                for (UInt32 i = 0; i < remainder; i++)
                    bytes.Insert(0x00, 0);

            }
            else
            {
                hexboxOffsetAddress = hexboxAddress;

            }

            mainHexBox.LineInfoOffset = hexboxOffsetAddress;

            mainHexBox.ByteProvider = new DynamicByteProvider(bytes);

        }

    }

}
