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
        public static string ToHexChars(string type, int size, string data, out Exception back_ex)
        {
            back_ex = null;
            string ret = null;

            if (type != numeralSystem.ASCII)
            {
                if (Regex.IsMatch(data, @"[0-9a-fA-F.]+$") != true)
                {
                    return ret;
                }

            }

            data = data.Replace(" ", "");

            byte[] bytes = null;

            try
            {
                switch (type)
                {
                    case numeralSystem.BIN:
                        {
                            var value = Convert.ToUInt64(data, 2);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.DEC:
                        {
                            var value = Convert.ToInt64(data);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.UDEC:
                        {
                            var value = Convert.ToUInt64(data);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.HEX:
                        {
                            var value = Convert.ToUInt64(data, 16);
                            bytes = BitConverter.GetBytes(value);
                        }

                        break;

                    case numeralSystem.FLT:
                        {
                            if (size == 4)
                            {
                                var value = float.Parse(data);
                                bytes = BitConverter.GetBytes(value);
                            }
                        }

                        break;

                    case numeralSystem.DBL:
                        {
                            if (size == 8)
                            {
                                var value = double.Parse(data);
                                bytes = BitConverter.GetBytes(value);
                            }
                        }

                        break;

                    case numeralSystem.ASCII:
                        {
                            bytes = System.Text.Encoding.ASCII.GetBytes(data);

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
            catch (Exception ex)
            {
                back_ex = ex;

            }

            if (bytes == null)
            {

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
                    ret = BitConverter.ToString(bytes);
                    ret = ret.Replace("-", "");

                }

            }

            return ret;

        }

        public static string FromHexChars(string type, int size, string data)
        {
            string ret = null;

            if (string.IsNullOrEmpty(data))
            {
                return ret;

            }


            byte[] bytes = new byte[data.Length / 2];

            for (int i = 0; i < (data.Length / 2); i++)
            {
                int start = 2 * i;
                bytes[i] = byte.Parse(data.Substring(start, 2), NumberStyles.HexNumber);

            }

            if (bytes.Length != size)
            {
                return ret;

            }

            Array.Reverse(bytes, 0, bytes.Length);

            switch (type)
            {
                case numeralSystem.BIN:
                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = Convert.ToString((UInt16)bytes[i], 2);
                        tmp = tmp.PadLeft(8, '0');

                        ret += tmp + " ";

                    }

                    ret = ret.Remove((ret.Length - 1), 1);

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
                        ret = ((SByte)dec_sum).ToString();

                    }
                    else if (size == 2)
                    {
                        ret = ((Int16)dec_sum).ToString();

                    }
                    else if (size == 4)
                    {
                        ret = ((Int32)dec_sum).ToString();

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

                    ret = udec_sum.ToString();

                    break;

                case numeralSystem.HEX:
                    for (int i = (bytes.Length - 1); i >= 0; i--)
                    {
                        var tmp = Convert.ToString((UInt16)bytes[i], 16);
                        tmp = tmp.PadLeft(2, '0');
                        ret += tmp;
                    }

                    ret = ret.ToUpper();

                    break;

                case numeralSystem.FLT:
                    if (bytes.Length == 4)
                    {
                        var tmp = BitConverter.ToSingle(bytes, 0);
                        ret = tmp.ToString();
                    }

                    break;

                case numeralSystem.DBL:
                    if (bytes.Length == 8)
                    {
                        var tmp = BitConverter.ToDouble(bytes, 0);
                        ret = tmp.ToString();
                    }

                    break;

                case numeralSystem.ASCII:
                    Array.Reverse(bytes);
                    ret = System.Text.Encoding.ASCII.GetString(bytes);
                    ret = ret.TrimEnd('\0');

                    break;

                default:
                    break;

            }

            return ret;

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

                    }

                }

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

            return ret;
        }

    }
}
