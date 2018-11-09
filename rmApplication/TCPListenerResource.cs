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
        private TcpListenerEx listener;
        private TcpClient client;
        private bool crlfEnable;

        public TCPListenerResource(bool isCRLF)
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

            if ((listener == null) ||
                (client == null) )
                return isSuccess;

            if (!client.Connected)
                return isSuccess;

            try
            {
                System.Net.Sockets.NetworkStream stream = client.GetStream();

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

        public async Task<bool> AcceptClientAsync(CancellationToken ct)
        {
            bool isSuccess = false;

            using (ct.Register(listener.Stop))
            {
                try
                {
                    client = new System.Net.Sockets.TcpClient();
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

        public string ReadTextClient()
        {
            string text = string.Empty;

            var bytes = ReadBytesClient();

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

        public List<byte> ReadBytesClient()
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

        public async Task<string> ReadTextClientAsync(CancellationToken ct)
        {
            string text = string.Empty;

            while(true)
            {
                var bytes = await ReadBytesClientAsync(ct);

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

        public async Task<List<byte>> ReadBytesClientAsync(CancellationToken ct)
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
                System.Diagnostics.Debug.WriteLine("Canceled ReadBytesClientAsync");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return bytes;
        }

        public bool IsClientConnected()
        {
            if (client == null)
                return false;

            return client.Connected;
        }

        public void CloseClient()
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

        public bool IsListenerActive()
        {
            if (listener == null)
                return false;

            return listener.Active;
            
        }

    }

}
