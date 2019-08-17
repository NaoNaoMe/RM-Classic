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

        public string Delimiter;

        private StringBuilder receivedTextBuffer;
        private TcpClient client;

        public TCPClientResource()
        {
            Delimiter = string.Empty;
        }

        public async Task<bool> WriteAsync(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            var bytes = new List<Byte>(Encoding.UTF8.GetBytes(text + Delimiter));

            isSuccess = await WriteAsync(bytes);

            return isSuccess;
        }

        public async Task<bool> WriteAsync(List<byte> bytes)
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

                await stream.WriteAsync(txBuff, 0, txBuff.Length);

                isSuccess = true;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);

            }

            return isSuccess;
        }

        public async Task<bool> ConnectAsync(System.Net.IPAddress ip, int port, CancellationToken ct)
        {
            bool isSuccess = false;

            try
            {
                receivedTextBuffer = new StringBuilder();

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
            if (receivedTextBuffer != null)
                receivedTextBuffer.Clear();

            if (client == null)
                return;

            if (!client.Connected)
                return;

            try
            {
                while (true)
                {
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];

                    if (stream.DataAvailable)
                    {
                        var readBytes = stream.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        break;
                    }

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

        public List<byte> ReadBytes()
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

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

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
                System.Diagnostics.Debug.WriteLine("Canceled ReadBytesAsync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public void Disconncet()
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }

        }

    }
}
