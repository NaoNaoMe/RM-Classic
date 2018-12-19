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
        public bool IsOpen
        {
            get
            {
                if (comm == null)
                    return false;

                return comm.IsOpen;

            }
        }

        private SerialPort comm;

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

        public bool Open(string commPort, int commBaudRate)
        {
            bool isOpen = false;

            if ((string.IsNullOrEmpty(commPort) == false) &&
                (commBaudRate != 0))
            {
                comm = new SerialPort();

                comm.PortName = commPort;
                comm.BaudRate = commBaudRate;

                comm.DataBits = 8;
                comm.Parity = Parity.None;
                comm.StopBits = StopBits.One;
                comm.Handshake = Handshake.None;

                comm.Encoding = Encoding.ASCII;

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
            int size = comm.BytesToRead;

            if (size > 0)
            {
                var tmp = new byte[size];
                comm.Read(tmp, 0, size);
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
            string text = string.Empty;

            var bytes = await ReadBytesAsync(ct);

            text = Encoding.UTF8.GetString(bytes.ToArray());

            return text;
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

    }
}
