using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SerialTerminal;
using TcpClientTerminal;

namespace rmApplication
{
    public class CommMainCtrl
    {
        public enum CommunicationMode : int
        {
            Serial = 0,
            LocalNet
        };

        public CommunicationMode Mode
        {
            get
            {
                return config.CommMode;
            }
        }

        public bool IsOpen
        {
            get
            {
                if (serialPort != null)
                    return serialPort.IsOpen;

                if (tcpClient != null)
                    return tcpClient.IsConnected;

                return false;
            }
        }

        private CommProtocol myCommProtocol;

        private SerialPortResource serialPort;
        private TCPClientResource tcpClient;

        private Queue<byte> rxFIFO;

        private Configuration config;

        public CommMainCtrl(Configuration tmp)
        {
            config = new Configuration(tmp);
            myCommProtocol = new CommProtocol();
            rxFIFO = new Queue<byte>();

            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    serialPort = new SerialPortResource();
                    break;

                case CommunicationMode.LocalNet:
                    tcpClient = new TCPClientResource();
                    break;

            }

        }

        public async Task<bool> OpenAsync(CancellationToken ct)
        {
            if (IsOpen)
                return false;

            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    if (serialPort != null)
                    {
                        SerialPortResource.Parameters param;

                        param.PortName = config.SerialPortName;
                        param.BaudRate = config.BaudRate;
                        param.HandShake = System.IO.Ports.Handshake.None;
                        param.DataBits = 8;
                        param.Parity = System.IO.Ports.Parity.None;
                        param.StopBits = System.IO.Ports.StopBits.One;

                        serialPort.Open(param);
                    }
                    break;

                case CommunicationMode.LocalNet:
                    if (tcpClient != null)
                    {
                        await tcpClient.ConnectAsync(config.ClientAddress, config.ClientPort, ct);
                    }
                    break;

            }

            return IsOpen;
        }

        public void Close()
        {
            rxFIFO.Clear();

            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    if (serialPort != null)
                        serialPort.Close();
                    break;

                case CommunicationMode.LocalNet:
                    if (tcpClient != null)
                        tcpClient.Disconncet();
                    break;

            }

        }

        public void PurgeReceiveBuffer()
        {
            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    if (serialPort != null)
                        serialPort.PurgeReceiveBuffer();
                    break;

                case CommunicationMode.LocalNet:
                    if (tcpClient != null)
                        tcpClient.PurgeReceiveBuffer();
                    break;

            }

            rxFIFO.Clear();
        }

        public async Task PushAsync(byte[] bytes)
        {
            OutputDebugMessage("Tx", bytes);

            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    if (serialPort != null)
                        serialPort.Write(myCommProtocol.Encode(bytes));
                    break;

                case CommunicationMode.LocalNet:
                    if (tcpClient != null)
                        await tcpClient.WriteAsync(myCommProtocol.Encode(bytes));
                    break;

            }

        }

        public byte[] Pull()
        {
            var frame = new byte[0];

            byte[] bytes = new byte[] { };

            switch (config.CommMode)
            {
                case CommunicationMode.Serial:
                    if (serialPort != null)
                        bytes = serialPort.ReadBytes();
                    break;

                case CommunicationMode.LocalNet:
                    if (tcpClient != null)
                        bytes = tcpClient.ReadBytes();
                    break;

            }

            foreach (var item in bytes)
            {
                rxFIFO.Enqueue(item);

            }

            while (rxFIFO.Count != 0)
            {
                frame = myCommProtocol.Decode(rxFIFO.Dequeue());

                if (frame.Length != 0)
                {
                    OutputDebugMessage("Rx", frame);
                    break;
                }

            }

            return frame;
        }

        public async Task<byte[]> PullAsync(CancellationToken ct)
        {
            var frame = new byte[0];

            bool isBreak = false;

            try
            {
                while (true)
                {
                    while (rxFIFO.Count != 0)
                    {
                        frame = myCommProtocol.Decode(rxFIFO.Dequeue());

                        if (frame.Length != 0)
                        {
                            isBreak = true;
                            OutputDebugMessage("Rx", frame);
                            break;
                        }

                    }

                    if (isBreak)
                        break;

                    byte[] bytes = new byte[] { };

                    switch (config.CommMode)
                    {
                        case CommunicationMode.Serial:
                            if (serialPort != null)
                                bytes = await serialPort.ReadBytesAsync(ct);

                            break;

                        case CommunicationMode.LocalNet:
                            if (tcpClient != null)
                                bytes = await tcpClient.ReadBytesAsync(ct);
                            break;

                    }

                    if(bytes.Length == 0)
                        break;

                    foreach (var item in bytes)
                        rxFIFO.Enqueue(item);

                    if (ct.IsCancellationRequested)
                        break;

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return frame;
        }

        private void OutputDebugMessage(string dir, byte[] bytes)
        {
            string newText = BitConverter.ToString(bytes);

            string time = DateTime.Now.ToString("HH:mm:ss.fff");

            string logText = time + " " + dir + " " + newText;

            System.Diagnostics.Debug.WriteLine(logText);

        }
    }
}
