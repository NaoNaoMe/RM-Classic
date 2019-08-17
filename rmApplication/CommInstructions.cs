using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rmApplication
{
    public class CommInstructions
    {
        public static readonly int MaxPayloadSize = 128;
        public static readonly int MaxElementNum = 32;

        public enum RmAddr : int
        {
            Byte2 = 0,
            Byte4
        };

        public enum RmInstr : byte
        {
            StartLog = 1,
            StopLog = 2,
            SetTimeStep = 3,
            Write = 4,
            SetAddr = 5,
            ReadInfo = 6,
            ReadDump = 7,
            Bypass = 8,
            Text = 9
        };

        private RmAddr ByteRange;

        private int MasCnt;
        private int NextSlvCnt;

        public CommInstructions(RmAddr range)
        {
            ByteRange = range;

            Terminate();

        }


        public void Terminate()
        {
            MasCnt = 0;
            NextSlvCnt = 0;

        }

        private byte GenerateOpCode(RmInstr instruction)
        {
            MasCnt = MasCnt + 16;

            if (MasCnt > 255)
                MasCnt = 16;

            int tmp = MasCnt + (int)instruction;

            return ((byte)tmp);
        }


        public bool IsResponseValid(List<byte> txFrame, List<byte> rxFrame)
        {
            if( (txFrame.Count == 0) ||
                (rxFrame.Count == 0) )
                return false;

            if (CommProtocol.Crc8.Calculate(rxFrame) != 0x00)
                return false;

            if ( (rxFrame[0] & 0xF0) != (txFrame[0] & 0xF0) )
                return false;

            return true;
        }


        public List<byte> MakeTryConnectionRequest(uint passNumber)
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.ReadInfo));

            List<byte> bytes = BitConverter.GetBytes(passNumber).ToList();

            if (!BitConverter.IsLittleEndian)
                bytes.Reverse();

            frame.AddRange(bytes);

            return frame;

        }


        public List<byte> MakeStartLogModeRequest()
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.StartLog));

            return frame;

        }


        public List<byte> MakeStopLogModeRequest()
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.StopLog));

            return frame;

        }


        public List<byte> MakeSetTimeStepRequest(uint timeStep = 500)
        {
            List<byte> frame = new List<byte>();

            if(timeStep > 65535)
            {
                return frame;
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

            return frame;

        }


        public List<byte> MakeWirteDataRequest(uint address, uint size, ulong value)
        {
            List<byte> frame = new List<byte>();

            if ((size != 1) &&
                (size != 2) &&
                (size != 4) &&
                (size != 8))
            {
                return frame;
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

            return frame;

        }


        private Queue<uint> LogDataSizeQueue;
        private Queue<byte> LogDataImageQueue;
        private Queue<List<byte>> LogDataRequestQueue;

        public void ClearLogDataConfiguration()
        {
            NextSlvCnt = 0;

            LogDataSizeQueue = new Queue<uint>();
            LogDataImageQueue = new Queue<byte>();
            LogDataRequestQueue = new Queue<List<byte>>();

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

            LogDataRequestQueue = new Queue<List<byte>>();

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

                LogDataRequestQueue.Enqueue(frame);
                frameCnt++;

            }

            return true;

        }

        public bool IsAvailableLogDataRequest(out List<byte> frame)
        {
            frame = new List<byte>();

            if(LogDataRequestQueue.Count <= 0)
            {
                return false;

            }

            frame = LogDataRequestQueue.Dequeue();

            return true;
        }

        public bool CheckLogSequence(List<byte> decodedData, ref Queue<ulong> dataList, out int lostCnt)
        {
            lostCnt = 0;

            if (decodedData.Count <= 0)
                return false;

            if (CommProtocol.Crc8.Calculate(decodedData) != 0x00)
                return false;

            // delete useless crc data size
            decodedData.RemoveAt(decodedData.Count - 1);

            bool isValid = true;
            int slvCnt = decodedData[0] & 0x0F;

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

            decodedData.RemoveAt(0);    // remove opcode

            var sizeQueue = new Queue<uint>(LogDataSizeQueue);

            while(sizeQueue.Count != 0)
            {
                var size = sizeQueue.Dequeue();
                if(decodedData.Count >= size)
                {
                    var tmp = decodedData.GetRange(0, (int)size);

                    if (!BitConverter.IsLittleEndian)
                        tmp.Reverse();

                    while(tmp.Count < 8)
                    {
                        tmp.Add(0x00);
                    }

                    dataList.Enqueue(BitConverter.ToUInt64(tmp.ToArray(), 0));

                    decodedData.RemoveRange(0, (int)size);
                }
                else
                {
                    isValid = false;
                    break;
                }

            }


            return isValid;
        }

        public List<byte> MakeDumpDataRequest(uint address, uint size)
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

            return frame;

        }


        public List<byte> MakeBypassRequest(List<byte> bytes)
        {
            List<byte> frame = new List<byte>();

            frame.Add(GenerateOpCode(RmInstr.Bypass));

            frame.AddRange(bytes);

            return frame;

        }

        // Unmanaged data frame
        public bool CheckUnmanagedStream(List<byte> decodedData, ref List<byte> bytes, out int code)
        {
            code = 0;

            if (decodedData.Count <= 2)
                return false;

            if (decodedData[0] != 0)    // Unmanaged data frame
                return false;

            code = decodedData[1];      // Unmanaged data code

            for (int i = 2; i < decodedData.Count; i++)
                bytes.Add(decodedData[i]);

            return true;
        }

        public List<byte> MakeSendTextRequest(List<byte> bytes)
        {
            List<byte> frame = new List<byte>();

            frame.Add(0x00);    // Unmanaged data frame
            frame.Add(0x01);    // Text data

            frame.AddRange(bytes);

            return frame;

        }

    }
}
