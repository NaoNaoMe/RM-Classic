using System;
using System.Windows.Forms;

namespace rmApplication
{
    /// <summary>
    /// Severity of a user notification.
    /// </summary>
    public enum InteractionSeverity { Info, Warning, Error }

    /// <summary>
    /// Abstraction over every user dialog so that headless (remote) sessions never block.
    /// Route all notifications and confirmations through this interface instead of calling
    /// MessageBox directly; that keeps the remote/headless policy in one place and makes any
    /// future dialog remote-safe by construction.
    /// </summary>
    public interface IUserInteraction
    {
        /// <summary>Fire-and-forget message. No-op in headless mode.</summary>
        void Notify(string message, InteractionSeverity severity = InteractionSeverity.Warning);

        /// <summary>
        /// Yes/No question. In headless mode this returns <paramref name="remoteDefault"/>
        /// without showing any dialog, so the caller picks the safe default per call site.
        /// </summary>
        bool Confirm(string message, bool remoteDefault);
    }

    /// <summary>
    /// Interactive implementation backed by WinForms MessageBox (local GUI mode).
    /// </summary>
    public sealed class GuiInteraction : IUserInteraction
    {
        public void Notify(string message, InteractionSeverity severity = InteractionSeverity.Warning)
        {
            // A timestamp is appended automatically (preserves the previous ShowWarning behavior).
            string dateTime = DateTime.Now.ToString("MM/dd HH:mm:ss");
            MessageBox.Show(
                message + Environment.NewLine + dateTime,
                CaptionFor(severity),
                MessageBoxButtons.OK,
                IconFor(severity));
        }

        public bool Confirm(string message, bool remoteDefault)
        {
            // remoteDefault is ignored in GUI mode; the user answers. Default button is No (safe).
            var result = MessageBox.Show(
                message,
                "Question",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation,
                MessageBoxDefaultButton.Button2);

            return result == DialogResult.Yes;
        }

        private static string CaptionFor(InteractionSeverity severity)
        {
            switch (severity)
            {
                case InteractionSeverity.Error: return "Error";
                case InteractionSeverity.Info: return "Information";
                default: return "Caution";
            }
        }

        private static MessageBoxIcon IconFor(InteractionSeverity severity)
        {
            switch (severity)
            {
                case InteractionSeverity.Error: return MessageBoxIcon.Error;
                case InteractionSeverity.Info: return MessageBoxIcon.Information;
                default: return MessageBoxIcon.Warning;
            }
        }
    }

    /// <summary>
    /// Non-interactive implementation for headless (remote) sessions: notifications are
    /// dropped (logged to the debugger only) and confirmations return the caller's default.
    /// </summary>
    public sealed class HeadlessInteraction : IUserInteraction
    {
        public void Notify(string message, InteractionSeverity severity = InteractionSeverity.Warning)
        {
            System.Diagnostics.Debug.WriteLine($"[Notify:{severity}] {message}");
        }

        public bool Confirm(string message, bool remoteDefault)
        {
            System.Diagnostics.Debug.WriteLine($"[Confirm->{remoteDefault}] {message}");
            return remoteDefault;
        }
    }
}
