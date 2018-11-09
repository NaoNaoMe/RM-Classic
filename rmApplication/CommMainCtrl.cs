using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rmApplication
{
    public class CommMainCtrl
    {
        public enum CommunicationMode : int
        {
            Serial = 0,
            LocalNet
        };

        public bool IsOpen { get; private set; }

        private CommProtocol myCommProtocol;

        private SerialPortResource mySerialPort;
        private TCPClientResource myTCPClient;

        private Queue<byte> rxFIFO;

        private Configuration mySettings;

        private Queue<string> messageLog;

        public CommMainCtrl(Configuration tmp)
        {
            mySettings = new Configuration(tmp);
            myCommProtocol = new CommProtocol();
            rxFIFO = new Queue<byte>();

            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    mySerialPort = new SerialPortResource();
                    break;

                case CommunicationMode.LocalNet:
                    myTCPClient = new TCPClientResource(false);
                    break;

            }

            messageLog = new Queue<string>();

            IsOpen = false;
        }

        public async Task<bool> OpenAsync(CancellationToken ct)
        {
            IsOpen = false;

            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    if (mySerialPort != null)
                    {
                        if (mySerialPort.Open(mySettings.SerialPortName, mySettings.BaudRate))
                            IsOpen = true;

                    }
                    break;

                case CommunicationMode.LocalNet:
                    if (myTCPClient != null)
                    {
                        if (await myTCPClient.AcceptListenerAsync(mySettings.ClientAddress, mySettings.ClientPort, ct))
                            IsOpen = true;

                    }
                    break;

            }

            return IsOpen;
        }

        public void Close()
        {
            IsOpen = false;

            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    if (mySerialPort != null)
                        mySerialPort.Close();
                    break;

                case CommunicationMode.LocalNet:
                    if (myTCPClient != null)
                        myTCPClient.Close();
                    break;

            }

        }

        public void PurgeReceiveBuffer()
        {
#if false
            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    if (mySerialPort != null)
                        mySerialPort.PurgeReceiveBuffer();
                    break;

                case CommunicationMode.LocalNet:
                    if (myTCPClient != null)
                        myTCPClient.PurgeReceiveBuffer();
                    break;

            }

            rxFIFO.Clear();

#endif
        }

        public void Push(List<byte> bytes)
        {
            RecordMessage("Tx", bytes);

            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    if (mySerialPort != null)
                        mySerialPort.Push(myCommProtocol.Encode(bytes));
                    break;

                case CommunicationMode.LocalNet:
                    if (myTCPClient != null)
                        myTCPClient.Publish(myCommProtocol.Encode(bytes));
                    break;

            }

        }

        public List<byte> Pull()
        {
            List<byte> frame = new List<byte>();

            List<byte> bytes = new List<byte>();

            switch (mySettings.CommMode)
            {
                case CommunicationMode.Serial:
                    if (mySerialPort != null)
                        bytes = mySerialPort.ReadBytes();
                    break;

                case CommunicationMode.LocalNet:
                    if (myTCPClient != null)
                        bytes = myTCPClient.ReadBytesListener();
                    break;

            }

            foreach (var item in bytes)
            {
                rxFIFO.Enqueue(item);

            }

            while (rxFIFO.Count != 0)
            {
                frame = myCommProtocol.Decode(rxFIFO.Dequeue());

                if (frame.Count != 0)
                {
                    RecordMessage("Rx", frame);
                    break;
                }

            }

            return frame;
        }

        public async Task<List<byte>> PullAsync(CancellationToken ct)
        {
            List<byte> frame = new List<byte>();

            bool isBreak = false;

            try
            {
                while (true)
                {
                    while (rxFIFO.Count != 0)
                    {
                        frame = myCommProtocol.Decode(rxFIFO.Dequeue());

                        if (frame.Count != 0)
                        {
                            isBreak = true;
                            RecordMessage("Rx", frame);
                            break;
                        }

                    }

                    if (isBreak == true)
                    {
                        break;
                    }

                    List<byte> bytes = new List<byte>();

                    switch (mySettings.CommMode)
                    {
                        case CommunicationMode.Serial:
                            if (mySerialPort != null)
                                bytes = await mySerialPort.ReadBytesAsync(ct);

                            break;

                        case CommunicationMode.LocalNet:
                            if (myTCPClient != null)
                                bytes = await myTCPClient.ReadBytesListenerAsync(ct);
                            break;

                    }

                    foreach (var item in bytes)
                    {
                        rxFIFO.Enqueue(item);

                    }

                    if (ct.IsCancellationRequested == true)
                    {
                        break;
                    }

                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return frame;
        }

        private void RecordMessage(string dir, List<byte> bytes)
        {
            string newText = BitConverter.ToString(bytes.ToArray());

            string time = DateTime.Now.ToString("HH:mm:ss.fff");

            string logText = time + " " + dir + " " + newText;

            messageLog.Enqueue(logText);

            while (messageLog.Count > 1000)
                messageLog.Dequeue();

        }
    }
}
