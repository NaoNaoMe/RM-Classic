using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace rmApplication
{
    class TCPClientResource
    {
        public bool IsConnected
        {
            get
            {
                if (client == null)
                    return false;

                return client.Connected;

            }
        }

        private TcpClient client;
        private bool crlfEnable;

        public TCPClientResource(bool isCRLF)
        {
            crlfEnable = isCRLF;
        }

        public bool Publish(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            string terminator = string.Empty;
            if (crlfEnable)
                terminator = "\r\n";

            var bytes = new List<Byte>(Encoding.UTF8.GetBytes(text + terminator));

            isSuccess = Publish(bytes);

            return isSuccess;
        }

        public bool Publish(List<byte> bytes)
        {
            bool isSuccess = false;

            if (client == null)
                return isSuccess;

            if (!client.Connected)
                return isSuccess;

            try
            {
                NetworkStream stream = client.GetStream();

                var txBuff = bytes.ToArray();

                stream.Write(txBuff, 0, txBuff.Length);

                isSuccess = true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

            }

            return isSuccess;
        }

        public async Task<bool> AcceptListenerAsync(System.Net.IPAddress ip, int port, CancellationToken ct)
        {
            bool isSuccess = false;

            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ip, port).ContinueWith(t => t.IsCompleted, ct);
                isSuccess = client.Connected;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled AcceptListenerAsync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return isSuccess;
        }

        public void PurgeReceiveBuffer()
        {
            if (client == null)
                return;

            if (!client.Connected)
                return;

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                if (stream.DataAvailable)
                {
                    var readBytes = stream.Read(buffer, 0, buffer.Length);
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

        public string ReadTextListener()
        {
            string text = string.Empty;

            var bytes = ReadBytesListener();

            string rawText = Encoding.UTF8.GetString(bytes.ToArray());

            if (!crlfEnable)
            {
                text = rawText;
            }
            else if (rawText.Contains("\r\n"))
            {
                text = rawText.Substring(0, rawText.IndexOf("\r\n"));
            }

            return text;
        }

        public List<byte> ReadBytesListener()
        {
            List<byte> bytes = new List<byte>();

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                if(stream.DataAvailable)
                {
                    var readBytes = stream.Read(buffer, 0, buffer.Length);

                    if (readBytes != 0)
                    {
                        bytes = new List<byte>(buffer.ToList());

                        while (bytes.Count > readBytes)
                        {
                            bytes.RemoveAt(bytes.Count - 1);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public async Task<string> ReadTextListenerAsync(CancellationToken ct)
        {
            string text = string.Empty;

            while(true)
            {
                var bytes = await ReadBytesListenerAsync(ct);

                string rawText = Encoding.UTF8.GetString(bytes.ToArray());

                if (!crlfEnable)
                {
                    text = rawText;
                    break;
                }

                if (string.IsNullOrEmpty(rawText))
                {
                    text = string.Empty;
                    break;
                }

                if (rawText.Contains("\r\n"))
                {
                    text += rawText.Substring(0, rawText.IndexOf("\r\n"));
                    break;
                }
                else
                {
                    text += rawText;
                }

            }

            return text;
        }

        public async Task<List<byte>> ReadBytesListenerAsync(CancellationToken ct)
        {
            List<byte> bytes = new List<byte>();

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[256];

                var readTask = stream.ReadAsync(buffer, 0, buffer.Length, ct);
                await await Task.WhenAny(readTask, Task.Delay(-1, ct));

                var readBytes = readTask.Result;
                if (readBytes != 0)
                {
                    bytes = new List<byte>(buffer.ToList());

                    while (bytes.Count > readBytes)
                    {
                        bytes.RemoveAt(bytes.Count - 1);
                    }

                }

            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Canceled ReadBytesListenerAsync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public void Close()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

        }

    }
}
