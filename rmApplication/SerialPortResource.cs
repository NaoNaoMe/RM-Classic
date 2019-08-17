using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace rmApplication
{
    class SerialPortResource
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

        public struct Parameters{
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


        public SerialPortResource()
        {
            Delimiter = string.Empty;
        }

        public bool Write(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            var bytes = new List<Byte>(Encoding.UTF8.GetBytes(text + Delimiter));

            isSuccess = Write(bytes);

            return isSuccess;
        }

        public bool Write(List<byte> bytes)
        {
            bool isSuccess = false;

            if (comm == null)
                return isSuccess;

            if (!comm.IsOpen)
                return isSuccess;

            try
            {
                comm.Write(bytes.ToArray(), 0, bytes.Count());
                isSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return isSuccess;
        }

        public bool Open(Parameters param)
        {
            bool isOpen = false;

            if ((string.IsNullOrEmpty(param.PortName) == false) &&
                (param.BaudRate != 0))
            {
                receivedTextBuffer = new StringBuilder();

                comm = new SerialPort();

                comm.PortName = param.PortName;
                comm.BaudRate = param.BaudRate;

                comm.DataBits = param.DataBits;
                comm.Parity = param.Parity;
                comm.StopBits = param.StopBits;
                comm.Handshake = param.HandShake;

                comm.Encoding = Encoding.ASCII;

                comm.PinChanged += PinChanged;

                try
                {
                    comm.Open();

                    isOpen = true;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);

                }

            }

            return isOpen;
        }

        public void PurgeReceiveBuffer()
        {
            if (receivedTextBuffer != null)
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

        public List<byte> ReadBytes()
        {
            List<byte> bytes = new List<byte>(); ;

            if (comm == null)
                return bytes;

            if (!comm.IsOpen)
                return bytes;

            try
            {
                int size = comm.BytesToRead;

                if (size > 0)
                {
                    var tmp = new byte[size];
                    comm.Read(tmp, 0, size);

                    foreach (var item in tmp)
                    {
                        bytes.Add(item);
                    }

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public async Task<string> ReadTextAsync(CancellationToken ct)
        {
            if (string.IsNullOrEmpty(Delimiter))
            {
                var bytes = await ReadBytesAsync(ct);
                return Encoding.UTF8.GetString(bytes.ToArray());
            }

            string output = string.Empty;
            string rawText = receivedTextBuffer.ToString();

            int firstOccurrence = rawText.IndexOf(Delimiter);

            if (firstOccurrence != -1)
            {
                output = rawText.Substring(0, firstOccurrence);
                receivedTextBuffer.Clear();

                var startindex = firstOccurrence + Delimiter.Length;
                var length = rawText.Length - startindex;

                if (length > 0)
                    receivedTextBuffer.Append(rawText.Substring(startindex, length));

                return output;
            }

            while (true)
            {
                var bytes = await ReadBytesAsync(ct);

                rawText = Encoding.UTF8.GetString(bytes.ToArray());

                if (string.IsNullOrEmpty(rawText))
                    break;

                firstOccurrence = rawText.IndexOf(Delimiter);

                if (firstOccurrence != -1)
                {
                    receivedTextBuffer.Append(rawText.Substring(0, firstOccurrence));
                    output = receivedTextBuffer.ToString();
                    receivedTextBuffer.Clear();

                    var startindex = firstOccurrence + Delimiter.Length;
                    var length = rawText.Length - startindex;

                    if (length > 0)
                        receivedTextBuffer.Append(rawText.Substring(startindex, length));

                    break;
                }

                receivedTextBuffer.Append(rawText);

            }

            return output;
        }

        public async Task<List<byte>> ReadBytesAsync(CancellationToken ct)
        {
            List<byte> bytes = new List<byte>();

            if (comm == null)
                return bytes;

            if (!comm.IsOpen)
                return bytes;

            await Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        int size = comm.BytesToRead;

                        if (size > 0)
                        {
                            var tmp = new byte[size];
                            comm.Read(tmp, 0, size);

                            foreach (var item in tmp)
                                bytes.Add(item);

                            break;

                        }

                        Thread.Sleep(1);

                    }

                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("Canceled ReadBytesAsync");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            );

            return bytes;
        }

        public void Close()
        {
            try
            {
                if (comm.IsOpen == true)
                    comm.Close();
                comm.Dispose();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

            }

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
                if(comm.CtsHolding)
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


    }
}
