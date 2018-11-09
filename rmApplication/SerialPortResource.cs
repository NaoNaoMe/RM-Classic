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
        private SerialPort port;

        public SerialPortResource()
        {

        }

        public bool Push(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            var bytes = new List<Byte>(Encoding.UTF8.GetBytes(text));

            isSuccess = Push(bytes);

            return isSuccess;
        }

        public bool Push(List<byte> bytes)
        {
            bool isSuccess = false;

            if (port == null)
                return isSuccess;

            if (!port.IsOpen)
                return isSuccess;

            try
            {
                port.Write(bytes.ToArray(), 0, bytes.Count());
                isSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return isSuccess;
        }

        public bool Open(string commPort, int commBaudRate)
        {
            bool isOpen = false;

            if ((string.IsNullOrEmpty(commPort) == false) &&
                (commBaudRate != 0))
            {
                port = new SerialPort();

                port.PortName = commPort;
                port.BaudRate = commBaudRate;

                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Handshake = Handshake.None;

                port.Encoding = Encoding.ASCII;

                try
                {
                    port.Open();

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
            int size = port.BytesToRead;

            if (size > 0)
            {
                var tmp = new byte[size];
                port.Read(tmp, 0, size);
            }
        }

        public string ReadText()
        {
            string text = string.Empty;

            var bytes = ReadBytes();

            text = Encoding.UTF8.GetString(bytes.ToArray());

            return text;
        }

        public List<byte> ReadBytes()
        {
            List<byte> bytes = new List<byte>(); ;

            if (port == null)
                return bytes;

            if (!port.IsOpen)
                return bytes;

            try
            {
                int size = port.BytesToRead;

                if (size > 0)
                {
                    var tmp = new byte[size];
                    port.Read(tmp, 0, size);

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
            string text = string.Empty;

            var bytes = await ReadBytesAsync(ct);

            text = Encoding.UTF8.GetString(bytes.ToArray());

            return text;
        }

        public async Task<List<byte>> ReadBytesAsync(CancellationToken ct)
        {
            List<byte> bytes = new List<byte>();

            if (port == null)
                return bytes;

            if (!port.IsOpen)
                return bytes;

            await Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        ct.ThrowIfCancellationRequested();

                        int size = port.BytesToRead;

                        if (size > 0)
                        {
                            var tmp = new byte[size];
                            port.Read(tmp, 0, size);

                            foreach (var item in tmp)
                            {
                                bytes.Add(item);
                            }

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
                }
            }
            );

            return bytes;
        }

        public bool IsOpen()
        {
            if (port == null)
                return false;

            return port.IsOpen;
        }

        public void Close()
        {
            try
            {
                if (port.IsOpen == true)
                    port.Close();
                port.Dispose();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

            }

        }

    }
}
