using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rmApplication
{
    public class BusinessLogic
    {
        public static readonly int TimeStepDefault = 500;
        public static readonly int TimeStepMin = 1;
        public static readonly int TimeStepMax = 2000;

        public struct DataParameter
        {
            public uint Address;
            public uint Size;
            public ulong Value;
        }

        public struct LogData
        {
            public string Status;
            public long OsTime;
            public long SlvTime;
            public Queue<ulong> RawData;

        }

        public enum CommunicationTasks : int
        {
            Nothing = 0,
            Open,
            Close,
            Initialize,
            TimeStep,
            Config,
            Dump,
            Logging,
            Bypass,
            Terminate
        }

        private struct LogActivityParameter
        {
            public bool IsTimeout;
            public bool IsSuccess;
        }

        private struct InitializeResponse
        {
            public bool IsSuccess;
            public bool IsEchoed;
            public string VersionName;
        }


        public delegate void InitializeCompletedFunction(string message);
        public InitializeCompletedFunction InitializeCompletedCallBack;

        public delegate void LogCommunicationTimeoutFunction();

        public delegate void CollectDumpCompletedFunction(List<byte> bytes);
        public CollectDumpCompletedFunction CollectDumpCompletedCallBack;

        public delegate void TextReceivedFunction(List<byte> bytes);
        public TextReceivedFunction TextReceivedCallBack;

        public uint LogTimeStep { private get; set; }
        public List<DataParameter> LogConfigParameter { private get; set; }
        public DataParameter DumpConfigParameter { private get; set; }

        public CommunicationTasks TaskState { get; private set; }

        public bool IsCommAvailable
        {
            get
            {
                return myCommMainCtrl.IsOpen;
            }
        }

        public ConcurrentQueue<string> BypassRequest { get; set; }
        public ConcurrentQueue<string> BypassResponse { get; private set; }

        private ConcurrentQueue<CommunicationTasks> myTaskQueue;

        private uint myPassNumber;

        private CommMainCtrl myCommMainCtrl;
        private CommInstructions myCommInstructions;

        private int currentTimeStep;

        private CancellationTokenSource myCancellationTokenSource;

        private ConcurrentQueue<LogData> mylogData;

        public BusinessLogic()
        {
            var config = new Configuration();
            myPassNumber = config.PassNumber;
            myCommInstructions = new CommInstructions(config.RmRange);
            myCommMainCtrl = new CommMainCtrl(config);

            myTaskQueue = new ConcurrentQueue<CommunicationTasks>();

            LogConfigParameter = new List<DataParameter>();
            DumpConfigParameter = new DataParameter();

            mylogData = new ConcurrentQueue<LogData>();

            TaskState = CommunicationTasks.Nothing;

            BypassRequest = new ConcurrentQueue<string>();
            BypassResponse = new ConcurrentQueue<string>();
        }

        public bool UpdateResource(Configuration config)
        {
            if (myCommMainCtrl.IsOpen)
                return false;

            myPassNumber = config.PassNumber;
            myCommInstructions = new CommInstructions(config.RmRange);
            myCommMainCtrl = new CommMainCtrl(config);

            return true;
        }

        public void ClearWaitingTasks()
        {
            while (myTaskQueue.Count != 0)
            {
                CommunicationTasks tmp;
                myTaskQueue.TryDequeue(out tmp);
            }
        }

        public void EnqueueTask(CommunicationTasks task)
        {
            myTaskQueue.Enqueue(task);
        }

        public void CancelCurrentTask()
        {
            if (myCancellationTokenSource != null)
                myCancellationTokenSource.Cancel();

        }

        private ConcurrentQueue<List<byte>> WriteDataRequest = new ConcurrentQueue<List<byte>>();
        public void EditValue(DataParameter param)
        {
            WriteDataRequest.Enqueue(myCommInstructions.MakeWirteDataRequest(param.Address, param.Size, param.Value));
        }

        private ConcurrentQueue<List<byte>> SendTextRequest = new ConcurrentQueue<List<byte>>();
        public void SendText(byte[] bytes)
        {
            SendTextRequest.Enqueue(myCommInstructions.MakeSendTextRequest(bytes.ToList()));
        }

        public async Task<string> RunAsync(bool isBreakable)
        {
            string msg = string.Empty;
            bool isSuccess;
            myCancellationTokenSource = new CancellationTokenSource();

            while (true)
            {
                try
                {
                    bool isBreak = false;
                    CommunicationTasks request;
                    if (!myTaskQueue.TryDequeue(out request))
                    {
                        TaskState = CommunicationTasks.Nothing;
                        await Task.Delay(10);
                    }
                    else
                    {
                        TaskState = request;

                        switch (request)
                        {
                            case CommunicationTasks.Open:
                                if(!myCommMainCtrl.IsOpen)
                                {
                                    var isOpen = await myCommMainCtrl.OpenAsync(myCancellationTokenSource.Token);

                                    if (isBreakable && !isOpen)
                                    {
                                        msg = "Failed to open a communication resource.";
                                        ClearWaitingTasks();
                                        EnqueueTask(CommunicationTasks.Terminate);
                                    }

                                }

                                break;

                            case CommunicationTasks.Close:
                                myCommMainCtrl.Close();
                                break;

                            case CommunicationTasks.Initialize:
                                var response = await InitializeAsync(myPassNumber ,myCancellationTokenSource.Token);

                                if(response.IsSuccess)
                                {
                                    InitializeCompletedCallBack?.Invoke(response.VersionName);
                                }
                                else 
                                {
                                    if (isBreakable)
                                    {
                                        if (response.IsEchoed)
                                            msg = "The request was echoed back." + Environment.NewLine + "The Tx and Rx lines might be shorted together.";
                                        else
                                            msg = "Failed to initialize.";

                                        ClearWaitingTasks();
                                        EnqueueTask(CommunicationTasks.Terminate);
                                    }

                                }

                                break;

                            case CommunicationTasks.TimeStep:
                                isSuccess = await SetTimeStepAsync(LogTimeStep, myCancellationTokenSource.Token);

                                if (isBreakable && !isSuccess)
                                {
                                    msg = "Failed to change time-step.";
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                break;

                            case CommunicationTasks.Config:
                                isSuccess = await ConfigLogDataAsync(LogConfigParameter, myCancellationTokenSource.Token);

                                if (isBreakable && !isSuccess)
                                {
                                    msg = "Failed to configure current settings.";
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                break;

                            case CommunicationTasks.Dump:

                                var bytes = await CorrectDumpDataAsync(DumpConfigParameter);

                                CollectDumpCompletedCallBack?.Invoke(bytes);
                                break;

                            case CommunicationTasks.Logging:
                                LogActivityParameter activity;
                                activity = await RunLoggingAsync(myCancellationTokenSource.Token);

                                if (isBreakable && !activity.IsSuccess)
                                {
                                    msg = "Something happened during logging.";
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                if (isBreakable && activity.IsTimeout)
                                {
                                    msg = "Communication timeout.";
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                break;

                            case CommunicationTasks.Bypass:

                                string inputText;
                                if(BypassRequest.TryDequeue(out inputText))
                                {
                                    var outputText = await BypassFunctionAsync(inputText);
                                    if (!string.IsNullOrEmpty(outputText))
                                        BypassResponse.Enqueue(outputText);
                                }

                                break;

                            case CommunicationTasks.Terminate:
                                myCommMainCtrl.Close();
                                isBreak = true;
                                break;

                        }

                    }

                    if (isBreak == true)
                        break;

                    if (myCancellationTokenSource.IsCancellationRequested == true)
                        myCancellationTokenSource = new CancellationTokenSource();

                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);

                    ClearWaitingTasks();

                    if (isBreakable)
                        EnqueueTask(CommunicationTasks.Terminate);
                    else
                        EnqueueTask(CommunicationTasks.Close);

                    myCancellationTokenSource = new CancellationTokenSource();

                }

            }

            return msg;
        }

        private async Task<InitializeResponse> InitializeAsync(uint passNumber, CancellationToken ct)
        {
            InitializeResponse response = new InitializeResponse();
            response.IsSuccess = false;
            response.IsEchoed = false;
            response.VersionName = string.Empty;

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            myCommMainCtrl.PurgeReceiveBuffer();

            txFrame = myCommInstructions.MakeTryConnectionRequest(passNumber);

            await myCommMainCtrl.PushAsync(txFrame);

            while(true)
            {
                rxFrame = await myCommMainCtrl.PullAsync(ct);

                if (rxFrame.Count == 0)
                {
                    break;
                }

                if (ct.IsCancellationRequested == true)
                {
                    break;
                }

                if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                {
                    if(txFrame.SequenceEqual(rxFrame))
                    {
                        response.IsEchoed = true;
                    }
                    else
                    {
                        rxFrame.RemoveAt(0);                    // remove unnecessary header data
                        rxFrame.RemoveAt(rxFrame.Count - 1);    // remove unnecessary crc data

                        var endIndex = rxFrame.Count() - 1;
                        if (rxFrame[endIndex] == 0x00)
                        {
                            rxFrame.RemoveAt(endIndex);         // remove null data
                        }

                        response.IsSuccess = true;
                        response.VersionName = System.Text.Encoding.ASCII.GetString(rxFrame.ToArray());

                    }

                    break;

                }

            }

            return response;
        }

        private async Task<bool> SetTimeStepAsync(uint timeStep, CancellationToken ct)
        {
            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            myCommMainCtrl.PurgeReceiveBuffer();

            currentTimeStep = 0;

            txFrame = myCommInstructions.MakeSetTimeStepRequest(timeStep);

            await myCommMainCtrl.PushAsync(txFrame);

            bool isSuccess = false;
            while(true)
            {
                rxFrame = await myCommMainCtrl.PullAsync(ct);

                if (rxFrame.Count == 0)
                {
                    break;
                }

                if (ct.IsCancellationRequested == true)
                {
                    break;
                }

                if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                {
                    isSuccess = true;
                    currentTimeStep = (int)timeStep;
                    break;
                }

            }

            return isSuccess;
        }

        private async Task<bool> ConfigLogDataAsync(List<DataParameter> parameters, CancellationToken ct)
        {
            if(parameters.Count <= 0)
            {
                return false;
            }

            myCommMainCtrl.PurgeReceiveBuffer();

            myCommInstructions.ClearLogDataConfiguration();

            UInt32 address;
            UInt32 size;

            bool isFailed = false;
            foreach(var item in parameters)
            {
                address = item.Address;
                size = item.Size;

                if (!myCommInstructions.PushDataForLogDataConfiguration(address, size))
                {
                    isFailed = true;
                    break;
                }

            }

            if(isFailed == true)
            {
                return false;
            }

            if(!myCommInstructions.UpdateLogDataConfiguration())
            {
                return false;
            }

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            bool isSuccess = true;
            while (myCommInstructions.IsAvailableLogDataRequest(out txFrame))
            {
                await myCommMainCtrl.PushAsync(txFrame);

                int retryCnt = 0;
                while(true)
                {
                    rxFrame = await myCommMainCtrl.PullAsync(ct);

                    if(rxFrame.Count == 0)
                    {
                        isSuccess = false;
                        break;
                    }

                    if (ct.IsCancellationRequested == true)
                    {
                        isSuccess = false;
                        break;
                    }

                    if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                    {
                        break;
                    }
                    else
                    {
                        retryCnt++;
                        if(retryCnt > 10)
                        {
                            isSuccess = false;
                            break;
                        }
                        else
                        {
                            await myCommMainCtrl.PushAsync(txFrame);
                        }

                    }

                }

                if (isSuccess == false)
                    break;

            }

            return isSuccess;

        }


        private async Task<List<byte>> CorrectDumpDataAsync(DataParameter parameter)
        {
            List<byte> dumpData = new List<byte>();

            myCommMainCtrl.PurgeReceiveBuffer();

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            uint address = parameter.Address;
            uint size = parameter.Size;

            bool isRequestAssert = true;
            bool isRetry = false;
            int retryCount = 0;
            while (true)
            {
                if(isRequestAssert)
                {
                    if (!isRetry)
                        txFrame = myCommInstructions.MakeDumpDataRequest(address, size);

                    await myCommMainCtrl.PushAsync(txFrame);

                }

                CancellationTokenSource dumpCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                rxFrame = await myCommMainCtrl.PullAsync(dumpCts.Token);

                if (dumpCts.IsCancellationRequested)
                {
                    isRequestAssert = true;
                    isRetry = true;
                }
                else if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                {
                    isRequestAssert = true;
                    isRetry = false;
                    retryCount = 0;

                    rxFrame.RemoveAt(0);                    // remove unnecessary header data
                    rxFrame.RemoveAt(rxFrame.Count - 1);    // remove unnecessary crc data

                    dumpData.AddRange(rxFrame);

                    if(size <= (uint)rxFrame.Count)
                        break;

                    address += (uint)rxFrame.Count;
                    size -= (uint)rxFrame.Count;

                }
                else
                {
                    isRequestAssert = false;
                    isRetry = true;
                }

                if(isRetry)
                {
                    retryCount++;
                    if (retryCount > 10)
                        break;

                }

            }

            return dumpData;
        }

        private async Task<LogActivityParameter> RunLoggingAsync(CancellationToken ct)
        {
            // Main task
            LogActivityParameter activity = new LogActivityParameter();
            activity.IsSuccess = true;
            activity.IsTimeout = false;

            var txSW = new System.Diagnostics.Stopwatch();
            var rxSW = new System.Diagnostics.Stopwatch();
            var timeoutSW = new System.Diagnostics.Stopwatch();
            long rxTimeOffset = 0;
            long slvRxTime = 0;

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            myCommMainCtrl.PurgeReceiveBuffer();

            try
            {
                txSW.Restart();
                rxSW.Restart();
                timeoutSW.Restart();

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    if (SendTextRequest.TryDequeue(out txFrame))
                    {

                    }
                    else if (WriteDataRequest.TryDequeue(out txFrame))
                    {

                    }
                    else
                    {
                        var msec = txSW.ElapsedMilliseconds;
                        if (msec > 500)
                        {
                            txFrame = myCommInstructions.MakeStartLogModeRequest();
                            txSW.Restart();
                        }
                    }

                    if (txFrame != null)
                    {
                        if (txFrame.Count != 0)
                        {
                            await myCommMainCtrl.PushAsync(txFrame);

                        }

                    }

                    rxFrame = myCommMainCtrl.Pull();

                    if (rxFrame.Count != 0)
                    {
                        timeoutSW.Restart();

                        var msec = rxSW.ElapsedMilliseconds;

                        var tmp = new LogData();
                        tmp.RawData = new Queue<ulong>();
                        int lostCnt;

                        var bytes = new List<byte>();
                        int code;

                        if (myCommInstructions.CheckUnmanagedStream(rxFrame, ref bytes, out code))
                        {
                            if(code == 1)
                                TextReceivedCallBack?.Invoke(bytes);

                        }
                        else if (myCommInstructions.CheckLogSequence(rxFrame, ref tmp.RawData, out lostCnt))
                        {
                            if (rxTimeOffset == 0)
                            {
                                rxTimeOffset = msec;
                            }
                            else
                            {
                                slvRxTime += currentTimeStep * (lostCnt + 1);

                            }

                            if (lostCnt != 0)
                            {
                                tmp.Status = lostCnt.ToString() + " messages might be lost";
                            }
                            else
                            {
                                tmp.Status = "OK";
                            }

                            tmp.SlvTime = slvRxTime;
                            tmp.OsTime = msec - rxTimeOffset;

                            mylogData.Enqueue(tmp);

                        }

                    }

                    if (timeoutSW.ElapsedMilliseconds >= 5000)
                    {
                        activity.IsTimeout = true;

                        var tmp = new LogData();
                        tmp.RawData = new Queue<ulong>();
                        tmp.Status = "Timeout";
                        tmp.SlvTime = 0;
                        tmp.OsTime = 0;

                        mylogData.Enqueue(tmp);

                        break;
                    }

                    await Task.Delay(1);

                }

            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                activity.IsSuccess = false;
            }

            //Clear log data
            mylogData = new ConcurrentQueue<LogData>();

            // Assert "StopLog" when logging task was finished
            try
            {
                await myCommMainCtrl.PushAsync(myCommInstructions.MakeStopLogModeRequest());

                CancellationTokenSource otherCT = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

                await myCommMainCtrl.PullAsync(otherCT.Token);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return activity;
        }

        public bool GetLogData(out LogData result)
        {
            result = new LogData();

            if (!mylogData.TryDequeue(out result))
                return false;
            else
                return true;

        }

        private async Task<string> BypassFunctionAsync(string inputText)
        {
            string outputText = string.Empty;

            if (string.IsNullOrEmpty(inputText))
                return outputText;

            if (!System.Text.RegularExpressions.Regex.IsMatch(inputText, @"\A\b[0-9a-fA-F]+\b\Z") ||
                (inputText.Length % 2) != 0)
            {
                return outputText;
            }

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < inputText.Length; i+=2)
                bytes.Add(Convert.ToByte(inputText.Substring(i, 2), 16));

            myCommMainCtrl.PurgeReceiveBuffer();

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            bool isRequestAssert = true;
            bool isRetry = false;
            int retryCount = 0;
            while (true)
            {
                if(isRequestAssert)
                {
                    txFrame = myCommInstructions.MakeBypassRequest(bytes);

                    await myCommMainCtrl.PushAsync(txFrame);

                }

                CancellationTokenSource dumpCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                rxFrame = await myCommMainCtrl.PullAsync(dumpCts.Token);

                if (dumpCts.IsCancellationRequested)
                {
                    isRequestAssert = true;
                    isRetry = true;
                }
                else if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                {
                    isRequestAssert = true;
                    isRetry = false;
                    retryCount = 0;

                    rxFrame.RemoveAt(0);                    // remove unnecessary header data
                    rxFrame.RemoveAt(rxFrame.Count - 1);    // remove unnecessary crc data

                    outputText = string.Empty;
                    foreach (var abyte in rxFrame)
                        outputText += abyte.ToString("X2");

                    break;
                }
                else
                {
                    isRequestAssert = false;
                    isRetry = true;
                }

                if (isRetry)
                {
                    retryCount++;
                    if (retryCount > 3)
                        break;

                }

            }

            return outputText;
        }

    }
}
