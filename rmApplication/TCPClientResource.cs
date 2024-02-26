using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TcpClientTerminal
{
    public class TCPClientResource
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
            receivedTextBuffer = new StringBuilder();
            Delimiter = string.Empty;
        }

        public async Task<bool> WriteAsync(string text)
        {
            bool isSuccess = false;

            if (string.IsNullOrEmpty(text))
                return isSuccess;

            var bytes = Encoding.UTF8.GetBytes(text + Delimiter);

            isSuccess = await WriteAsync(bytes);

            return isSuccess;
        }

        public async Task<bool> WriteAsync(byte[] bytes)
        {
            bool isSuccess = false;

            if (client == null)
                return isSuccess;

            if (!client.Connected)
                return isSuccess;

            try
            {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(bytes, 0, bytes.Length);

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
                receivedTextBuffer.Clear();

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

        public byte[] ReadBytes()
        {
            byte[] bytes = new byte[0];

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                bytes = new byte[1024];

                if (stream.DataAvailable)
                {
                    var readBytes = stream.Read(bytes, 0, bytes.Length);

                    if (readBytes == 0)
                    {
                        client.Close();
                        client.Dispose();
                    }
                    else
                    {
                        Array.Resize(ref bytes, readBytes);
                    }

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
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

                if (bytes.Length == 0)
                    break;

                if (ct.IsCancellationRequested)
                    break;

                await Task.Delay(10);

            } while (true);

            return output;
        }

        public async Task<byte[]> ReadBytesAsync(CancellationToken ct)
        {
            byte[] bytes = new byte[0];

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                bytes = new byte[1024];

                var readTask = stream.ReadAsync(bytes, 0, bytes.Length, ct);
                var cancellationTask = Task.Delay(-1, ct);
                var completedTask = await Task.WhenAny(readTask, cancellationTask);

                if (completedTask == cancellationTask)
                {
                    client.Close();
                    client.Dispose();
                }
                else
                {
                    var readBytes = readTask.Result;
                    if (readBytes == 0)
                    {
                        client.Close();
                        client.Dispose();
                    }
                    else
                    {
                        Array.Resize(ref bytes, readBytes);
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
                client.Dispose();
            }

        }

    }
}
