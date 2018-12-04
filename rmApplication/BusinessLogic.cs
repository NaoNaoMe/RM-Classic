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
            public uint Value;
        }

        public struct LogData
        {
            public string Status;
            public long OsTime;
            public long SlvTime;
            public Queue<uint> RawData;

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

            myCommMainCtrl.Push(txFrame);

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
                        rxFrame.RemoveAt(0);    // remove unnecessary header data

                        var endIndex = rxFrame.Count() - 1;
                        if (rxFrame[endIndex] == 0x00)
                        {
                            rxFrame.RemoveAt(endIndex);
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

            myCommMainCtrl.Push(txFrame);

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
                myCommMainCtrl.Push(txFrame);

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
                            myCommMainCtrl.Push(txFrame);
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

            if (!myCommInstructions.UpdateDumpDataConfiguration(parameter.Address, parameter.Size))
                return dumpData;

            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            bool isRetry = false;
            int retryCount = 0;
            while (true)
            {
                if(!isRetry)
                {
                    if (!myCommInstructions.IsAvailableDumpDataRequest(out txFrame))
                        break;
                }

                myCommMainCtrl.Push(txFrame);

                CancellationTokenSource dumpCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
                rxFrame = await myCommMainCtrl.PullAsync(dumpCts.Token);

                if (dumpCts.IsCancellationRequested)
                {
                    isRetry = true;
                }
                else if (myCommInstructions.IsResponseValid(txFrame, rxFrame))
                {
                    isRetry = false;
                    retryCount = 0;

                    rxFrame.RemoveAt(0);    // remove unnecessary header data
                    dumpData.AddRange(rxFrame);
                }
                else
                {
                    isRetry = true;
                }

                if(isRetry)
                {
                    if (retryCount > 10)
                        break;

                    retryCount++;
                }

            }

            return dumpData;
        }

        private async Task<LogActivityParameter> RunLoggingAsync(CancellationToken ct)
        {
            LogActivityParameter activity;

            // Main task
            activity = await Task.Run(() => RunLogging(ct), ct);

            // Assert "StopLog" when logging task was finished
            try
            {
                myCommMainCtrl.Push(myCommInstructions.MakeStopLogModeRequest());

                CancellationTokenSource otherCT = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

                await myCommMainCtrl.PullAsync(otherCT.Token);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return activity;
        }

        private LogActivityParameter RunLogging(CancellationToken ct)
        {
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

                    if(!WriteDataRequest.TryDequeue(out txFrame))
                    {
                        var msec = txSW.ElapsedMilliseconds;
                        if (msec > 500)
                        {
                            txFrame = myCommInstructions.MakeStartLogModeRequest();
                            txSW.Restart();
                        }

                    }

                    if(txFrame != null)
                    {
                        if (txFrame.Count != 0)
                        {
                            myCommMainCtrl.Push(txFrame);

                        }

                    }

                    rxFrame = myCommMainCtrl.Pull();

                    if (rxFrame.Count != 0)
                    {
                        timeoutSW.Restart();

                        var msec = rxSW.ElapsedMilliseconds;

                        var tmp = new LogData();
                        tmp.RawData = new Queue<uint>();
                        int lostCnt;

                        if(myCommInstructions.CheckLogSequence(rxFrame, ref tmp.RawData, out lostCnt))
                        {
                            if(rxTimeOffset == 0)
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

                    if(timeoutSW.ElapsedMilliseconds >= 5000)
                    {
                        activity.IsTimeout = true;

                        var tmp = new LogData();
                        tmp.RawData = new Queue<uint>();
                        tmp.Status = "Timeout";
                        tmp.SlvTime = 0;
                        tmp.OsTime = 0;

                        mylogData.Enqueue(tmp);

                        break;
                    }

                    Thread.Sleep(1);

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

    }
}
