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
        private class DumpConfig
        {
            public int Size;
            public UserType Type;
            public List<string> Values;
            public string Name;

            public DumpConfig()
            {
                Size = 1;
                Type = UserType.Hex;
                Values = new List<string>();
                Name = string.Empty;
            }
        }

        private enum FixedColumns : int     // Column name of datagridview
        {
            Size = 0,
            Type,
            Name,
            Copy,
            DataStart
        }

        private System.Windows.Forms.DataGridViewTextBoxColumn sizeColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn typeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameColumn;
        private System.Windows.Forms.DataGridViewButtonColumn copyButtonColumn;

        private List<SymbolFactor> mapList;
        private AutoCompleteStringCollection autoCompleteSourceForSymbol;

        private long hexboxAddress;

        private List<DumpConfig> configList;

        private double incrementalValue;

        public delegate void RequestFunction(BusinessLogic.DataParameter param);
        public RequestFunction RequestFunctionCallback;


        private bool TryParseAddress(string text, out long address)
        {
            address = 0;
            if (string.IsNullOrEmpty(text)) return false;

            if (text.StartsWith("0x"))
                return long.TryParse(text.Substring(2),
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out address);

            return false;
        }

        private bool TryParseSize(string text, out int size)
        {
            size = 0;
            if (string.IsNullOrEmpty(text)) return false;

            if (text.StartsWith("0x"))
                return int.TryParse(text.Substring(2),
                    System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out size);

            return int.TryParse(text, out size);
        }

        public DumpForm(List<SymbolFactor> maplist)
        {
            mapList = maplist;
            InitializeComponent();

            this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.copyButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.dumpDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.sizeColumn,
            this.typeColumn,
            this.nameColumn,
            this.copyButtonColumn});

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
            this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.typeColumn.Width = 48;
            // 
            // nameColumn
            // 
            this.nameColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Name.ToString();
            this.nameColumn.HeaderText = "Name";
            this.nameColumn.Name = FixedColumns.Name.ToString();
            // 
            // copyButtonColumn
            // 
            this.copyButtonColumn.HeaderText = "Copy";
            this.copyButtonColumn.Name = FixedColumns.Copy.ToString();
            this.copyButtonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.copyButtonColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.copyButtonColumn.Width = 42;

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

            hexboxAddress = 0x103;

            mainHexBox.Write(test.ToArray());
            mainHexBox.BaseAddress = hexboxAddress;

            if ((mapList != null) &&
                (mapList.Count > 0))
            {
                symbolTextBox.AutoCompleteMode = AutoCompleteMode.Suggest;
                symbolTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();

                foreach (var factor in mapList)
                {
                    autoCompleteSourceForSymbol.Add(factor.Symbol);

                }

                symbolTextBox.AutoCompleteCustomSource = autoCompleteSourceForSymbol;
            }

            incrementalValue = 1;
            incrementalValueTextBox.Text = incrementalValue.ToString();
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
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;

            if (mapList != null)
            {
                string tmpSymbol = symbolTextBox.Text;

                var result = mapList.Find(item => item.Symbol == tmpSymbol);

                if (result != null)
                {
                    addressTextBox.Text = result.Address;
                    sizeTextBox.Text = result.Size;

                }

            }
        }

        private void addressTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                e.Handled = true;
        }

        private void sizeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                e.Handled = true;
        }

        private void requestButton_Click(object sender, EventArgs e)
        {
            mainHexBox.Clear();

            if (!TryParseAddress(addressTextBox.Text, out long address))
            {
                MessageBox.Show("Invalid address format. (e.g. 0x1A2B3C4D)", "Input Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                addressTextBox.Focus();
                return;
            }

            if (!TryParseSize(sizeTextBox.Text, out int size))
            {
                MessageBox.Show("Invalid size format. (e.g. 256 or 0x100)", "Input Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                sizeTextBox.Focus();
                return;
            }

            // C28x is word-addressed (16-bit minimum unit), but ReadDump uses byte
            // addresses on the wire so that chunk arithmetic on this side stays simple:
            //   next_address = current_address + chunk_size  (no word/byte conversion needed)
            // The MCU recovers the word address with addr >> 1 in copy_block().
            if (tms320c28xEndianRadioButton.Checked)
                address = address * 2;

            hexboxAddress = address;
            var param = new BusinessLogic.DataParameter
            {
                Address = (uint)address,
                Size = (uint)size
            };
            RequestFunctionCallback?.Invoke(param);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (mainHexBox.Data == null || mainHexBox.Data.Length == 0)
                return;

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var address = mainHexBox.BaseAddress.ToString("X8");
            var size = mainHexBox.Data.Length;
            var fileName = $"{timestamp}_addr0x{address}_size{size}.bin";

            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = fileName;
                dialog.Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*";
                dialog.DefaultExt = "bin";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllBytes(dialog.FileName, mainHexBox.Data);
                }
            }
        }

        private void makeButton_Click(object sender, EventArgs e)
        {
            if (mainHexBox.Data == null || mainHexBox.Data.Length == 0)
                return;

            Queue<byte> image = new Queue<byte>();
            for (var index = 0; index < mainHexBox.Data.Length; index++)
                image.Enqueue(mainHexBox.Data[(int)index]);

            for (int index = dumpDataGridView.Columns.Count; index > (int)FixedColumns.DataStart; index--)
                dumpDataGridView.Columns.RemoveAt((index - 1));

            configList = new List<DumpConfig>();

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

                    if(!String.IsNullOrEmpty(item.Cells[(int)FixedColumns.Name].Value as string))
                        config.Name = item.Cells[(int)FixedColumns.Name].Value.ToString();

                    clusterSize += config.Size;

                    configList.Add(config);

                }

            }

            if (isFailed)
                return;

            if (configList.Count <= 0)
                return;

            var quotient = image.Count / clusterSize;

            if (quotient == 0)
            {
                return;
            }

            bool done = false;
            var maxDataColumnSize = quotient;

            while (image.Count != 0 || !done)
            {
                foreach (var config in configList)
                {
                    List<byte> byteList = new List<byte>();

                    if (image.Count < config.Size)
                    {
                        done = true;
                        break;
                    }

                    for (int i = 0; i < config.Size; i++)
                        byteList.Add(image.Dequeue());

                    if (bigEndianRadioButton.Checked)
                        byteList.Reverse();

                    ulong rowData = 0;
                    int digit = 0;
                    foreach (var tmp in byteList)
                    {
                        rowData += (ulong)(tmp * Math.Pow(2, (digit * 8)));
                        digit++;
                    }

                    string value;
                    UserString.TryParse(config.Type, config.Size, rowData, out value);
                    config.Values.Add(value);

                }
            }

            if (maxDataColumnSize > 16)
                maxDataColumnSize = 16;

            for (int index = 0; index < maxDataColumnSize; index++)
            {
                DataGridViewTextBoxColumn textColumn = new DataGridViewTextBoxColumn();
                textColumn.HeaderText = "Preview" + index.ToString();
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

        private void copyToClipBoardButton_Click(object sender, EventArgs e)
        {
            if (configList == null || configList.Count <= 0)
                return;

            int rowCount = configList[0].Values.Count;
            var sb = new System.Text.StringBuilder();

            var header = new List<string> { "" };
            foreach (var config in configList)
            {
                string name = "Series" + configList.IndexOf(config);
                if (!String.IsNullOrEmpty(config.Name))
                    name = config.Name;
                header.Add(name);
            }
            sb.AppendLine(string.Join("\t", header));

            double xValue = 0;
            for (int i = 0; i < rowCount; i++)
            {
                var row = new List<string> { xValue.ToString() };
                foreach (var config in configList)
                    row.Add(config.Values[i].ToString());
                sb.AppendLine(string.Join("\t", row));
                xValue += incrementalValue;
            }

            Clipboard.SetText(sb.ToString());
        }

        public void UploadHexbox(List<byte> bytes)
        {
            mainHexBox.BaseAddress = hexboxAddress;

            mainHexBox.Write(bytes.ToArray());

        }

        private void incrementalValueTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
                return;

            e.Handled = true;

            double value;
            if (!double.TryParse(incrementalValueTextBox.Text, out value))
            {
                incrementalValueTextBox.Text = incrementalValue.ToString();
                return;
            }

            if (value == 0)
            {
                incrementalValueTextBox.Text = incrementalValue.ToString();
                return;
            }

            incrementalValue = value;

        }

        private void incrementalValueTextBox_Leave(object sender, EventArgs e)
        {
            incrementalValueTextBox.Text = incrementalValue.ToString();
        }

        private void dumpDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int copyColumnIndex = dgv.Columns[(int)FixedColumns.Copy].Index;

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;
            }

            if (e.RowIndex >= configList.Count)
            {
                return;
            }

            var index = e.RowIndex;

            if (e.ColumnIndex == copyColumnIndex)
            {
                var sb = new StringBuilder();

                foreach (var item in configList[index].Values)
                    sb.AppendLine(item);

                Clipboard.SetText(sb.ToString());

            }
        }
    }

}
