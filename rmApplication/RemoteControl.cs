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
            Connect,
            Disconnect,
            OpenView,
            Page,
            SymbolInfo,
            Initialize,
            Config,
            TimeStep,
            LogStart,
            LogStop,
            LogRead,
            LogWrite,
            Dump,
            Help,
            Status,
            Symbols,
            LogHeader,
            UploadViewData,
            OpenMap,
            UploadMapData,
            LogReadAll,
            View,
            DownloadViewData,
            ResyncMap
        }

        public struct TaskResponseInformation
        {
            public RequestTasks Task;
            public bool IsSuccess;
            public string Text;
        }

        public delegate void TaskRequestFunction(RequestTasks task, string message);
        public TaskRequestFunction TaskRequestFunctionCallback;

        private const string CommandConnect = "connect";
        private const string CommandDisconnect = "disconnect";

        private const string CommandOpenView = "openview";
        private const string CommandPage = "page";
        private const string CommandSymbolInfo = "symbolinfo";

        private const string CommandInitialize = "init";
        private const string CommandConfig = "config";
        private const string CommandTimeStep = "timestep";

        private const string CommandLogStart = "logstart";
        private const string CommandLogStop = "logstop";
        private const string CommandLogRead = "logread";
        private const string CommandLogReadAll = "logreadall";
        private const string CommandLogWrite = "logwrite";

        private const string CommandDump = "dump";

        // Self-description commands (machine-readable; usable without an active connection).
        private const string CommandHelp = "help";
        private const string CommandStatus = "status";
        private const string CommandSymbols = "symbols";
        private const string CommandLogHeader = "logheader";
        private const string CommandView = "view";

        // Over-the-wire config/map upload/download (base64 payload) and remote map load by path.
        private const string CommandUploadViewData = "uploadviewdata";
        private const string CommandDownloadViewData = "downloadviewdata";
        private const string CommandOpenMap = "openmap";
        private const string CommandUploadMapData = "uploadmapdata";
        private const string CommandResyncMap = "resyncmap";

        private CancellationTokenSource remoteCancellationTokenSource;
        private TCPListenerResource remote;

        private TaskCompletionSource<TaskResponseInformation> RequestedTaskResponse;
        private RequestTasks RequestedTask;

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

 
        public void UpdateRequestedTaskResponse(RequestTasks task, bool isSuccess, string text = "")
        {
            var tmp = new TaskResponseInformation();
            // task must match the command RequestedTaskResponse was created for - otherwise
            // this is a stray completion from an earlier command's async side effect (e.g.
            // page's internal StopLog/Config/StartLog reconfig) racing a later command that
            // has already replaced RequestedTaskResponse with its own.
            if (RequestedTaskResponse != null && !RequestedTaskResponse.Task.IsCompleted && task == RequestedTask)
            {
                tmp.Task = task;
                tmp.IsSuccess = isSuccess;
                tmp.Text = text;
                RequestedTaskResponse.SetResult(tmp);
            }
        }


        // Binds a throwaway listener just to test whether the address/port is free, then
        // immediately releases it - lets a caller detect a conflict before committing to
        // anything (e.g. before activating MCU communication), instead of only finding out
        // once RunAsync's own bind fails. A small, inherent TOCTOU race remains between this
        // check and RunAsync's actual bind (another process could grab the port in between);
        // that window is not eliminated, only made unlikely to matter in practice.
        public bool IsRemotePortAvailable(System.Net.IPAddress netAddress, int netPort)
        {
            var probe = new TCPListenerResource();
            bool available = probe.StartListening(netAddress, netPort);

            if (available)
                probe.StopListening();

            return available;
        }

        public async Task<bool> RunAsync(System.Net.IPAddress netAddress, int netPort)
        {
            remoteCancellationTokenSource = new CancellationTokenSource();

            if (!remote.StartListening(netAddress, netPort))
                return false;

            while (true)
            {
                try
                {
                    if (remote.IsClientConnected)
                    {
                        var text = await remote.ReadTextAsync(remoteCancellationTokenSource.Token);

                        if (!string.IsNullOrEmpty(text))
                        {
                            var response = await ProcessCommandAsync(text);

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

            return true;
        }

        private async Task<string> ProcessCommandAsync(string text)
        {
            try
            {
                return await AnalyzeQueryAsync(text, remoteCancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // A genuine cancellation (not an application bug) - let RunAsync's own
                // try/catch handle it as the normal Terminate() shutdown path.
                throw;
            }
            catch (Exception ex)
            {
                // A bug in command parsing/dispatch fails just this one command instead
                // of restarting the whole TCP listener and dropping the current client.
                System.Diagnostics.Debug.WriteLine(ex);
                ErrorLog.Write("RemoteControl.RunAsync AnalyzeQueryAsync", ex);
                return BuildErrorJson("failed");
            }
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
                case CommandConnect:
                    request = RequestTasks.Connect;
                    break;
                case CommandDisconnect:
                    request = RequestTasks.Disconnect;
                    break;
                case CommandOpenView:
                    request = RequestTasks.OpenView;
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
                case CommandLogReadAll:
                    request = RequestTasks.LogReadAll;
                    break;
                case CommandLogWrite:
                    request = RequestTasks.LogWrite;
                    break;
                case CommandDump:
                    request = RequestTasks.Dump;
                    break;
                case CommandHelp:
                    request = RequestTasks.Help;
                    break;
                case CommandStatus:
                    request = RequestTasks.Status;
                    break;
                case CommandSymbols:
                    request = RequestTasks.Symbols;
                    break;
                case CommandLogHeader:
                    request = RequestTasks.LogHeader;
                    break;
                case CommandView:
                    request = RequestTasks.View;
                    break;
                case CommandUploadViewData:
                    request = RequestTasks.UploadViewData;
                    break;
                case CommandDownloadViewData:
                    request = RequestTasks.DownloadViewData;
                    break;
                case CommandOpenMap:
                    request = RequestTasks.OpenMap;
                    break;
                case CommandUploadMapData:
                    request = RequestTasks.UploadMapData;
                    break;
                case CommandResyncMap:
                    request = RequestTasks.ResyncMap;
                    break;
                default:
                    break;
            }

            // Unknown command: answer directly with a clear reason (don't bother the app).
            if (request == RequestTasks.Undef)
                return BuildErrorJson("unknown command: " + command + " (send 'help' for the catalog)");

            RequestedTaskResponse = new TaskCompletionSource<TaskResponseInformation>();
            RequestedTask = request;
            TaskRequestFunctionCallback?.Invoke(request, parameters);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), ct);
            var completedTask = await Task.WhenAny(RequestedTaskResponse.Task, timeoutTask);

            if (ct.IsCancellationRequested)
                return string.Empty;

            bool timedOut = (completedTask == timeoutTask);
            bool isSuccess = false;
            string text = string.Empty;

            if (!timedOut)
            {
                var result = RequestedTaskResponse.Task.Result;
                isSuccess = result.IsSuccess;
                text = result.Text;
            }

            return BuildAnswer(request, timedOut, isSuccess, text);
        }

        /// <summary>
        /// Formats every response as a uniform JSON envelope:
        /// {"status":"ok"} | {"status":"ok","data":...} | {"status":"error","reason":...}.
        /// For commands that already produce JSON (help/status/symbols) the payload is embedded
        /// raw; otherwise the text is embedded as a JSON string.
        /// </summary>
        private string BuildAnswer(RequestTasks request, bool timedOut, bool isSuccess, string text)
        {
            if (timedOut)
                return BuildErrorJson("timeout");
            if (!isSuccess)
                return BuildErrorJson(string.IsNullOrEmpty(text) ? "failed" : text);
            if (string.IsNullOrEmpty(text))
                return "{\"status\":\"ok\"}";
            if (IsJsonProducing(request))
                return "{\"status\":\"ok\",\"data\":" + text + "}";
            return "{\"status\":\"ok\",\"data\":" + Json.Str(text) + "}";
        }

        private static string BuildErrorJson(string reason)
            => "{\"status\":\"error\",\"reason\":" + Json.Str(reason) + "}";

        private static bool IsJsonProducing(RequestTasks request)
            => request == RequestTasks.Help
            || request == RequestTasks.Status
            || request == RequestTasks.Symbols
            || request == RequestTasks.LogHeader
            || request == RequestTasks.View
            || request == RequestTasks.ResyncMap;

        /// <summary>
        /// Machine-readable catalog of the remote commands. RemoteControl owns the command
        /// list, so this is the single source of truth for `help`. Output is single-line JSON.
        /// </summary>
        public static string BuildHelpJson()
        {
            // name, args, summary, precondition, stopsLogging
            var cmds = new(string name, string args, string summary, string requires, bool stopsLogging)[]
            {
                (CommandConnect, "(none)=inherit | <pass>,Serial,<Byte2|Byte4>,<baud>,<comPort> | <pass>,LocalNet,<Byte2|Byte4>,<baud>,<ip>,<port>", "Connect to the MCU comm resource. Serial uses a COM port; LocalNet (TCP) takes ip and port separately. <pass> may be hex (0x...) or decimal. The connection target (COM port / ip / port / baud) is environment-specific and NOT discoverable - ask the user, do not scan or guess. No args inherits the current settings.", "", false),
                (CommandDisconnect, "", "Disconnect the comm resource. Releases the serial/TCP link (e.g. to let another tool flash the MCU); commonly paired with logstop.", "", false),
                (CommandOpenView, "<rmxml path>", "Open a view-setting (.rmxml) file by path. Side effect: always resets the symbol map to one self-seeded from this view's own addresses, discarding any map previously loaded via openmap/uploadmapdata/resyncmap - call resyncmap or openmap/uploadmapdata again afterward if you need a real map back.", "not while logging", false),
                (CommandPage, "<page name>", "Switch the active view page (exact-match group name). Does NOT push the new page's log set or start logging by itself - call config, then logstart, if you want to log the new page's variables.", "not while logging", false),
                (CommandSymbolInfo, "<symbol>", "Resolve a symbol to 'address,offset,size'.", "", false),
                (CommandInitialize, "", "Handshake; returns the firmware version string.", "comm open", false),
                (CommandConfig, "", "Push the current page's checked variables as the log set. Stops logging if active (see the top-level 'logging' note).", "comm open", true),
                (CommandTimeStep, "<milliseconds, 1-1000>", "Set the logging sample period (the interval between log frames). Stops logging if active (see the top-level 'logging' note). A value shorter than what the current baud rate and log set can physically transmit will not be achieved - check status().estTxTimeMinMs/estTxTimeMaxMs (after config) before picking one.", "comm open", true),
                (CommandLogStart, "", "Start MCU-side logging. Logging then persists (kept alive automatically) - just keep calling logread/logreadall; logstop is NOT needed to continue. To read a value that's already in the log set, use logread/logreadall instead of dump.", "comm open", false),
                (CommandLogStop, "", "Stop logging. Only when you want to stop; commonly paired with close.", "", false),
                (CommandLogRead, "", "Dequeue one buffered log row (tab-separated).", "logging", false),
                (CommandLogReadAll, "", "Drain all queued log rows in one response (rows joined by '\\n'; 'OK' alone = empty). Buffered rows are capped at status().logLength (default 1000): once full, the OLDEST undrained rows are silently discarded (no status marker) to make room for new ones - drain frequently enough relative to timestep to avoid gaps during long unattended monitoring.", "logging", false),
                (CommandLogWrite, "<addr>,<size>,<Type>,<value> | <symbol>,<Type>,<value> | <Name>", "Write a value to MCU memory. <Name> refers to a DataSetting row's Name field in the currently loaded view (a preset macro, e.g. a trigger/enable toggle) - NOT a row number. It writes that row's own preset Write value; no separate address/type/value needed. Value format: Hex takes an optional 0x prefix; Bin/Dec/UsD are plain digits; FLT/DBL are plain decimal numbers (e.g. 3.14), not hex bit patterns. Any field size (1/2/4/8 bytes) works the same way - byte order is handled internally; you never need to think about it. 'ok' means the write request was accepted and queued for transmission - it does NOT confirm the MCU received or applied it. There is no write acknowledgment tracked back to this response. To verify a write took effect, read the value back independently (logread/logreadall if it's in the current log set, or dump).", "logging", false),
                (CommandDump, "<hexAddr>,<size> | <symbol>", "Read <size> bytes from an address; returns hex. A single instantaneous snapshot with NO timestamp and NO correlation across multiple dump calls - not a time series. For observing one or more variables over time (or together at a shared timebase), use logstart + logread/logreadall instead. Stops logging if active (see the top-level 'logging' note).", "comm open", true),
                (CommandHelp, "", "This command catalog (JSON).", "", false),
                (CommandStatus, "", "Current session state (JSON). taskLoopActive: the comm task loop is running - by itself this does NOT mean the MCU is connected (a remote session never queues the full open/init/config/timestep/logstart sequence a Local GUI session does). portOpen: the underlying serial/TCP resource is open, independent of taskLoopActive - the closest available signal to \"is the MCU link actually up\". logging/currentPageIndex/currentPage/pageCount: current session/view state. timestep: the configured logging period (ms) - see timestep's own entry for the estTxTimeMinMs/estTxTimeMaxMs constraint on it. logLength: the log-row buffer cap referenced by logreadall's overflow behavior. viewPath/mapPath/mapDate: the loaded .rmview/.rmmap paths and the map's last-written time. targetVersion: the firmware version expected by the loaded view. receivedVersion: the version actually received via init - cleared only on connect, so a non-empty value can be a stale leftover from a previous session rather than proof the MCU is connected now.", "", false),
                (CommandSymbols, "", "Known symbol table as symbol,address,offset,size (JSON).", "", false),
                (CommandLogHeader, "", "Column labels for log rows: {columns:[...]} aligned with logread/logreadall. Valid types: Bin, Dec, UsD, Hex, FLT, DBL. The first column ('Status') uses a separate meta-type, 'Str' (plain text) - not one of the 6 value types above, and not usable as a view row's own Type. 'Received Time' = host arrival time (seconds, relative to logging start); 'Nominal Time' = the ideal sample time computed from the configured timestep (also seconds) - the two normally diverge slightly due to host-side scheduling jitter, which is not itself an anomaly. Status column values: 'OK' (normal); '<N> messages might be lost' (a gap was detected in the MCU's frame sequence numbering - N frames presumed lost before this row); 'Timeout' (no frame received for 5s - a single synthetic row with zero times). A frame that fails CRC/framing validation is silently dropped (produces no row at all), not reported as a distinct status.", "", false),
                (CommandView, "", "Current view page's rows in display order (row is 1-based; includes blank/unchecked rows) with symbol, name, address, offset, size, type, checked, write(macro), description (JSON). Valid types: Bin, Dec, UsD, Hex, FLT, DBL - an unrecognized type disables the row (checked:false) rather than silently reinterpreting it. Read-only and safe during logging.", "", false),
                (CommandUploadViewData, "<base64 rmxml>", "Apply a view-setting from uploaded content (no shared filesystem). Side effect: always resets the symbol map to one self-seeded from this view's own addresses, discarding any map previously loaded via openmap/uploadmapdata/resyncmap - call resyncmap or openmap/uploadmapdata again afterward if you need a real map back.", "not while logging", false),
                (CommandDownloadViewData, "", "Current LIVE view state (all pages) as base64 rmxml, byte-identical to File > Save. Use to safely round-trip an edit via uploadviewdata without touching disk / risking unsaved GUI changes.", "", false),
                (CommandOpenMap, "<rmmap path>", "Open an address map by path; enables file-mtime auto-resync.", "", false),
                (CommandUploadMapData, "<base64 rmmap>", "Load an address map from uploaded content; populates the symbol table.", "", false),
                (CommandResyncMap, "", "Re-resolve every page's row addresses/offset/size against the currently loaded map (openmap/uploadmapdata); returns {updated, unresolved:[{page,row,symbol}]}. Unresolved rows are unchecked (same safety rule as loading a map). This is the explicit replacement for the GUI's automatic resync-on-Comm-Open, which does NOT run while in remote mode.", "", false),
            };

            var sb = new StringBuilder();
            sb.Append("{\"protocol\":\"RM Classic remote\",\"delimiter\":\"\\r\\n\",");
            sb.Append("\"about\":").Append(Json.Str(
                "RM Classic remote control: read/write live variables in a connected MCU " +
                "over serial/UART (or TCP), and stream them as time-series logs.")).Append(",");
            // Breadcrumb to the RM Classic repository (source, releases, protocol spec).
            sb.Append("\"repo\":").Append(Json.Str("https://github.com/NaoNaoMe/RM-Classic")).Append(",");
            // Stated once here instead of repeated per command.
            sb.Append("\"logging\":").Append(Json.Str(
                "Typical usage: call logstart ONCE near the start of your session and leave " +
                "logging running continuously - logread/logreadall/logwrite all work freely " +
                "while it's active; logstop is NOT needed between operations, only when you " +
                "are actually done. Only logstart starts logging; it is never auto-resumed. " +
                "config/timestep/dump each stop logging if it was active (the MCU itself " +
                "requires a fresh logstart after any of them) - call logstart again afterward " +
                "if you need it back.")).Append(",");
            sb.Append("\"response\":\"{status:ok[,data]} | {status:error,reason}\",\"commands\":[");
            for (int i = 0; i < cmds.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{\"name\":").Append(Json.Str(cmds[i].name));
                sb.Append(",\"args\":").Append(Json.Str(cmds[i].args));
                sb.Append(",\"summary\":").Append(Json.Str(cmds[i].summary));
                sb.Append(",\"requires\":").Append(Json.Str(cmds[i].requires));
                sb.Append(",\"stopsLogging\":").Append(cmds[i].stopsLogging ? "true" : "false");
                sb.Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

    }

}
