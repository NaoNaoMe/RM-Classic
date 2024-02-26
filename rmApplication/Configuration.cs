using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rmApplication
{
    public class Configuration
    {
        public uint PassNumber { get; set; }
        public CommMainCtrl.CommunicationMode CommMode { get; set; }
        public CommInstructions.RmAddr RmRange { get; set; }
        public int BaudRate { get; set; }

        public string SerialPortName { get; set; }
        
        public System.Net.IPAddress ClientAddress { get; set; }
        public int ClientPort { get; set; }
        
        public System.Net.IPAddress ServerAddress { get; set; }
        public int ServerPort { get; set; }

        public Configuration()
        {
            PassNumber = 0x0000FFFF;

            CommMode = CommMainCtrl.CommunicationMode.Serial;
            RmRange = CommInstructions.RmAddr.Byte2;
            BaudRate = 0;

            SerialPortName = string.Empty;

            ClientAddress = System.Net.IPAddress.Parse("192.168.0.39");
            ClientPort = 23;

            ServerAddress = System.Net.IPAddress.Parse("127.0.0.2");
            ServerPort = 4001;

        }

        public Configuration(Configuration conf)
        {
            PassNumber = conf.PassNumber;

            CommMode = conf.CommMode;
            RmRange = conf.RmRange;
            BaudRate = conf.BaudRate;

            SerialPortName = conf.SerialPortName;

            ClientAddress = conf.ClientAddress;
            ClientPort = conf.ClientPort;

            ServerAddress = conf.ServerAddress;
            ServerPort = conf.ServerPort;

        }

        public bool ValidateCommunicationResource(string text, out Configuration config)
        {
            config = new Configuration();
            if (string.IsNullOrEmpty(text))
                return false;

            var factors = text.Split(',');

            if (factors.Count() < 4)
                return false;

            uint passNumber;
            if (uint.TryParse(factors[0], out passNumber))
                config.PassNumber = passNumber;
            else
                return false;

            CommMainCtrl.CommunicationMode mode;
            if (Enum.TryParse<CommMainCtrl.CommunicationMode>(factors[1], out mode))
                config.CommMode = mode;
            else
                return false;

            CommInstructions.RmAddr range;
            if (Enum.TryParse<CommInstructions.RmAddr>(factors[2], out range))
                config.RmRange = range;
            else
                return false;

            int baudRate;
            if (int.TryParse(factors[3], out baudRate))
                config.BaudRate = baudRate;
            else
                return false;

            if (mode == CommMainCtrl.CommunicationMode.Serial && factors.Count() == 5)
            {
                config.SerialPortName = factors[4];
                return true;

            }
            else if (mode == CommMainCtrl.CommunicationMode.LocalNet && factors.Count() == 6)
            {
                System.Net.IPAddress clientAddress;
                if (System.Net.IPAddress.TryParse(factors[4], out clientAddress))
                    config.ClientAddress = clientAddress;
                else
                    return false;

                int clientPort;
                if (int.TryParse(factors[5], out clientPort))
                    config.ClientPort = clientPort;
                else
                    return false;

                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
