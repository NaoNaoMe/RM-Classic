using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace rmApplication
{
    public class CommProtocol
    {
        #region CRC
        public class Crc8
        {
            //http://sanity-free.org/146/crc8_implementation_in_csharp.html
            static byte[] table = new byte[256];

            const byte init = 0x00;
            const byte poly = 0xd5;     // x8 + x7 + x6 + x4 + x2 + 1

            public static byte Calculate(List<byte> bytes)
            {
                byte crc = init;
                if (bytes != null && bytes.Count > 0)
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


        public List<byte> Encode(List<byte> rawBytes)
        {
            List<byte> tmpBytes = new List<byte>(rawBytes);

            tmpBytes.Add(Crc8.Calculate(tmpBytes));

            List<byte> encodedData = new List<byte>();

            encodedData.Add((byte)FrameChar.END);

            for (int i = 0; i < tmpBytes.Count; i++)
            {
                switch (tmpBytes[i])
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
                        encodedData.Add(tmpBytes[i]);
                        break;

                }
            }

            encodedData.Add((byte)FrameChar.END);

            return encodedData;
        }


        public List<byte> Decode(byte rawByte)
        {
            List<byte> decodedData = new List<byte>();

            if (rawByte == (byte)FrameChar.END)
            {
                // Start SLIP Frame
                if (IsReceiving == false)
                {
                    IsReceiving = true;
                    DecodingData = new List<byte>();

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
                            decodedData = new List<byte>(DecodingData);
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
