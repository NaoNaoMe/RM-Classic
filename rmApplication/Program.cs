using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace rmApplication
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Any(a => a == "--help" || a == "-h" || a == "/?"))
            {
                ShowUsage();
                return;
            }

            AnnounceRemoteIfRequested(args);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }

        /// <summary>
        /// When started with --remote, print a one-line confirmation. A WinExe returns
        /// control to the launcher immediately while the GUI server keeps running in the
        /// background, which can look like a silent exit; this message makes the intent
        /// explicit and points to the real confirmation (connect and send `help`).
        /// </summary>
        private static void AnnounceRemoteIfRequested(string[] args)
        {
            if (args == null)
                return;

            int i = Array.IndexOf(args, "--remote");
            if (i < 0 || i + 2 >= args.Length)
                return;

            string ip = args[i + 1];
            string port = args[i + 2];

            WriteConsole(
                "RM Classic: opening the remote server on " + ip + ":" + port + ".\r\n" +
                "The GUI runs in the background and the launcher returns immediately (this is normal,\r\n" +
                "not an exit). Confirm it is up by connecting to " + ip + ":" + port + " and sending the line `help`.");
        }

        /// <summary>
        /// Prints command-line usage. See <see cref="WriteConsole"/> for how a WinExe
        /// reaches stdout/the launching console.
        /// </summary>
        private static void ShowUsage()
        {
            string usage =
                "RM Classic - real-time variable monitoring over a serial or TCP link.\r\n" +
                "\r\n" +
                "Usage:\r\n" +
                "  RM-Classic.exe                       Launch the GUI.\r\n" +
                "  RM-Classic.exe --remote <ip> <port>  Run the remote TCP command server\r\n" +
                "                                        (example: --remote 127.0.0.2 4001).\r\n" +
                "  RM-Classic.exe --help                Show this message.\r\n" +
                "\r\n" +
                "Launching --remote returns to the launcher immediately while the GUI server keeps\r\n" +
                "running in the background - that is expected, not a crash. Confirm it is up by\r\n" +
                "connecting to <ip>:<port> and sending the line `help` (commands are CRLF-terminated).\r\n" +
                "Responses are JSON: {\"status\":\"ok\"[,\"data\":...]} | {\"status\":\"error\",\"reason\":...}.";

            WriteConsole(usage);
        }

        /// <summary>
        /// Writes a line to stdout. This is a WinExe (no console of its own): if stdout is
        /// redirected, the inherited handle is used (capture with -RedirectStandardOutput);
        /// otherwise it attaches to the launching console so the text is visible there.
        /// </summary>
        internal static void WriteConsole(string text)
        {
            if (!Console.IsOutputRedirected)
                AttachConsole(ATTACH_PARENT_PROCESS);

            Console.WriteLine();
            Console.WriteLine(text);
        }
    }
}
