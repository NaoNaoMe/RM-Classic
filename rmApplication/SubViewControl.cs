using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rmApplication
{
    public partial class SubViewControl : UserControl
    {
        public class Components
        {
            public enum CommMode
            {
                NotDefine,
                Serial,
                NetWork
            }

            public CommMode CommunicationMode { set; get; }
            public string CommPort { set; get; }
            public int CommBaudRate { set; get; }
            public System.Net.IPAddress NetIP { set; get; }
            public int NetPort { set; get; }
            public string Password { set; get; }

            public bool CommActiveFlg { set; get; }
            public bool LoggingActiveFlg { set; get; }
            public bool CustomizingModeFlg { set; get; }
            public string ValidMapPath { set; get; }
            public DateTime ValidMapLastWrittenDate { set; get; }

            public List<MapFactor> MapList { set; get; }
            public List<ViewSetting> ViewSettingList { set; get; }
            public string SettingName { set; get; }
            public string SettingVer { set; get; }
            public string TargetVer { set; get; }
            public int TimingValue { set; get; }

            public Components()
            {
                CommunicationMode = CommMode.NotDefine;
                CommPort = "";
                CommBaudRate = 9600;
                NetIP = System.Net.IPAddress.Parse("192.168.0.255");
                NetPort = 49152;
                Password = "0000FFFF";

                CommActiveFlg = false;
                LoggingActiveFlg = false;
                CustomizingModeFlg = false;
                ValidMapPath = "";
                ValidMapLastWrittenDate = DateTime.MinValue;

                MapList = new List<MapFactor>();
                ViewSettingList = new List<ViewSetting>();
                SettingName = "Sample";
                SettingVer = "001";
                TargetVer = "";
                TimingValue = INITIAL_TIMING_VALUE;

            }

            public Components(Components data)
            {
                CommunicationMode = data.CommunicationMode;
                CommPort = data.CommPort;
                CommBaudRate = data.CommBaudRate;
                NetIP = data.NetIP;
                NetPort = data.NetPort;
                Password = data.Password;

                CommActiveFlg = data.CommActiveFlg;
                LoggingActiveFlg = data.LoggingActiveFlg;
                CustomizingModeFlg = data.CustomizingModeFlg;
                ValidMapPath = data.ValidMapPath;
                ValidMapLastWrittenDate = data.ValidMapLastWrittenDate;

                MapList = data.MapList;
                ViewSettingList = data.ViewSettingList;
                SettingName = data.SettingName;
                SettingVer = data.SettingVer;
                TargetVer = data.TargetVer;
                TimingValue = data.TimingValue;

            }

        }

        public Components myComponents;
        public CommProtocol myCommProtocol;

        private enum DgvRowName : int       // Column name of datagridview
        {
            Group = 0,
            Check,
            Size,
            Variable,
            Addrlock,
            Address,
            Offset,
            Name,
            Type,
            ReadText,       // hidden cell
            ReadValue,
            WriteText,      // hidden cell
            WriteValue,
            WrTrg
        }

        private enum RecordMode
        {
            ClipBoard,
            CSV
        }



        private struct SocketsAsyncParam
        {
            public System.Net.Sockets.TcpClient Client;
            public byte[] ReadBuff;
        }

        private const int MAIN_CTRL_INTERVAL = 100;

        private const int COLUMN_NUM = 32;
        private const int SELECT_NUM = 32;
        private const int MAX_TOTAL_SIZE = 128;

        private const string GROUP_TEMPORARY_TAG = "x:Test";
        private const string TARGET_VER_TAG = "_TgV";
        private const string SETTING_VER_TAG = "_StV";

        private const int INITIAL_TIMING_VALUE = 500;
        private const int MAXIMUM_TIMING_VALUE = 10000;

        private const int COMM_LOG_MAX = 500;
        private const int RCV_LOGDATA_MAX = 10000;

        private const int WARNING_SHOWUP_CNT_MAX = 20;
        private const int CONTINUE_CNT_MAX = 10;

        private DumpForm DumpFormInstance;
        private ContextMenuStrip contextMenuStrip;

        private AutoCompleteStringCollection AutoCompleteSourceForVariable;
        private DataGridViewRow[] CheckedCellData;
        private SocketsAsyncParam SocketsParam;
        private DateTime LogStartTime;
        private List<List<string>> RcvLogData;
        private int ContinueCnt;
        private int NextSlvCnt;
        private int RxSilentCnt;
        private string WarningText;
        private int WarningShowUpCount;
        private bool WarningReminder;

        private string WorkaroundInputBuffer;

        public string getViewSettingFileName()
        {
            if (string.IsNullOrEmpty(myComponents.TargetVer))
            {
                return myComponents.SettingName + SETTING_VER_TAG + myComponents.SettingVer;

            }
            else
            {
                return myComponents.SettingName + SETTING_VER_TAG + myComponents.SettingVer + TARGET_VER_TAG + myComponents.TargetVer;

            }

        }

        public string getViewName(string fileName)
        {
            string retText = null;

            if (fileName == null)
            {

            }
            else
            {
                int firstCharacter = fileName.IndexOf(SETTING_VER_TAG);
                int secondCharacter = fileName.IndexOf(TARGET_VER_TAG);


                if (firstCharacter > 0)
                {
                    string text;
                    int tmpIndex;
                    int length;

                    length = firstCharacter;
                    text = fileName.Substring(0, length);
                    myComponents.SettingName = text;

                    if ((secondCharacter > 0) &&
                        (secondCharacter > firstCharacter))
                    {
                        tmpIndex = firstCharacter + 4;
                        length = secondCharacter - tmpIndex;
                        text = fileName.Substring(tmpIndex, length);
                        myComponents.SettingVer = text;

                        tmpIndex = secondCharacter + 4;
                        length = fileName.Length - tmpIndex;
                        text = fileName.Substring(tmpIndex, length);
                        myComponents.TargetVer = text;

                        targetVerViewControl.TextBox = myComponents.TargetVer;

                    }
                    else
                    {
                        tmpIndex = firstCharacter + 4;
                        length = fileName.Length - tmpIndex;
                        text = fileName.Substring(tmpIndex, length);
                        myComponents.SettingVer = text;

                    }

                    retText = myComponents.SettingName + "(" + myComponents.SettingVer + ")";

                }

            }

            return retText;

        }


        public void loadViewSettingFile(ViewSetting tmp)
        {
            myComponents.ViewSettingList = new List<ViewSetting>();

            var pageList = new List<string>();

            string groupName = null;

            var tmpVSettingFactor = new ViewSetting();

            foreach (var factor in tmp.DataSetting)
            {
                if (groupName == null)
                {
                    groupName = factor.Group;
                    tmpVSettingFactor.DataSetting.Add(factor);

                    if (groupName == null)
                    {
                        groupName = SubViewControl.GROUP_TEMPORARY_TAG;
                        tmpVSettingFactor.DataSetting[0].Group = groupName;

                    }

                    pageList.Add(groupName);

                }
                else if ((factor.Group != null) &&
                          (factor.Group != groupName))
                {
                    groupName = factor.Group;
                    pageList.Add(groupName);

                    myComponents.ViewSettingList.Add(tmpVSettingFactor);

                    tmpVSettingFactor = new ViewSetting();

                    tmpVSettingFactor.DataSetting.Add(factor);

                }
                else
                {
                    tmpVSettingFactor.DataSetting.Add(factor);

                }

            }

            if (tmpVSettingFactor != null)
            {
                myComponents.ViewSettingList.Add(tmpVSettingFactor);

            }

            pageValComboBox.SelectedIndexChanged -= new System.EventHandler(pageValComboBox_SelectedIndexChanged);

            pageValComboBox.Items.Clear();

            foreach (var factor in pageList)
            {
                pageValComboBox.Items.Add(factor);

            }

            pageValComboBox.SelectedIndexChanged += new System.EventHandler(pageValComboBox_SelectedIndexChanged);


            if ((myComponents.MapList != null) &&
                (myComponents.MapList.Count > 0))
            {
                PutWarningMessage("Purge map file info.");

                myComponents.MapList = new List<MapFactor>();
                myComponents.ValidMapPath = null;
                myComponents.ValidMapLastWrittenDate = DateTime.MinValue;
                AutoCompleteSourceForVariable = new AutoCompleteStringCollection();

            }

            pageValComboBox.SelectedIndex = 0;

        }

        public bool loadMapFile(string path)
        {
            myComponents.MapList = new List<MapFactor>();

            bool retFlg = false;
            string[] textArray;

            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(path, Encoding.GetEncoding("utf-8")))
                {
                    string wholeText = sr.ReadToEnd();
                    textArray = wholeText.Replace("\r\n", "\n").Split('\n');

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                return retFlg;

            }

            var date = System.IO.File.GetLastWriteTime(path);

            if (retFlg == false)
            {
                retFlg = ReadElfMap.Interpret(textArray, myComponents.MapList);

            }

            if (retFlg == false)
            {
                retFlg = RmAddressMap.Interpret(textArray, myComponents.MapList);

            }

            if (retFlg == true)
            {
                myComponents.ValidMapPath = path;
                myComponents.ValidMapLastWrittenDate = date;

                List<string> variableList = new List<string>();

                foreach (var factor in myComponents.MapList)
                {
                    variableList.Add(factor.VariableName);

                }

                AutoCompleteSourceForVariable = new AutoCompleteStringCollection();
                AutoCompleteSourceForVariable.AddRange(variableList.ToArray());

                if ((myComponents.ViewSettingList != null) &&
                    (myComponents.ViewSettingList.Count > 0))
                {
                    reviseDataFromViewSettingList();
                    checkDataGridViewCells();
                    dataGridView.Refresh();

                }

                if (string.IsNullOrEmpty(myComponents.TargetVer))
                {
                    myComponents.TargetVer = System.IO.Path.GetFileNameWithoutExtension(path);
                    targetVerViewControl.TextBox = myComponents.TargetVer;

                }

            }
            else
            {
                myComponents.ValidMapPath = null;
                myComponents.ValidMapLastWrittenDate = DateTime.MinValue;
                AutoCompleteSourceForVariable = new AutoCompleteStringCollection();

            }

            return retFlg;
        }

        private void reviseDataFromViewSettingList()
        {
            foreach (var factor in myComponents.ViewSettingList)
            {
                foreach (var itemDS in factor.DataSetting)
                {
                    if ((myComponents.MapList == null) ||
                        (myComponents.MapList.Count <= 0))
                    {
                        //not import MapFile

                    }
                    else
                    {
                        if ((string.IsNullOrEmpty(itemDS.Variable) == false) &&
                            (itemDS.AddrLock != true))
                        {
                            string tmpVariable = itemDS.Variable.ToString();

                            MapFactor result = myComponents.MapList.Find(key => key.VariableName == tmpVariable);

                            if (result != null)
                            {
                                if ((result.Size == "1") ||
                                    (result.Size == "2") ||
                                    (result.Size == "4"))
                                {
                                    itemDS.Size = result.Size;

                                }

                                itemDS.Address = result.Address;

                            }
                            else
                            {
                                itemDS.Address = null;

                            }

                        }

                    }

                }

            }

        }


        private void refreshDataGridView()
        {
            if (myComponents.ViewSettingList == null)
            {
                return;
            }

            if (pageValComboBox.SelectedIndex <= -1)
            {
                pageValComboBox.SelectedIndex = 0;

            }

            this.dataGridView.DataSource = myComponents.ViewSettingList[pageValComboBox.SelectedIndex].DataSetting;

            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].HeaderCell.Value = i.ToString();
            }

            dataGridView.RowHeadersWidth = 48;

            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                if ((string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Size].Value as string)) ||
                    (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Type].Value as string)) ||
                    (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.WriteValue].Value as string)))
                {

                }
                else
                {
                    string size = item.Cells[(int)DgvRowName.Size].Value.ToString();
                    string type = item.Cells[(int)DgvRowName.Type].Value.ToString();

                    int intSize = int.Parse(size);
                    Exception ex;

                    string retValue = item.Cells[(int)DgvRowName.WriteValue].Value.ToString();
                    string retText = TypeConvert.ToHexChars(type, intSize, retValue, out ex);
                    item.Cells[(int)DgvRowName.WriteText].Value = retText;
                    //retValue = TypeConvert.FromHexChars(type, intSize, retText);
                    //item.Cells[(int)DgvRowName.WriteValue].Value = retValue;

                }

            }

            dataGridView.Refresh();

        }


        public bool checkDataGridViewCells()
        {
            dataGridView.EndEdit();

            foreach (DataGridViewRow item in dataGridView.Rows)
            {
                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Size].Value as string))
                {
                    item.Cells[(int)DgvRowName.Size].Value = 1;

                }

                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Type].Value as string))
                {
                    item.Cells[(int)DgvRowName.Type].Value = numeralSystem.HEX;

                }

                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Offset].Value as string))
                {
                    item.Cells[(int)DgvRowName.Offset].Value = 0;

                }

                if ((string.IsNullOrEmpty(item.Cells[(int)DgvRowName.WriteValue].Value as string) == false) &&
                    (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.WriteText].Value as string) == true))
                {
                    item.Cells[(int)DgvRowName.WriteValue].ErrorText = "Invalid value.";

                }
                else
                {
                    item.Cells[(int)DgvRowName.WriteValue].ErrorText = null;

                }

            }

            DataGridViewRow[] checkedRowData = (from DataGridViewRow x in dataGridView.Rows where (bool)x.Cells[(int)DgvRowName.Check].Value == true select x).ToArray();

            int maxIndex = checkedRowData.Length;

            bool errFlg = false;
            bool validFlg = false;
            int totalSize = 0;

            foreach (DataGridViewRow item in checkedRowData)
            {
                // Check size, address and offset

                validFlg = false;

                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Size].Value as string) == false)
                {
                    string str = item.Cells[(int)DgvRowName.Size].Value.ToString();

                    if (TypeConvert.IsNumeric(str) == true)
                    {
                        int num = int.Parse(str);

                        if ((num == 1) ||
                            (num == 2) ||
                            (num == 4))
                        {
                            totalSize += num;
                            validFlg = true;

                        }

                    }

                }

                if (validFlg == false)
                {
                    errFlg = true;
                    item.Cells[(int)DgvRowName.Size].ErrorText = "Invalid value.";
                    item.Cells[(int)DgvRowName.Check].Value = false;
                }
                else
                {
                    item.Cells[(int)DgvRowName.Size].ErrorText = null;

                }

                validFlg = false;

                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Address].Value as string) == false)
                {
                    string str = item.Cells[(int)DgvRowName.Address].Value.ToString();

                    if (TypeConvert.IsHexString(str) == true)
                    {
                        item.Cells[(int)DgvRowName.Address].ErrorText = null;

                    }
                    else
                    {
                        errFlg = true;
                        item.Cells[(int)DgvRowName.Address].ErrorText = "Invalid value.";
                        item.Cells[(int)DgvRowName.Check].Value = false;

                    }

                }
                else
                {
                    item.Cells[(int)DgvRowName.Address].ErrorText = null;
                    item.Cells[(int)DgvRowName.Check].Value = false;

                }

                validFlg = false;

                if (string.IsNullOrEmpty(item.Cells[(int)DgvRowName.Offset].Value as string) == false)
                {
                    string str = item.Cells[(int)DgvRowName.Offset].Value.ToString();

                    if ( (TypeConvert.IsNumeric(str) == true) ||
                       (TypeConvert.IsHexString(str) == true) )
                    {
                        validFlg = true;

                    }

                }

                if (validFlg == false)
                {
                    errFlg = true;
                    item.Cells[(int)DgvRowName.Offset].ErrorText = "Invalid value.";
                    item.Cells[(int)DgvRowName.Check].Value = false;
                }
                else
                {
                    item.Cells[(int)DgvRowName.Offset].ErrorText = null;

                }

            }


            if (maxIndex == 0)
            {
                errFlg = true;
                PutWarningMessage("No cheked cells.");
            }
            else if (errFlg == true)
            {
                PutWarningMessage("Invalid data found.");
            }
            else if (totalSize > MAX_TOTAL_SIZE)
            {
                PutWarningMessage("Total size is invalid. ( Total size <= 128 )");
            }
            else if (maxIndex > SELECT_NUM)
            {
                PutWarningMessage("Selected item is invalid. ( Total item <= 32 )");
            }
            else
            {

            }

            area1ToolStripStatusLabel.Text = "Number of checked rows =" + maxIndex.ToString() + "  /  " + "Total size =" + totalSize.ToString() + "bytes";

            if ((totalSize > 0) &&
                (myComponents.CommunicationMode != Components.CommMode.NotDefine))
            {
                // MaxSize: SlipCode(1byte) + (MSCnt(1byte) + payload(?byte) + crc(1byte)) * 2 + SlipCode(1byte)
                // MinSize: SlipCode(1byte) +  MSCnt(1byte) + payload(?byte) + crc(1byte)      + SlipCode(1byte)
                double frameMinSize = 1 + (1 + totalSize + 1) + 1;
                double frameMaxSize = 1 + (1 + totalSize + 1) * 2 + 1;
                double abyteTxTime = (1 / (double)myComponents.CommBaudRate) * 10;
                double targetTxMinTime = frameMinSize * abyteTxTime * 1000;
                double targetTxMaxTime = frameMaxSize * abyteTxTime * 1000;

                area2ToolStripStatusLabel.Text = "Target Tx Time =" + targetTxMinTime.ToString("F2") + " to " + targetTxMaxTime.ToString("F2") + "ms";

            }
            else
            {
                area2ToolStripStatusLabel.Text = "-";

            }


            return errFlg;

        }

        public bool checkTimingTextBox()
        {
            bool errFlg = true;
            string timingNum = timingValTextBox.Text.ToString();

            if (TypeConvert.IsNumeric(timingNum) == true)
            {
                errFlg = false;

            }

            return errFlg;

        }


        private StringBuilder makeLogData(RecordMode mode)
        {
            StringBuilder text = new StringBuilder();

            if (RcvLogData == null)
            {
                return text;
            }

            string note = "Start Logging time: " + LogStartTime.ToString();

            text.AppendLine(note);

            string delimiter;

            if (mode == RecordMode.ClipBoard)
            {
                delimiter = "\t";
            }
            else
            {
                delimiter = ",";
            }

            if (CheckedCellData != null)
            {
                string header = "1.Rcv Status" + delimiter + "2.OS Timer" + delimiter + "3.Slave Count";

                foreach (var name in CheckedCellData)
                {
                    header = header + delimiter + name.Cells[(int)DgvRowName.Name].Value;
                }

                text.AppendLine(header);

            }

            if (RcvLogData.Count != 0)
            {
                foreach (var list in RcvLogData)
                {
                    string line = "";

                    foreach (var data in list)
                    {
                        if (line == "")
                        {
                            line = data;

                        }
                        else
                        {
                            line = line + delimiter + data;

                        }

                    }

                    text.AppendLine(line);

                }

            }

            return text;

        }


        private void startLogMode()
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            myCommProtocol.setLogModeStart();

        }


        private void stopLogMode()
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            myCommProtocol.setLogModeStop();

        }


        private void readDUTVersion()
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            myCommProtocol.readVersion(myComponents.Password);

        }


        private void renewLogSetting()
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            CheckedCellData = (from DataGridViewRow x in dataGridView.Rows where (bool)x.Cells[(int)DgvRowName.Check].Value == true select x).ToArray();

            List<CommProtocol.SetLogParam> listParam = new List<CommProtocol.SetLogParam>();

            int maxIndex = CheckedCellData.Length;

            bool errFlg = false;

            for (int i = 0; i < maxIndex; i++)
            {
                CommProtocol.SetLogParam tmpParam = new CommProtocol.SetLogParam();

                if ((string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value as string)) ||
                    (string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Address].Value as string)) ||
                    (string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Offset].Value as string)) ||
                    (string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Type].Value as string)))
                {
                    errFlg = true;
                    break;

                }
                else
                {
                    string address = CheckedCellData[i].Cells[(int)DgvRowName.Address].Value.ToString();
                    string offset = CheckedCellData[i].Cells[(int)DgvRowName.Offset].Value.ToString();

                    Int64 intAddress = 0;
                    Int64 intOffset = 0;

                    try
                    {
                        intAddress = Convert.ToInt64(address, 16);

                        if (TypeConvert.IsHexString(offset) == true)
                        {
                            intOffset = Convert.ToInt64(offset, 16);

                        }
                        else
                        {
                            intOffset = Convert.ToInt64(offset);

                        }


                    }
                    catch (Exception ex)
                    {
                        PutWarningMessage(ex.Message);

                        errFlg = true;
                        break;

                    }

                    Exception ex_text = null;

                    address = TypeConvert.ToHexChars(numeralSystem.UDEC, 4, ((intAddress + intOffset).ToString()), out ex_text);

                    if (ex_text != null)
                    {
                        return;

                    }

                    tmpParam.Size = CheckedCellData[i].Cells[(int)DgvRowName.Size].Value.ToString();
                    tmpParam.Address = address;
                    listParam.Add(tmpParam);

                }


            }

            if (maxIndex == 0)
            {

            }
            else if (errFlg == true)
            {

            }
            else
            {
                myCommProtocol.setLogData(listParam);

                myCommProtocol.setLogModeStart();

            }

        }


        private void renewTimingSetting(string timingNum)
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            if (TypeConvert.IsNumeric(timingNum) == false)
            {
                timingNum = INITIAL_TIMING_VALUE.ToString();

            }

            myComponents.TimingValue = int.Parse(timingNum);

            Exception ex_text = null;
            string timngVal = TypeConvert.ToHexChars(numeralSystem.UDEC, 2, timingNum, out ex_text);

            myCommProtocol.setTiming(timngVal);

        }


        private void writeData(string size, string address, string offset, string writeVal)
        {
            if (myComponents.CommActiveFlg == false)
            {
                return;

            }

            Int64 intAddress = 0;
            Int64 intOffset = 0;

            try
            {
                intAddress = Convert.ToInt64(address, 16);

                if (TypeConvert.IsHexString(offset) == true)
                {
                    intOffset = Convert.ToInt64(offset, 16);

                }
                else
                {
                    intOffset = Convert.ToInt64(offset);

                }

            }
            catch (Exception ex)
            {
                PutWarningMessage(ex.Message);
                return;

            }

            Exception ex_text = null;

            address = TypeConvert.ToHexChars(numeralSystem.UDEC, 4, ((intAddress + intOffset).ToString()), out ex_text);

            writeVal = TypeConvert.FromHexChars(numeralSystem.HEX, int.Parse(size), writeVal);

            if (ex_text != null)
            {
                return;

            }

            if (string.IsNullOrEmpty(writeVal))
            {
                return;
            }

            myCommProtocol.wirteData(size, address, writeVal);

        }


        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen == false)
            {
                return;

            }

            try
            {
                int size = serialPort.BytesToRead;

                if (size > 0)
                {
                    byte[] rcvbuff = new byte[size];

                    serialPort.Read(rcvbuff, 0, size);

                    if (myComponents.CommActiveFlg == true)
                    {
                        myCommProtocol.decode(rcvbuff);

                    }

                }

            }
            catch (Exception ex)
            {
                PutWarningMessage(ex.Message);

            }

        }


        private bool serialPort_DataSend(List<byte> frame)
        {
            bool retFlg = false;

            if (serialPort.IsOpen == false)
            {

            }
            else
            {
                try
                {
                    List<byte> txBuff = new List<byte>(frame);

                    byte[] tmp = myCommProtocol.encode(txBuff);

                    serialPort.Write(tmp, 0, tmp.Length);

                    retFlg = true;

                }
                catch (Exception ex)
                {
                    PutWarningMessage(ex.Message);

                }

            }

            return retFlg;
        }


        private bool serialPort_SelectState(bool reqFlg)
        {
            bool retFlg = false;

            if (reqFlg == true)
            {
                if ((serialPort.IsOpen == false) &&
                    (myComponents.CommunicationMode == Components.CommMode.Serial) &&
                    (string.IsNullOrEmpty(myComponents.CommPort) == false) &&
                    (myComponents.CommBaudRate != 0))
                {
                    serialPort.PortName = myComponents.CommPort;
                    serialPort.BaudRate = myComponents.CommBaudRate;

                    serialPort.DataBits = 8;
                    serialPort.Parity = System.IO.Ports.Parity.None;
                    serialPort.StopBits = System.IO.Ports.StopBits.One;
                    serialPort.Handshake = System.IO.Ports.Handshake.None;

                    serialPort.Encoding = Encoding.ASCII;

                    try
                    {
                        serialPort.Open();

                        retFlg = true;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                    this.serialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);

                }


            }
            else
            {
                if (serialPort.IsOpen == true)
                {
                    serialPort.Close();

                    retFlg = true;

                    this.serialPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort_DataReceived);

                }

            }

            return retFlg;
        }


        private void sockets_DataReceived(IAsyncResult ar)
        {
            SocketsAsyncParam ap = (SocketsAsyncParam)ar.AsyncState;

            if (ap.Client.Connected == false)
            {
                return;
            }

            System.Net.Sockets.NetworkStream stream = ap.Client.GetStream();

            int size = stream.EndRead(ar);

            byte[] rcvbuff = new byte[size];

            for (int i = 0; i < size; i++)
            {
                rcvbuff[i] = ap.ReadBuff[i];

            }

            if (myComponents.CommActiveFlg == true)
            {
                myCommProtocol.decode(rcvbuff);

            }

            stream.BeginRead(ap.ReadBuff, 0, ap.ReadBuff.Length, new AsyncCallback(sockets_DataReceived), ap);

        }


        private bool sockets_DataSend(List<byte> frame)
        {
            bool retFlg = false;

            if (SocketsParam.Client == null)
            {
                return retFlg;

            }

            System.Net.Sockets.NetworkStream stream = SocketsParam.Client.GetStream();

            List<byte> txBuff = new List<byte>(frame);

            byte[] tmp = myCommProtocol.encode(txBuff);

            stream.Write(tmp, 0, tmp.Length);

            retFlg = true;

            return retFlg;
        }


        private bool sockets_SelectState(bool reqFlg)
        {
            bool retFlg = false;

            if (reqFlg == true)
            {
                if ((myComponents.CommunicationMode == Components.CommMode.NetWork) &&
                    (myComponents.NetIP != null) &&
                    (myComponents.NetPort != 0))
                {
                    try
                    {
                        SocketsParam.Client = new System.Net.Sockets.TcpClient();
                        SocketsParam.ReadBuff = new byte[16];
                        SocketsParam.Client.Connect(myComponents.NetIP, myComponents.NetPort);

                        System.Net.Sockets.NetworkStream stream = SocketsParam.Client.GetStream();

                        stream.BeginRead(SocketsParam.ReadBuff, 0, SocketsParam.ReadBuff.Length, new AsyncCallback(sockets_DataReceived), SocketsParam);

                        retFlg = true;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);

                    }

                }


            }
            else
            {
                if (SocketsParam.Client != null)
                {
                    SocketsParam.Client.Close();
                    SocketsParam.Client = null;
                    retFlg = true;

                }

            }

            return retFlg;
        }


        private bool commResource_CheckState()
        {
            bool retFlg = false;

            switch (myComponents.CommunicationMode)
            {
                case Components.CommMode.NotDefine:

                    break;

                case Components.CommMode.Serial:
                    if (serialPort.IsOpen == true)
                    {
                        retFlg = true;

                    }

                    break;

                case Components.CommMode.NetWork:
                    if (SocketsParam.Client != null)
                    {
                        retFlg = true;

                    }

                    break;

            }

            return retFlg;
        }


        public SubViewControl()
        {
            myComponents = new Components();
            myCommProtocol = new CommProtocol();

            InitializeComponent();
            commonInitialRoutine();

        }


        public SubViewControl(Components tmp)
        {
            myComponents = tmp;
            myCommProtocol = new CommProtocol();

            InitializeComponent();
            commonInitialRoutine();

        }

        private void commonInitialRoutine()
        {
            // Redefined Column Name
            dataGridView.Columns[0].Name = DgvRowName.Group.ToString();
            dataGridView.Columns[1].Name = DgvRowName.Check.ToString();
            dataGridView.Columns[2].Name = DgvRowName.Size.ToString();
            dataGridView.Columns[3].Name = DgvRowName.Variable.ToString();
            dataGridView.Columns[4].Name = DgvRowName.Addrlock.ToString();
            dataGridView.Columns[5].Name = DgvRowName.Address.ToString();
            dataGridView.Columns[6].Name = DgvRowName.Offset.ToString();
            dataGridView.Columns[7].Name = DgvRowName.Name.ToString();
            dataGridView.Columns[8].Name = DgvRowName.Type.ToString();
            dataGridView.Columns[9].Name = DgvRowName.ReadText.ToString();
            dataGridView.Columns[10].Name = DgvRowName.ReadValue.ToString();
            dataGridView.Columns[11].Name = DgvRowName.WriteText.ToString();
            dataGridView.Columns[12].Name = DgvRowName.WriteValue.ToString();
            dataGridView.Columns[13].Name = DgvRowName.WrTrg.ToString();

            dataGridView.AutoGenerateColumns = false;
            dataGridView.AllowUserToDeleteRows = false;

            dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
            dataGridView.Columns[(int)DgvRowName.Group].Visible = false;

            dataGridView.Columns[(int)DgvRowName.Check].ReadOnly = true;
            dataGridView.Columns[(int)DgvRowName.Addrlock].ReadOnly = true;

            System.Data.DataTable typeTable = new System.Data.DataTable("typeTable");
            typeTable.Columns.Add("Display", typeof(string));
            typeTable.Rows.Add(numeralSystem.HEX);
            typeTable.Rows.Add(numeralSystem.UDEC);
            typeTable.Rows.Add(numeralSystem.DEC);
            typeTable.Rows.Add(numeralSystem.BIN);
            typeTable.Rows.Add(numeralSystem.FLT);

            (dataGridView.Columns[(int)DgvRowName.Type] as DataGridViewComboBoxColumn).ValueType = typeof(string);
            (dataGridView.Columns[(int)DgvRowName.Type] as DataGridViewComboBoxColumn).ValueMember = "Display";
            (dataGridView.Columns[(int)DgvRowName.Type] as DataGridViewComboBoxColumn).DisplayMember = "Display";
            (dataGridView.Columns[(int)DgvRowName.Type] as DataGridViewComboBoxColumn).DataSource = typeTable;

            contextMenuStrip = new ContextMenuStrip(this.components);
            contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new System.Drawing.Size(61, 4);

            contextMenuStrip.Items.Clear();
            contextMenuStrip.Items.Add("Delete this Item", null, OnDeleteItemButtonPressed);
            contextMenuStrip.Items.Add("Insert an Item to next row", null, OnInsertItemButtonPressed);
            contextMenuStrip.Items.Add("Copy this Item to next row", null, OnCopyItemButtonPressed);
            contextMenuStrip.Items.Add("Delete this Page", null, OnDeletePageButtonPressed);
            contextMenuStrip.Items.Add("Insert an Page to next", null, OnInsertPageButtonPressed);
            contextMenuStrip.Items.Add("Copy this Page to next", null, OnCopyPageButtonPressed);

            timingValTextBox.Text = INITIAL_TIMING_VALUE.ToString();

            ContinueCnt = 1;
            NextSlvCnt = 0;
            RxSilentCnt = 0;

            mainTimer.Interval = MAIN_CTRL_INTERVAL;
            mainTimer.Enabled = true;

        }

        public void commonClosingRoutine()
        {
            mainTimer.Enabled = false;

            if (myComponents.CommActiveFlg == true)
            {
                if (myComponents.CommunicationMode == Components.CommMode.Serial)
                {
                    serialPort_SelectState(false);

                }
                else if (myComponents.CommunicationMode == Components.CommMode.NetWork)
                {
                    sockets_SelectState(false);

                }

            }

        }

        private void pageValComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
            bool errFlg = checkDataGridViewCells();

            if (myComponents.CommActiveFlg == true)
            {
                if (errFlg == false)
                {
                    renewLogSetting();

                }
                else
                {
                    stopLogMode();

                }

            }

        }

        private void customizeToolStripButton_Click(object sender, EventArgs e)
        {
            if (myComponents.CommActiveFlg == true)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

            }
            else
            {
                if (myComponents.CustomizingModeFlg == false)
                {
                    myComponents.CustomizingModeFlg = true;

                    customizeToolStripButton.Image = Properties.Resources.Complete_and_ok_green;

                    dataGridView.Columns[(int)DgvRowName.Group].Visible = true;

                    if (System.IO.File.Exists(myComponents.ValidMapPath) != true)
                    {
                        PutWarningMessage("Map file was not found.");

                        myComponents.MapList = new List<MapFactor>();
                        myComponents.ValidMapPath = null;
                        myComponents.ValidMapLastWrittenDate = DateTime.MinValue;
                        AutoCompleteSourceForVariable = new AutoCompleteStringCollection();
                    }
                    else
                    {
                        DateTime now = System.IO.File.GetLastWriteTime(myComponents.ValidMapPath);

                        if (now > myComponents.ValidMapLastWrittenDate)
                        {
                            DialogResult result = MessageBox.Show("Map file was updated.\nDo you want to reload Address in Data Grid View?",
                                                                    "Question",
                                                                    MessageBoxButtons.YesNo,
                                                                    MessageBoxIcon.Exclamation,
                                                                    MessageBoxDefaultButton.Button2);

                            if (result == DialogResult.Yes)
                            {
                                if (loadMapFile(myComponents.ValidMapPath) == false)
                                {
                                    MessageBox.Show("Can't read map file",
                                                        "Caution",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Warning);

                                }

                            }

                        }

                    }

                    dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.Orange;

                    targetVerViewControl.TextEnabled = true;

                    dataGridView.ContextMenuStrip = contextMenuStrip;

                    dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);

                }
                else
                {
                    refreshDataGridView();
                    checkDataGridViewCells();

                    myComponents.CustomizingModeFlg = false;

                    customizeToolStripButton.Image = Properties.Resources.Complete_and_ok_gray;

                    dataGridView.Columns[(int)DgvRowName.Group].Visible = false;

                    dataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;

                    targetVerViewControl.TextEnabled = false;

                    dataGridView.ContextMenuStrip = null;

                    dataGridView.MouseDown -= new System.Windows.Forms.MouseEventHandler(dataGridView_MouseDown);

                    myComponents.TargetVer = targetVerViewControl.TextBox;

                }

            }

        }

        private void timingValTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                string timingNum = timingValTextBox.Text.ToString();

                if (checkTimingTextBox())
                {
                    timingNum = INITIAL_TIMING_VALUE.ToString();
                    timingValTextBox.Text = INITIAL_TIMING_VALUE.ToString();
                    PutWarningMessage("Invalid timing value. Initial value is set.");

                }
                else
                {
                    int num = int.Parse(timingNum);

                    if (num > MAXIMUM_TIMING_VALUE)
                    {
                        num = MAXIMUM_TIMING_VALUE;

                        timingNum = num.ToString();
                        timingValTextBox.Text = num.ToString();
                        PutWarningMessage("Timing value is limited by maximum value.");

                    }

                }


                renewTimingSetting(timingNum);

            }

        }

        private void logCtrlButton_Click(object sender, EventArgs e)
        {
            string DATALOG_STOP_TEXT = "Stop  Log";
            string DATALOG_START_TEXT = "Start Log";

            if (myComponents.LoggingActiveFlg == true)
            {
                logCtrlButton.Image = Properties.Resources.Complete_and_ok_gray;
                logCtrlButton.Text = DATALOG_START_TEXT;
                myComponents.LoggingActiveFlg = false;

                var text = makeLogData(RecordMode.ClipBoard);

                if (text.Length != 0)
                {
                    Clipboard.SetText(text.ToString());
                    PutWarningMessage("Logging data has been copied to clipboard.");

                }

            }
            else
            {
                myComponents.LoggingActiveFlg = true;
                logCtrlButton.Image = Properties.Resources.Complete_and_ok_green;
                logCtrlButton.Text = DATALOG_STOP_TEXT;

                LogStartTime = DateTime.MinValue;

            }

        }

        private void dumpEntryButton_Click(object sender, EventArgs e)
        {
            if (DumpFormInstance != null)
            {
                DumpFormInstance.Close();
            }

            DumpFormInstance = new DumpForm(this);
            DumpFormInstance.Show();

        }

        private void opclCommButton_Click(object sender, EventArgs e)
        {
            const string COMM_OPEN_TEXT = "Comm Open ";
            const string COMM_CLOSE_TEXT = "Comm Close";

            if (myComponents.CustomizingModeFlg == true)
            {
                MessageBox.Show("Quit custmizing mode.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

            }
            else
            {
                if (myComponents.CommActiveFlg == false)
                {
                    if (System.IO.File.Exists(myComponents.ValidMapPath) == true)
                    {
                        DateTime now = System.IO.File.GetLastWriteTime(myComponents.ValidMapPath);

                        if (now > myComponents.ValidMapLastWrittenDate)
                        {
                            DialogResult result = MessageBox.Show("Map file was updated.\nDo you want to reload Address in Data Grid View?",
                                                                    "Question",
                                                                    MessageBoxButtons.YesNo,
                                                                    MessageBoxIcon.Exclamation,
                                                                    MessageBoxDefaultButton.Button2);

                            if (result == DialogResult.Yes)
                            {
                                if (loadMapFile(myComponents.ValidMapPath) == false)
                                {
                                    MessageBox.Show("Can't read map file",
                                                        "Caution",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Warning);

                                }

                            }

                        }

                    }

                    bool dgvErrFlg = checkDataGridViewCells();
                    bool ttbErrFlg = checkTimingTextBox();

                    if ((dgvErrFlg == false) &&
                        (ttbErrFlg == false))
                    {
                        bool retFlg = false;

                        if (myComponents.CommunicationMode == Components.CommMode.Serial)
                        {
                            retFlg = serialPort_SelectState(true);

                        }
                        else if (myComponents.CommunicationMode == Components.CommMode.NetWork)
                        {
                            retFlg = sockets_SelectState(true);

                        }
                        else
                        {
                            PutWarningMessage("Communication setting is undefined.");

                        }

                        if (retFlg == true)
                        {
                            myCommProtocol.startStopWatch();

                            myCommProtocol.initial();

                            myComponents.CommActiveFlg = true;

                            opclCommButton.Image = Properties.Resources.FlagThread_red;
                            opclCommButton.Text = COMM_CLOSE_TEXT;

                            readDUTVersion();

                            string timingNum = timingValTextBox.Text.ToString();
                            renewTimingSetting(timingNum);

                            renewLogSetting();

                            PutWarningMessage("");  //Clear warning meassage.

                        }

                    }


                }
                else
                {
                    myCommProtocol.stopStopWatch();

                    bool retFlg = false;

                    myComponents.CommActiveFlg = false;

                    opclCommButton.Image = Properties.Resources.FlagThread_white;
                    opclCommButton.Text = COMM_OPEN_TEXT;

                    if (myComponents.CommunicationMode == Components.CommMode.Serial)
                    {
                        retFlg = serialPort_SelectState(false);

                    }
                    else if (myComponents.CommunicationMode == Components.CommMode.NetWork)
                    {
                        retFlg = sockets_SelectState(false);

                    }

                    //Revise Timing
                    timingValTextBox.Text = INITIAL_TIMING_VALUE.ToString();

                    if (retFlg == true)
                    {
                        myCommProtocol.clear();

                    }

                    if (myComponents.LoggingActiveFlg == true)
                    {
                        logCtrlButton.PerformClick();

                    }

                }

            }

        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            // Warning information
            if (WarningText != "")
            {
                WarningViewControl.TextBox = WarningText;
                WarningText = "";
            }

            if (WarningViewControl.TextBox != "")
            {
                if (WarningReminder == false)
                {
                    WarningShowUpCount++;
                    if (WarningShowUpCount > WARNING_SHOWUP_CNT_MAX)
                    {
                        WarningViewControl.TextBox = "";
                    }
                }

            }
            else
            {
                WarningShowUpCount = 0;

            }

            while (myCommProtocol.myComponents.CommLog.Count != 0)
            {
                string data = myCommProtocol.myComponents.CommLog.Dequeue();

                if (data != null)
                {
                    List<string> lines = new List<string>(commLogtextBox.Lines);

                    if (lines.Count > COMM_LOG_MAX)
                    {
                        lines.RemoveAt(0);
                        commLogtextBox.Text = String.Join("\r\n", lines);

                    }

                    commLogtextBox.AppendText(data + Environment.NewLine);

                }

            }

            if ((commResource_CheckState() == false) ||
                (myComponents.CommActiveFlg == false))
            {
                dispRxDStatusLabel.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);
                dispTxDStatusLabel.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);

                return;

            }

            bool rcvFlg = myCommProtocol.mainControl();

            if (rcvFlg == true)
            {
                dispRxDStatusLabel.BackColor = System.Drawing.Color.Red;
                RxSilentCnt = 0;
            }
            else
            {
                dispRxDStatusLabel.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);
                RxSilentCnt++;

                int tmpTimeoutCnt = myComponents.TimingValue * 10 / MAIN_CTRL_INTERVAL;
                int maxTimeoutCnt = MAXIMUM_TIMING_VALUE * 2 / MAIN_CTRL_INTERVAL;

                if (tmpTimeoutCnt > maxTimeoutCnt)
                {
                    tmpTimeoutCnt = maxTimeoutCnt;
                }

                if (RxSilentCnt > tmpTimeoutCnt)
                {
                    DateTime dtNow = DateTime.Now;
                    string strDataTime = dtNow.ToString("yyyy/MM/dd (ddd) HH:mm:ss");

                    stopLogMode();
                    PutUnerasableWarningMessage("Communication stopped at " + strDataTime);
                }

            }

            var isLogMode = myCommProtocol.isLogMode();

            if (isLogMode == false)
            {
                ContinueCnt = 1;
                NextSlvCnt = 0;
                RxSilentCnt = 0;
                RcvLogData = new List<List<string>>();

            }
            else
            {
                if (ContinueCnt > CONTINUE_CNT_MAX)
                {
                    ContinueCnt = 1;

                    List<byte> peekTxBuff = myCommProtocol.getTxData();

                    if (peekTxBuff == null)
                    {
                        startLogMode();

                    }

                }
                else
                {
                    ContinueCnt++;

                }

                while (myCommProtocol.myComponents.ReceiveStream.Count != 0)
                {
                    CommProtocol.RxDataParam rxStream = myCommProtocol.myComponents.ReceiveStream.Dequeue();

                    List<string> lostLogBuff = new List<string>();

                    int slvCnt = (int)(rxStream.Data[0] & 0x0F);

                    if (NextSlvCnt == 0)
                    {
                        NextSlvCnt = slvCnt + 1;

                    }
                    else if (slvCnt == NextSlvCnt)
                    {
                        NextSlvCnt = slvCnt + 1;

                        if (NextSlvCnt >= 16)
                        {
                            NextSlvCnt = 1;
                        }
                    }
                    else
                    {
                        int tmp = slvCnt - NextSlvCnt;

                        if (tmp < 0)
                        {
                            tmp = 15 + tmp;

                        }

                        NextSlvCnt = slvCnt + 1;

                        lostLogBuff.Add(tmp.ToString() + " messages might be lost");
                        lostLogBuff.Add("-");       //for OS Timer
                        lostLogBuff.Add("-");       //for Count

                    }

                    rxStream.Data.RemoveAt(0);

                    List<int> listNumSize = new List<int>();
                    int maxIndex = CheckedCellData.Length;

                    for (int i = 0; i < maxIndex; i++)
                    {
                        int numSize = 0;

                        if ((CheckedCellData[i].Index != -1) &&
                            (string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value as string) == false))
                        {
                            int.TryParse(CheckedCellData[i].Cells[(int)DgvRowName.Size].Value.ToString(), out numSize);
                        }

                        listNumSize.Add(numSize);

                    }

                    bool validflg;
                    List<string> rxData = myCommProtocol.interpretRxFrameToHexChars(rxStream.Data, listNumSize, out validflg);
                    List<string> logBuff = new List<string>();

                    if (validflg == true)
                    {
                        logBuff.Add("OK");
                        logBuff.Add(rxStream.Time);
                        logBuff.Add(slvCnt.ToString());

                        for (int i = 0; i < maxIndex; i++)
                        {
                            string retText = rxData[i];

                            CheckedCellData[i].Cells[(int)DgvRowName.ReadText].Value = retText;

                            string type = numeralSystem.HEX;

                            if (string.IsNullOrEmpty(CheckedCellData[i].Cells[(int)DgvRowName.Type].Value as string) == false)
                            {
                                type = CheckedCellData[i].Cells[(int)DgvRowName.Type].Value.ToString();

                            }

                            string retValue = TypeConvert.FromHexChars(type, listNumSize[i], retText);

                            CheckedCellData[i].Cells[(int)DgvRowName.ReadValue].Value = retValue;

                            logBuff.Add(retValue);

                        }

                    }
                    else
                    {
                        logBuff.Add("Invalid DataLength");
                        logBuff.Add(rxStream.Time);
                        logBuff.Add(slvCnt.ToString());

                    }

                    if (myComponents.LoggingActiveFlg == true)
                    {
                        if (LogStartTime == DateTime.MinValue)
                        {
                            LogStartTime = DateTime.Now;

                        }

                        if (lostLogBuff.Count != 0)
                        {
                            RcvLogData.Add(lostLogBuff);

                        }

                        RcvLogData.Add(logBuff);

                        if (RcvLogData.Count > RCV_LOGDATA_MAX)
                        {
                            RcvLogData.RemoveAt(0);

                        }

                    }
                    else
                    {
                        RcvLogData = new List<List<string>>();

                    }

                }

            }

            List<byte> txBuff = myCommProtocol.getTxData();

            if (txBuff != null)
            {
                dispTxDStatusLabel.BackColor = System.Drawing.Color.Orange;

                bool retFlg = false;

                if (myComponents.CommunicationMode == Components.CommMode.Serial)
                {
                    retFlg = serialPort_DataSend(txBuff);

                }
                else if (myComponents.CommunicationMode == Components.CommMode.NetWork)
                {
                    retFlg = sockets_DataSend(txBuff);

                }

                myCommProtocol.setTxCondtion(retFlg);

            }
            else
            {
                dispTxDStatusLabel.BackColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);

            }

            dutVerViewControl.TextBox = myCommProtocol.myComponents.DutVersion;

            if ((DumpFormInstance != null) &&
                (string.IsNullOrEmpty(myCommProtocol.myComponents.DumpData) == false))
            {
                DumpFormInstance.PutDumpData(myCommProtocol.myComponents.DumpData);
                myCommProtocol.myComponents.DumpData = "";
            }

        }

        private void PutWarningMessage(string text)
        {
            WarningText = text;
            WarningShowUpCount = 0;
            WarningReminder = false;

        }

        private void PutUnerasableWarningMessage(string text)
        {
            WarningText = text;
            WarningShowUpCount = 0;
            WarningReminder = true;

        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;

            }

            DataGridView dgv = (DataGridView)sender;
            int columnIndex = e.ColumnIndex;

            switch ((DgvRowName)columnIndex)
            {
                case DgvRowName.Check:
                    if (Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == true)
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = false;

                        if (checkDataGridViewCells() == false)
                        {
                            renewLogSetting();
                        }
                        else
                        {
                            stopLogMode();
                        }

                    }
                    else
                    {
                        if ((dgv[(int)DgvRowName.Size, e.RowIndex].ErrorText != "") ||
                            (dgv[(int)DgvRowName.Address, e.RowIndex].ErrorText != "") ||
                            (dgv[(int)DgvRowName.Offset, e.RowIndex].ErrorText != ""))
                        {
                            PutWarningMessage("Size, Address or offset data might has error.");
                        }
                        else if ((string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string)) ||
                                (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value as string)) ||
                                (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value as string)))
                        {
                            PutWarningMessage("Size, Address or offset data might be empty.");
                        }
                        else
                        {
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = true;

                            if (checkDataGridViewCells() == false)
                            {
                                renewLogSetting();
                            }
                            else
                            {
                                stopLogMode();
                            }

                        }

                    }
                    break;

                case DgvRowName.Addrlock:
                    if (Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Addrlock].Value) == true)
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Addrlock].Value = false;

                    }
                    else
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Addrlock].Value = true;

                    }
                    break;

                case DgvRowName.WrTrg:
                    if ((dgv[(int)DgvRowName.Size, e.RowIndex].ErrorText != "") ||
                        (dgv[(int)DgvRowName.Address, e.RowIndex].ErrorText != "") ||
                        (dgv[(int)DgvRowName.Offset, e.RowIndex].ErrorText != "") ||
                        (dgv[(int)DgvRowName.WriteValue, e.RowIndex].ErrorText != ""))
                    {
                        PutWarningMessage("Size, Address, Offset or WriteValue might be invalid.");
                    }
                    else if ((string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string)) ||
                            (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value as string)) ||
                            (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value as string)) ||
                            (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value as string)))
                    {
                        PutWarningMessage("Size, Address, Offset or WriteValue might be empty.");
                    }
                    else
                    {
                        string size = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();
                        string address = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value.ToString();
                        string offset = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value.ToString();
                        string type = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value.ToString();
                        string writeText = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value.ToString();

                        writeData(size, address, offset, writeText);

                    }

                    break;

            }

        }

        private void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if (e.Control is TextBox)
            {
                TextBox tb = (TextBox)e.Control;

                if (dgv.CurrentCell.OwningColumn.Name == DgvRowName.Variable.ToString())
                {
                    tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

                    tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tb.AutoCompleteCustomSource = AutoCompleteSourceForVariable;
                }
                else
                {
                    tb.AutoCompleteMode = AutoCompleteMode.None;
                }

            }

        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) &&
                (myComponents.CustomizingModeFlg == true))
            {
                foreach (DataGridViewCell cell in dataGridView.SelectedCells)
                {
                    int tmp = cell.ColumnIndex;

                    if (tmp == (int)DgvRowName.Group)
                    {
                        tmp = (int)DgvRowName.Group + 1;

                    }

                    dataGridView[tmp, cell.RowIndex].Value = null;

                }

            }

        }

        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = dataGridView.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    dataGridView.ClearSelection();
                    dataGridView.Rows[hitTest.RowIndex].Selected = true;

                }

            }

        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //Workaround: Problem that a user set value in last row
            dataGridView.CurrentCellChanged -= new System.EventHandler(dataGridView_CurrentCellChanged);
            dataGridView.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView_CellValueChanged);

            DataGridView dgv = (DataGridView)sender;

            if (dgv.CurrentCell == null)
            {
                return;
            }

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;

            }

            if (e.RowIndex == (dataGridView.Rows.Count - 1))
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = WorkaroundInputBuffer;
            }

        }

        private void dataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            //Workaround: Problem that a user set value in last row
            dataGridView.CurrentCellChanged -= new System.EventHandler(dataGridView_CurrentCellChanged);
            dataGridView.CellValueChanged -= new System.Windows.Forms.DataGridViewCellEventHandler(dataGridView_CellValueChanged);

        }

        private void dataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            dgv.CancelEdit();

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;

            }

            string inputText = e.FormattedValue.ToString();
            string size = null;
            string type = null;
            string readText = null;
            string writeText = null;

            if (string.IsNullOrEmpty(inputText))
            {
                inputText = null;

            }

            if ((dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null) &&
                (inputText == null))
            {
                return;

            }

            if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string) == false)
            {
                string currentText = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                if (inputText == currentText)
                {
                    return;

                }

            }

            //Workaround: Problem that a user set value in last row
            int workaroundConditionCnt = 0;

            if (e.RowIndex == (dataGridView.Rows.Count - 1))
            {
                workaroundConditionCnt++;
                WorkaroundInputBuffer = inputText;
            }

            int columnIndex = e.ColumnIndex;

            switch ((DgvRowName)columnIndex)
            {
                case DgvRowName.Group:
                    if ((e.RowIndex == 0) &&
                        (string.IsNullOrEmpty(inputText) == false))
                    {
                        //Group tag is a cell at the upper left.
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Group].Value = inputText;
                        workaroundConditionCnt++;
                        pageValComboBox.SelectedIndexChanged -= new System.EventHandler(pageValComboBox_SelectedIndexChanged);
                        pageValComboBox.Items[pageValComboBox.SelectedIndex] = inputText;
                        pageValComboBox.SelectedIndexChanged += new System.EventHandler(pageValComboBox_SelectedIndexChanged);

                    }

                    break;

                case DgvRowName.Check:
                    //Ignore
                    break;

                case DgvRowName.Size:
                    if ((Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == false) ||
                        (myComponents.CustomizingModeFlg == true))
                    {
                        bool validFlg = false;

                        //Clear Read/Write Values due to unmatch current size of values
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value = null;
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadValue].Value = null;
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value = null;
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value = null;

                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value = inputText;
                        workaroundConditionCnt++;

                        if (TypeConvert.IsNumeric(inputText) == true)
                        {
                            int num = int.Parse(inputText);

                            if ((num == 1) ||
                                (num == 2) ||
                                (num == 4))
                            {
                                validFlg = true;

                            }

                        }

                        if (validFlg == false)
                        {
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].ErrorText = "Invalid Value.";

                        }
                        else
                        {
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].ErrorText = null;

                        }

                    }

                    break;

                case DgvRowName.Variable:
                    if ((Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == false) ||
                        (myComponents.CustomizingModeFlg == true))
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Variable].Value = inputText;
                        workaroundConditionCnt++;

                        if ((myComponents.MapList == null) ||
                            (myComponents.MapList.Count <= 0))
                        {

                        }
                        else if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Variable].Value as string))
                        {
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].ErrorText = null;
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value = null;
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value = "0";

                        }
                        else
                        {
                            string cellValue = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Variable].Value.ToString();
                            MapFactor result = myComponents.MapList.Find(key => key.VariableName == cellValue);

                            if (result != null)
                            {
                                if (TypeConvert.IsNumeric(result.Size) == true)
                                {
                                    int num = int.Parse(result.Size);

                                    if ((num == 1) ||
                                        (num == 2) ||
                                        (num == 4))
                                    {
                                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value = result.Size;
                                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].ErrorText = null;

                                    }

                                }

                                dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value = result.Address;

                                if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value as string))
                                {
                                    dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value = "0";

                                }

                            }
                            else
                            {
                                dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value = false;
                                dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value = null;

                            }
                        }

                    }
                    break;

                case DgvRowName.Addrlock:
                    //Ignore
                    break;

                case DgvRowName.Address:
                    if ((Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == false) ||
                        (myComponents.CustomizingModeFlg == true))
                    {
                        bool validFlg = false;

                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value = inputText;
                        workaroundConditionCnt++;

                        if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Address].Value as string))
                        {
                            validFlg = true;
                        }
                        else
                        {
                            if (TypeConvert.IsHexString(inputText) == true)
                            {
                                validFlg = true;

                            }

                        }

                        if (validFlg == false)
                        {
                            dgv[e.ColumnIndex, e.RowIndex].ErrorText = "Invalid Address.";

                        }
                        else
                        {
                            dgv[e.ColumnIndex, e.RowIndex].ErrorText = null;

                        }

                    }

                    break;

                case DgvRowName.Offset:
                    if ((Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == false) ||
                        (myComponents.CustomizingModeFlg == true))
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Offset].Value = inputText;
                        workaroundConditionCnt++;

                        if (TypeConvert.IsNumeric(inputText) == true)
                        {
                            dgv[e.ColumnIndex, e.RowIndex].ErrorText = null;

                        }
                        else if (TypeConvert.IsHexString(inputText) == true)
                        {
                            dgv[e.ColumnIndex, e.RowIndex].ErrorText = null;

                        }
                        else
                        {
                            dgv[e.ColumnIndex, e.RowIndex].ErrorText = "Invalid Value.";

                        }

                    }

                    break;

                case DgvRowName.Name:
                    if ((Convert.ToBoolean(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Check].Value) == false) ||
                        (myComponents.CustomizingModeFlg == true))
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Name].Value = inputText;
                        workaroundConditionCnt++;

                    }

                    break;

                case DgvRowName.Type:
                    dgv.Rows[e.RowIndex].Cells[columnIndex].Value = inputText;
                    workaroundConditionCnt++;

                    if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value != null)
                    {
                        type = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value.ToString();

                    }
                    else
                    {
                        type = numeralSystem.BIN;

                    }

                    if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value != null)
                    {
                        readText = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value.ToString();

                    }

                    if (dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value != null)
                    {
                        writeText = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value.ToString();

                    }

                    size = null;

                    if (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string))
                    {
                        size = "1";

                    }
                    else
                    {
                        size = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();

                    }

                    string readVal = TypeConvert.FromHexChars(type, int.Parse(size), readText);
                    string writeVal = TypeConvert.FromHexChars(type, int.Parse(size), writeText);

                    if (readVal != null)
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadValue].Value = readVal;

                    }
                    else
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.ReadText].Value = null;

                    }

                    if (writeVal != null)
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value = writeVal;

                    }
                    else
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value = null;

                    }

                    break;

                case DgvRowName.ReadValue:
                    //Ignore
                    break;

                case DgvRowName.WriteValue:
                    dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value = inputText;
                    workaroundConditionCnt++;

                    if (string.IsNullOrEmpty(inputText))
                    {
                        dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].ErrorText = null;

                    }
                    else if ((string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value as string)) ||
                        (string.IsNullOrEmpty(dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value as string)))
                    {
                        PutWarningMessage("Size or Type are invalid.");

                    }
                    else if (dgv[(int)DgvRowName.Size, e.RowIndex].ErrorText != "")
                    {
                        PutWarningMessage("Size is invalid.");

                    }
                    else
                    {
                        size = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Size].Value.ToString();
                        type = dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.Type].Value.ToString();

                        Exception ex_text = null;

                        writeText = TypeConvert.ToHexChars(type, int.Parse(size), inputText, out ex_text);

                        if (ex_text != null)
                        {
                            PutWarningMessage(ex_text.Message);
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].ErrorText = "Invalid Value.";

                        }
                        else if (writeText == null)
                        {
                            PutWarningMessage("Write data is invalid.");
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].ErrorText = "Invalid Value.";

                        }
                        else
                        {
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].ErrorText = null;

                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteText].Value = writeText;
                            string writeValue = TypeConvert.FromHexChars(type, int.Parse(size), writeText);
                            dgv.Rows[e.RowIndex].Cells[(int)DgvRowName.WriteValue].Value = writeValue;

                        }

                    }

                    break;

            }

            if (workaroundConditionCnt == 2)
            {
                //Workaround: Problem that a user set value in last row
                dataGridView.CurrentCellChanged += new System.EventHandler(dataGridView_CurrentCellChanged);
                dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellValueChanged);

            }

        }

        private void OnDeleteItemButtonPressed(object sender, EventArgs e)
        {
            Int32 rowValue = dataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            if (rowValue != 0)
            {
                dataGridView.Rows.RemoveAt(rowValue);
                this.dataGridView.ClearSelection();
            }
            else
            {
                PutWarningMessage("The first row can not be deleted.");

            }

        }

        private void OnInsertItemButtonPressed(object sender, EventArgs e)
        {
            Int32 rowValue = dataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataSetting factor = new DataSetting();
            factor.Type = numeralSystem.HEX;
            myComponents.ViewSettingList[pageValComboBox.SelectedIndex].DataSetting.Insert(rowValue + 1, factor);
            this.dataGridView.ClearSelection();

        }

        private void OnCopyItemButtonPressed(object sender, EventArgs e)
        {
            Int32 rowValue = dataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            DataSetting factor = new DataSetting(myComponents.ViewSettingList[pageValComboBox.SelectedIndex].DataSetting[rowValue]);
            factor.Group = null;
            myComponents.ViewSettingList[pageValComboBox.SelectedIndex].DataSetting.Insert(rowValue + 1, factor);
            this.dataGridView.ClearSelection();

        }

        private void OnDeletePageButtonPressed(object sender, EventArgs e)
        {
            var index = pageValComboBox.SelectedIndex;

            if (index != 0)
            {
                DialogResult result = MessageBox.Show("Do you want to delete this page?",
                                                        "Question",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Exclamation,
                                                        MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    this.dataGridView.DataSource = null;
                    myComponents.ViewSettingList.RemoveAt(index);
                    pageValComboBox.Items.RemoveAt(index);
                    pageValComboBox.SelectedIndex = index - 1;

                }

            }
            else
            {
                MessageBox.Show("Forbidden to delete first page.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

            }

        }

        private void OnInsertPageButtonPressed(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to insert a page next to this page?",
                            "Question",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                var index = pageValComboBox.SelectedIndex;
                string groupName = GROUP_TEMPORARY_TAG;

                var tmpVSettingFactor = new ViewSetting();

                for (int i = 0; i < COLUMN_NUM; i++)
                {
                    tmpVSettingFactor.DataSetting.Add(new DataSetting());
                }

                tmpVSettingFactor.DataSetting[0].Group = groupName;

                myComponents.ViewSettingList.Insert((index + 1), tmpVSettingFactor);

                pageValComboBox.Items.Insert((index + 1), groupName);

                pageValComboBox.SelectedIndex = index + 1;

            }

        }

        private void OnCopyPageButtonPressed(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to copy this page to next page?",
                            "Question",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                var index = pageValComboBox.SelectedIndex;
                string groupName = GROUP_TEMPORARY_TAG;

                var tmpVSettingFactor = new ViewSetting();

                foreach (var row in myComponents.ViewSettingList[index].DataSetting)
                {
                    DataSetting factor = new DataSetting(row);
                    tmpVSettingFactor.DataSetting.Add(factor);

                }

                tmpVSettingFactor.DataSetting[0].Group = groupName;

                myComponents.ViewSettingList.Insert((index + 1), tmpVSettingFactor);

                pageValComboBox.Items.Insert((index + 1), groupName);

                pageValComboBox.SelectedIndex = index + 1;

            }

        }

    }
}
