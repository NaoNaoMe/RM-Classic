using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SerialTerminal
{
    public class SerialPortResource : IDisposable
    {
        // https://stackoverflow.com/questions/3742022/c-sharp-event-with-custom-arguments
        public enum CtsDsrEvents
        {
            CtsHighEvent,
            CtsLowEvent,
            DsrHighEvent,
            DsrLowEvent
        }

        public class CtsDsrEventArgs : EventArgs
        {
            public CtsDsrEvents CtsDsrEvent { get; set; }
        }

        public struct Parameters
        {
            public string PortName;
            public int BaudRate;
            public Handshake HandShake;
            public int DataBits;
            public Parity Parity;
            public StopBits StopBits;
        };

        public bool IsOpen
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.IsOpen;

            }
        }

        public bool DtrEnable
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.DtrEnable;
            }

            set
            {
                if (comm == null)
                    return;
                if (!comm.IsOpen)
                    return;

                comm.DtrEnable = value;

            }
        }

        public bool RtsEnable
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.RtsEnable;
            }

            set
            {
                if (comm == null)
                    return;
                if (!comm.IsOpen)
                    return;

                comm.RtsEnable = value;

            }
        }

        public bool DsrHolding
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.DsrHolding;
            }
        }

        public bool CtsHolding
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.CtsHolding;
            }
        }

        public string Delimiter;
        public event EventHandler<CtsDsrEventArgs> EventPinChanged;

        private StringBuilder receivedTextBuffer;
        private SerialPort comm;
        private bool disposedValue;

        public SerialPortResource()
        {
            receivedTextBuffer = new StringBuilder();
            Delimiter = string.Empty;
        }

        ~SerialPortResource()
        {
            Close();
        }

        public bool Write(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            var bytes = Encoding.UTF8.GetBytes(text + Delimiter);

            isSuccess = Write(bytes);

            return isSuccess;
        }

        public bool Write(byte[] bytes)
        {
            bool isSuccess = false;

            if (comm == null)
                return isSuccess;

            if (!comm.IsOpen)
                return isSuccess;

            comm.Write(bytes, 0, bytes.Length);
            isSuccess = true;

            return isSuccess;
        }

        public bool Open(Parameters param)
        {
            if (string.IsNullOrEmpty(param.PortName) ||
                param.BaudRate == 0)
            {
                return false;
            }

            receivedTextBuffer.Clear();

            comm = new SerialPort();

            comm.PortName = param.PortName;
            comm.BaudRate = param.BaudRate;

            comm.DataBits = param.DataBits;
            comm.Parity = param.Parity;
            comm.StopBits = param.StopBits;
            comm.Handshake = param.HandShake;

            comm.Encoding = Encoding.ASCII;

            comm.PinChanged += PinChanged;

            comm.Open();

            return true;
        }

        public void PurgeReceiveBuffer()
        {
            receivedTextBuffer.Clear();

            if (comm == null)
                return;

            if (!comm.IsOpen)
                return;

            int size = comm.BytesToRead;

            if (size > 0)
            {
                var tmp = new byte[size];
                comm.Read(tmp, 0, size);
            }
        }

        public byte[] ReadBytes()
        {
            byte[] bytes = new byte[0];

            if (comm == null)
                return bytes;

            if (!comm.IsOpen)
                return bytes;

            int size = comm.BytesToRead;

            if (size > 0)
            {
                bytes = new byte[size];
                comm.Read(bytes, 0, size);
            }

            return bytes;
        }

        private string ExtractDelimitedString(byte[] newBytes)
        {
            if (newBytes != null && newBytes.Length > 0)
                receivedTextBuffer.Append(Encoding.UTF8.GetString(newBytes));

            string output = string.Empty;
            var firstOccurrence = receivedTextBuffer.ToString().IndexOf(Delimiter);

            if (firstOccurrence != -1)
            {
                output = receivedTextBuffer.ToString().Substring(0, firstOccurrence);

                var length = firstOccurrence + Delimiter.Length;

                if (length > 0)
                    receivedTextBuffer.Remove(0, length);

            }

            return output;
        }

        public async Task<string> ReadTextAsync(CancellationToken ct)
        {
            var bytes = new byte[0];

            if (string.IsNullOrEmpty(Delimiter))
            {
                bytes = await ReadBytesAsync(ct);
                return Encoding.UTF8.GetString(bytes);
            }

            string output = string.Empty;

            do
            {
                output = ExtractDelimitedString(bytes);
                if (!string.IsNullOrEmpty(output))
                    break;

                bytes = await ReadBytesAsync(ct);

            } while (true);

            return output;
        }

        public async Task<byte[]> ReadBytesAsync(CancellationToken ct)
        {
            return await ReadBytesAsync(0, ct);
        }

        public async Task<byte[]> ReadBytesAsync(int timeout, CancellationToken ct)
        {
            Exception exception = new Exception();
            byte[] bytes = new byte[0];

            if (comm == null)
                return bytes;

            if (!comm.IsOpen)
                return bytes;

            var timeoutSW = new System.Diagnostics.Stopwatch();
            if (timeout > 0)
                timeoutSW.Restart();
            else
                timeout = 1;

            while (true)
            {
                if (timeoutSW.ElapsedMilliseconds >= timeout)
                    break;

                int size = comm.BytesToRead;

                if (size > 0)
                {
                    bytes = new byte[size];
                    comm.Read(bytes, 0, size);
                    break;

                }

                ct.ThrowIfCancellationRequested();

                await Task.Delay(1);
            }

            return bytes;
        }

        public void Close()
        {
            if (comm == null)
                return;

            if (comm.IsOpen == true)
                comm.Close();
            comm.Dispose();

        }

        private void PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if (e.EventType == SerialPinChange.Break)
            {
            }
            else if (e.EventType == SerialPinChange.CDChanged)
            {
            }
            else if (e.EventType == SerialPinChange.CtsChanged)
            {
                if (comm.CtsHolding)
                    EventPinChanged?.Invoke(null, new CtsDsrEventArgs { CtsDsrEvent = CtsDsrEvents.CtsHighEvent });
                else
                    EventPinChanged?.Invoke(null, new CtsDsrEventArgs { CtsDsrEvent = CtsDsrEvents.CtsLowEvent });

            }
            else if (e.EventType == SerialPinChange.DsrChanged)
            {
                if (comm.DsrHolding)
                    EventPinChanged?.Invoke(null, new CtsDsrEventArgs { CtsDsrEvent = CtsDsrEvents.DsrHighEvent });
                else
                    EventPinChanged?.Invoke(null, new CtsDsrEventArgs { CtsDsrEvent = CtsDsrEvents.DsrLowEvent });

            }
            else if (e.EventType == SerialPinChange.Ring)
            {

            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SerialPortResource()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
