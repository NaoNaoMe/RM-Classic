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
            StartLog,
            Logging,
            StopLog
        }

        public enum TaskCompletionStatus
        {
            Success,
            Failure,
            Timeout
        }

        public struct TaskCompletionInformation
        {
            public TaskCompletionStatus Status;
            public bool EchoDetected;
            public byte[] Data;
        }

        public delegate void TaskCompletaionFunction(CommunicationTasks task, TaskCompletionInformation info);
        public TaskCompletaionFunction TaskCompletaionFunctionCallback;

        public delegate void DerivedFrameReceivedFunction(byte[] bytes);
        public DerivedFrameReceivedFunction SerialCommunicationEmulationReceivedCallBack;

        public uint LogTimeStep { private get; set; }
        public List<DataParameter> LogConfigParameter { private get; set; }
        public DataParameter DumpConfigParameter { private get; set; }

        public CommunicationTasks TaskState { get; private set; }

        private ConcurrentQueue<CommunicationTasks> taskQueue;

        private uint passNumber;

        private CommMainCtrl commMainCtrl;
        private CommInstructions commInstructions;

        private int currentMillisStep;

        private CancellationTokenSource logicCancellationTokenSource;
        private CancellationTokenSource taskCancellationTokenSource;

        private ConcurrentQueue<LogData> logDataQueue;

        public BusinessLogic()
        {
            var config = new Configuration();
            passNumber = config.PassNumber;
            commInstructions = new CommInstructions(config.RmRange);
            commMainCtrl = new CommMainCtrl(config);

            taskQueue = new ConcurrentQueue<CommunicationTasks>();

            LogConfigParameter = new List<DataParameter>();
            DumpConfigParameter = new DataParameter();

            logDataQueue = new ConcurrentQueue<LogData>();

            TaskState = CommunicationTasks.Nothing;

        }

        public bool UpdateResource(Configuration config)
        {
            if (commMainCtrl.IsOpen)
                return false;

            passNumber = config.PassNumber;
            commInstructions = new CommInstructions(config.RmRange);
            commMainCtrl = new CommMainCtrl(config);

            return true;
        }

        public void ClearWaitingTasks()
        {
            while (taskQueue.Count != 0)
                taskQueue.TryDequeue(out var tmp);
        }

        public void EnqueueTask(CommunicationTasks task)
        {
            taskQueue.Enqueue(task);
        }

        public void Terminate()
        {
            CancelCurrentTask();

            if (logicCancellationTokenSource != null && !logicCancellationTokenSource.IsCancellationRequested)
                logicCancellationTokenSource.Cancel();

        }

        public void CancelCurrentTask()
        {
            if (taskCancellationTokenSource != null && !taskCancellationTokenSource.IsCancellationRequested)
                taskCancellationTokenSource.Cancel();

        }

        private ConcurrentQueue<byte[]> DataRequest = new ConcurrentQueue<byte[]>();
        public void EditValue(DataParameter param)
        {
            DataRequest.Enqueue(commInstructions.MakeWirteDataRequest(param.Address, param.Size, param.Value));
        }

        public void SendDataUsingSerialCommunicationEmulation(byte[] bytes)
        {
            DataRequest.Enqueue(commInstructions.MakeDerivedFrame(bytes.ToList(),CommInstructions.RmDerivedMode.SerialCommunicationEmulation));
        }

        public async Task RunAsync()
        {
            logicCancellationTokenSource = new CancellationTokenSource();

            try
            {
                while (true)
                {
                    CommunicationTasks request;
                    TaskState = CommunicationTasks.Nothing;

                    while (true)
                    {
                        if (taskQueue.TryDequeue(out request))
                            break;

                        logicCancellationTokenSource.Token.ThrowIfCancellationRequested();

                        await Task.Delay(10);
                    }

                    taskCancellationTokenSource = new CancellationTokenSource();

                    TaskState = request;
                    var info = new TaskCompletionInformation();
                    info.Status = TaskCompletionStatus.Failure;

                    switch (request)
                    {
                        case CommunicationTasks.Open:
                            System.Diagnostics.Debug.WriteLine("--Open--");
                            bool isSuccess = await commMainCtrl.OpenAsync(taskCancellationTokenSource.Token);
                            if (isSuccess)
                                info.Status = TaskCompletionStatus.Success;
                            break;

                        case CommunicationTasks.Close:
                            System.Diagnostics.Debug.WriteLine("--Close--");
                            commMainCtrl.Close();
                            info.Status = TaskCompletionStatus.Success;
                            break;

                        case CommunicationTasks.Initialize:
                            System.Diagnostics.Debug.WriteLine("--Initialize--");
                            info = await InitializeAsync(passNumber, taskCancellationTokenSource.Token);

                            if(info.EchoDetected)
                                info.Status = TaskCompletionStatus.Failure;

                            break;

                        case CommunicationTasks.TimeStep:
                            System.Diagnostics.Debug.WriteLine("--TimeStep--");
                            info = await SetTimeStepAsync(LogTimeStep, taskCancellationTokenSource.Token);
                            break;

                        case CommunicationTasks.Config:
                            System.Diagnostics.Debug.WriteLine("--Config--");
                            info = await ConfigLogDataAsync(LogConfigParameter, taskCancellationTokenSource.Token);
                            break;

                        case CommunicationTasks.StartLog:
                            System.Diagnostics.Debug.WriteLine("--StartLog--");
                            info = await StartLogAsync(taskCancellationTokenSource.Token);
                            if (info.Status == TaskCompletionStatus.Success)
                                EnqueueTask(CommunicationTasks.Logging);
                            break;

                        case CommunicationTasks.Logging:
                            System.Diagnostics.Debug.WriteLine("--Logging--");
                            info = await LoggingAsync(taskCancellationTokenSource.Token);
                            break;

                        case CommunicationTasks.StopLog:
                            System.Diagnostics.Debug.WriteLine("--StopLog--");
                            info = await StopLogAsync(taskCancellationTokenSource.Token);
                            break;

                        case CommunicationTasks.Dump:
                            System.Diagnostics.Debug.WriteLine("--Dump--");
                            info = await CollectDumpDataAsync(DumpConfigParameter, taskCancellationTokenSource.Token);
                            break;

                    }

                    if (info.Status != TaskCompletionStatus.Success)
                        ClearWaitingTasks();

                    if (info.Status == TaskCompletionStatus.Timeout)
                        EnqueueTask(CommunicationTasks.Close);

                    TaskCompletaionFunctionCallback?.Invoke(request, info);

                    taskCancellationTokenSource.Dispose();
                    taskCancellationTokenSource = null;
                }
            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

            }

            commMainCtrl.Close();

            logicCancellationTokenSource.Dispose();
            logicCancellationTokenSource = null;

            return;
        }

        private async Task<TaskCompletionInformation> QueryAsync(byte[] txFrame, CancellationToken ct, int retry = 1)
        {
            var info = new TaskCompletionInformation();
            info.Status = TaskCompletionStatus.Failure;

            commMainCtrl.PurgeReceiveBuffer();

            await commMainCtrl.PushAsync(txFrame);

            while(retry-- > 0)
            {
                double timeout = 100;
                if (commMainCtrl.Mode == CommMainCtrl.CommunicationMode.LocalNet)
                    timeout = 1000;

                using (var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout)))
                {
                    var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);

                    var rxFrame = await commMainCtrl.PullAsync(linkedCts.Token);

                    if (rxFrame.Length == 0)
                        break;

                    if (ct.IsCancellationRequested == true)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            info.Status = TaskCompletionStatus.Timeout;
                            break;
                        }
                    }

                    info.EchoDetected = false;
                    if (txFrame.SequenceEqual(rxFrame))
                        info.EchoDetected = true;

                    if (commInstructions.IsResponseValid(txFrame, rxFrame))
                    {
                        info.Status = TaskCompletionStatus.Success;

                        // remove unnecessary header data and crc data
                        if (rxFrame.Length > 2)
                        {
#if true
                            info.Data = new byte[rxFrame.Length - 2];
                            Array.Copy(rxFrame, 1, info.Data, 0, info.Data.Length);
#else
                            info.Data = rxFrame.Skip(1).Take(rxFrame.Length - 2).ToArray();
#endif
                        }
                        break;

                    }

                }

            }

            return info;
        }

        private async Task<TaskCompletionInformation> InitializeAsync(uint passNumber, CancellationToken ct)
        {
            return await QueryAsync(commInstructions.MakeTryConnectionRequest(passNumber), ct);
        }

        private async Task<TaskCompletionInformation> SetTimeStepAsync(uint millis, CancellationToken ct)
        {
            var info = await QueryAsync(commInstructions.MakeSetTimeStepRequest(millis), ct);

            if (info.Status == TaskCompletionStatus.Success)
            {
                currentMillisStep = (int)millis;
            }

            return info;
        }

        private async Task<TaskCompletionInformation> ConfigLogDataAsync(List<DataParameter> parameters, CancellationToken ct)
        {
            var info = new TaskCompletionInformation();
            info.Status = TaskCompletionStatus.Failure;

            if (parameters.Count <= 0)
                return info;

            commInstructions.ClearLogDataConfiguration();

            UInt32 address;
            UInt32 size;

            bool isFailed = false;
            foreach(var item in parameters)
            {
                address = item.Address;
                size = item.Size;

                if (!commInstructions.PushDataForLogDataConfiguration(address, size))
                {
                    isFailed = true;
                    break;
                }

            }

            if(isFailed == true)
            {
                info.Status = TaskCompletionStatus.Failure;
                return info;
            }

            if (!commInstructions.UpdateLogDataConfiguration())
            {
                info.Status = TaskCompletionStatus.Failure;
                return info;
            }

            while (commInstructions.IsAvailableLogDataRequest(out var txFrame))
            {
                info = await QueryAsync(txFrame, ct);
                if (info.Status != TaskCompletionStatus.Success)
                    break;
            }

            return info;

        }

        private async Task<TaskCompletionInformation> StartLogAsync(CancellationToken ct)
        {
            return await QueryAsync(commInstructions.MakeStartLogModeRequest(), ct, 3);
        }

        private async Task<TaskCompletionInformation> StopLogAsync(CancellationToken ct)
        {
            return await QueryAsync(commInstructions.MakeStopLogModeRequest(), ct, 3);
        }

        private async Task<TaskCompletionInformation> CollectDumpDataAsync(DataParameter parameter, CancellationToken ct)
        {
            var info = new TaskCompletionInformation();
            info.Status = TaskCompletionStatus.Failure;

            var dumpData = new List<byte>();

            uint address = parameter.Address;
            uint size = parameter.Size;

            while (true)
            {
                info = await QueryAsync(commInstructions.MakeDumpDataRequest(address, size), ct);
                if (info.Status != TaskCompletionStatus.Success)
                    break;

                dumpData.AddRange(info.Data);

                if (size <= (uint)info.Data.Length)
                {
                    info.Status = TaskCompletionStatus.Success;
                    info.Data = dumpData.ToArray();
                    break;
                }

                address += (uint)info.Data.Length;
                size -= (uint)info.Data.Length;

            }

            return info;
        }

        private async Task<TaskCompletionInformation> LoggingAsync(CancellationToken ct)
        {
            var info = new TaskCompletionInformation();
            info.Status = TaskCompletionStatus.Success;

            while (DataRequest.Count != 0)
                DataRequest.TryDequeue(out var tmp);

            var txSW = new System.Diagnostics.Stopwatch();
            var rxSW = new System.Diagnostics.Stopwatch();
            var timeoutSW = new System.Diagnostics.Stopwatch();
            long rxTimeOffset = 0;
            long slvRxTime = 0;

            txSW.Restart();
            rxSW.Restart();
            timeoutSW.Restart();

            while (logDataQueue.Count != 0)
                logDataQueue.TryDequeue(out var tmp);

            while (true)
            {
                if (ct.IsCancellationRequested)
                    break;

                if (!DataRequest.TryDequeue(out var txFrame))
                {
                    var msec = txSW.ElapsedMilliseconds;
                    if (msec > 500)
                    {
                        txFrame = commInstructions.MakeStartLogModeRequest();
                        txSW.Restart();
                    }
                }

                if (txFrame != null)
                    await commMainCtrl.PushAsync(txFrame);

                Queue<byte[]> rxFrames = new Queue<byte[]>();
                while (true)
                {
                    var rxFrame = commMainCtrl.Pull();
                    if (rxFrame.Length == 0)
                        break;
                    rxFrames.Enqueue(rxFrame);
                }

                while (rxFrames.Count > 0)
                {
                    var rxFrame = rxFrames.Dequeue();

                    timeoutSW.Restart();

                    var msec = rxSW.ElapsedMilliseconds;

                    var tmp = new LogData();
                    tmp.RawData = new Queue<ulong>();
                    int lostCnt;

                    var bytes = new byte[0];
                    CommInstructions.RmDerivedMode mode;

                    if (commInstructions.CheckDerivedFrame(rxFrame, ref bytes, out mode))
                    {
                        if (mode == CommInstructions.RmDerivedMode.SerialCommunicationEmulation)
                            SerialCommunicationEmulationReceivedCallBack?.Invoke(bytes);

                    }
                    else if (commInstructions.CheckLogSequence(rxFrame, ref tmp.RawData, out lostCnt))
                    {
                        long osRXTime = 0;
                        if (rxTimeOffset == 0)
                        {
                            rxTimeOffset = msec;
                        }
                        else
                        {
                            osRXTime = msec - rxTimeOffset;
                            slvRxTime += currentMillisStep;
                        }

                        if (lostCnt != 0)
                        {
                            tmp.Status = lostCnt.ToString() + " messages might be lost";
                            slvRxTime = ((osRXTime / currentMillisStep) + 1) * currentMillisStep;
                        }
                        else
                        {
                            tmp.Status = "OK";
                        }

                        tmp.SlvTime = slvRxTime;
                        tmp.OsTime = osRXTime;

                        logDataQueue.Enqueue(tmp);

                    }

                }

                if (timeoutSW.ElapsedMilliseconds >= 5000)
                {
                    info.Status = TaskCompletionStatus.Timeout;

                    var tmp = new LogData();
                    tmp.RawData = new Queue<ulong>();
                    tmp.Status = "Timeout";
                    tmp.SlvTime = 0;
                    tmp.OsTime = 0;

                    logDataQueue.Enqueue(tmp);

                    break;
                }

                await Task.Delay(1);

            }

            return info;
        }

        public bool GetLogData(out LogData result)
        {
            result = new LogData();

            if (!logDataQueue.TryDequeue(out result))
                return false;
            else
                return true;

        }

    }
}
