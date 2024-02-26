using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rmApplication
{
    public class CommInstructions
    {
        #region CRC
        public class Crc8
        {
            //http://sanity-free.org/146/crc8_implementation_in_csharp.html
            static byte[] table = new byte[256];

            const byte init = 0x00;
            const byte poly = 0xd5;     // x8 + x7 + x6 + x4 + x2 + 1

            public static byte Calculate(byte[] bytes)
            {
                byte crc = init;
                if (bytes != null && bytes.Length > 0)
                {
                    foreach (byte b in bytes)
                    {
                        crc = table[crc ^ b];
                    }
                }
                return crc;
            }

            static Crc8()
            {
                for (int i = 0; i < 256; ++i)
                {
                    int temp = i + (int)init;
                    for (int j = 0; j < 8; ++j)
                    {
                        if ((temp & 0x80) != 0)
                        {
                            temp = (temp << 1) ^ poly;
                        }
                        else
                        {
                            temp <<= 1;
                        }
                    }
                    table[i] = (byte)temp;
                }
            }
        }
        #endregion

        public static readonly int MaxPayloadSize = 128;
        public static readonly int MaxElementNum = 32;

        public enum RmAddr
        {
            Byte2 = 0,
            Byte4
        };

        public enum RmInstr
        {
            StartLog = 1,
            StopLog = 2,
            SetTimeStep = 3,
            Write = 4,
            SetAddr = 5,
            ReadInfo = 6,
            ReadDump = 7,
            Bypass = 8
        };

        public enum RmFrameType
        {
            Derived = 0x00
        }

        public enum RmDerivedMode
        {
            Undefined = 0x00,
            SerialCommunicationEmulation = 0x01
        }

        private RmAddr ByteRange;

        private int MasCnt;
        private int NextSlvCnt;

        private Queue<uint> LogDataSizeQueue;
        private Queue<byte> LogDataImageQueue;
        private Queue<byte[]> LogDataRequestQueue;


        public CommInstructions(RmAddr range)
        {
            ByteRange = range;

            MasCnt = 0;
            NextSlvCnt = 0;

            LogDataSizeQueue = new Queue<uint>();
            LogDataImageQueue = new Queue<byte>();
            LogDataRequestQueue = new Queue<byte[]>();
        }



        private byte GenerateOpCode(RmInstr instruction)
        {
            MasCnt = MasCnt + 16;

            if (MasCnt > 255)
                MasCnt = 16;

            int tmp = MasCnt + (int)instruction;

            return ((byte)tmp);
        }


        public bool IsResponseValid(byte[] txFrame, byte[] rxFrame)
        {
            if( (txFrame.Length == 0) ||
                (rxFrame.Length == 0) )
                return false;

            if (Crc8.Calculate(rxFrame) != 0x00)
                return false;

            if ( (rxFrame[0] & 0xF0) != (txFrame[0] & 0xF0) )
                return false;

            return true;
        }


        public byte[] MakeTryConnectionRequest(uint passNumber)
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.ReadInfo));

            List<byte> bytes = BitConverter.GetBytes(passNumber).ToList();

            if (!BitConverter.IsLittleEndian)
                bytes.Reverse();

            frame.AddRange(bytes);

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }

        public byte[] MakeStartLogModeRequest()
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.StartLog));

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }


        public byte[] MakeStopLogModeRequest()
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.StopLog));

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }


        public byte[] MakeSetTimeStepRequest(uint timeStep = 500)
        {
            List<byte> frame = new List<byte>();

            if(timeStep > 65535)
            {
                return frame.ToArray();
            }

            frame.Add(GenerateOpCode(RmInstr.SetTimeStep));

            List<byte> bytes = BitConverter.GetBytes(timeStep).ToList();

            if(!BitConverter.IsLittleEndian)
                bytes.Reverse();

            while (bytes.Count > 2)
            {
                bytes.RemoveAt(bytes.Count - 1);
            }

            frame.AddRange(bytes);

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }


        public byte[] MakeWirteDataRequest(uint address, uint size, ulong value)
        {
            List<byte> frame = new List<byte>();

            if ((size != 1) &&
                (size != 2) &&
                (size != 4) &&
                (size != 8))
            {
                return frame.ToArray();
            }

            frame.Add(GenerateOpCode(RmInstr.Write));

            var sizeBytes = BitConverter.GetBytes(size).ToList();

            if (!BitConverter.IsLittleEndian)
                sizeBytes.Reverse();

            while (sizeBytes.Count > 1)
            {
                sizeBytes.RemoveAt(sizeBytes.Count - 1);
            }

            var addressBytes =  BitConverter.GetBytes(address).ToList();

            if (!BitConverter.IsLittleEndian)
                addressBytes.Reverse();

            if (ByteRange == RmAddr.Byte2)
            {
                while (addressBytes.Count > 2)
                {
                    addressBytes.RemoveAt((addressBytes.Count - 1));
                }

            }

            var valueBytes = BitConverter.GetBytes(value).ToList();

            if (!BitConverter.IsLittleEndian)
                valueBytes.Reverse();

            while (valueBytes.Count > (int)size)
            {
                valueBytes.RemoveAt((valueBytes.Count - 1));
            }

            frame.AddRange(sizeBytes);
            frame.AddRange(addressBytes);
            frame.AddRange(valueBytes);

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }


        public void ClearLogDataConfiguration()
        {
            NextSlvCnt = 0;

            LogDataSizeQueue.Clear();
            LogDataImageQueue.Clear();
            LogDataRequestQueue.Clear();
        }


        public bool PushDataForLogDataConfiguration(uint address, uint size)
        {
            if ((size != 1) &&
                (size != 2) &&
                (size != 4) &&
                (size != 8))
            {
                return false;
            }

            if(LogDataSizeQueue.Count > MaxElementNum)
            {
                return false;
            }

            LogDataSizeQueue.Enqueue(size);

            List<byte> frame = new List<byte>();

            var sizeBytes = BitConverter.GetBytes(size).ToList();

            if (!BitConverter.IsLittleEndian)
                sizeBytes.Reverse();

            while (sizeBytes.Count > 1)
            {
                sizeBytes.RemoveAt(sizeBytes.Count - 1);
            }

            var addressBytes = BitConverter.GetBytes(address).ToList();

            if (!BitConverter.IsLittleEndian)
                addressBytes.Reverse();

            if (ByteRange == RmAddr.Byte2)
            {
                while (addressBytes.Count > 2)
                {
                    addressBytes.RemoveAt((addressBytes.Count - 1));
                }

            }

            frame.AddRange(sizeBytes);
            frame.AddRange(addressBytes);

            foreach(var item in frame)
            {
                LogDataImageQueue.Enqueue(item);
            }

            return true;
        }

        public bool UpdateLogDataConfiguration()
        {
            if(LogDataImageQueue.Count <= 0)
            {
                return false;
            }

            int maxFrameDataCnt = 20;  // size = 1, address = 4 => total 5 bytes => 5 * 4 slots = 20

            if (ByteRange == RmAddr.Byte2)
            {
                maxFrameDataCnt = 24;  // size = 1, address = 2 => total 3 bytes => 3 * 8 slots = 24
            }

            LogDataRequestQueue.Clear();

            int totalFrameCnt = LogDataImageQueue.Count / maxFrameDataCnt;

            if(LogDataImageQueue.Count > totalFrameCnt * maxFrameDataCnt)
            {
                totalFrameCnt++;
            }

            int frameCnt = 1;
            while (frameCnt <= totalFrameCnt)
            {
                List<byte> frame = new List<byte>();

                frame.Add(GenerateOpCode(RmInstr.SetAddr));

                byte frameCode = 0x00;

                if(frameCnt == 1)
                {
                    // Begin
                    frameCode |= 0x10;
                }

                if (frameCnt >= totalFrameCnt)
                {
                    // End
                    frameCode |= 0x20;
                }

                frameCode |= (byte)(frameCnt-1);

                frame.Add(frameCode);

                int currentFrameDataCnt = 0;
                while (LogDataImageQueue.Count != 0)
                {
                    if(currentFrameDataCnt >= maxFrameDataCnt)
                    {
                        break;
                    }

                    frame.Add(LogDataImageQueue.Dequeue());
                    currentFrameDataCnt++;

                }

                frame.Add(Crc8.Calculate(frame.ToArray()));

                LogDataRequestQueue.Enqueue(frame.ToArray());
                frameCnt++;

            }

            return true;
        }

        public bool IsAvailableLogDataRequest(out byte[] frame)
        {
            frame = new byte[0];

            if(LogDataRequestQueue.Count <= 0)
                return false;

            frame = LogDataRequestQueue.Dequeue();

            return true;
        }

        public bool CheckLogSequence(byte[] decodedData, ref Queue<ulong> dataList, out int lostCnt)
        {
            lostCnt = 0;

            if (decodedData.Length <= 0)
                return false;

            if (Crc8.Calculate(decodedData) != 0x00)
                return false;

            var tmpDecodedData = new List<byte>(decodedData);

            // delete useless crc data size
            tmpDecodedData.RemoveAt(tmpDecodedData.Count - 1);

            bool isValid = true;
            int slvCnt = tmpDecodedData[0] & 0x0F;

            if( (NextSlvCnt != 0) &&
                (slvCnt != NextSlvCnt) )
            {
                lostCnt = slvCnt - NextSlvCnt;
                if (lostCnt < 0)
                {
                    lostCnt = 15 + lostCnt;

                }

            }

            NextSlvCnt = slvCnt + 1;
            if (NextSlvCnt >= 16)
            {
                NextSlvCnt = 1;
            }

            tmpDecodedData.RemoveAt(0);    // remove opcode

            var sizeQueue = new Queue<uint>(LogDataSizeQueue);

            while(sizeQueue.Count != 0)
            {
                var size = sizeQueue.Dequeue();
                if(tmpDecodedData.Count >= size)
                {
                    var tmp = tmpDecodedData.GetRange(0, (int)size);

                    if (!BitConverter.IsLittleEndian)
                        tmp.Reverse();

                    while(tmp.Count < 8)
                        tmp.Add(0x00);

                    dataList.Enqueue(BitConverter.ToUInt64(tmp.ToArray(), 0));

                    tmpDecodedData.RemoveRange(0, (int)size);
                }
                else
                {
                    isValid = false;
                    break;
                }

            }

            return isValid;
        }

        public byte[] MakeDumpDataRequest(uint address, uint size)
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.ReadDump));

            var bytes = BitConverter.GetBytes(address).ToList();

            if (!BitConverter.IsLittleEndian)
                bytes.Reverse();

            if (ByteRange == RmAddr.Byte2)
            {
                while (bytes.Count > 2)
                {
                    bytes.RemoveAt((bytes.Count - 1));
                }

            }

            frame.AddRange(bytes);

            if (size > MaxPayloadSize)
                size = (uint)MaxPayloadSize;

            frame.Add((byte)size);

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }


        public byte[] MakeBypassRequest(List<byte> bytes)
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.Bypass));

            frame.AddRange(bytes);

            frame.Add(Crc8.Calculate(frame.ToArray()));

            return frame.ToArray();
        }

        public bool CheckDerivedFrame(byte[] decodedData, ref byte[] bytes, out RmDerivedMode mode)
        {
            const int FrameIdentificationIndex = 0;
            const int DerivedModeIndex = 1;
            const int DerivedPayloadIndex = 2;

            mode = RmDerivedMode.Undefined;

            if (decodedData.Length <= 2)
                return false;


            if (decodedData[FrameIdentificationIndex] != (int)RmFrameType.Derived)
                return false;

            if (Enum.IsDefined(typeof(RmDerivedMode), (int)decodedData[DerivedModeIndex]))
            {
                mode = (RmDerivedMode)decodedData[DerivedModeIndex];
            }

            var total = decodedData.Length - DerivedPayloadIndex;
            bytes = new byte[total];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = decodedData[DerivedPayloadIndex+i];

            // CRC is not used for the derived frame.

            return true;
        }

        public byte[] MakeDerivedFrame(List<byte> bytes, RmDerivedMode mode)
        {
            List<byte> frame = new List<byte>();

            frame.Add((byte)RmFrameType.Derived);
            frame.Add((byte)mode);

            frame.AddRange(bytes);

            // CRC is not used for the derived frame.

            return frame.ToArray();
        }

    }
}
