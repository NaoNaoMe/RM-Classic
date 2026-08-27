using System;
using System.IO;

namespace rmApplication
{
    /// <summary>
    /// Minimal, file-based fallback for exceptions that would otherwise only reach
    /// Debug.WriteLine (invisible outside an attached debugger, i.e. in every Release/
    /// --remote run). Diagnostic-only: writing to this file must never itself throw and
    /// mask the original failure, so every failure mode here is swallowed silently.
    /// </summary>
    internal static class ErrorLog
    {
        private static readonly string LogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rmApplication_error.log");

        public static void Write(string context, Exception ex)
        {
            try
            {
                string line = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] " +
                              context + ": " + ex + Environment.NewLine;
                File.AppendAllText(LogPath, line);
            }
            catch
            {
            }
        }
    }
}
