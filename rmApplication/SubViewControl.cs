using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace rmApplication
{
    public partial class SubViewControl : UserControl
    {
        public Configuration Config { get; set; }
        public bool IsCommunicationActive { get; private set; }
        public bool IsCustomizingMode { get; private set; }
        public bool IsRemote { get; private set; }
        public List<SymbolFactor> MapList { get; private set; }
        public List<ViewSetting> ViewSettingList { get; private set; }


        private DataGridViewTextBoxColumn groupColumn;
        private DataGridViewCheckBoxColumn checkColumn;
        private DataGridViewTextBoxColumn symbolColumn;
        private DataGridViewTextBoxColumn addressColumn;
        private DataGridViewTextBoxColumn offsetColumn;
        private DataGridViewTextBoxColumn sizeColumn;
        private DataGridViewTextBoxColumn nameColumn;
        private DataGridViewComboBoxColumn typeColumn;
        private DataGridViewTextBoxColumn readRawColumn;
        private DataGridViewTextBoxColumn readValueColumn;
        private DataGridViewTextBoxColumn writeRawColumn;
        private DataGridViewTextBoxColumn writeValueColumn;
        private DataGridViewButtonColumn writeButtonColumn;
        private DataGridViewTextBoxColumn descriptionColumn;

        private enum DgvRowName : int       // Column name of datagridview
        {
            Group = 0,
            CK,
            Symbol,
            Address,
            Offset,
            Size,
            Name,
            Type,
            ReadRaw,
            Read,
            WriteRaw,
            Write,
            WR,
            Description
        }

        private enum Area2DisplayMode
        {
            Time,
            Counts
        }

        private string ValidMapPath;
        private DateTime ValidMapLastWrittenDate;

        private int timeStep;

        private int logLength;
        private readonly int logLengthDefault = 1000;
        private readonly int logLengthMin = 10;
        private readonly int logLengthMax = 200000;

        private string targetVersionName;
        private int currentPageIndex;
        private AutoCompleteStringCollection autoCompleteSourceForSymbol;

        private DumpForm dumpFormInstance;
        private TerminalForm terminalFormInstance;

        private char logTextDelimiter = '\t';
        private string logHeader;

        private ConcurrentQueue<string> currentLog;
        private ConcurrentQueue<string> currentLogForRemote;

        private ContextMenuStrip contextMenuStrip;

        private PeriodAveraging periodAveraging;

        private bool isFormClosing;

        private Area2DisplayMode displayMode;

        private readonly string COMM_OPEN_TEXT = "Comm Open ";
        private readonly string COMM_CLOSE_TEXT = "Comm Close";

        private BusinessLogic logic;
        private RemoteControl remoteCtrl;

        public SubViewControl()
        {
            InitializeComponent();

            InitializeDataGridView();

            timeStepToolStripTextBox.Text = BusinessLogic.TimeStepDefault.ToString();
            logLengthToolStripTextBox.Text = logLengthDefault.ToString();

            mainTimer.Interval = 100;
            mainTimer.Enabled = true;

            Config = new Configuration();

            logic = new BusinessLogic();
            logic.TaskCompletaionFunctionCallback = TaskCompletaionFunction;
            logic.SerialCommunicationEmulationReceivedCallBack = TerminalReceived;

            remoteCtrl = new RemoteControl();
            remoteCtrl.TaskRequestFunctionCallback = TaskRequestFunction;

            IsCommunicationActive = false;
            IsCustomizingMode = false;

            IsRemote = false;

            timeStep = BusinessLogic.TimeStepDefault;
            logLength = logLengthDefault;

            currentPageIndex = 0;

            periodAveraging = new PeriodAveraging();

            isFormClosing = false;

            displayMode = Area2DisplayMode.Time;

            currentLog = new ConcurrentQueue<string>();
            currentLogForRemote = new ConcurrentQueue<string>();
        }

        private bool ValidateDumpConfigrations(string text, out BusinessLogic.DataParameter param)
        {
            param = new BusinessLogic.DataParameter();

            string addressText;
            string sizeText;

            var factors = text.Split(',');

            if (factors.Count() == 1)
            {
                if (!FindSymbolInfo(factors[0], out var info))
                    return false;

                addressText = info.Address;
                sizeText = info.Size;
            }
            else if (factors.Count() == 2)
            {
                addressText = factors[0];
                sizeText = factors[1];
            }
            else
            {
                return false;
            }

            if (string.IsNullOrEmpty(addressText))
                return false;

            if (!IsHexString(addressText))
                return false;

            UInt64 address = Convert.ToUInt64(addressText, 16);

            if (string.IsNullOrEmpty(sizeText))
                return false;

            if (!int.TryParse(sizeText, out var size))
                return false;

            param.Address = (uint)address;
            param.Size = (uint)size;

            return true;

        }

        private void TaskRequestFunction(RemoteControl.RequestTasks task, string message)
        {
            if (!IsCommunicationActive)
            {
                remoteCtrl.UpadateRequestedTaskResponse(task, false);
                return;
            }

            if (task == RemoteControl.RequestTasks.Setting)
            {
                if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                }
                else if (!LoadViewFile(message))
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                }
                else
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(message);
                    if (ViewFileName.GetName(fileName, out var tmp))
                        SetTargetVersionName(tmp.SoftwareVersion);

                    remoteCtrl.UpadateRequestedTaskResponse(task, true);
                }

                return;
            }

            if (task == RemoteControl.RequestTasks.Page)
            {
                if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                }
                else if (!ChangeViewPage(message))
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                }
                else
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, true);
                }

                return;
            }

            if (task == RemoteControl.RequestTasks.SymbolInfo)
            {
                if (!FindSymbolInfo(message, out var info))
                {
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                }
                else
                {
                    var text = info.Address + "," + info.Offset + "," + info.Size;
                    remoteCtrl.UpadateRequestedTaskResponse(task, true, text);
                }

                return;
            }

            switch (task)
            {
                case RemoteControl.RequestTasks.Open:
                    if (string.IsNullOrEmpty(message))
                    {
                        // Inherit the current settings
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                    }
                    else if (!Config.ValidateCommunicationResource(message, out var tmpConfig))
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    }
                    else if (!logic.UpdateResource(tmpConfig))
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    }
                    else
                    {
                        Config = tmpConfig;
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                    }
                    break;

                case RemoteControl.RequestTasks.Close:
                    logic.ClearWaitingTasks();
                    logic.CancelCurrentTask();
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Close);
                    break;

                case RemoteControl.RequestTasks.Initialize:
                    logic.ClearWaitingTasks();
                    logic.CancelCurrentTask();
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Initialize);
                    break;

                case RemoteControl.RequestTasks.Config:
                    if (UpdateConfigurationParameter(ViewSettingList[currentPageIndex].Settings, out var parameters))
                    {
                        logic.LogConfigParameter = parameters;
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Config);
                    }
                    else
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, true);
                    }
                    break;

                case RemoteControl.RequestTasks.TimeStep:
                    if (ChangeTimeStep(message))
                    {
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.TimeStep);
                    }
                    else
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, true);
                    }
                    break;

                case RemoteControl.RequestTasks.LogStart:
                    RefreshLogData();
                    UpdateInformation();

                    logic.ClearWaitingTasks();
                    logic.CancelCurrentTask();
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);
                    break;

                case RemoteControl.RequestTasks.LogStop:
                    logic.ClearWaitingTasks();
                    logic.CancelCurrentTask();
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.StopLog);
                    break;

                case RemoteControl.RequestTasks.LogRead:
                    if (currentLogForRemote.TryDequeue(out var text))
                        remoteCtrl.UpadateRequestedTaskResponse(task, true, text);
                    else
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    break;

                case RemoteControl.RequestTasks.LogWrite:
                    if (logic.TaskState != BusinessLogic.CommunicationTasks.Logging)
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    }
                    else if (!ValidateWriteInformation(message, out var wrParam))
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    }
                    else
                    {
                        logic.EditValue(wrParam);
                        remoteCtrl.UpadateRequestedTaskResponse(task, true);
                    }
                    break;

                case RemoteControl.RequestTasks.Dump:
                    if (!ValidateDumpConfigrations(message, out var param))
                    {
                        remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    }
                    else
                    {
                        logic.DumpConfigParameter = param;

                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Dump);

                    }
                    break;

                default:
                    remoteCtrl.UpadateRequestedTaskResponse(task, false);
                    break;
            }

        }

        private void TaskCompletaionFunction(BusinessLogic.CommunicationTasks task, BusinessLogic.TaskCompletionInformation info)
        {
            if (isFormClosing)
                return;

            switch (task)
            {
                case BusinessLogic.CommunicationTasks.Open:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Open, true, null);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Open, false, null);
                    }

                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to open a communication resource.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.Close:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Close, true);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Close, false);
                    }

                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to close a communication resource.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.Initialize:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                        {
                            var text = System.Text.Encoding.ASCII.GetString(info.Data);
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Initialize, true, text.TrimEnd());
                        }
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Initialize, false);
                    }

                    if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                    {
                        receivedVersionViewControl.TextBox = System.Text.Encoding.ASCII.GetString(info.Data);

                    }
                    else
                    {
                        if (!IsRemote)
                        {
                            if(info.EchoDetected)
                            {
                                string message = "The request was echoed back." + Environment.NewLine + "The Tx and Rx lines might be shorted together.";
                                string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                                MessageBox.Show(message + Environment.NewLine + dateTime,
                                                "Caution",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                            }
                            else
                            {
                                string message = "Failed to initialize.";
                                string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                                MessageBox.Show(message + Environment.NewLine + dateTime,
                                                "Caution",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Warning);
                            }

                        }

                    }

                    break;

                case BusinessLogic.CommunicationTasks.Config:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Config, true);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Config, false);
                    }

                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to config.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.TimeStep:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.TimeStep, true);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.TimeStep, false);
                    }

                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to change timestep.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.StartLog:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.LogStart, true);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.LogStart, false);
                    }

                    if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                    {
                        activityToolStripProgressBar.Style = ProgressBarStyle.Marquee;
                        activityToolStripProgressBar.MarqueeAnimationSpeed = 30;
                    }
                    else
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to start logging.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    periodAveraging.Clear();
                    break;

                case BusinessLogic.CommunicationTasks.Logging:

                    activityToolStripProgressBar.Style = ProgressBarStyle.Blocks;
                    activityToolStripProgressBar.MarqueeAnimationSpeed = 0;

                    if (info.Status == BusinessLogic.TaskCompletionStatus.Timeout)
                    {
                        if (!IsRemote)
                        {
                            string message = "Communication timeout.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.StopLog:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.LogStop, true);
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.LogStop, false);
                    }

                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                    {
                        if (!IsRemote)
                        {
                            string message = "Failed to stop logging.";
                            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
                            MessageBox.Show(message + Environment.NewLine + dateTime,
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                        }
                    }

                    break;

                case BusinessLogic.CommunicationTasks.Dump:
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                        {
                            string hex = BitConverter.ToString(info.Data).Replace("-", string.Empty);
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Dump, true, hex);
                        }
                        else
                            remoteCtrl.UpadateRequestedTaskResponse(RemoteControl.RequestTasks.Dump, false);
                    }

                    if (dumpFormInstance != null && !dumpFormInstance.IsDisposed)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            dumpFormInstance.UploadHexbox(info.Data.ToList());
                        else
                        {
                            var tmp = new byte[] { 0x46, 0x61, 0x69, 0x6c, 0x65, 0x64 };    // Failed
                            dumpFormInstance.UploadHexbox(tmp.ToList());
                        }

                    }

                    break;
            }

        }

        private void TerminalReceived(byte[] bytes)
        {
            if (terminalFormInstance != null && !terminalFormInstance.IsDisposed)
                terminalFormInstance.UploadReceivedBytes(bytes);

        }

        private void InitializeDataGridView()
        {
            this.groupColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.checkColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.symbolColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.addressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.offsetColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.readRawColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.readValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.writeRawColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.writeValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.writeButtonColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();

            this.mainDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.groupColumn,
            this.checkColumn,
            this.symbolColumn,
            this.addressColumn,
            this.offsetColumn,
            this.sizeColumn,
            this.nameColumn,
            this.typeColumn,
            this.readRawColumn,
            this.readValueColumn,
            this.writeRawColumn,
            this.writeValueColumn,
            this.writeButtonColumn,
            this.descriptionColumn});

            // 
            // GroupColumn
            // 
            this.groupColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Group.ToString();
            this.groupColumn.Frozen = true;
            this.groupColumn.HeaderText = DgvRowName.Group.ToString();
            this.groupColumn.Name = DgvRowName.Group.ToString();
            this.groupColumn.Visible = false;
            // 
            // CheckColumn
            // 
            this.checkColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Check.ToString();
            this.checkColumn.Frozen = true;
            this.checkColumn.HeaderText = DgvRowName.CK.ToString();
            this.checkColumn.Name = DgvRowName.CK.ToString();
            this.checkColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.checkColumn.Width = 38;
            // 
            // SymbolColumn
            // 
            this.symbolColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Symbol.ToString();
            this.symbolColumn.HeaderText = DgvRowName.Symbol.ToString();
            this.symbolColumn.Name = DgvRowName.Symbol.ToString();
            this.symbolColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // AddressColumn
            // 
            this.addressColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Address.ToString();
            this.addressColumn.HeaderText = DgvRowName.Address.ToString();
            this.addressColumn.Name = DgvRowName.Address.ToString();
            this.addressColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // OffsetColumn
            // 
            this.offsetColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Offset.ToString();
            this.offsetColumn.HeaderText = DgvRowName.Offset.ToString();
            this.offsetColumn.Name = DgvRowName.Offset.ToString();
            this.offsetColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.offsetColumn.Width = 58;
            // 
            // SizeColumn
            // 
            this.sizeColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Size.ToString();
            this.sizeColumn.HeaderText = DgvRowName.Size.ToString();
            this.sizeColumn.Name = DgvRowName.Size.ToString();
            this.sizeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.sizeColumn.Width = 58;
            // 
            // NameColumn
            // 
            this.nameColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Name.ToString();
            this.nameColumn.HeaderText = DgvRowName.Name.ToString();
            this.nameColumn.Name = DgvRowName.Name.ToString();
            // 
            // TypeColumn
            // 
            this.typeColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Type.ToString();
            this.typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.typeColumn.HeaderText = DgvRowName.Type.ToString();
            this.typeColumn.Name = DgvRowName.Type.ToString();
            this.typeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.typeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.typeColumn.Width = 58;

            System.Data.DataTable typeTable = new System.Data.DataTable("typeTable");
            typeTable.Columns.Add("Display", typeof(string));
            typeTable.Rows.Add(UserType.Hex);
            typeTable.Rows.Add(UserType.UsD);
            typeTable.Rows.Add(UserType.Dec);
            typeTable.Rows.Add(UserType.Bin);
            typeTable.Rows.Add(UserType.FLT);
            typeTable.Rows.Add(UserType.DBL);

            this.typeColumn.ValueType = typeof(string);
            this.typeColumn.ValueMember = "Display";
            this.typeColumn.DisplayMember = "Display";
            this.typeColumn.DataSource = typeTable;

            // 
            // ReadRawColumn
            // 
            this.readRawColumn.DataPropertyName = ViewSetting.DgvPropertyNames.ReadRaw.ToString();
            this.readRawColumn.HeaderText = DgvRowName.ReadRaw.ToString();
            this.readRawColumn.Name = DgvRowName.ReadRaw.ToString();
            this.readRawColumn.Visible = false;
            // 
            // ReadColumn
            // 
            this.readValueColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Read.ToString();
            this.readValueColumn.HeaderText = DgvRowName.Read.ToString();
            this.readValueColumn.Name = DgvRowName.Read.ToString();
            // 
            // WriteRawColumn
            // 
            this.writeRawColumn.DataPropertyName = ViewSetting.DgvPropertyNames.WriteRaw.ToString();
            this.writeRawColumn.HeaderText = DgvRowName.WriteRaw.ToString();
            this.writeRawColumn.Name = DgvRowName.WriteRaw.ToString();
            this.writeRawColumn.Visible = false;
            // 
            // WriteColumn
            // 
            this.writeValueColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Write.ToString();
            this.writeValueColumn.HeaderText = DgvRowName.Write.ToString();
            this.writeValueColumn.Name = DgvRowName.Write.ToString();
            // 
            // WriteButtonColumn
            // 
            this.writeButtonColumn.HeaderText = DgvRowName.WR.ToString();
            this.writeButtonColumn.Name = DgvRowName.WR.ToString();
            this.writeButtonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.writeButtonColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.writeButtonColumn.Width = 42;
            // 
            // DescriptionColumn
            // 
            this.descriptionColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Description.ToString();
            this.descriptionColumn.HeaderText = DgvRowName.Description.ToString();
            this.descriptionColumn.Name = DgvRowName.Description.ToString();
            this.descriptionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            mainDataGridView.AutoGenerateColumns = false;
            mainDataGridView.AllowUserToDeleteRows = false;

            mainDataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;

            //Context menu
            contextMenuStrip = new ContextMenuStrip(this.components);
            contextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new System.Drawing.Size(61, 4);

            contextMenuStrip.Items.Clear();
            contextMenuStrip.Items.Add("Delete this item", null, OnDeleteItemButtonPressed);
            contextMenuStrip.Items.Add("Insert an item", null, OnInsertItemButtonPressed);
            contextMenuStrip.Items.Add("Duplicate this item", null, OnDuplicateItemButtonPressed);
            contextMenuStrip.Items.Add("Delete this page", null, OnDeletePageButtonPressed);
            contextMenuStrip.Items.Add("Insert a page", null, OnInsertPageButtonPressed);
            contextMenuStrip.Items.Add("Duplicate this page", null, OnDuplicatePageButtonPressed);
        }

        public void RunRemoteMode()
        {
            remoteToolStripButton.PerformClick();
            commToolStripButton.PerformClick();
        }


        public void ClosingRoutine()
        {
            isFormClosing = true;

            logic.ClearWaitingTasks();
            logic.CancelCurrentTask();
            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Close);

            logic.Terminate();
            remoteCtrl.Terminate();

            mainTimer.Enabled = false;

        }

        public void SetTargetVersionName(string name)
        {
            targetVersionName = name;
            targetVersionViewControl.TextBox = targetVersionName;

        }

        public string GetTargetVersionName()
        {
            return targetVersionName;
        }

        public void LoadViewSettingFile(ViewSetting tmp)
        {
            ViewSettingList = new List<ViewSetting>();
            var view = new ViewSetting();
            var pageList = new List<string>();

            string previousGroupName = null;
            bool isFirstRow = true;
            bool isFailed = false;
            foreach (var setting in tmp.Settings)
            {
                if (isFirstRow == true)
                {
                    isFirstRow = false;

                    if (setting.Group == null)
                    {
                        isFailed = true;
                        break;

                    }

                    view.Settings.Add(setting);
                    pageList.Add(setting.Group);
                    previousGroupName = setting.Group;

                }
                else if ((!string.IsNullOrEmpty(setting.Group)) &&
                          (setting.Group != previousGroupName))
                {
                    ViewSettingList.Add(view);

                    view = new ViewSetting();
                    view.Settings.Add(setting);
                    pageList.Add(setting.Group);
                    previousGroupName = setting.Group;

                }
                else
                {
                    view.Settings.Add(setting);

                }

            }

            if (isFailed)
                return;

            if (view.Settings.Count > 0)
                ViewSettingList.Add(view);

            viewPageComboBox.SelectedIndexChanged -= new System.EventHandler(viewPageComboBox_SelectedIndexChanged);

            viewPageComboBox.Items.Clear();

            if (pageList.Count > 0)
            {
                foreach (var factor in pageList)
                    viewPageComboBox.Items.Add(factor);

                viewPageComboBox.SelectedIndex = 0;
                currentPageIndex = viewPageComboBox.SelectedIndex;

            }
            else
            {
                currentPageIndex = 0;
            }

            RefreshLogData();
            RefreshDataGridView();
            UpdateInformation();

            viewPageComboBox.SelectedIndexChanged += new System.EventHandler(viewPageComboBox_SelectedIndexChanged);

            MapList = new List<SymbolFactor>();
            ValidMapPath = string.Empty;
            ValidMapLastWrittenDate = DateTime.MinValue;
            autoCompleteSourceForSymbol = new AutoCompleteStringCollection();

            var uniqueSettings = tmp.Settings
                .GroupBy(item => item.Symbol)
                .Select(group => group.First())
                .ToList();

            foreach (var setting in uniqueSettings)
            {
                if (string.IsNullOrEmpty(setting.Symbol))
                    continue;

                var data = new SymbolFactor();

                data.Symbol = setting.Symbol;
                data.Address = setting.Address;
                data.Offset = setting.Offset;
                data.Size = setting.Size;

                MapList.Add(data);

                autoCompleteSourceForSymbol.Add(setting.Symbol);
            }

        }

        public bool LoadViewFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            bool isSuccess = false;

            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(path, Encoding.GetEncoding("utf-8")))
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ViewSetting));
                    var deserializedData = (ViewSetting)serializer.Deserialize(reader);

                    LoadViewSettingFile(deserializedData);

                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                if(!IsRemote)
                    MessageBox.Show(ex.Message);

            }

            return isSuccess;
        }

        public bool LoadMapFile(string path)
        {
            MapList = new List<SymbolFactor>();

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
                if (!IsRemote)
                    MessageBox.Show(ex.Message);

                return false;

            }

            var date = System.IO.File.GetLastWriteTime(path);

            bool isValid = ReadELFMap.Interpret(textArray, MapList);

            if (isValid == false)
                isValid = GCCLinkerMap.Interpret(textArray, MapList);

            if (isValid == false)
                isValid = RmAddressMap.Interpret(textArray, MapList);

            if (isValid == true)
            {
                ValidMapPath = path;
                ValidMapLastWrittenDate = date;

                List<string> symbolList = new List<string>();

                foreach (var factor in MapList)
                    symbolList.Add(factor.Symbol);

                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();
                autoCompleteSourceForSymbol.AddRange(symbolList.ToArray());

                if ((ViewSettingList != null) &&
                    (ViewSettingList.Count > 0))
                {
                    foreach (var item in ViewSettingList)
                    {
                        foreach (var setting in item.Settings)
                        {

                            if (string.IsNullOrEmpty(setting.Symbol))
                                continue;

                            if (!FindSymbolInfo(setting.Symbol.ToString(), out var result))
                            {
                                setting.Check = false;
                                setting.Address = null;
                            }
                            else
                            {
                                setting.Address = result.Address;

                                if (int.TryParse(result.Size, out var size))
                                {
                                    setting.Size = result.Size;
                                    setting.Offset = result.Offset;
                                }

                            }

                        }

                    }

                    mainDataGridView.Refresh();

                }

                if (string.IsNullOrEmpty(targetVersionName))
                {
                    targetVersionName = System.IO.Path.GetFileNameWithoutExtension(path);
                    targetVersionViewControl.TextBox = targetVersionName;

                }

            }
            else
            {
                ValidMapPath = null;
                ValidMapLastWrittenDate = DateTime.MinValue;
                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();

            }

            return true;
        }

        public void UpdateInformation()
        {
            UpdateInformation(displayMode);
        }

        private void UpdateInformation(Area2DisplayMode mode)
        {
            string communicationInfo = "-";

            if (ViewSettingList.Count > currentPageIndex)
            {
                int totalChecked = 0;
                int totalSize = 0;
                foreach (var setting in ViewSettingList[currentPageIndex].Settings)
                {
                    if (setting.Check && !string.IsNullOrEmpty(setting.Size))
                    {
                        totalChecked++;
                        totalSize += int.Parse(setting.Size);
                    }

                }

                if (totalSize > 0)
                {
                    if (mode == Area2DisplayMode.Counts)
                    {
                        communicationInfo = "Checked cells = " + totalChecked.ToString() + " / TotalSize(byte) = " + totalSize.ToString();
                    }
                    else
                    {
                        if (Config.BaudRate > 0)
                        {
                            // MaxSize: SlipCode(1byte) + (MSCnt(1byte) + payload(?byte) + crc(1byte)) * 2 + SlipCode(1byte)
                            // MinSize: SlipCode(1byte) +  MSCnt(1byte) + payload(?byte) + crc(1byte)      + SlipCode(1byte)
                            double frameMinSize = 1 + (1 + totalSize + 1) + 1;
                            double frameMaxSize = 1 + (1 + totalSize + 1) * 2 + 1;
                            double abyteTxTime = (1 / (double)Config.BaudRate) * 10;
                            double targetTxMinTime = frameMinSize * abyteTxTime * 1000;
                            double targetTxMaxTime = frameMaxSize * abyteTxTime * 1000;

                            communicationInfo =
                                "Target Tx Time =" + targetTxMinTime.ToString("F2") + " to " + targetTxMaxTime.ToString("F2") + "ms" + " / "
                                + Config.BaudRate.ToString() + "bps";

                            if (Config.CommMode == CommMainCtrl.CommunicationMode.Serial)
                            {
                                communicationInfo += " @ " + Config.SerialPortName.ToString();
                            }
                            else
                            {
                                communicationInfo += " @ " + Config.ClientAddress.ToString() + ":" + Config.ClientPort.ToString();
                            }

                            //+"MemoryType: " + Config.RmRange.ToString();
                        }

                    }

                }

            }

            area2ToolStripStatusLabel.Text = communicationInfo;

        }

        private bool ChangeViewPage(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return false;

            if (viewPageComboBox.Items.Count == 0)
                return false;

            int index = viewPageComboBox.FindStringExact(itemName);

            if (index < 0)
                return false;

            viewPageComboBox.SelectedIndex = index;

            return true;
        }

        private bool ChangeTimeStep(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (!int.TryParse(text, out var value))
            {
                timeStepToolStripTextBox.Text = timeStep.ToString();
                return false;
            }

            if (value < BusinessLogic.TimeStepMin)
                value = BusinessLogic.TimeStepMin;
            else if (value > BusinessLogic.TimeStepMax)
                value = BusinessLogic.TimeStepMax;

            logic.LogTimeStep = (uint)value;

            timeStep = value;
            timeStepToolStripTextBox.Text = value.ToString();
            this.ActiveControl = mainDataGridView;

            return true;
        }

        private bool ValidateWriteInformation(string text, out BusinessLogic.DataParameter result)
        {
            result = new BusinessLogic.DataParameter();

            var elements = text.Split(',');

            bool isSuccess = false;
            if (elements.Count() == 1)
            {
                // text = "TriggerON";
                var found = ViewSettingList[currentPageIndex].Settings.ToList().Find(key => key.Name == text);

                if (found != null)
                    isSuccess = ValidateWriteParameters(found.Address, found.Offset, found.Size, found.Type, found.Write, out result);

            }
            else if (elements.Count() == 3)
            {
                // text = "debugData.trigger,Hex,1";
                isSuccess = ValidateWriteParametersFromMap(elements[0], elements[1], elements[2], out result);
            }
            else if (elements.Count() == 4)
            {
                // text = "0x0000C8F2,2,Hex,1";
                isSuccess = ValidateWriteParameters(elements[0], elements[1], elements[2], elements[3], out result);
            }

            return isSuccess;
        }

        private bool FindSymbolInfo(string name, out SymbolFactor info)
        {
            info = new SymbolFactor();

            if ((MapList == null) ||
                (MapList.Count <= 0))
            {
                return false;
            }

            if(string.IsNullOrEmpty(name))
                return false;

            SymbolFactor found = MapList.Find(key => key.Symbol == name);
            if (found == null)
                return false;

            info = found;
            return true;
        }

        private bool ValidateWriteParametersFromMap(string symbol, string typeText, string writeText, out BusinessLogic.DataParameter result)
        {
            result = new BusinessLogic.DataParameter();

            if(!FindSymbolInfo(symbol, out var info))
                return false;

            return ValidateWriteParameters(info.Address, info.Offset, info.Size, typeText, writeText, out result);
        }

        private bool ValidateWriteParameters(string addressText, string offsetText, string sizeText, string typeText, string writeText, out BusinessLogic.DataParameter result)
        {
            result = new BusinessLogic.DataParameter();

            if (string.IsNullOrEmpty(addressText))
                return false;

            UInt64 address = Convert.ToUInt64(addressText, 16);

            bool isOffsetValid = false;
            UInt32 offset;
            if (UInt32.TryParse(offsetText, out offset))
            {
                isOffsetValid = true;
            }
            else if (IsHexString(offsetText))
            {
                isOffsetValid = true;
                offset = Convert.ToUInt32(offsetText, 16);
            }

            if (!isOffsetValid)
                return false;

            address += offset;

            if (address >= (UInt64)UInt32.MaxValue)
                return false;

            addressText = "0x" + address.ToString("X");
            return ValidateWriteParameters(addressText, sizeText, typeText, writeText, out result);
        }

        private bool ValidateWriteParameters(string addressText, string sizeText, string typeText, string writeText, out BusinessLogic.DataParameter result)
        {
            result = new BusinessLogic.DataParameter();

            int size = 1;
            if (!ValidateSize(sizeText, ref size))
                return false;

            if (!Enum.TryParse<UserType>(typeText, out var type))
                return false;

            if(type == UserType.FLT && size != 4)
                return false;

            if (type == UserType.DBL && size != 8)
                return false;

            if (!IsHexString(addressText))
                return false;

            UInt64 address = Convert.ToUInt64(addressText, 16);

            if (!UserUlong.TryParse(type, size, writeText, out var data))
                return false;

            result.Address = (uint)address;
            result.Size = (uint)size;
            result.Value = data;

            return true;
        }

        public void RefreshLogData()
        {
            periodAveraging.Clear();

            while (currentLog.Count != 0)
                currentLog.TryDequeue(out var tmp);

            while (currentLogForRemote.Count != 0)
                currentLogForRemote.TryDequeue(out var tmp);

            var text = new StringBuilder();

            if ((ViewSettingList == null) ||
                (ViewSettingList.Count <= 0))
            {
                return;
            }

            string note = "Start Logging time: " + DateTime.Now.ToString();

            text.AppendLine(note);

            string header = "1.Status" + logTextDelimiter + "2.OS Time" + logTextDelimiter + "3.Slave Time";

            foreach (var setting in ViewSettingList[currentPageIndex].Settings)
            {
                if (setting.Check == true &&
                    !string.IsNullOrEmpty(setting.Address))
                {
                    header += logTextDelimiter;
                    if (string.IsNullOrEmpty(setting.Name))
                    {
                        header += setting.Symbol;
                    }
                    else
                    {
                        header += setting.Name;
                    }

                }
            }

            text.Append(header);

            logHeader = text.ToString();
        }

        private void RefreshDataGridView()
        {
            if (ViewSettingList.Count <= currentPageIndex)
            {
                mainDataGridView.DataSource = new List<ViewSetting>();
            }
            else
            {
                int totalChecked = 0;
                int totalSize = 0;
                foreach (var setting in ViewSettingList[currentPageIndex].Settings)
                {
                    setting.ReadRaw = null;
                    setting.Read = null;
                    setting.WriteRaw = null;

                    int size = 1;
                    if (string.IsNullOrEmpty(setting.Size))
                    {
                        setting.Check = false;
                        setting.Size = "1";
                    }
                    else
                    {
                        if (!ValidateSize(setting.Size, ref size))
                            setting.Check = false;

                    }

                    if (string.IsNullOrEmpty(setting.Address))
                    {
                        setting.Check = false;
                        setting.Address = null;
                    }
                    else
                    {
                        if (!IsHexString(setting.Address))
                        {
                            setting.Check = false;
                            setting.Address = null;
                        }
                    }

                    if (string.IsNullOrEmpty(setting.Offset))
                    {
                        setting.Check = false;
                        setting.Offset = "0";
                    }
                    else
                    {
                        if (!UInt32.TryParse(setting.Offset, out var tmp) &&
                            !IsHexString(setting.Offset))
                        {
                            setting.Check = false;
                            setting.Offset = "0";
                        }

                    }

                    UserType type = UserType.Hex;
                    if (string.IsNullOrEmpty(setting.Type))
                    {
                        setting.Type = UserType.Hex.ToString();
                    }
                    else
                    {
                        if (!Enum.TryParse<UserType>(setting.Type, out type))
                        {
                            setting.Type = UserType.Hex.ToString();
                        }
                    }

                    if (!string.IsNullOrEmpty(setting.Write))
                    {
                        string inputText = setting.Write;

                        UInt64 result;
                        if (UserUlong.TryParse(type, size, inputText, out result))
                        {
                            setting.WriteRaw = result.ToString();
                            UserString.TryParse(type, size, result, out inputText);
                            setting.Write = inputText;

                        }
                        else
                        {
                            setting.Write = null;
                        }

                    }

                    if (setting.Check)
                    {
                        totalChecked++;
                        totalSize += size;
                    }

                    if ((totalSize > CommInstructions.MaxPayloadSize) ||
                        (totalChecked > CommInstructions.MaxElementNum))
                    {
                        setting.Check = false;
                        totalChecked--;
                        totalSize -= size;
                    }

                }

                mainDataGridView.DataSource = ViewSettingList[currentPageIndex].Settings;

            }

            mainDataGridView.Refresh();

        }

        private void viewPageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (viewPageComboBox.Items.Count == 0)
                return;

            currentPageIndex = viewPageComboBox.SelectedIndex;

            RefreshLogData();
            RefreshDataGridView();
            UpdateInformation();

            if (IsCommunicationActive)
            {
                logic.ClearWaitingTasks();
                logic.CancelCurrentTask();

                if (UpdateConfigurationParameter(ViewSettingList[currentPageIndex].Settings, out var parameters))
                {
                    logic.LogConfigParameter = parameters;
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Config);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);
                }

            }


        }

        private bool UpdateConfigurationParameter(BindingList<DataSetting> settings, out List<BusinessLogic.DataParameter> parameters)
        {
            parameters = new List<BusinessLogic.DataParameter>();

            bool isSuccess = true;
            foreach (var setting in settings)
            {
                if (setting.Check == true)
                {
                    //Add data;
                    var tmp = new BusinessLogic.DataParameter();

                    UInt64 address = 0;
                    uint offset = 0;
                    uint size = 0;
                    try
                    {
                        address = Convert.ToUInt64(setting.Address, 16);

                        if (!uint.TryParse(setting.Offset, out offset))
                        {
                            offset = Convert.ToUInt32(setting.Offset, 16);
                        }

                        address += offset;

                        if (address >= (UInt64)UInt32.MaxValue)
                            address = (UInt64)UInt32.MaxValue;

                        size = uint.Parse(setting.Size);

                    }
                    catch (Exception)
                    {
                        isSuccess = false;
                        break;
                    }

                    tmp.Address = (uint)address;
                    tmp.Size = size;
                    parameters.Add(tmp);

                }

            }

            if(parameters.Count <= 0)
                isSuccess = false;

            return isSuccess;
        }

        private void customizeToolStripButton_Click(object sender, EventArgs e)
        {
            if (IsRemote)
                return;

            if (IsCommunicationActive)
            {
                MessageBox.Show("Stop communication.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            if (!IsCustomizingMode)
            {
                IsCustomizingMode = true;

                customizeToolStripButton.Image = Properties.Resources.Complete_and_ok_green;
                mainDataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.Orange;
                this.groupColumn.Visible = true;
                targetVersionViewControl.TextEnabled = true;
                mainDataGridView.ContextMenuStrip = contextMenuStrip;
                mainDataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(mainDataGridView_MouseDown);

                var receivedVer = receivedVersionViewControl.TextBox;
                var targetVer = targetVersionViewControl.TextBox;

                if (!string.IsNullOrEmpty(receivedVer))
                {
                    if (string.IsNullOrEmpty(targetVer))
                        targetVersionViewControl.TextBox = receivedVer;
                    else
                    {
                        if (targetVer != receivedVer)
                        {
                            DialogResult result = MessageBox.Show("The target version name is unmatch the received version name.\n" +
                                                                  "Do you want to use the received name as the target name?",
                                            "Question",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Exclamation,
                                            MessageBoxDefaultButton.Button2);

                            if (result == DialogResult.Yes)
                                targetVersionViewControl.TextBox = receivedVer;

                        }

                    }

                }

                if (System.IO.File.Exists(ValidMapPath) != true)
                {
                    MessageBox.Show("The address map file is not found.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                    MapList = new List<SymbolFactor>();
                    ValidMapPath = null;
                    ValidMapLastWrittenDate = DateTime.MinValue;
                    autoCompleteSourceForSymbol = new AutoCompleteStringCollection();
                }
                else
                {
                    DateTime now = System.IO.File.GetLastWriteTime(ValidMapPath);

                    if (now <= ValidMapLastWrittenDate)
                        return;

                    DialogResult result = MessageBox.Show("The map file is updated.\nDo you want to reload Address in the DataGridView?",
                                    "Question",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Exclamation,
                                    MessageBoxDefaultButton.Button2);

                    if (result != DialogResult.Yes)
                        return;

                    if (!LoadMapFile(ValidMapPath))
                    {
                        MessageBox.Show("Can't read address map file",
                                            "Caution",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);

                    }

                }

            }
            else
            {
                IsCustomizingMode = false;

                customizeToolStripButton.Image = Properties.Resources.Complete_and_ok_gray;
                mainDataGridView.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.LightBlue;
                this.groupColumn.Visible = false;
                targetVersionViewControl.TextEnabled = false;
                mainDataGridView.ContextMenuStrip = null;
                mainDataGridView.MouseDown -= new System.Windows.Forms.MouseEventHandler(mainDataGridView_MouseDown);

                RefreshLogData();
                RefreshDataGridView();
                UpdateInformation();

            }

            targetVersionName = targetVersionViewControl.TextBox;

        }

        private void timeStepToolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;

            if (e.KeyChar != (char)Keys.Enter)
                return;

            ChangeTimeStep(timeStepToolStripTextBox.Text);

            if (IsCommunicationActive)
            {
                RefreshLogData();

                logic.ClearWaitingTasks();
                logic.CancelCurrentTask();
                logic.EnqueueTask(BusinessLogic.CommunicationTasks.TimeStep);
                logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);

            }

        }

        private void timeStepToolStripTextBox_Leave(object sender, EventArgs e)
        {
            timeStepToolStripTextBox.Text = timeStep.ToString();
        }

        private void logLengthToolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;

            if (e.KeyChar != (char)Keys.Enter)
                return;

            if (int.TryParse(logLengthToolStripTextBox.Text, out var length))
            {
                if (length < logLengthMin)
                    length = logLengthMin;
                else if (length > logLengthMax)
                    length = logLengthMax;

                logLength = length;
                logLengthToolStripTextBox.Text = length.ToString();
                this.ActiveControl = mainDataGridView;

            }

        }

        private void logLengthToolStripTextBox_Leave(object sender, EventArgs e)
        {
            logLengthToolStripTextBox.Text = logLength.ToString();
        }

        private void copyLogToolStripButton_Click(object sender, EventArgs e)
        {
            if ((logHeader == null) &&
                (currentLog == null))
                return;

            if (currentLog.Count() == 0)
                return;

            StringBuilder text = new StringBuilder();

            text.AppendLine(logHeader);

            foreach (var item in currentLog)
            {
                text.AppendLine(item);
            }

            Clipboard.SetText(text.ToString());

        }

        private void dumpToolStripButton_Click(object sender, EventArgs e)
        {
            if (dumpFormInstance == null || dumpFormInstance.IsDisposed)
            {
                dumpFormInstance = new DumpForm(MapList);
                dumpFormInstance.RequestFunctionCallback = RequestFunction;
                dumpFormInstance.Show();
            }

            dumpFormInstance.Activate();
        }

        public void RequestFunction(BusinessLogic.DataParameter param)
        {
            logic.DumpConfigParameter = param;
            logic.ClearWaitingTasks();
            logic.CancelCurrentTask();
            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Dump);
            logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);
        }

        private void terminalToolStripButton_Click(object sender, EventArgs e)
        {
            if (terminalFormInstance == null || terminalFormInstance.IsDisposed)
            {
                terminalFormInstance = new TerminalForm();
                terminalFormInstance.SendDataFunctionCallback = SendDataFunction;
                terminalFormInstance.Show();
            }

            terminalFormInstance.Activate();
        }

        public void SendDataFunction(byte[] data)
        {
            logic.SendDataUsingSerialCommunicationEmulation(data);
        }

        private async void remoteToolStripButton_Click(object sender, EventArgs e)
        {
            if (IsRemote)
            {
                remoteCtrl.Terminate();

            }
            else
            {
                if (IsCustomizingMode)
                    customizeToolStripButton.PerformClick();

                IsRemote = true;
                remoteToolStripButton.Text = "LOCAL";

                area1ToolStripStatusLabel.Text = "Remote @ " + Config.ServerAddress.ToString() + " - " + Config.ServerPort.ToString();

                await remoteCtrl.RunAsync(Config.ServerAddress, Config.ServerPort);

                if (isFormClosing)
                    return;

                area1ToolStripStatusLabel.Text = "-";

                remoteToolStripButton.Text = "REMOTE";
                IsRemote = false;

            }

        }

        private async void commToolStripButton_Click(object sender, EventArgs e)
        {
            if (IsCustomizingMode)
            {
                MessageBox.Show("Quit custmizing mode.",
                                    "Caution",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                return;
            }

            if (IsCommunicationActive)
            {
                logic.ClearWaitingTasks();
                logic.CancelCurrentTask();
                logic.EnqueueTask(BusinessLogic.CommunicationTasks.StopLog);
                logic.EnqueueTask(BusinessLogic.CommunicationTasks.Close);

                logic.Terminate();
            }
            else
            {
                if (System.IO.File.Exists(ValidMapPath) == true)
                {
                    DateTime now = System.IO.File.GetLastWriteTime(ValidMapPath);

                    if (now > ValidMapLastWrittenDate)
                    {
                        DialogResult result = MessageBox.Show("The map file is updated.\nDo you want to reload Address in the DataGridView?",
                                                                "Question",
                                                                MessageBoxButtons.YesNo,
                                                                MessageBoxIcon.Exclamation,
                                                                MessageBoxDefaultButton.Button2);

                        if (result == DialogResult.Yes)
                        {
                            if (LoadMapFile(ValidMapPath) == false)
                            {
                                MessageBox.Show("Can't read address map file",
                                                    "Caution",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Warning);

                            }

                        }

                    }

                }

                IsCommunicationActive = true;

                RefreshLogData();

                receivedVersionViewControl.TextBox = string.Empty;

                commToolStripButton.Image = Properties.Resources.FlagThread_red;
                commToolStripButton.Text = COMM_CLOSE_TEXT;

                logic.ClearWaitingTasks();
                if (UpdateConfigurationParameter(ViewSettingList[currentPageIndex].Settings, out var parameters))
                {
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Initialize);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Config);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.TimeStep);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);

                    logic.LogTimeStep = (uint)timeStep;
                    logic.UpdateResource(Config);

                    logic.LogConfigParameter = parameters;
                    await logic.RunAsync();

                }
                else
                {
                    await logic.RunAsync();
                }

                if (isFormClosing)
                    return;

                commToolStripButton.Image = Properties.Resources.FlagThread_white;
                commToolStripButton.Text = COMM_OPEN_TEXT;

                IsCommunicationActive = false;

            }

        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            string status = string.Empty;
            Queue<long> osTimeBuffer = new Queue<long>();
            long osTime = 0;
            long slvTime = 0;
            Queue<ulong> rawData = new Queue<ulong>();

            while (logic.GetLogData(out var logData))
            {
                string lineText = string.Empty;
                status = logData.Status;
                lineText += status + logTextDelimiter;
                osTime = logData.OsTime;
                lineText += ((double)osTime / 1000).ToString() + logTextDelimiter;
                slvTime = logData.SlvTime;
                lineText += ((double)slvTime / 1000).ToString();
                rawData = logData.RawData;

                osTimeBuffer.Enqueue(osTime);

                //Debug
                //System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", osTime, slvTime, status);

                int index = 0;
                foreach (var setting in ViewSettingList[currentPageIndex].Settings)
                {
                    if (rawData.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        if (setting.Check)
                        {
                            var value = rawData.Dequeue();

                            setting.ReadRaw = value.ToString();

                            if (UserString.TryParse(setting.Type, setting.Size, value, out var textValue))
                                mainDataGridView[DgvRowName.Read.ToString(), index].Value = textValue;
                            else
                                mainDataGridView[DgvRowName.Read.ToString(), index].Value = null;

                            lineText += logTextDelimiter;
                            lineText += textValue;

                        }

                    }

                    index++;

                }

                currentLog.Enqueue(lineText);
                while (currentLog.Count() > logLength)
                    currentLog.TryDequeue(out var dummy);

                if (IsRemote)
                {
                    currentLogForRemote.Enqueue(lineText);
                    while (currentLogForRemote.Count() > logLength)
                        currentLogForRemote.TryDequeue(out var dummy);
                }

            }

            if(logic.TaskState != BusinessLogic.CommunicationTasks.Logging)
            {
                periodAveraging.Clear();
            }
            else
            {
                if (osTimeBuffer.Count > 0)
                {
                    double avg = 0;
                    while (osTimeBuffer.Count != 0)
                        avg = periodAveraging.Calculate(osTimeBuffer.Dequeue());

                    rxPeriodToolStripStatusLabel.Text = "Avg: " + avg.ToString("0.0") + "ms";

                }

            }


        }

        private void mainDataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
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

        private void mainDataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if (e.Control is TextBox)
            {
                TextBox tb = (TextBox)e.Control;

                if (dgv.CurrentCell.OwningColumn.Name == DgvRowName.Symbol.ToString())
                {
                    tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

                    tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    tb.AutoCompleteCustomSource = autoCompleteSourceForSymbol;
                }
                else
                {
                    tb.AutoCompleteMode = AutoCompleteMode.None;
                }

            }

        }

        private void mainDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Delete) &&
                (IsCustomizingMode))
            {
                foreach (DataGridViewCell cell in mainDataGridView.SelectedCells)
                {
                    int tmp = cell.ColumnIndex;

                    if (tmp == (int)DgvRowName.Group)
                    {
                        tmp = (int)DgvRowName.Group + 1;

                    }

                    mainDataGridView[tmp, cell.RowIndex].Value = null;

                }

            }
        }

        private void mainDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int ckColumnIndex = dgv.Columns[DgvRowName.CK.ToString()].Index;
            int addressColumnIndex = dgv.Columns[DgvRowName.Address.ToString()].Index;
            int offsetColumnIndex = dgv.Columns[DgvRowName.Offset.ToString()].Index;
            int sizeColumnIndex = dgv.Columns[DgvRowName.Size.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvRowName.Type.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvRowName.Write.ToString()].Index;
            int wrColumnIndex = dgv.Columns[DgvRowName.WR.ToString()].Index;

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;
            }

            if (e.ColumnIndex == ckColumnIndex)
            {
                if (IsRemote)
                    return;

                bool isUpdate = false;
                if (Convert.ToBoolean(dgv[ckColumnIndex, e.RowIndex].Value) == true)
                {
                    isUpdate = true;
                    dgv[ckColumnIndex, e.RowIndex].Value = false;

                }
                else
                {
                    if ((string.IsNullOrEmpty(dgv[addressColumnIndex, e.RowIndex].Value as string)) ||
                        (string.IsNullOrEmpty(dgv[offsetColumnIndex, e.RowIndex].Value as string)) ||
                        (string.IsNullOrEmpty(dgv[sizeColumnIndex, e.RowIndex].Value as string)))
                    {
                        
                    }
                    else
                    {
                        int totalChecked = 0;
                        int totalSize = 0;
                        int size = 1;
                        foreach(DataGridViewRow item in dgv.Rows)
                        {
                            // Correct current configuration
                            if (Convert.ToBoolean(item.Cells[ckColumnIndex].Value))
                            {
                                totalChecked++;
                                if (ValidateSize(item.Cells[sizeColumnIndex].Value.ToString(), ref size))
                                {
                                    totalSize += size;
                                }
                                else
                                {
                                    isUpdate = true;
                                    item.Cells[ckColumnIndex].Value = false;
                                }

                            }

                        }

                        bool isSizeOK = false;
                        if (!string.IsNullOrEmpty(dgv[sizeColumnIndex, e.RowIndex].Value as string))
                        {
                            size = 1;
                            var sizeObj = dgv[sizeColumnIndex, e.RowIndex].Value;
                            if (ValidateSize(sizeObj.ToString(), ref size))
                            {
                                isSizeOK = true;
                                totalChecked++;
                                totalSize += size;
                            }

                        }

                        if ( isSizeOK &&
                            (totalChecked <= CommInstructions.MaxElementNum) &&
                            (totalSize <= CommInstructions.MaxPayloadSize) )
                        {
                            isUpdate = true;
                            dgv[ckColumnIndex, e.RowIndex].Value = true;

                        }

                    }

                }

                if(isUpdate)
                {
                    RefreshLogData();
                    UpdateInformation();

                    if (IsCommunicationActive)
                    {
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();

                        if (UpdateConfigurationParameter(ViewSettingList[currentPageIndex].Settings, out var parameters))
                        {
                            logic.LogConfigParameter = parameters;
                            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Config);
                            logic.EnqueueTask(BusinessLogic.CommunicationTasks.StartLog);
                            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Logging);
                        }

                    }

                }

            }
            else if (e.ColumnIndex == wrColumnIndex)
            {
                if ((string.IsNullOrEmpty(dgv[addressColumnIndex, e.RowIndex].Value as string)) ||
                    (string.IsNullOrEmpty(dgv[offsetColumnIndex, e.RowIndex].Value as string)) ||
                    (string.IsNullOrEmpty(dgv[sizeColumnIndex, e.RowIndex].Value as string)) ||
                    (string.IsNullOrEmpty(dgv[typeColumnIndex, e.RowIndex].Value as string)) ||
                    (string.IsNullOrEmpty(dgv[writeColumnIndex, e.RowIndex].Value as string)))
                {
                    return;
                }

                var addressObj = dgv[addressColumnIndex, e.RowIndex].Value;
                var offsetObj = dgv[offsetColumnIndex, e.RowIndex].Value;
                var sizeObj = dgv[sizeColumnIndex, e.RowIndex].Value;
                var typeObj = dgv[typeColumnIndex, e.RowIndex].Value;
                var writeObj = dgv[writeColumnIndex, e.RowIndex].Value;

                if(ValidateWriteParameters(addressObj.ToString(), offsetObj.ToString(), sizeObj.ToString(), typeObj.ToString(), writeObj.ToString(), out var param))
                    logic.EditValue(param);

            }


        }

        private string cellValueBuffer;
        private void mainDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int ckColumnIndex = dgv.Columns[DgvRowName.CK.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvRowName.Type.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvRowName.Write.ToString()].Index;
            int descriptionColumnIndex = dgv.Columns[DgvRowName.Description.ToString()].Index;

            dgv.CancelEdit();

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;

            }

            cellValueBuffer = null;

            if (IsCustomizingMode)
            {
                cellValueBuffer = e.FormattedValue.ToString();
                return;
            }

            if (e.ColumnIndex == typeColumnIndex)
            {
                if (!IsRemote)
                    cellValueBuffer = e.FormattedValue.ToString();
            }
            else if ((e.ColumnIndex == writeColumnIndex) ||
                     (e.ColumnIndex == descriptionColumnIndex))
            {
                cellValueBuffer = e.FormattedValue.ToString();
            }
            else
            {
                if (!Convert.ToBoolean(dgv[ckColumnIndex, e.RowIndex].Value))
                    cellValueBuffer = e.FormattedValue.ToString();
            }

        }

        private void mainDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int groupColumnIndex = dgv.Columns[DgvRowName.Group.ToString()].Index;
            int symbolColumnIndex = dgv.Columns[DgvRowName.Symbol.ToString()].Index;
            int addressColumnIndex = dgv.Columns[DgvRowName.Address.ToString()].Index;
            int offsetColumnIndex = dgv.Columns[DgvRowName.Offset.ToString()].Index;
            int sizeColumnIndex = dgv.Columns[DgvRowName.Size.ToString()].Index;
            int nameColumnIndex = dgv.Columns[DgvRowName.Name.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvRowName.Type.ToString()].Index;
            int readrawColumnIndex = dgv.Columns[DgvRowName.ReadRaw.ToString()].Index;
            int readColumnIndex = dgv.Columns[DgvRowName.Read.ToString()].Index;
            int writerawColumnIndex = dgv.Columns[DgvRowName.WriteRaw.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvRowName.Write.ToString()].Index;
            int descriptionColumnIndex = dgv.Columns[DgvRowName.Description.ToString()].Index;

            if ((e.ColumnIndex < 0) ||
                (e.RowIndex < 0))
            {
                return;
            }

            if (cellValueBuffer == null)
            {
                return;
            }

            if ((string.IsNullOrEmpty(dgv[e.ColumnIndex, e.RowIndex].Value as string)) &&
                (string.IsNullOrEmpty(cellValueBuffer)))
            {
                return;
            }

            var inputText = cellValueBuffer;

            if (e.ColumnIndex == groupColumnIndex)
            {
                if ((e.RowIndex == 0) &&
                    (string.IsNullOrEmpty(inputText) == false))
                {
                    dgv[groupColumnIndex, e.RowIndex].Value = inputText;
                    viewPageComboBox.SelectedIndexChanged -= new System.EventHandler(viewPageComboBox_SelectedIndexChanged);
                    viewPageComboBox.Items[currentPageIndex] = inputText;
                    viewPageComboBox.SelectedIndexChanged += new System.EventHandler(viewPageComboBox_SelectedIndexChanged);
                }

            }
            if (e.ColumnIndex == sizeColumnIndex)
            {
                //Clear Read/Write Values due to unmatch current values
                dgv[readrawColumnIndex, e.RowIndex].Value = null;
                dgv[readColumnIndex, e.RowIndex].Value = null;
                dgv[writerawColumnIndex, e.RowIndex].Value = null;
                dgv[writeColumnIndex, e.RowIndex].Value = null;

                if (int.TryParse(inputText, out var size))
                {
                    dgv[e.ColumnIndex, e.RowIndex].Value = size;
                }
                else
                {
                    dgv[e.ColumnIndex, e.RowIndex].Value = 1;
                }

            }
            else if (e.ColumnIndex == symbolColumnIndex)
            {
                dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

                if (FindSymbolInfo(inputText, out var result))
                {
                    if (int.TryParse(result.Size, out var size))
                    {
                        dgv[sizeColumnIndex, e.RowIndex].Value = size;
                        dgv[offsetColumnIndex, e.RowIndex].Value = result.Offset;
                    }

                    dgv[addressColumnIndex, e.RowIndex].Value = result.Address;
                }
                else
                {
                    dgv[addressColumnIndex, e.RowIndex].Value = null;
                }
            }
            else if (e.ColumnIndex == addressColumnIndex)
            {
                if (IsHexString(inputText))
                {
                    dgv[addressColumnIndex, e.RowIndex].Value = inputText;
                }
                else
                {
                    dgv[addressColumnIndex, e.RowIndex].Value = null;
                }

            }
            else if (e.ColumnIndex == offsetColumnIndex)
            {
                if (UInt32.TryParse(inputText, out var tmp))
                {
                    dgv[e.ColumnIndex, e.RowIndex].Value = tmp;
                }
                else if (IsHexString(inputText))
                {
                    dgv[e.ColumnIndex, e.RowIndex].Value = inputText;
                }
                else
                {
                    dgv[e.ColumnIndex, e.RowIndex].Value = 0;
                }

            }
            else if (e.ColumnIndex == nameColumnIndex)
            {
                dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

            }
            else if (e.ColumnIndex == typeColumnIndex)
            {
                UserType type;
                if (string.IsNullOrEmpty(inputText))
                {
                    type = UserType.Hex;
                }
                else
                {
                    type = (UserType)Enum.Parse(typeof(UserType), inputText);
                }

                inputText = type.ToString();

                if (!string.IsNullOrEmpty(dgv[sizeColumnIndex, e.RowIndex].Value as string)){
                    int size = 1;
                    if (!ValidateSize(dgv[sizeColumnIndex, e.RowIndex].Value.ToString(), ref size))
                    {
                        //ignore
                    }
                    else if (size != 4 && type == UserType.FLT)
                    {
                        //ignore
                    }
                    else if (size != 8 && type == UserType.DBL)
                    {
                        //ignore
                    }
                    else
                    {
                        dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

                        string raw;

                        if (dgv[readrawColumnIndex, e.RowIndex].Value != null)
                        {
                            raw = dgv[readrawColumnIndex, e.RowIndex].Value.ToString();
                            if (ulong.TryParse(raw, out var result))
                            {
                                if (UserString.TryParse(type, size, result, out var value))
                                    dgv[readColumnIndex, e.RowIndex].Value = value;
                                else
                                    dgv[readColumnIndex, e.RowIndex].Value = null;

                            }

                        }

                        if (dgv[writerawColumnIndex, e.RowIndex].Value != null)
                        {
                            raw = dgv[writerawColumnIndex, e.RowIndex].Value.ToString();
                            if (ulong.TryParse(raw, out var result))
                            {
                                if (UserString.TryParse(type, size, result, out var value))
                                    dgv[writeColumnIndex, e.RowIndex].Value = value;
                                else
                                    dgv[writeColumnIndex, e.RowIndex].Value = null;

                            }

                        }

                    }
                }

            }
            else if (e.ColumnIndex == readrawColumnIndex)
            {
                //Debug
                dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

            }
            else if (e.ColumnIndex == writeColumnIndex)
            {
                if(!string.IsNullOrEmpty(inputText))
                {
                    try
                    {
                        int size = int.Parse(dgv[sizeColumnIndex, e.RowIndex].Value.ToString());
                        UserType type = (UserType)Enum.Parse(typeof(UserType), dgv[typeColumnIndex, e.RowIndex].Value.ToString());

                        ulong result;
                        if (UserUlong.TryParse(type, size, inputText, out result))
                        {
                            dgv[writerawColumnIndex, e.RowIndex].Value = result;
                            UserString.TryParse(type, size, result, out inputText);

                        }
                        else
                        {
                            if (dgv[writerawColumnIndex, e.RowIndex].Value == null)
                            {
                                inputText = string.Empty;
                            }
                            else
                            {
                                result = ulong.Parse(dgv[writerawColumnIndex, e.RowIndex].Value.ToString());
                                UserString.TryParse(type, size, result, out inputText);
                            }
                        }

                    }
                    catch
                    {
                        inputText = null;
                    }

                }

                dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

            }
            else if (e.ColumnIndex == descriptionColumnIndex)
            {
                dgv[e.ColumnIndex, e.RowIndex].Value = inputText;

            }

        }

        private bool ValidateSize(string value, ref int result)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            bool isValid = false;
            if (uint.TryParse(value, out var tmp) == true)
            {
                if ((tmp == 1) ||
                    (tmp == 2) ||
                    (tmp == 4) ||
                    (tmp == 8))
                {
                    result = (int)tmp;
                    isValid = true;
                }

            }

            return isValid;
        }

        private bool IsHexString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (Regex.IsMatch(text, @"^(0[xX]){1}[A-Fa-f0-9]+$"))
                return true;
            else
                return false;
        }

        private void mainDataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = mainDataGridView.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    mainDataGridView.ClearSelection();
                    mainDataGridView.Rows[hitTest.RowIndex].Selected = true;

                }

            }

        }

        private void OnDeleteItemButtonPressed(object sender, EventArgs e)
        {
            var rowValue = mainDataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            if (rowValue == -1)
                return;

            if(mainDataGridView.Rows.Count == 1)
            {
                OnDeletePageButtonPressed(sender, e);
            }
            else
            {
                if (rowValue == 0)
                {
                    var tmpGroupName = mainDataGridView[DgvRowName.Group.ToString(), rowValue].Value.ToString();
                    mainDataGridView[DgvRowName.Group.ToString(), (rowValue+1)].Value = tmpGroupName;
                }

                mainDataGridView.Rows.RemoveAt(rowValue);
                this.mainDataGridView.ClearSelection();
            }

        }

        private void OnInsertItemButtonPressed(object sender, EventArgs e)
        {
            if (ViewSettingList.Count <= 0)
                return;

            Int32 rowValue = mainDataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            if (rowValue == -1)
                return;

            DataSetting setting = new DataSetting();

            if (rowValue == 0)
            {
                setting.Group = ViewSettingList[currentPageIndex].Settings[0].Group;
                ViewSettingList[currentPageIndex].Settings[0].Group = string.Empty;
            }

            ViewSettingList[currentPageIndex].Settings.Insert(rowValue, setting);
            this.mainDataGridView.ClearSelection();

        }

        private void OnDuplicateItemButtonPressed(object sender, EventArgs e)
        {
            if (ViewSettingList.Count <= 0)
                return;

            var rowValue = mainDataGridView.Rows.GetFirstRow(DataGridViewElementStates.Selected);

            if (rowValue == -1)
                return;

            DataSetting setting = new DataSetting(ViewSettingList[currentPageIndex].Settings[rowValue]);
            setting.Group = null;
            ViewSettingList[currentPageIndex].Settings.Insert(rowValue + 1, setting);
            this.mainDataGridView.ClearSelection();

        }

        private void OnDeletePageButtonPressed(object sender, EventArgs e)
        {
            if ((viewPageComboBox.Items.Count == 1) &&
                (viewPageComboBox.SelectedIndex == -0))
            {
                MessageBox.Show("Can not delete.",
                "Caution",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

                return;
            }

            var index = viewPageComboBox.SelectedIndex;

            if (index != -1)
            {
                DialogResult result = MessageBox.Show("Do you want to delete this page?",
                                                        "Question",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Exclamation,
                                                        MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    ViewSettingList.RemoveAt(index);
                    viewPageComboBox.Items.RemoveAt(index);

                    if (index == 0)
                        viewPageComboBox.SelectedIndex = 0;
                    else
                        viewPageComboBox.SelectedIndex = index - 1;

                }

            }

        }

        private void OnInsertPageButtonPressed(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to insert a page?",
                            "Question",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                var index = viewPageComboBox.SelectedIndex;
                string groupName = "x:Sample";

                var view = new ViewSetting();

                for (int i = 0; i < 16; i++)
                {
                    view.Settings.Add(new DataSetting());
                }

                view.Settings[0].Group = groupName;

                ViewSettingList.Insert((index + 1), view);

                viewPageComboBox.Items.Insert((index + 1), groupName);

                viewPageComboBox.SelectedIndex = index + 1;

            }

        }

        private void OnDuplicatePageButtonPressed(object sender, EventArgs e)
        {
            if (ViewSettingList.Count <= 0)
                return;

            DialogResult result = MessageBox.Show("Do you want to duplicate this page?",
                            "Question",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                var index = viewPageComboBox.SelectedIndex;
                string groupName = "x:Sample";

                var view = new ViewSetting();

                foreach (var setting in ViewSettingList[index].Settings)
                {
                    DataSetting factor = new DataSetting(setting);
                    view.Settings.Add(factor);

                }

                view.Settings[0].Group = groupName;

                ViewSettingList.Insert((index + 1), view);

                viewPageComboBox.Items.Insert((index + 1), groupName);

                viewPageComboBox.SelectedIndex = index + 1;

            }

        }

        private void area2ToolStripStatusLabel_Click(object sender, EventArgs e)
        {
            if(displayMode == Area2DisplayMode.Counts)
                displayMode = Area2DisplayMode.Time;
            else
                displayMode = Area2DisplayMode.Counts;

            UpdateInformation(displayMode);

        }

    }
}
