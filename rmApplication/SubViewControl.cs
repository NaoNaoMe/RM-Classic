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

namespace rmApplication
{
    public partial class SubViewControl : UserControl
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Configuration Config { get; set; }
        public bool IsCommunicationActive { get; private set; }
        public bool IsCustomizingMode { get; private set; }
        public bool IsRemote { get; private set; }
        private bool exitOnRemoteBindFailure;
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

        private enum DgvColumnName : int
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
        private string ValidViewPath;

        // All user dialogs go through this so remote/headless sessions never block.
        // Swapped to HeadlessInteraction while remote mode is active (see remoteToolStripButton_Click).
        private IUserInteraction ui = new GuiInteraction();

        private int timeStep;

        private int logLength;
        private readonly int logLengthDefault = 1000;
        private readonly int logLengthMin = 1;
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
            logic.TaskCompletionFunctionCallback = TaskCompletionFunction;
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

        private bool ValidateDumpConfigurations(string text, out BusinessLogic.DataParameter param)
        {
            param = new BusinessLogic.DataParameter();

            string addressText;
            string sizeText;

            var factors = text.Split(',');

            if (factors.Count() == 1)
            {
                if (!FindSymbolInfo(factors[0], out var info))
                {
                    var found = ViewSettingList[currentPageIndex].Settings.ToList().Find(key => key.Symbol == factors[0]);
                    if (found != null)
                    {
                        info.Address = found.Address;
                        info.Size = found.Size;
                    }
                    else
                    {
                        return false;
                    }

                }

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
            // Self-description commands answer regardless of communication state.
            switch (task)
            {
                case RemoteControl.RequestTasks.Help:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, RemoteControl.BuildHelpJson());
                    return;
                case RemoteControl.RequestTasks.Status:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildStatusJson());
                    return;
                case RemoteControl.RequestTasks.Symbols:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildSymbolsJson());
                    return;
                case RemoteControl.RequestTasks.LogHeader:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildLogHeaderJson());
                    return;
                case RemoteControl.RequestTasks.View:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildViewJson());
                    return;
                case RemoteControl.RequestTasks.DownloadViewData:
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildViewDataBase64());
                    return;
                default:
                    break;
            }

            if (!IsCommunicationActive)
            {
                remoteCtrl.UpdateRequestedTaskResponse(task, false, "communication not active");
                return;
            }

            if (task == RemoteControl.RequestTasks.OpenView)
            {
                if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "rejected while logging");
                }
                else if (!System.IO.File.Exists(message))
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, FileNotFoundReason(message));
                }
                else if (!LoadViewFile(message))
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "cannot read or parse the rmxml");
                }
                else
                {
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(message);
                    if (ViewFileName.GetName(fileName, out var tmp))
                        SetTargetVersionName(tmp.SoftwareVersion);

                    remoteCtrl.UpdateRequestedTaskResponse(task, true);
                }

                return;
            }

            if (task == RemoteControl.RequestTasks.UploadViewData)
            {
                string xml = TryDecodeBase64Utf8(message);

                if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "rejected while logging");
                else if (xml == null)
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "invalid base64");
                else if (!LoadViewFromXmlString(xml))
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "cannot parse the rmxml");
                else
                    remoteCtrl.UpdateRequestedTaskResponse(task, true);

                return;
            }

            if (task == RemoteControl.RequestTasks.OpenMap)
            {
                // Load a map by path: sets ValidMapPath so the file-mtime auto-resync applies.
                if (!System.IO.File.Exists(message))
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, FileNotFoundReason(message));
                else if (LoadMapFile(message))
                    remoteCtrl.UpdateRequestedTaskResponse(task, true);
                else
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "cannot read or parse the map");

                return;
            }

            if (task == RemoteControl.RequestTasks.UploadMapData)
            {
                string text = TryDecodeBase64Utf8(message);
                if (text == null)
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "invalid base64");
                else if (!LoadMapFromText(text))
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "cannot parse the map");
                else
                    remoteCtrl.UpdateRequestedTaskResponse(task, true);

                return;
            }

            if (task == RemoteControl.RequestTasks.ResyncMap)
            {
                // Explicit, reportable substitute for the GUI's automatic resync-on-Comm-Open,
                // which is suppressed while IsRemote (see commToolStripButton_Click).
                if (MapList == null || MapList.Count == 0)
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "no map loaded");
                }
                else
                {
                    var (updated, unresolved) = ResyncViewAddressesFromMap();
                    remoteCtrl.UpdateRequestedTaskResponse(task, true, BuildResyncMapJson(updated, unresolved));
                }

                return;
            }

            if (task == RemoteControl.RequestTasks.Page)
            {
                if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "rejected while logging");
                }
                else if (!ChangeViewPageForRemote(message))
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "unknown page name");
                }
                else
                {
                    remoteCtrl.UpdateRequestedTaskResponse(task, true);
                }

                return;
            }

            if (task == RemoteControl.RequestTasks.SymbolInfo)
            {
                if (!FindSymbolInfo(message, out var info))
                {
                    var found = ViewSettingList[currentPageIndex].Settings.ToList().Find(key => key.Symbol == message);
                    if (found != null)
                    {
                        info.Address = found.Address;
                        info.Size = found.Size;
                    }
                    else
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "symbol not found");
                        return;
                    }

                }

                var text = info.Address + "," + info.Offset + "," + info.Size;
                remoteCtrl.UpdateRequestedTaskResponse(task, true, text);

                return;
            }

            switch (task)
            {
                case RemoteControl.RequestTasks.Connect:
                    if (string.IsNullOrEmpty(message))
                    {
                        // Inherit the current settings
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                    }
                    else if (!Config.ValidateCommunicationResource(message, out var tmpConfig))
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false,
                            "invalid open parameters (expected <pass>,Serial,<Byte2|Byte4>,<baud>,<com> "
                            + "or <pass>,LocalNet,<Byte2|Byte4>,<baud>,<ip>,<port>)");
                    }
                    else if (!logic.UpdateResource(tmpConfig))
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "cannot open the comm resource");
                    }
                    else
                    {
                        Config = tmpConfig;
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                    }
                    break;

                case RemoteControl.RequestTasks.Disconnect:
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
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "no loggable variables on the current page");
                    }
                    break;

                case RemoteControl.RequestTasks.TimeStep:
                    if (!TryValidateTimeStep(message, out var tsValue, out var tsReason))
                    {
                        // Out-of-range is rejected (not silently clamped) so a remote caller
                        // is not surprised; the reason carries the valid range.
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, tsReason);
                    }
                    else
                    {
                        ApplyTimeStep(tsValue);
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                        logic.EnqueueTask(BusinessLogic.CommunicationTasks.TimeStep);
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
                        remoteCtrl.UpdateRequestedTaskResponse(task, true, text);
                    else
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "no buffered log data");
                    break;

                case RemoteControl.RequestTasks.LogReadAll:
                    {
                        // Drain every queued row into one response (rows joined by '\n';
                        // rows never contain CRLF, so the protocol delimiter is unaffected).
                        // Empty payload ("OK") means the queue was already drained.
                        var rows = new System.Text.StringBuilder();
                        bool first = true;
                        while (currentLogForRemote.TryDequeue(out var row))
                        {
                            if (!first) rows.Append('\n');
                            rows.Append(row);
                            first = false;
                        }
                        remoteCtrl.UpdateRequestedTaskResponse(task, true, rows.ToString());
                    }
                    break;

                case RemoteControl.RequestTasks.LogWrite:
                    if (logic.TaskState != BusinessLogic.CommunicationTasks.Logging)
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "not in logging state");
                    }
                    else if (!ValidateWriteInformation(message, out var wrParam))
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "invalid write (symbol/name/format/range)");
                    }
                    else
                    {
                        logic.EditValue(wrParam);
                        remoteCtrl.UpdateRequestedTaskResponse(task, true);
                    }
                    break;

                case RemoteControl.RequestTasks.Dump:
                    if (!ValidateDumpConfigurations(message, out var param))
                    {
                        remoteCtrl.UpdateRequestedTaskResponse(task, false, "invalid dump (symbol not found or bad address/size)");
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
                    remoteCtrl.UpdateRequestedTaskResponse(task, false, "unsupported task");
                    break;
            }

        }

        private void TaskCompletionFunction(BusinessLogic.CommunicationTasks task, BusinessLogic.TaskCompletionInformation info)
        {
            if (isFormClosing)
                return;

            switch (task)
            {
                case BusinessLogic.CommunicationTasks.Open:
                    NotifyRemote(RemoteControl.RequestTasks.Connect, info);
                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                        ShowWarningIfLocal("Failed to open a communication resource.");
                    break;

                case BusinessLogic.CommunicationTasks.Close:
                    NotifyRemote(RemoteControl.RequestTasks.Disconnect, info);
                    if (info.Status != BusinessLogic.TaskCompletionStatus.Success)
                        ShowWarningIfLocal("Failed to close a communication resource.");
                    break;

                case BusinessLogic.CommunicationTasks.Initialize:
                    // Initialize returns a version string in the data payload on success, so it needs individual handling.
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                        {
                            var text = System.Text.Encoding.ASCII.GetString(info.Data);
                            remoteCtrl.UpdateRequestedTaskResponse(RemoteControl.RequestTasks.Initialize, true, text.TrimEnd());
                        }
                        else
                        {
                            string reason = info.EchoDetected
                                ? "handshake echoed (tx/rx shorted?)"
                                : StatusReason(info);
                            remoteCtrl.UpdateRequestedTaskResponse(RemoteControl.RequestTasks.Initialize, false, reason);
                        }
                    }

                    if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                    {
                        receivedVersionViewControl.TextBox = System.Text.Encoding.ASCII.GetString(info.Data);
                    }
                    else if (!IsRemote)
                    {
                        string initMessage = info.EchoDetected
                            ? "The request was echoed back." + Environment.NewLine + "The Tx and Rx lines might be shorted together."
                            : "Failed to initialize.";
                        ShowWarning(initMessage);
                    }
                    break;

                case BusinessLogic.CommunicationTasks.Config:
                    NotifyRemote(RemoteControl.RequestTasks.Config, info);
                    if (IsFailureOrTimeout(info))
                        ShowWarningIfLocal("Failed to config.");
                    break;

                case BusinessLogic.CommunicationTasks.TimeStep:
                    NotifyRemote(RemoteControl.RequestTasks.TimeStep, info);
                    if (IsFailureOrTimeout(info))
                        ShowWarningIfLocal("Failed to change timestep.");
                    break;

                case BusinessLogic.CommunicationTasks.StartLog:
                    NotifyRemote(RemoteControl.RequestTasks.LogStart, info);
                    if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                    {
                        activityToolStripProgressBar.Style = ProgressBarStyle.Marquee;
                        activityToolStripProgressBar.MarqueeAnimationSpeed = 30;
                    }
                    else
                    {
                        ShowWarningIfLocal("Failed to start logging.");
                    }
                    periodAveraging.Clear();
                    break;

                case BusinessLogic.CommunicationTasks.Logging:
                    activityToolStripProgressBar.Style = ProgressBarStyle.Blocks;
                    activityToolStripProgressBar.MarqueeAnimationSpeed = 0;
                    if (info.Status == BusinessLogic.TaskCompletionStatus.Failure)
                        ShowWarning("Something happened.", MessageBoxIcon.Error);
                    else if (info.Status == BusinessLogic.TaskCompletionStatus.Timeout)
                        ShowWarningIfLocal("Communication timeout.");
                    break;

                case BusinessLogic.CommunicationTasks.StopLog:
                    NotifyRemote(RemoteControl.RequestTasks.LogStop, info);
                    if (IsFailureOrTimeout(info))
                        ShowWarningIfLocal("Failed to stop logging.");
                    break;

                case BusinessLogic.CommunicationTasks.Dump:
                    // Dump returns hex data in the payload on success, so it needs individual handling.
                    if (IsRemote)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                        {
                            string hex = BitConverter.ToString(info.Data).Replace("-", string.Empty);
                            remoteCtrl.UpdateRequestedTaskResponse(RemoteControl.RequestTasks.Dump, true, hex);
                        }
                        else
                        {
                            remoteCtrl.UpdateRequestedTaskResponse(RemoteControl.RequestTasks.Dump, false, StatusReason(info));
                        }
                    }

                    if (dumpFormInstance != null && !dumpFormInstance.IsDisposed)
                    {
                        if (info.Status == BusinessLogic.TaskCompletionStatus.Success)
                            dumpFormInstance.UploadHexbox(info.Data.ToList());
                        else
                            ShowWarningIfLocal("Failed to correct dump data.");
                    }
                    break;
            }
        }

        // --- TaskCompletionFunction helpers ---

        /// <summary>
        /// Returns true when the task ended with Failure or Timeout.
        /// Shared predicate used by Config, TimeStep, StopLog, and similar tasks.
        /// </summary>
        private bool IsFailureOrTimeout(BusinessLogic.TaskCompletionInformation info)
            => info.Status == BusinessLogic.TaskCompletionStatus.Failure
            || info.Status == BusinessLogic.TaskCompletionStatus.Timeout;

        /// <summary>
        /// Notifies RemoteControl of a task completion result.
        /// Not used for Initialize or Dump, which carry response data and are handled individually.
        /// </summary>
        private void NotifyRemote(RemoteControl.RequestTasks remoteTask, BusinessLogic.TaskCompletionInformation info)
        {
            if (!IsRemote)
                return;
            bool ok = info.Status == BusinessLogic.TaskCompletionStatus.Success;
            remoteCtrl.UpdateRequestedTaskResponse(remoteTask, ok, ok ? string.Empty : StatusReason(info));
        }

        /// <summary>
        /// Maps an MCU-side task completion to the reason we can honestly report: only the
        /// observable status (timeout vs failed). The physical root cause (power, cabling,
        /// pass key, baud) is not distinguishable from here, so it is not fabricated.
        /// </summary>
        private static string StatusReason(BusinessLogic.TaskCompletionInformation info)
        {
            return info.Status == BusinessLogic.TaskCompletionStatus.Timeout
                ? "timeout (no response from MCU)"
                : "failed";
        }

        /// <summary>
        /// Shows a warning only in local mode. Headless (remote) sessions drop it,
        /// because <see cref="ui"/> is a HeadlessInteraction whose Notify is a no-op.
        /// </summary>
        private void ShowWarningIfLocal(string message)
        {
            ui.Notify(message, InteractionSeverity.Warning);
        }

        /// <summary>
        /// Shows a warning/error notification through the interaction abstraction.
        /// </summary>
        private void ShowWarning(string message, MessageBoxIcon icon = MessageBoxIcon.Warning)
        {
            ui.Notify(message,
                icon == MessageBoxIcon.Error ? InteractionSeverity.Error : InteractionSeverity.Warning);
        }

        /// <summary>
        /// Builds the `status` response: current session state as single-line JSON.
        /// </summary>
        private string BuildStatusJson()
        {
            bool isLogging = (logic != null) && (logic.TaskState == BusinessLogic.CommunicationTasks.Logging);

            string pageName = string.Empty;
            if (currentPageIndex >= 0 && currentPageIndex < viewPageComboBox.Items.Count)
                pageName = viewPageComboBox.Items[currentPageIndex]?.ToString() ?? string.Empty;

            string mapDate = (ValidMapLastWrittenDate == DateTime.MinValue)
                ? string.Empty : ValidMapLastWrittenDate.ToString("yyyy-MM-ddTHH:mm:ss");

            string receivedVersion = (receivedVersionViewControl != null)
                ? (receivedVersionViewControl.TextBox ?? string.Empty) : string.Empty;

            // Same estimate the GUI already shows (area2ToolStripStatusLabel, "Target Tx
            // Time = ..."), for the current page's checked byte size at the configured baud
            // rate - lets a remote caller check whether a timestep it's about to request is
            // even achievable before finding out the hard way. 0/0 if nothing is checked yet
            // or the baud rate isn't known.
            GetCurrentPageCheckedTotals(out _, out var checkedByteSize);
            CommInstructions.EstimateLogFrameTxTimeMs(checkedByteSize, Config.BaudRate, out var estTxTimeMinMs, out var estTxTimeMaxMs);

            var sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.Append("\"taskLoopActive\":").Append(IsCommunicationActive ? "true" : "false");
            sb.Append(",\"portOpen\":").Append((logic != null && logic.IsPortOpen) ? "true" : "false");
            sb.Append(",\"logging\":").Append(isLogging ? "true" : "false");
            sb.Append(",\"currentPageIndex\":").Append(currentPageIndex);
            sb.Append(",\"currentPage\":").Append(Json.Str(pageName));
            sb.Append(",\"pageCount\":").Append(viewPageComboBox.Items.Count);
            sb.Append(",\"timestep\":").Append(timeStep);
            sb.Append(",\"estTxTimeMinMs\":").Append(estTxTimeMinMs.ToString("F2"));
            sb.Append(",\"estTxTimeMaxMs\":").Append(estTxTimeMaxMs.ToString("F2"));
            sb.Append(",\"logLength\":").Append(logLength);
            sb.Append(",\"viewPath\":").Append(Json.Str(ValidViewPath ?? string.Empty));
            sb.Append(",\"mapPath\":").Append(Json.Str(ValidMapPath ?? string.Empty));
            sb.Append(",\"mapDate\":").Append(Json.Str(mapDate));
            sb.Append(",\"targetVersion\":").Append(Json.Str(targetVersionName ?? string.Empty));
            sb.Append(",\"receivedVersion\":").Append(Json.Str(receivedVersion));
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Builds the `symbols` response: the known symbol table as single-line JSON.
        /// Uses the loaded map (MapList) when present, otherwise the addressed entries of
        /// the current view settings. Fields match the .rmmap layout (symbol,address,offset,size).
        /// </summary>
        private string BuildSymbolsJson()
        {
            string source;
            var list = new List<SymbolFactor>();

            if (MapList != null && MapList.Count > 0)
            {
                source = "map";
                list = MapList;
            }
            else
            {
                source = "view";
                if (ViewSettingList != null)
                {
                    foreach (var page in ViewSettingList)
                    {
                        foreach (var s in page.Settings)
                        {
                            if (string.IsNullOrEmpty(s.Symbol) || string.IsNullOrEmpty(s.Address))
                                continue;

                            list.Add(new SymbolFactor
                            {
                                Symbol = s.Symbol,
                                Address = s.Address,
                                Offset = Convert.ToString(s.Offset),
                                Size = Convert.ToString(s.Size),
                            });
                        }
                    }
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("{\"source\":").Append(Json.Str(source));
            sb.Append(",\"count\":").Append(list.Count);
            sb.Append(",\"symbols\":[");
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{\"symbol\":").Append(Json.Str(list[i].Symbol));
                sb.Append(",\"address\":").Append(Json.Str(list[i].Address));
                sb.Append(",\"offset\":").Append(Json.Str(Convert.ToString(list[i].Offset)));
                sb.Append(",\"size\":").Append(Json.Str(Convert.ToString(list[i].Size)));
                sb.Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// Builds the `logheader` response: the column labels for log rows, aligned 1:1 with
        /// logread/logreadall (Status, Received Time, Nominal Time, then each checked+addressed
        /// variable on the current page using Name, or Symbol when Name is empty). This mirrors
        /// the GUI "Copy log" header so a remote client can label otherwise value-only rows.
        /// </summary>
        private string BuildLogHeaderJson()
        {
            // Fixed leading columns; their parse types so a client can decode each column.
            var cols = new List<string> { "Status", "Received Time", "Nominal Time" };
            var types = new List<string> { "Str", "FLT", "FLT" };

            if (ViewSettingList != null &&
                currentPageIndex >= 0 && currentPageIndex < ViewSettingList.Count)
            {
                foreach (var setting in ViewSettingList[currentPageIndex].Settings)
                {
                    if (setting.Check && !string.IsNullOrEmpty(setting.Address))
                    {
                        cols.Add(string.IsNullOrEmpty(setting.Name) ? setting.Symbol : setting.Name);
                        types.Add(string.IsNullOrEmpty(setting.Type) ? "" : setting.Type);
                    }
                }
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("{\"columns\":[");
            for (int i = 0; i < cols.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(Json.Str(cols[i]));
            }
            sb.Append("],\"types\":[");
            for (int i = 0; i < types.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(Json.Str(types[i]));
            }
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// Builds the `view` response: every row of the CURRENT view page in display order,
        /// as single-line JSON. Unlike `logheader` (checked+addressed variables only), this
        /// includes blank and unchecked rows so a remote client can resolve "the Nth row"
        /// exactly as a human sees it in the grid. `row` is 1-based (top-to-bottom); fields
        /// mirror DataSetting (symbol, name, address, offset, size, type, checked, write, description),
        /// where `write` is the preset one-click write value (macro) and `name` its label.
        /// </summary>
        private string BuildViewJson()
        {
            string pageName = string.Empty;
            if (currentPageIndex >= 0 && currentPageIndex < viewPageComboBox.Items.Count)
                pageName = viewPageComboBox.Items[currentPageIndex]?.ToString() ?? string.Empty;

            var rows = new List<DataSetting>();
            if (ViewSettingList != null &&
                currentPageIndex >= 0 && currentPageIndex < ViewSettingList.Count)
            {
                rows = ViewSettingList[currentPageIndex].Settings.ToList();
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("{\"page\":").Append(Json.Str(pageName));
            sb.Append(",\"pageIndex\":").Append(currentPageIndex);
            sb.Append(",\"pageCount\":").Append(viewPageComboBox.Items.Count);
            sb.Append(",\"rowCount\":").Append(rows.Count);
            sb.Append(",\"rows\":[");
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                if (i > 0) sb.Append(",");
                sb.Append("{\"row\":").Append(i + 1);
                sb.Append(",\"symbol\":").Append(Json.Str(r.Symbol ?? string.Empty));
                sb.Append(",\"name\":").Append(Json.Str(r.Name ?? string.Empty));
                sb.Append(",\"address\":").Append(Json.Str(r.Address ?? string.Empty));
                sb.Append(",\"offset\":").Append(Json.Str(r.Offset ?? string.Empty));
                sb.Append(",\"size\":").Append(Json.Str(r.Size ?? string.Empty));
                sb.Append(",\"type\":").Append(Json.Str(r.Type ?? string.Empty));
                sb.Append(",\"checked\":").Append(r.Check ? "true" : "false");
                sb.Append(",\"write\":").Append(Json.Str(r.Write ?? string.Empty));
                sb.Append(",\"description\":").Append(Json.Str(r.Description ?? string.Empty));
                sb.Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// Builds the `downloadviewdata` response: the CURRENT live view state (all pages,
        /// not just the active one), serialized exactly like the GUI's "Save Configuration
        /// File" menu item (`MainForm.saveViewFileToolStripMenuItem_Click`), but into memory
        /// and returned as base64 rather than written to a file. This is the safe way to read
        /// back an in-memory edit (or a GUI change not yet saved to disk) for a round-trip
        /// via `uploadviewdata`, since it never touches the filesystem.
        /// </summary>
        private string BuildViewDataBase64()
        {
            var flat = new ViewSetting();

            if (ViewSettingList != null)
            {
                foreach (var page in ViewSettingList)
                {
                    foreach (var factor in page.Settings)
                    {
                        flat.Settings.Add(factor);
                    }
                }
            }

            using (var ms = new System.IO.MemoryStream())
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ViewSetting));
                serializer.Serialize(ms, flat);
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Builds the `resyncmap` response: {"updated":N,"unresolved":[{"page":...,"row":...,"symbol":...},...]}.
        /// </summary>
        private string BuildResyncMapJson(int updated, List<(string page, int row, string symbol)> unresolved)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("{\"updated\":").Append(updated);
            sb.Append(",\"unresolved\":[");
            for (int i = 0; i < unresolved.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{\"page\":").Append(Json.Str(unresolved[i].page));
                sb.Append(",\"row\":").Append(unresolved[i].row);
                sb.Append(",\"symbol\":").Append(Json.Str(unresolved[i].symbol));
                sb.Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        // --- Map file management helpers ---

        /// <summary>
        /// Reloads the address map file if it has been updated since the last load.
        /// When <paramref name="requireConfirmation"/> is true, a Yes/No dialog is shown before reloading (used on Comm Open).
        /// When false, a missing file produces a warning and resets map state; an updated file prompts
        /// a confirmation dialog before reloading (used when entering Customize mode).
        /// NOTE: the Comm Open call site is skipped while IsRemote (see commToolStripButton_Click) -
        /// a remote/AI client gets no event for this silent resync, so it must call `resyncmap`
        /// explicitly instead; this stays purely a local-GUI convenience.
        /// </summary>
        private void ReloadMapFileIfUpdated(bool requireConfirmation)
        {
            if (!System.IO.File.Exists(ValidMapPath))
            {
                if (!requireConfirmation)
                {
                    // Entering Customize mode: warn and reset map state if the file is missing.
                    ui.Notify("The address map file is not found.", InteractionSeverity.Warning);

                    MapList = new List<SymbolFactor>();
                    ValidMapPath = null;
                    ValidMapLastWrittenDate = DateTime.MinValue;
                    autoCompleteSourceForSymbol = new AutoCompleteStringCollection();
                }
                return;
            }

            DateTime now = System.IO.File.GetLastWriteTime(ValidMapPath);

            if (now <= ValidMapLastWrittenDate)
                return;

            // Remote default = Yes: a newer map should auto-resync addresses headlessly.
            bool reload = ui.Confirm(
                "The address map file is updated.\nDo you want to reload Address in the DataGridView?",
                remoteDefault: true);

            if (!reload)
                return;

            if (!LoadMapFile(ValidMapPath))
            {
                ui.Notify("Can't read the address map file", InteractionSeverity.Warning);
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
            this.groupColumn.HeaderText = DgvColumnName.Group.ToString();
            this.groupColumn.Name = DgvColumnName.Group.ToString();
            this.groupColumn.Visible = false;
            // 
            // CheckColumn
            // 
            this.checkColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Check.ToString();
            this.checkColumn.Frozen = true;
            this.checkColumn.HeaderText = DgvColumnName.CK.ToString();
            this.checkColumn.Name = DgvColumnName.CK.ToString();
            this.checkColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.checkColumn.Width = 38;
            // 
            // SymbolColumn
            // 
            this.symbolColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Symbol.ToString();
            this.symbolColumn.HeaderText = DgvColumnName.Symbol.ToString();
            this.symbolColumn.Name = DgvColumnName.Symbol.ToString();
            this.symbolColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // AddressColumn
            // 
            this.addressColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Address.ToString();
            this.addressColumn.HeaderText = DgvColumnName.Address.ToString();
            this.addressColumn.Name = DgvColumnName.Address.ToString();
            this.addressColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // OffsetColumn
            // 
            this.offsetColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Offset.ToString();
            this.offsetColumn.HeaderText = DgvColumnName.Offset.ToString();
            this.offsetColumn.Name = DgvColumnName.Offset.ToString();
            this.offsetColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.offsetColumn.Width = 58;
            // 
            // SizeColumn
            // 
            this.sizeColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Size.ToString();
            this.sizeColumn.HeaderText = DgvColumnName.Size.ToString();
            this.sizeColumn.Name = DgvColumnName.Size.ToString();
            this.sizeColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.sizeColumn.Width = 58;
            // 
            // NameColumn
            // 
            this.nameColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Name.ToString();
            this.nameColumn.HeaderText = DgvColumnName.Name.ToString();
            this.nameColumn.Name = DgvColumnName.Name.ToString();
            // 
            // TypeColumn
            // 
            this.typeColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Type.ToString();
            this.typeColumn.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;
            this.typeColumn.HeaderText = DgvColumnName.Type.ToString();
            this.typeColumn.Name = DgvColumnName.Type.ToString();
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
            this.readRawColumn.HeaderText = DgvColumnName.ReadRaw.ToString();
            this.readRawColumn.Name = DgvColumnName.ReadRaw.ToString();
            this.readRawColumn.Visible = false;
            // 
            // ReadColumn
            // 
            this.readValueColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Read.ToString();
            this.readValueColumn.HeaderText = DgvColumnName.Read.ToString();
            this.readValueColumn.Name = DgvColumnName.Read.ToString();
            // 
            // WriteRawColumn
            // 
            this.writeRawColumn.DataPropertyName = ViewSetting.DgvPropertyNames.WriteRaw.ToString();
            this.writeRawColumn.HeaderText = DgvColumnName.WriteRaw.ToString();
            this.writeRawColumn.Name = DgvColumnName.WriteRaw.ToString();
            this.writeRawColumn.Visible = false;
            // 
            // WriteColumn
            // 
            this.writeValueColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Write.ToString();
            this.writeValueColumn.HeaderText = DgvColumnName.Write.ToString();
            this.writeValueColumn.Name = DgvColumnName.Write.ToString();
            // 
            // WriteButtonColumn
            // 
            this.writeButtonColumn.HeaderText = DgvColumnName.WR.ToString();
            this.writeButtonColumn.Name = DgvColumnName.WR.ToString();
            this.writeButtonColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.writeButtonColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.writeButtonColumn.Width = 42;
            // 
            // DescriptionColumn
            // 
            this.descriptionColumn.DataPropertyName = ViewSetting.DgvPropertyNames.Description.ToString();
            this.descriptionColumn.HeaderText = DgvColumnName.Description.ToString();
            this.descriptionColumn.Name = DgvColumnName.Description.ToString();
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
            exitOnRemoteBindFailure = true;
            remoteToolStripButton.PerformClick();
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
            // Page-splitting logic is consolidated in ViewSetting.SplitByGroup.
            ViewSettingList = ViewSetting.SplitByGroup(tmp, out var pageList);

            if (ViewSettingList.Count == 0)
                return;

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

                    ValidViewPath = path;
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                ui.Notify(ex.Message, InteractionSeverity.Error);

            }

            return isSuccess;
        }

        /// <summary>
        /// Loads a view-setting from raw XML content (e.g. an uploaded .rmxml) instead of a path.
        /// </summary>
        public bool LoadViewFromXmlString(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return false;

            bool isSuccess = false;

            try
            {
                using (var reader = new System.IO.StringReader(xml))
                {
                    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(ViewSetting));
                    var deserializedData = (ViewSetting)serializer.Deserialize(reader);

                    LoadViewSettingFile(deserializedData);

                    ValidViewPath = string.Empty;
                    isSuccess = true;
                }
            }
            catch (Exception ex)
            {
                ui.Notify(ex.Message, InteractionSeverity.Error);
            }

            return isSuccess;
        }

        /// <summary>
        /// Decodes a base64 payload to a UTF-8 string, or returns null on malformed input.
        /// Used by the over-the-wire upload commands (settingdata / loadmapdata).
        /// </summary>
        private static string TryDecodeBase64Utf8(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;

            try
            {
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Builds the "file not found" reason for openview/openmap. Suggests using an
        /// absolute path only when the given path is NOT already rooted - a caller with
        /// an already-absolute (but wrong/moved) path shouldn't get a misleading hint.
        /// </summary>
        private static string FileNotFoundReason(string path)
        {
            string hint = System.IO.Path.IsPathRooted(path) ? "" : " (use an absolute path)";
            return "file not found: " + path + hint;
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
                ui.Notify(ex.Message, InteractionSeverity.Error);

                return false;

            }

            return InterpretAndApplyMap(textArray, path, System.IO.File.GetLastWriteTime(path));
        }

        /// <summary>
        /// Loads a map from raw text content (e.g. an uploaded .rmmap) instead of a file path.
        /// ValidMapPath is left empty, so there is no file-mtime auto-resync for uploaded content
        /// (the client re-uploads when it rebuilds).
        /// </summary>
        public bool LoadMapFromText(string wholeText)
        {
            MapList = new List<SymbolFactor>();

            if (string.IsNullOrEmpty(wholeText))
                return false;

            var textArray = wholeText.Replace("\r\n", "\n").Split('\n');

            return InterpretAndApplyMap(textArray, string.Empty, DateTime.Now);
        }

        /// <summary>
        /// Interprets map lines into MapList and re-syncs every view-setting address by symbol.
        /// <paramref name="sourcePath"/> is stored as ValidMapPath (empty for uploaded content).
        /// </summary>
        private bool InterpretAndApplyMap(string[] textArray, string sourcePath, DateTime date)
        {
            bool isValid = ReadELFMap.Interpret(textArray, MapList);

            if (isValid == false)
                isValid = NmMap.Interpret(textArray, MapList);

            if (isValid == false)
                isValid = GCCLinkerMap.Interpret(textArray, MapList);

            if (isValid == false)
                isValid = RmAddressMap.Interpret(textArray, MapList);

            if (isValid == true)
            {
                ValidMapPath = sourcePath;
                ValidMapLastWrittenDate = date;

                List<string> symbolList = new List<string>();

                foreach (var factor in MapList)
                    symbolList.Add(factor.Symbol);

                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();
                autoCompleteSourceForSymbol.AddRange(symbolList.ToArray());

                ResyncViewAddressesFromMap();

                if (string.IsNullOrEmpty(targetVersionName) && !string.IsNullOrEmpty(sourcePath))
                {
                    targetVersionName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
                    targetVersionViewControl.TextBox = targetVersionName;

                }

            }
            else
            {
                ValidMapPath = null;
                ValidMapLastWrittenDate = DateTime.MinValue;
                autoCompleteSourceForSymbol = new AutoCompleteStringCollection();

            }

            return isValid;
        }

        /// <summary>
        /// Re-resolves every page's row addresses/offset/size against the CURRENTLY loaded
        /// MapList (however it got there - openmap/uploadmapdata, or a fresh InterpretAndApplyMap),
        /// mutating ViewSettingList in place. Rows whose symbol no longer resolves are unchecked
        /// and their address cleared (safety: never log/write against a stale/nonexistent
        /// address). Shared by InterpretAndApplyMap (load-time resync) and the `resyncmap`
        /// remote command (on-demand resync with a report).
        /// </summary>
        /// <returns>Count of resolved rows, and (page, 1-based row, symbol) for unresolved ones.</returns>
        private (int updated, List<(string page, int row, string symbol)> unresolved) ResyncViewAddressesFromMap()
        {
            int updated = 0;
            var unresolved = new List<(string, int, string)>();

            if (ViewSettingList == null || ViewSettingList.Count == 0)
                return (updated, unresolved);

            for (int pageIdx = 0; pageIdx < ViewSettingList.Count; pageIdx++)
            {
                string pageName = (pageIdx < viewPageComboBox.Items.Count)
                    ? viewPageComboBox.Items[pageIdx]?.ToString() ?? string.Empty
                    : string.Empty;

                var settings = ViewSettingList[pageIdx].Settings;
                for (int rowIdx = 0; rowIdx < settings.Count; rowIdx++)
                {
                    var setting = settings[rowIdx];

                    if (string.IsNullOrEmpty(setting.Symbol))
                        continue;

                    if (!FindSymbolInfo(setting.Symbol.ToString(), out var result))
                    {
                        setting.Check = false;
                        setting.Address = null;
                        unresolved.Add((pageName, rowIdx + 1, setting.Symbol));
                    }
                    else
                    {
                        setting.Address = result.Address;

                        if (int.TryParse(result.Size, out var size))
                        {
                            setting.Size = result.Size;
                            setting.Offset = result.Offset;
                        }

                        updated++;
                    }
                }
            }

            mainDataGridView.Refresh();

            return (updated, unresolved);
        }

        public void UpdateInformation()
        {
            UpdateInformation(displayMode);
        }

        // Shared with BuildStatusJson (remote's estTxTimeMinMs/estTxTimeMaxMs) so both the
        // GUI display and the remote status reflect the exact same current-page byte count.
        private void GetCurrentPageCheckedTotals(out int totalChecked, out int totalSize)
        {
            totalChecked = 0;
            totalSize = 0;

            if (ViewSettingList.Count <= currentPageIndex)
                return;

            foreach (var setting in ViewSettingList[currentPageIndex].Settings)
            {
                if (setting.Check && !string.IsNullOrEmpty(setting.Size))
                {
                    totalChecked++;
                    totalSize += int.Parse(setting.Size);
                }

            }
        }

        private void UpdateInformation(Area2DisplayMode mode)
        {
            string communicationInfo = "-";

            GetCurrentPageCheckedTotals(out var totalChecked, out var totalSize);

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
                        CommInstructions.EstimateLogFrameTxTimeMs(totalSize, Config.BaudRate, out var targetTxMinTime, out var targetTxMaxTime);

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

        // Remote-only variant of ChangeViewPage. Keeps the GUI's page selector in sync
        // for a human supervising the session, but does NOT re-trigger
        // viewPageComboBox_SelectedIndexChanged's GUI-convenience chain
        // (StopLog/Config/StartLog) - a remote page switch stays fully explicit; the
        // caller must call config()/logstart() themselves afterward if they want to log
        // the new page's variables.
        private bool ChangeViewPageForRemote(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
                return false;

            if (viewPageComboBox.Items.Count == 0)
                return false;

            int index = viewPageComboBox.FindStringExact(itemName);

            if (index < 0)
                return false;

            viewPageComboBox.SelectedIndexChanged -= new System.EventHandler(viewPageComboBox_SelectedIndexChanged);
            viewPageComboBox.SelectedIndex = index;
            viewPageComboBox.SelectedIndexChanged += new System.EventHandler(viewPageComboBox_SelectedIndexChanged);

            currentPageIndex = index;
            RefreshLogData();
            RefreshDataGridView();
            UpdateInformation();

            return true;
        }

        /// <summary>
        /// Validates a timestep string without side effects. On failure, <paramref name="reason"/>
        /// describes why and includes the valid range so the caller can surface the limits.
        /// </summary>
        private static bool TryValidateTimeStep(string text, out int value, out string reason)
        {
            reason = null;

            if (!int.TryParse(text, out value))
            {
                reason = "timestep must be an integer";
                return false;
            }

            if (value < BusinessLogic.TimeStepMin || value > BusinessLogic.TimeStepMax)
            {
                reason = "timestep out of range (" + BusinessLogic.TimeStepMin
                       + "-" + BusinessLogic.TimeStepMax + " ms)";
                return false;
            }

            return true;
        }

        /// <summary>Applies an already-validated timestep to the logger and the UI.</summary>
        private void ApplyTimeStep(int value)
        {
            logic.LogTimeStep = (uint)value;
            timeStep = value;
            timeStepToolStripTextBox.Text = value.ToString();
        }

        /// <summary>
        /// GUI path: parse the textbox, clamping out-of-range to the limits (friendly
        /// auto-correct). Returns false only for non-integer input.
        /// </summary>
        private bool ChangeTimeStep(string text)
        {
            if (!int.TryParse(text, out var value))
            {
                timeStepToolStripTextBox.Text = timeStep.ToString();
                return false;
            }

            if (value < BusinessLogic.TimeStepMin)
                value = BusinessLogic.TimeStepMin;
            else if (value > BusinessLogic.TimeStepMax)
                value = BusinessLogic.TimeStepMax;

            ApplyTimeStep(value);

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
                var found = ViewSettingList[currentPageIndex].Settings.ToList().Find(key => key.Name == elements[0]);

                if (found != null)
                    isSuccess = ValidateWriteParameters(found.Address, found.Offset, found.Size, found.Type, found.Write, out result);

            }
            else if (elements.Count() == 3)
            {
                // text = "debugData.trigger,Hex,1";
                isSuccess = ValidateWriteParametersFromMap(elements[0], elements[1], elements[2], out result);

                if(!isSuccess)
                {
                    // Fallback when the symbol isn't in the loaded map: resolve
                    // address/offset/size from the current page's own view settings
                    // instead. Must still use the CALLER's type/value (elements[1]/[2])
                    // here, matching ValidateWriteParametersFromMap's contract above.
                    var found = ViewSettingList[currentPageIndex].Settings.ToList().Find(key => key.Symbol == elements[0]);

                    if (found != null)
                        isSuccess = ValidateWriteParameters(found.Address, found.Offset, found.Size, elements[1], elements[2], out result);
                }

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

            if (!IsHexString(addressText))
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

            string note = "Log Start time: " + DateTime.Now.ToString();

            text.AppendLine(note);

            string header = "1.Status" + logTextDelimiter + "2.Received Time" + logTextDelimiter + "3.Nominal Time";

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
                            // Matches the Size/Address/Offset checks above: an invalid value
                            // disables the row (visible via `view`) instead of silently
                            // reinterpreting it as Hex and leaving it checked.
                            setting.Check = false;
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
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.StopLog);
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
                ui.Notify("Stop communication.", InteractionSeverity.Warning);

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
                            // Customize mode is local-only; remoteDefault is irrelevant here.
                            if (ui.Confirm("The target version name is unmatch the received version name.\n" +
                                           "Do you want to use the received name as the target name?",
                                           remoteDefault: false))
                                targetVersionViewControl.TextBox = receivedVer;

                        }

                    }

                }

                ReloadMapFileIfUpdated(requireConfirmation: false);

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
            if (e.KeyChar != (char)Keys.Enter)
                return;

            e.Handled = true;

            if(ChangeTimeStep(timeStepToolStripTextBox.Text))
                this.ActiveControl = mainDataGridView;

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
            if (e.KeyChar != (char)Keys.Enter)
                return;

            e.Handled = true;

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
            else
            {
                logLengthToolStripTextBox.Text = logLength.ToString();
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
            if (IsCustomizingMode)
            {
                ui.Notify("Quit custmizing mode.", InteractionSeverity.Warning);

                return;
            }

            if (IsRemote)
            {
                remoteCtrl.Terminate();

            }
            else
            {
                if (IsCustomizingMode)
                    customizeToolStripButton.PerformClick();

                // Check the port before touching any state, so the common case (another
                // instance already holds it) never opens Comm in the first place - nothing
                // to undo. See ReportRemoteBindFailure/IsRemotePortAvailable.
                if (!remoteCtrl.IsRemotePortAvailable(Config.ServerAddress, Config.ServerPort))
                {
                    ReportRemoteBindFailure();

                    // Reached only when exitOnRemoteBindFailure is false (the CLI path
                    // already exited the process above) - i.e. only for a manual GUI click.
                    var text = Config.ServerAddress.ToString() + " - " + Config.ServerPort.ToString() + " is occupied.";
                    ui.Notify(text, InteractionSeverity.Error);

                    return;
                }

                IsRemote = true;
                ui = new HeadlessInteraction();
                remoteToolStripButton.Text = "LOCAL";

                area1ToolStripStatusLabel.Text = "Remote @ " + Config.ServerAddress.ToString() + " - " + Config.ServerPort.ToString();

                bool weOpenedComm = !IsCommunicationActive;
                if (weOpenedComm)
                    commToolStripButton.PerformClick();

                bool remoteStarted = await remoteCtrl.RunAsync(Config.ServerAddress, Config.ServerPort);

                if (!remoteStarted)
                {
                    // Extremely rare: IsRemotePortAvailable just confirmed the port was free,
                    // but another process grabbed it in the brief window before RunAsync's own
                    // bind. Same undo as the pre-check failure above, just after Comm was
                    // already opened: re-clicking now sees IsCommunicationActive==true and
                    // takes the existing "close" branch, which correctly tears everything
                    // down. Skipped if communication was already active before this click (a
                    // pre-existing, unrelated connection must not be closed here).
                    if (weOpenedComm)
                        commToolStripButton.PerformClick();

                    ReportRemoteBindFailure();
                }

                if (isFormClosing)
                    return;

                area1ToolStripStatusLabel.Text = "-";

                remoteToolStripButton.Text = "REMOTE";
                IsRemote = false;
                ui = new GuiInteraction();

            }

        }

        private void ReportRemoteBindFailure()
        {
            if (!exitOnRemoteBindFailure)
                return;

            Program.WriteConsole(
                "RM Classic: FAILED to open the remote server on " + Config.ServerAddress + ":" + Config.ServerPort + " " +
                "(the address/port may already be in use by another instance). Exiting.");
            Environment.Exit(1);
        }

        private async void commToolStripButton_Click(object sender, EventArgs e)
        {
            if (IsCustomizingMode)
            {
                ui.Notify("Quit custmizing mode.", InteractionSeverity.Warning);

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
                // Suppressed while remote: a remote/AI client gets no event for this silent
                // resync, so `resyncmap` is the explicit substitute (see its doc-comment above).
                if (!IsRemote)
                    ReloadMapFileIfUpdated(requireConfirmation: true);

                IsCommunicationActive = true;

                RefreshLogData();

                receivedVersionViewControl.TextBox = string.Empty;

                commToolStripButton.Image = Properties.Resources.FlagThread_red;
                commToolStripButton.Text = COMM_CLOSE_TEXT;

                logic.ClearWaitingTasks();
                if(IsRemote)
                {
                    await logic.RunAsync();

                }
                else if (UpdateConfigurationParameter(ViewSettingList[currentPageIndex].Settings, out var parameters))
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
                    ui.Notify("The current view's configuration is either empty or invalid.",
                              InteractionSeverity.Warning);
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
                                mainDataGridView[DgvColumnName.Read.ToString(), index].Value = textValue;
                            else
                                mainDataGridView[DgvColumnName.Read.ToString(), index].Value = null;

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

                if (dgv.CurrentCell.OwningColumn.Name == DgvColumnName.Symbol.ToString())
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
                    if (cell.ColumnIndex != (int)DgvColumnName.Group)
                        mainDataGridView[cell.ColumnIndex, cell.RowIndex].Value = null;
                }
            }
        }

        private void mainDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int ckColumnIndex = dgv.Columns[DgvColumnName.CK.ToString()].Index;
            int addressColumnIndex = dgv.Columns[DgvColumnName.Address.ToString()].Index;
            int offsetColumnIndex = dgv.Columns[DgvColumnName.Offset.ToString()].Index;
            int sizeColumnIndex = dgv.Columns[DgvColumnName.Size.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvColumnName.Type.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvColumnName.Write.ToString()].Index;
            int wrColumnIndex = dgv.Columns[DgvColumnName.WR.ToString()].Index;

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

        // The DataGridView edit flow fires events in the order: CellValidating -> CellEndEdit.
        // CellValidating decides whether to accept the edit and, if so, stores the validated value here.
        // CellEndEdit then applies that value to the underlying model.
        // A null value means the edit was rejected (cancelled).
        private string cellValueBuffer;

        private void mainDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            int ckColumnIndex = dgv.Columns[DgvColumnName.CK.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvColumnName.Type.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvColumnName.Write.ToString()].Index;
            int descriptionColumnIndex = dgv.Columns[DgvColumnName.Description.ToString()].Index;

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
            int groupColumnIndex = dgv.Columns[DgvColumnName.Group.ToString()].Index;
            int symbolColumnIndex = dgv.Columns[DgvColumnName.Symbol.ToString()].Index;
            int addressColumnIndex = dgv.Columns[DgvColumnName.Address.ToString()].Index;
            int offsetColumnIndex = dgv.Columns[DgvColumnName.Offset.ToString()].Index;
            int sizeColumnIndex = dgv.Columns[DgvColumnName.Size.ToString()].Index;
            int nameColumnIndex = dgv.Columns[DgvColumnName.Name.ToString()].Index;
            int typeColumnIndex = dgv.Columns[DgvColumnName.Type.ToString()].Index;
            int readrawColumnIndex = dgv.Columns[DgvColumnName.ReadRaw.ToString()].Index;
            int readColumnIndex = dgv.Columns[DgvColumnName.Read.ToString()].Index;
            int writerawColumnIndex = dgv.Columns[DgvColumnName.WriteRaw.ToString()].Index;
            int writeColumnIndex = dgv.Columns[DgvColumnName.Write.ToString()].Index;
            int descriptionColumnIndex = dgv.Columns[DgvColumnName.Description.ToString()].Index;

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

        // These methods have been moved to ValidationHelper.
        // The wrappers below are kept to avoid touching every call site.
        private bool ValidateSize(string value, ref int result)
            => ValidationHelper.ValidateSize(value, ref result);

        private bool IsHexString(string text)
            => ValidationHelper.IsHexString(text);

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
                    var tmpGroupName = mainDataGridView[DgvColumnName.Group.ToString(), rowValue].Value.ToString();
                    mainDataGridView[DgvColumnName.Group.ToString(), (rowValue+1)].Value = tmpGroupName;
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
                (viewPageComboBox.SelectedIndex == 0))
            {
                ui.Notify("Can not delete.", InteractionSeverity.Warning);

                return;
            }

            var index = viewPageComboBox.SelectedIndex;

            if (index != -1)
            {
                // Destructive: remote default = No.
                if (ui.Confirm("Do you want to delete this page?", remoteDefault: false))
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
            if (ui.Confirm("Do you want to insert a page?", remoteDefault: false))
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

            if (ui.Confirm("Do you want to duplicate this page?", remoteDefault: false))
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
