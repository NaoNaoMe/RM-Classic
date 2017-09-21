using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace rmApplication
{
    enum ConvDir
    {
        toDec,
        fromDec

    }


    public static class numeralSystem
    {
        public const string ASCII = "Ascii";
        public const string DBL = "DBL";
        public const string FLT = "FLT";
        public const string HEX = "Hex";
        public const string DEC = "Dec";
        public const string UDEC = "UsD";
        public const string BIN = "Bin";

    }

    public class TypeConvert
    {
        public static bool ToHexChars(string type, int size, string dataStrings, out string hexCharStrings)
        {
            hexCharStrings = null;

            if (type != numeralSystem.ASCII)
            {
                if (Regex.IsMatch(dataStrings, @"[0-9a-fA-F.]+$") != true)
                {
                    return false;
                }

            }

            dataStrings = dataStrings.Replace(" ", "");

            byte[] bytes = null;

            try
            {
                switch (type)
                {
                    case numeralSystem.BIN:
                        {
                            var value = Convert.ToUInt64(dataStrings, 2);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.DEC:
                        {
                            var value = Convert.ToInt64(dataStrings);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.UDEC:
                        {
                            var value = Convert.ToUInt64(dataStrings);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.HEX:
                        {
                            var value = Convert.ToUInt64(dataStrings, 16);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.FLT:
                        {
                            if (size == 4)
                            {
                                var value = float.Parse(dataStrings);
                                bytes = BitConverter.GetBytes(value);
                            }
                        }

                        break;

                    case numeralSystem.DBL:
                        {
                            if (size == 8)
                            {
                                var value = double.Parse(dataStrings);
                                bytes = BitConverter.GetBytes(value);
                            }
                        }

                        break;

                    case numeralSystem.ASCII:
                        {
                            bytes = System.Text.Encoding.ASCII.GetBytes(dataStrings);

                            var list = bytes.ToList();

                            if (list.Count >= size)
                            {

                            }
                            else
                            {
                                for (int i = 0; i < (size - list.Count); i++)
                                {
                                    list.Add(0x00);
                                }

                            }

                            bytes = list.ToArray();

                            Array.Reverse(bytes);

                        }

                        break;

                    default:
                        break;

                }

            }
            catch (Exception)
            {
                return false;

            }

            if (bytes == null)
            {
                return false;

            }
            else
            {
                if (type != numeralSystem.ASCII)
                {
                    var list = bytes.ToList();
                    list.RemoveRange(size, (list.Count - size));
                    bytes = list.ToArray();

                }

                if (bytes.Length == size)
                {
                    Array.Reverse(bytes, 0, bytes.Length);
                    hexCharStrings = BitConverter.ToString(bytes);
                    hexCharStrings = hexCharStrings.Replace("-", "");

                }

            }

            return true;

        }

        public static bool FromHexChars(string type, int size, string hexCharStrings, out string dataStrings)
        {
            dataStrings = null;

            if (string.IsNullOrEmpty(hexCharStrings))
            {
                return false;

            }


            byte[] bytes = new byte[hexCharStrings.Length / 2];

            for (int i = 0; i < (hexCharStrings.Length / 2); i++)
            {
                int start = 2 * i;
                bytes[i] = byte.Parse(hexCharStrings.Substring(start, 2), NumberStyles.HexNumber);

            }

            if (bytes.Length != size)
            {
                return false;

            }

            Array.Reverse(bytes, 0, bytes.Length);

            switch (type)
            {
                case numeralSystem.BIN:
                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = Convert.ToString((UInt16)bytes[i], 2);
                        tmp = tmp.PadLeft(8, '0');

                        dataStrings += tmp + " ";

                    }

                    dataStrings = dataStrings.Remove((dataStrings.Length - 1), 1);

                    break;

                case numeralSystem.DEC:
                    UInt64 dec_sum = 0;

                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = (UInt64)bytes[i];
                        dec_sum += tmp;
                        dec_sum *= 256;
                    }

                    dec_sum /= 256;

                    if (size == 1)
                    {
                        dataStrings = ((SByte)dec_sum).ToString();

                    }
                    else if (size == 2)
                    {
                        dataStrings = ((Int16)dec_sum).ToString();

                    }
                    else if (size == 4)
                    {
                        dataStrings = ((Int32)dec_sum).ToString();

                    }
                    else
                    {

                    }

                    break;

                case numeralSystem.UDEC:
                    UInt64 udec_sum = 0;

                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = (UInt64)bytes[i];
                        udec_sum += tmp;
                        udec_sum *= 256;
                    }

                    udec_sum /= 256;

                    dataStrings = udec_sum.ToString();

                    break;

                case numeralSystem.HEX:
                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = Convert.ToString((UInt16)bytes[i], 16);
                        tmp = tmp.PadLeft(2, '0');
                        dataStrings += tmp;
                    }

                    dataStrings = dataStrings.ToUpper();

                    break;

                case numeralSystem.FLT:
                    if (bytes.Length == 4)
                    {
                        var tmp = BitConverter.ToSingle(bytes, 0);
                        dataStrings = tmp.ToString();
                    }

                    break;

                case numeralSystem.DBL:
                    if (bytes.Length == 8)
                    {
                        var tmp = BitConverter.ToDouble(bytes, 0);
                        dataStrings = tmp.ToString();
                    }

                    break;

                case numeralSystem.ASCII:
                    Array.Reverse(bytes);
                    dataStrings = System.Text.Encoding.ASCII.GetString(bytes);
                    dataStrings = dataStrings.TrimEnd('\0');

                    break;

                default:
                    break;

            }

            return true;

        }

        public static bool IsNumeric(string value)
        {
            bool ret = false;

            if (string.IsNullOrEmpty(value) == false)
            {
                int data;

                if (int.TryParse(value, out data) == true)
                {
                    ret = true;

                }

            }

            return ret;
        }

        public static bool IsHexString(string value)
        {
            bool ret = false;

            if (string.IsNullOrEmpty(value) == false)
            {
                if (value.Length >= 2)
                {
                    var header = value.Substring(0, 2);

                    if (header == "0x")
                    {
                        value = value.Remove(0, 2);

                        foreach (char factor in value)
                        {
                            if (Uri.IsHexDigit(factor) == false)
                            {
                                ret = false;
                                break;
                            }
                            else
                            {
                                ret = true;

                            }

                        }

                    }

                }


            }

            return ret;
        }

    }
}
