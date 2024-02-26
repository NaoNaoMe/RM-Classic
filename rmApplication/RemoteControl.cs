using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpListenerTerminal;

namespace rmApplication
{
    class RemoteControl
    {
        public enum RequestTasks : int
        {
            Undef,
            Open,
            Close,
            Setting,
            Page,
            SymbolInfo,
            Initialize,
            Config,
            TimeStep,
            LogStart,
            LogStop,
            LogRead,
            LogWrite,
            Dump
        }

        public struct TaskResponseInformation
        {
            public RequestTasks Task;
            public bool IsSuccess;
            public string Text;
        }

        public delegate void TaskRequestFunction(RequestTasks task, string message);
        public TaskRequestFunction TaskRequestFunctionCallback;

        private const string CommandOpen = "open";
        private const string CommandClose = "close";

        private const string CommandSetting = "setting";
        private const string CommandPage = "page";
        private const string CommandSymbolInfo = "symbolinfo";

        private const string CommandInitialize = "init";
        private const string CommandConfig = "config";
        private const string CommandTimeStep = "timestep";

        private const string CommandLogStart = "logstart";
        private const string CommandLogStop = "logstop";
        private const string CommandLogRead = "logread";
        private const string CommandLogWrite = "logwrite";

        private const string CommandDump = "dump";

        private CancellationTokenSource remoteCancellationTokenSource;
        private TCPListenerResource remote;

        private TaskCompletionSource<TaskResponseInformation> RequestedTaskResponse;

        public RemoteControl()
        {
            remote = new TCPListenerResource();

            remote.Delimiter = "\r\n";

        }

        public void Terminate()
        {
            if (remoteCancellationTokenSource != null && !remoteCancellationTokenSource.IsCancellationRequested)
                remoteCancellationTokenSource.Cancel();

        }

 
        public void UpadateRequestedTaskResponse(RequestTasks task, bool isSuccess, string text = "")
        {
            var tmp = new TaskResponseInformation();
            if (RequestedTaskResponse != null && !RequestedTaskResponse.Task.IsCompleted)
            {
                tmp.Task = task;
                tmp.IsSuccess = isSuccess;
                tmp.Text = text;
                RequestedTaskResponse.SetResult(tmp);
            }
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
                        var text = await remote.ReadTextAsync(remoteCancellationTokenSource.Token);

                        if (!string.IsNullOrEmpty(text))
                        {
                            var response = await AnalyzeQueryAsync(text, remoteCancellationTokenSource.Token);

                            await remote.WriteAsync(response);
                        }
                        else
                        {
                            remote.DisconnectClient();
                        }

                        if (remoteCancellationTokenSource.IsCancellationRequested)
                            break;

                        await Task.Delay(10);

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

            remoteCancellationTokenSource.Dispose();
            remoteCancellationTokenSource = null;
        }

        private async Task<string> AnalyzeQueryAsync(string message, CancellationToken ct)
        {
            RequestTasks request = RequestTasks.Undef;

            var splitIndex = message.IndexOf(' ');
            string command;
            string parameters = string.Empty;
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
                case CommandOpen:
                    request = RequestTasks.Open;
                    break;
                case CommandClose:
                    request = RequestTasks.Close;
                    break;
                case CommandSetting:
                    request = RequestTasks.Setting;
                    break;
                case CommandPage:
                    request = RequestTasks.Page;
                    break;
                case CommandSymbolInfo:
                    request = RequestTasks.SymbolInfo;
                    break;
                case CommandInitialize:
                    request = RequestTasks.Initialize;
                    break;
                case CommandConfig:
                    request = RequestTasks.Config;
                    break;
                case CommandTimeStep:
                    request = RequestTasks.TimeStep;
                    break;
                case CommandLogStart:
                    request = RequestTasks.LogStart;
                    break;
                case CommandLogStop:
                    request = RequestTasks.LogStop;
                    break;
                case CommandLogRead:
                    request = RequestTasks.LogRead;
                    break;
                case CommandLogWrite:
                    request = RequestTasks.LogWrite;
                    break;
                case CommandDump:
                    request = RequestTasks.Dump;
                    break;
                default:
                    break;
            }

            RequestedTaskResponse = new TaskCompletionSource<TaskResponseInformation>();
            TaskRequestFunctionCallback?.Invoke(request, parameters);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), ct);
            var completedTask = await Task.WhenAny(RequestedTaskResponse.Task, timeoutTask);

            if (ct.IsCancellationRequested)
                return string.Empty;

            string timeoutText = "Timeout";
            string failedText = "Failed";
            string okText = "OK";
            string answer = failedText;

            if (completedTask == timeoutTask)
            {
                answer = timeoutText;
            }
            else
            {
                var result = RequestedTaskResponse.Task.Result;

                if (result.IsSuccess)
                {
                    if(string.IsNullOrEmpty(result.Text))
                        answer = okText;
                    else
                        answer = okText + " " + result.Text;
                }

            }

            return answer;
        }

    }

}
