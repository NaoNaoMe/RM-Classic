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


        public delegate void InitializeCompletedFunction(string message);
        public InitializeCompletedFunction InitializeCompletedCallBack;

        public delegate void LogCommunicationTimeoutFunction();
        public LogCommunicationTimeoutFunction LogCommunicationTimeoutCallBack;

        public delegate void CollectDumpCompletedFunction(List<byte> bytes);
        public CollectDumpCompletedFunction CollectDumpCompletedCallBack;

        public uint LogTimeStep { private get; set; }
        public List<DataParameter> LogConfigParameter { private get; set; }
        public DataParameter DumpConfigParameter { private get; set; }

        public CommunicationTasks TaskState { get; private set; }

        private ConcurrentQueue<CommunicationTasks> myTaskQueue;

        private CommMainCtrl myCommMainCtrl;
        private CommInstructions myCommInstructions;

        private Configuration mySettings;

        private int currentTimeStep;

        private CancellationTokenSource myCancellationTokenSource;

        private ConcurrentQueue<LogData> mylogData;

        public BusinessLogic()
        {
            mySettings = new Configuration();
            myCommInstructions = new CommInstructions(mySettings.RmRange);
            myCommMainCtrl = new CommMainCtrl(mySettings);

            myTaskQueue = new ConcurrentQueue<CommunicationTasks>();

            LogConfigParameter = new List<DataParameter>();
            DumpConfigParameter = new DataParameter();

            mylogData = new ConcurrentQueue<LogData>();

            TaskState = CommunicationTasks.Nothing;
        }

        public bool UpdateResource(Configuration setting)
        {
            if (myCommMainCtrl.IsOpen == true)
                return false;

            mySettings = new Configuration(setting);
            myCommInstructions = new CommInstructions(mySettings.RmRange);
            myCommMainCtrl = new CommMainCtrl(mySettings);

            return true;
        }

        public Configuration GetBusinessLogicSettings()
        {
            return mySettings;
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

        public async Task RunAsync(bool isBreakable)
        {
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
                                var isOpen = await myCommMainCtrl.OpenAsync(myCancellationTokenSource.Token);

                                if (isBreakable && !isOpen)
                                {
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                break;

                            case CommunicationTasks.Close:
                                myCommMainCtrl.Close();
                                break;

                            case CommunicationTasks.Initialize:
                                var versionName = await InitializeAsync(myCancellationTokenSource.Token);

                                if (isBreakable &&
                                    string.IsNullOrEmpty(versionName))
                                {
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                InitializeCompletedCallBack?.Invoke(versionName);
                                break;

                            case CommunicationTasks.TimeStep:
                                isSuccess = await SetTimeStepAsync(LogTimeStep, myCancellationTokenSource.Token);

                                if (isBreakable && !isSuccess)
                                {
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                break;

                            case CommunicationTasks.Config:
                                isSuccess = await ConfigLogDataAsync(LogConfigParameter, myCancellationTokenSource.Token);

                                if (isBreakable && !isSuccess)
                                {
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
                                    ClearWaitingTasks();
                                    EnqueueTask(CommunicationTasks.Terminate);
                                }

                                if (isBreakable && activity.IsTimeout)
                                {
                                    LogCommunicationTimeoutCallBack?.Invoke();

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

        }

        private async Task<String> InitializeAsync(CancellationToken ct)
        {
            string versionName = string.Empty;
            List<byte> txFrame = new List<byte>();
            List<byte> rxFrame = new List<byte>();

            myCommMainCtrl.PurgeReceiveBuffer();

            txFrame = myCommInstructions.MakeTryConnectionRequest(mySettings.PassNumber);

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
                    rxFrame.RemoveAt(0);    // remove unnecessary header data

                    var endIndex = rxFrame.Count() - 1;
                    if (rxFrame[endIndex] == 0x00)
                    {
                        rxFrame.RemoveAt(endIndex);
                    }

                    versionName = System.Text.Encoding.ASCII.GetString(rxFrame.ToArray());
                    break;

                }

            }


            return versionName;
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
