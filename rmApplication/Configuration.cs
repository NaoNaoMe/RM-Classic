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

            ClientAddress = System.Net.IPAddress.Parse("0.0.0.0");
            ClientPort = 0;

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
    }
}
