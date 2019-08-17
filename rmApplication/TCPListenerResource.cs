using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace rmApplication
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

    class TCPListenerResource
    {
        public bool IsClientConnected
        {
            get
            {
                if (client == null)
                    return false;

                return client.Connected;

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

            if ((listener == null) ||
                (client == null))
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

        public async Task<bool> AcceptClientAsync(CancellationToken ct)
        {
            bool isSuccess = false;

            using (ct.Register(listener.Stop))
            {
                try
                {
                    receivedTextBuffer = new StringBuilder();

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
            List<byte> bytes = new List<byte>(); ;

            if (client == null)
                return bytes;

            if (!client.Connected)
                return bytes;

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                if (stream.DataAvailable)
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
                client = null;
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
            try
            {
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                listener.Stop();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

        }

    }

}
