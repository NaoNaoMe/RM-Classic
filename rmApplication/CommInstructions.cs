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
            ReadDump = 7
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
            {
                MasCnt = 16;

            }

            int tmp = MasCnt + (int)instruction;

            return ((byte)tmp);
        }


        public bool IsResponseValid(List<byte> txFrame, List<byte> rxFrame)
        {
            if( (txFrame.Count == 0) ||
                (rxFrame.Count == 0) )
            {
                return false;

            }

            if( (rxFrame[0] & 0xF0) != (txFrame[0] & 0xF0) )
            {
                return false;

            }

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


        public List<byte> MakeWirteDataRequest(uint address, uint size, uint value)
        {
            List<byte> frame = new List<byte>();

            if ((size != 1) &&
                (size != 2) &&
                (size != 4))
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
                (size != 4))
            {
                return false;
            }

            if(LogDataSizeQueue.Count > 32)
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

            int maxFrameDataCnt = 20;  // size = 1, address = 4 total 5 bytes : 5 * 4 slots = 20
            int maxFrameCnt = 8;

            if (ByteRange == RmAddr.Byte2)
            {
                maxFrameDataCnt = 24;  // size = 1, address = 2 total 3 bytes : 3 * 8 slots = 24
                maxFrameCnt = 4;
            }

            LogDataRequestQueue = new Queue<List<byte>>();

            int frameCnt = LogDataImageQueue.Count / maxFrameDataCnt;

            if(LogDataImageQueue.Count > frameCnt * maxFrameDataCnt)
            {
                frameCnt++;
            }

            if(frameCnt > maxFrameCnt)
            {
                return false;

            }

            maxFrameCnt = frameCnt;

            while (frameCnt != 0)
            {
                List<byte> frame = new List<byte>();

                frame.Add(GenerateOpCode(RmInstr.SetAddr));

                byte frameCode = 0x00;

                if(frameCnt == maxFrameCnt)
                {
                    //Begining
                    frameCode |= 0x10;
                }

                if (frameCnt <= 1)
                {
                    //Ending
                    frameCode |= 0x20;
                }

                frameCode |= (byte)(maxFrameCnt- frameCnt);

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
                frameCnt--;

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


        public bool CheckLogSequence(List<byte> decodedData, ref Queue<uint> dataList, out int lostCnt)
        {
            bool isValid = true;
            int slvCnt = decodedData[0] & 0x0F;

            lostCnt = 0;

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

                    while(tmp.Count < 4)
                    {
                        tmp.Add(0x00);
                    }

                    dataList.Enqueue(BitConverter.ToUInt32(tmp.ToArray(), 0));

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


        private Queue<List<byte>> DumpDataRequestQueue;

        public bool UpdateDumpDataConfiguration(uint address, uint size)
        {
            int maxPayload = MaxPayloadSize;
            int totalFrameCnt;
            int lastFrameSize;

            int totalSize = (int)size;

            totalFrameCnt = totalSize / maxPayload;
            lastFrameSize = totalSize - (totalFrameCnt * maxPayload);

            if (lastFrameSize != 0)
            {
                totalFrameCnt++;
            }
            else
            {
                lastFrameSize = maxPayload;
            }

            DumpDataRequestQueue = new Queue<List<byte>>();

            for (int cnt = 0; cnt < totalFrameCnt; cnt++)
            {
                List<byte> frame = new List<byte>();

                frame.Add(GenerateOpCode(RmInstr.ReadDump));

                uint offset = (uint)(maxPayload * cnt);

                int requestSize;
                if (cnt == (totalFrameCnt - 1))
                {
                    requestSize = lastFrameSize;

                }
                else
                {
                    requestSize = maxPayload;

                }

                var bytes = BitConverter.GetBytes(address + offset).ToList();

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
                frame.Add((byte)requestSize);

                DumpDataRequestQueue.Enqueue(frame);

            }

            return true;

        }


        public bool IsAvailableDumpDataRequest(out List<byte> frame)
        {
            frame = new List<byte>();

            if (DumpDataRequestQueue.Count <= 0)
            {
                return false;

            }

            frame = DumpDataRequestQueue.Dequeue();

            return true;
        }


    }
}
