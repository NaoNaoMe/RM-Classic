using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rmApplication
{
    public class CommProtocol
    {
        private enum FrameChar : byte
        {
            END = 0xC0,
            ESC = 0xDB,
            ESC_END = 0xDC,
            ESC_ESC = 0xDD
        };

        private bool IsReceiving;
        private bool IsFoundESC;
        private List<byte> DecodingData;

        public CommProtocol()
        {
            PurgeReceiveBuffer();
        }

        public void PurgeReceiveBuffer()
        {
            IsReceiving = false;
            IsFoundESC = false;

            DecodingData = new List<byte>();

        }


        public byte[] Encode(byte[] rawBytes)
        {
            List<byte> encodedData = new List<byte>();

            encodedData.Add((byte)FrameChar.END);

            foreach (var item in rawBytes)
            {
                switch (item)
                {
                    case (byte)FrameChar.END:
                        encodedData.Add((byte)FrameChar.ESC);
                        encodedData.Add((byte)FrameChar.ESC_END);
                        break;
                    case (byte)FrameChar.ESC:
                        encodedData.Add((byte)FrameChar.ESC);
                        encodedData.Add((byte)FrameChar.ESC_ESC);
                        break;

                    default:
                        encodedData.Add(item);
                        break;
                }
            }

            encodedData.Add((byte)FrameChar.END);

            return encodedData.ToArray();
        }


        public byte[] Decode(byte rawByte)
        {
            var decodedData = new byte[0];

            if (rawByte == (byte)FrameChar.END)
            {
                // Start SLIP Frame
                if (IsReceiving == false)
                {
                    IsReceiving = true;
                    DecodingData.Clear();

                }
                else
                {
                    if (DecodingData.Count == 0)
                    {
                        // receive order FRAMECHAR.END,FRAMECHAR.END
                        // Last FRAMECHAR.END might be Start of Frame
                        IsReceiving = true;

                    }
                    else
                    {
                        // Validation with CRC8 is depend on application
                        IsReceiving = false;

                        if (DecodingData.Count >= 2)
                        {
                            // End SLIP Frame
                            decodedData = new byte[DecodingData.Count];
                            Array.Copy(DecodingData.ToArray(), decodedData, decodedData.Length);
                        }

                    }

                }

            }
            else
            {
                if (IsFoundESC == true)
                {
                    IsFoundESC = false;

                    switch (rawByte)
                    {
                        case (byte)FrameChar.ESC_END:
                            DecodingData.Add((byte)FrameChar.END);
                            break;
                        case (byte)FrameChar.ESC_ESC:
                            DecodingData.Add((byte)FrameChar.ESC);
                            break;
                        default:
                            break;

                    }

                }
                else if (rawByte == (byte)FrameChar.ESC)
                {
                    IsFoundESC = true;
                }
                else
                {
                    if (IsReceiving == true)
                    {
                        DecodingData.Add(rawByte);
                    }

                }

            }

            return decodedData;
        }

    }

}
