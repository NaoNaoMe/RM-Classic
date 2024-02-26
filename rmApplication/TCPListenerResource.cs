using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace TcpListenerTerminal
{
    //https://stackoverflow.com/questions/7630094/is-there-a-property-method-for-determining-if-a-tcplistener-is-currently-listeni
    /// <summary>
    /// Wrapper around TcpListener that exposes the Active property
    /// </summary>
    public class TcpListenerEx : System.Net.Sockets.TcpListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class with the specified local endpoint.
        /// </summary>
        /// <param name="localEP">An <see cref="T:System.Net.IPEndPoint"/> that represents the local endpoint to which to bind the listener <see cref="T:System.Net.Sockets.Socket"/>. </param><exception cref="T:System.ArgumentNullException"><paramref name="localEP"/> is null. </exception>
        public TcpListenerEx(System.Net.IPEndPoint localEP) : base(localEP)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Net.Sockets.TcpListener"/> class that listens for incoming connection attempts on the specified local IP address and port number.
        /// </summary>
        /// <param name="localaddr">An <see cref="T:System.Net.IPAddress"/> that represents the local IP address. </param><param name="port">The port on which to listen for incoming connection attempts. </param><exception cref="T:System.ArgumentNullException"><paramref name="localaddr"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="port"/> is not between <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="F:System.Net.IPEndPoint.MaxPort"/>. </exception>
        public TcpListenerEx(System.Net.IPAddress localaddr, int port) : base(localaddr, port)
        {
        }

        public new bool Active
        {
            get { return base.Active; }
        }
    }

    public class TCPListenerResource
    {
        public bool IsClientConnected
        {
            get
            {
                if (client == null)
                    return false;

                try
                {
                    return client.Connected;
                }
                catch (NullReferenceException)
                {
                    return false;
                }
            }
        }

        public bool IsListenerActive
        {
            get
            {
                if (listener == null)
                    return false;

                return listener.Active;

            }
        }

        public string Delimiter;

        private StringBuilder receivedTextBuffer;
        private TcpListenerEx listener;
        private TcpClient client;

        public TCPListenerResource()
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

            if ((listener == null) ||
                (client == null))
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

        public async Task<bool> AcceptClientAsync(CancellationToken ct)
        {
            bool isSuccess = false;

            using (ct.Register(listener.Stop))
            {
                try
                {
                    receivedTextBuffer.Clear();

                    client = new TcpClient();
                    client = await listener.AcceptTcpClientAsync();
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }

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
                System.Diagnostics.Debug.WriteLine("Canceled ReadBytesClientAsync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public void DisconnectClient()
        {
            if (client != null)
            {
                client.Close();
                client.Dispose();
            }

        }

        public bool StartListening(System.Net.IPAddress ip, int port)
        {
            bool isSuccess = false;
            try
            {
                listener = new TcpListenerEx(ip, port);
                listener.Start();
                isSuccess = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return isSuccess;
        }

        public void StopListening()
        {
            DisconnectClient();

            try
            {
                listener.Stop();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

    }

}
