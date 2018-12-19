using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rmApplication
{
    class RemoteControl
    {
        public string DeviceVersion;

        public delegate void RegisterLogConfigFunction(ViewSetting currentDataSource);
        public RegisterLogConfigFunction RegisterLogConfigCallBack;

        public delegate bool ChangePageFunction(int value);
        public ChangePageFunction ChangePageCallBack;

        public delegate void ChangeTimeStepFunction(int value);
        public ChangeTimeStepFunction ChangeTimeStepCallBack;

        public delegate bool ValidateWriteValueFunction(int index, string inputText, out BusinessLogic.DataParameter param);
        public ValidateWriteValueFunction ValidateWriteValueCallBack;

        public delegate bool ValidateWriteFunction(string orderText, out BusinessLogic.DataParameter param);
        public ValidateWriteFunction ValidateWriteCallBack;

        private const string CommandResource = "resource";

        private const string CommandOpen = "open";
        private const string CommandClose = "close";

        private const string CommandInitialize = "init";
        private const string CommandGetVersion = "getver";

        private const string CommandRegister = "register";
        private const string CommandAdapt = "adapt";
        private const string CommandPage = "page";

        private const string CommandTimeStep = "timestep";

        private const string CommandLogStart = "logstart";
        private const string CommandLogStop = "logstop";
        private const string CommandLogGet = "logget";
        private const string CommandLogWrite = "logwrite";
        private const string CommandLogSize = "logsize";

        private const string CommandDumpSet = "dumpset";
        private const string CommandDumpGet = "dumpget";

        private CancellationTokenSource remoteCancellationTokenSource;
        private TCPListenerResource remote;
        private BusinessLogic logic;

        private BusinessLogic.CommunicationTasks requestTask;

        private ViewSetting viewSettingBuffer;

        private ConcurrentQueue<string> logTextDataBuff;
        private int logDataBuffSize;

        private ConcurrentQueue<byte> DumpDataBuff;

        public RemoteControl(BusinessLogic tmp)
        {
            remote = new TCPListenerResource(true);
            logic = tmp;

            requestTask = BusinessLogic.CommunicationTasks.Nothing;

            viewSettingBuffer = new ViewSetting();

            requestTask = BusinessLogic.CommunicationTasks.Nothing;

            logTextDataBuff = new ConcurrentQueue<string>();
            logDataBuffSize = 500;

            DumpDataBuff = new ConcurrentQueue<byte>();

            requestTask = BusinessLogic.CommunicationTasks.Nothing;
        }

        public void Cancel()
        {
            if (remoteCancellationTokenSource != null)
                remoteCancellationTokenSource.Cancel();

            requestTask = BusinessLogic.CommunicationTasks.Nothing;

        }

        public void EnqueueLogTextDataBuff(string logTextData, char delimiter)
        {
            string tmp = logTextData.Replace(delimiter, ',');

            logTextDataBuff.Enqueue(tmp);

            while (logTextDataBuff.Count() > logDataBuffSize)
            {
                logTextDataBuff.TryDequeue(out tmp);
            }

        }

        public void ClearDumpDataBuff()
        {
            DumpDataBuff = new ConcurrentQueue<byte>();
        }

        public void EnqueueDumpDataBuff(byte abyte)
        {
            DumpDataBuff.Enqueue(abyte);
        }

        public async Task RunAsync(System.Net.IPAddress netAddress, int netPort)
        {
            remoteCancellationTokenSource = new CancellationTokenSource();

            if (!remote.StartListening(netAddress, netPort))
                return;

            while (true)
            {
                try
                {
                    if (remote.IsClientConnected)
                    {
                        var text = await remote.ReadTextClientAsync(remoteCancellationTokenSource.Token);

                        if (remoteCancellationTokenSource.IsCancellationRequested)
                            break;

                        if (string.IsNullOrEmpty(text))
                        {
                            remote.CloseClient();
                        }
                        else
                        {
                            remote.Publish(AnalyzeCommand(text));
                        }

                    }
                    else
                    {
                        await remote.AcceptClientAsync(remoteCancellationTokenSource.Token);

                        if (remoteCancellationTokenSource.IsCancellationRequested)
                            break;

                        await Task.Delay(100);

                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);

                    remote.StopListening();

                    if (!remote.StartListening(netAddress, netPort))
                        break;
                }

            }

            remote.StopListening();

        }

        private string AnalyzeCommand(string message)
        {
            string failedText = "Failed";
            string okText = "OK";
            string busyText = "Busy";
            string emptyText = "Empty";

            string command = string.Empty;
            string parameters = string.Empty;
            string answer = failedText;

            var splitIndex = message.IndexOf(' ');
            if (splitIndex != -1)
            {
                command = message.Substring(0, splitIndex);
                parameters = message.Substring(splitIndex + 1, (message.Length - command.Length - 1));
            }
            else
            {
                command = message;
            }

            switch (command)
            {
                case CommandResource:
                    var config = new Configuration();
                    if (ValidateCommunicationResource(parameters, out config))
                    {
                        if(logic.UpdateResource(config))
                            answer = okText;
                    }

                    break;
                case CommandOpen:
                    answer = busyText;
                    if (requestTask != BusinessLogic.CommunicationTasks.Open)
                    {
                        requestTask = BusinessLogic.CommunicationTasks.Open;

                        if (!logic.IsCommAvailable)
                        {
                            logic.ClearWaitingTasks();
                            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Open);
                            logic.CancelCurrentTask();
                        }
                    }
                    else
                    {
                        if (logic.TaskState != BusinessLogic.CommunicationTasks.Open)
                        {
                            requestTask = BusinessLogic.CommunicationTasks.Nothing;
                            if (logic.IsCommAvailable)
                                answer = okText;
                            else
                                answer = failedText;
                        }
                    }
                    break;
                case CommandClose:
                    if (requestTask == BusinessLogic.CommunicationTasks.Open)
                        requestTask = BusinessLogic.CommunicationTasks.Nothing;
                    logic.ClearWaitingTasks();
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Close);
                    logic.CancelCurrentTask();
                    answer = okText;
                    break;
                case CommandInitialize:
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Initialize);
                    DeviceVersion = string.Empty;
                    answer = okText;
                    break;
                case CommandGetVersion:
                    if(string.IsNullOrEmpty(DeviceVersion))
                        answer = busyText;
                    else
                        answer = okText + " " +DeviceVersion;
                    break;
                case CommandRegister:
                    if (AddDataSetting(parameters))
                        answer = okText;
                    break;
                case CommandAdapt:
                    RegisterLogConfigCallBack?.Invoke(viewSettingBuffer);
                    viewSettingBuffer = new ViewSetting();
                    answer = okText;
                    break;
                case CommandPage:
                    if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                    {
                        answer = busyText;
                    }
                    else
                    {
                        int pageValue;
                        if (int.TryParse(parameters, out pageValue))
                        {
                            answer = failedText;
                        }
                        else
                        {
                            if (ChangePageCallBack != null)
                            {
                                if (!ChangePageCallBack(pageValue))
                                {
                                    answer = failedText;
                                }
                                else
                                {
                                    ClearLogTextDataBuff();
                                    answer = okText;
                                }
                            }
                        }
                    }
                    break;
                case CommandTimeStep:
                    int timeStep;
                    if (int.TryParse(parameters, out timeStep))
                    {
                        if (timeStep < BusinessLogic.TimeStepMin)
                            timeStep = BusinessLogic.TimeStepMin;
                        else if (timeStep > BusinessLogic.TimeStepMax)
                            timeStep = BusinessLogic.TimeStepMax;

                        ChangeTimeStepCallBack?.Invoke(timeStep);

                        logic.LogTimeStep = (uint)timeStep;
                        answer = okText;
                    }

                    break;
                case CommandLogStart:
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Config);
                    logic.EnqueueTask(BusinessLogic.CommunicationTasks.Logging);
                    ClearLogTextDataBuff();
                    answer = okText;
                    break;
                case CommandLogStop:
                    if(logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                    {
                        logic.ClearWaitingTasks();
                        logic.CancelCurrentTask();
                    }

                    answer = okText;

                    break;
                case CommandLogGet:
                    string data;
                    if(!logTextDataBuff.TryDequeue(out data))
                    {
                        if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                            answer = busyText;
                        else
                            answer = emptyText;
                    }
                    else
                    {
                        answer = okText + " " + data;
                    }

                    break;

                case CommandLogWrite:
                    if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                    {
                        var wrParam = new BusinessLogic.DataParameter();
                        if (IsValidWriteSetting(parameters, out wrParam))
                        {
                            logic.EditValue(wrParam);
                            answer = okText;
                        }

                    }

                    break;

                case CommandLogSize:
                    if (logic.TaskState == BusinessLogic.CommunicationTasks.Logging)
                    {
                        answer = busyText;
                    }
                    else
                    {
                        int size;
                        if (int.TryParse(parameters, out size))
                        {
                            if (size > 0)
                            {
                                ClearLogTextDataBuff();
                                logDataBuffSize = size;
                                answer = okText;
                            }
                        }
                    }

                    break;
                case CommandDumpSet:
                    if (requestTask == BusinessLogic.CommunicationTasks.Dump)
                    {
                        answer = busyText;
                    }
                    else
                    {
                        var param = new BusinessLogic.DataParameter();
                        if (ValidateDumpConfigrations(parameters, out param))
                        {
                            requestTask = BusinessLogic.CommunicationTasks.Dump;

                            logic.DumpConfigParameter = param;

                            logic.ClearWaitingTasks();
                            logic.EnqueueTask(BusinessLogic.CommunicationTasks.Dump);
                            logic.CancelCurrentTask();

                            answer = okText;
                        }

                    }

                    break;
                case CommandDumpGet:
                    if (logic.TaskState == BusinessLogic.CommunicationTasks.Dump)
                    {
                        answer = busyText;
                    }
                    else
                    {
                        requestTask = BusinessLogic.CommunicationTasks.Nothing;

                        if (DumpDataBuff.Count <= 0)
                        {
                            answer = emptyText;
                        }
                        else
                        {
                            answer = okText + " ";
                            int count = 0;
                            while (count < 16)
                            {
                                byte abyte;
                                if (DumpDataBuff.TryDequeue(out abyte))
                                {
                                    answer += abyte.ToString("X2");
                                    count++;
                                }
                                else
                                {
                                    break;
                                }

                            }

                        }

                    }
                    break;

                default:
                    break;
            }

            return answer;
        }

        private void ClearLogTextDataBuff()
        {
            logTextDataBuff = new ConcurrentQueue<string>();
        }

        private bool AddDataSetting(string line)
        {
            var factors = line.Split(',');

            bool isSuccess = false;
            if(factors.Count() == 10)
            {
                isSuccess = true;
                var tmp = new DataSetting();

                tmp.Group = factors[0];

                bool isCheck = false;
                if(bool.TryParse(factors[1], out isCheck))
                    tmp.Check = isCheck;

                tmp.Symbol = factors[2];
                tmp.Address = factors[3];
                tmp.Offset = factors[4];
                tmp.Size = factors[5];
                tmp.Name = factors[6];
                tmp.Type = factors[7];
                tmp.Write = factors[8];
                tmp.Description = factors[9];

                viewSettingBuffer.Settings.Add(tmp);

            }

            return isSuccess;
        }

        private bool ValidateDumpConfigrations(string text, out BusinessLogic.DataParameter param)
        {
            param = new BusinessLogic.DataParameter();
            var factors = text.Split(',');

            if (factors.Count() != 2)
                return false;

            var addressText = factors[0];
            var sizeText = factors[1];

            if (string.IsNullOrEmpty(addressText))
                return false;

            if (addressText.Length < 2)
                return false;

            var header = addressText.Substring(0, 2);

            if (header != "0x")
                return false;

            addressText = addressText.Remove(0, 2);

            int address = 0;
            if (!int.TryParse(addressText, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out address))
                return false;

            if (string.IsNullOrEmpty(sizeText))
                return false;

            int size = 0;
            if (!int.TryParse(sizeText, out size))
                return false;

            param.Address = (uint)address;
            param.Size = (uint)size;

            return true;

        }

        private bool ValidateCommunicationResource(string text, out Configuration config)
        {
            config = new Configuration();
            var factors = text.Split(',');

            if (factors.Count() < 4)
                return false;

            uint passNumber;
            if (uint.TryParse(factors[0], out passNumber))
                config.PassNumber = passNumber;
            else
                return false;

            CommMainCtrl.CommunicationMode mode;
            if (Enum.TryParse<CommMainCtrl.CommunicationMode>(factors[1], out mode))
                config.CommMode = mode;
            else
                return false;

            CommInstructions.RmAddr range;
            if (Enum.TryParse<CommInstructions.RmAddr>(factors[2], out range))
                config.RmRange = range;
            else
                return false;

            int baudRate;
            if (int.TryParse(factors[3], out baudRate))
                config.BaudRate = baudRate;
            else
                return false;

            if (mode == CommMainCtrl.CommunicationMode.Serial && factors.Count() == 5)
            {
                config.SerialPortName = factors[4];
                return true;

            }
            else if (mode == CommMainCtrl.CommunicationMode.LocalNet && factors.Count() == 6)
            {
                System.Net.IPAddress clientAddress;
                if (System.Net.IPAddress.TryParse(factors[4], out clientAddress))
                    config.ClientAddress = clientAddress;
                else
                    return false;

                int clientPort;
                if (int.TryParse(factors[5], out clientPort))
                    config.ClientPort = clientPort;
                else
                    return false;

                return true;
            }
            else
            {
                return false;
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

            return isSuccess;
        }


        private bool IsValidWriteSetting(string line, out BusinessLogic.DataParameter wrParam)
        {
            wrParam = new BusinessLogic.DataParameter();

            if( ValidateWriteCallBack == null ||
                ValidateWriteValueCallBack == null )
            {
                return false;
            }

            var factors = line.Split(',');

            bool isSuccess = false;
            if (factors.Count() == 1)
            {
                if (ValidateWriteCallBack(factors[0], out wrParam))
                    isSuccess = true;

            }
            else if (factors.Count() == 2)
            {
                int index;
                if (int.TryParse(factors[0], out index))
                {
                    if (ValidateWriteValueCallBack(index, factors[1], out wrParam))
                        isSuccess = true;

                }

            }

            return isSuccess;
        }


    }
}
