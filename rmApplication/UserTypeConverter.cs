using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace rmApplication
{
    public enum UserType : int
    {
        Bin = 0,
        Dec,
        UsD,
        Hex,
        FLT,
        DBL
    }

    public class UserUlong
    {
        public static bool TryParse(UserType type, int size, string text, out ulong result)
        {
            result = 0;

            if (string.IsNullOrEmpty(text))
                return false;

            text = text.Replace(" ", "");

            if (!Regex.IsMatch(text, @"^[0-9a-fA-F.]+$"))
                return false;

            try
            {
                switch (type)
                {
                    case UserType.Bin:
                        {
                            UInt64 min = UInt64.MinValue;
                            UInt64 max = UInt64.MaxValue;
                            var tmp = Convert.ToUInt64(text, 2);

                            switch (size)
                            {
                                case 1:
                                    min = byte.MinValue;
                                    max = byte.MaxValue;
                                    break;
                                case 2:
                                    min = UInt16.MinValue;
                                    max = UInt16.MaxValue;
                                    break;
                                case 4:
                                    min = UInt32.MinValue;
                                    max = UInt32.MaxValue;
                                    break;
                                default:
                                    break;
                            }

                            if (tmp < min)
                                tmp = min;
                            else if (tmp > max)
                                tmp = max;

                            result = tmp;
                        }

                        break;

                    case UserType.Dec:
                        {
                            Int64 min = Int64.MinValue;
                            Int64 max = Int64.MaxValue;
                            var tmp = Convert.ToInt64(text);

                            switch(size)
                            {
                                case 1:
                                    min = sbyte.MinValue;
                                    max = sbyte.MaxValue;
                                    break;
                                case 2:
                                    min = Int16.MinValue;
                                    max = Int16.MaxValue;
                                    break;
                                case 4:
                                    min = Int32.MinValue;
                                    max = Int32.MaxValue;
                                    break;
                                default:
                                    break;
                            }

                            if (tmp < min)
                                tmp = min;
                            else if (tmp > max)
                                tmp = max;

                            result = (ulong)tmp;

                        }

                        break;

                    case UserType.UsD:
                        {
                            UInt64 min = UInt64.MinValue;
                            UInt64 max = UInt64.MaxValue;
                            var tmp = Convert.ToUInt64(text);

                            switch (size)
                            {
                                case 1:
                                    min = byte.MinValue;
                                    max = byte.MaxValue;
                                    break;
                                case 2:
                                    min = UInt16.MinValue;
                                    max = UInt16.MaxValue;
                                    break;
                                case 4:
                                    min = UInt32.MinValue;
                                    max = UInt32.MaxValue;
                                    break;
                                default:
                                    break;
                            }

                            if (tmp < min)
                                tmp = min;
                            else if (tmp > max)
                                tmp = max;

                            result = tmp;
                        }

                        break;

                    case UserType.Hex:
                        {
                            UInt64 min = UInt64.MinValue;
                            UInt64 max = UInt64.MaxValue;
                            var tmp = Convert.ToUInt64(text, 16);

                            switch (size)
                            {
                                case 1:
                                    min = byte.MinValue;
                                    max = byte.MaxValue;
                                    break;
                                case 2:
                                    min = UInt16.MinValue;
                                    max = UInt16.MaxValue;
                                    break;
                                case 4:
                                    min = UInt32.MinValue;
                                    max = UInt32.MaxValue;
                                    break;
                                default:
                                    break;
                            }

                            if (tmp < min)
                                tmp = min;
                            else if (tmp > max)
                                tmp = max;

                            result = tmp;
                        }

                        break;

                    case UserType.FLT:
                        {
                            var tmp = float.Parse(text);
                            result = BitConverter.ToUInt32(BitConverter.GetBytes(tmp), 0);
                        }

                        break;

                    case UserType.DBL:
                        {
                            var tmp = double.Parse(text);
                            result = BitConverter.ToUInt64(BitConverter.GetBytes(tmp), 0);
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

            return true;

        }

    }


    public class UserString
    {
        public static bool TryParse(string typeText, string sizeText, ulong value, out string result)
        {
            result = string.Empty;

            bool isValid = true;

            UserType type;
            if (string.IsNullOrEmpty(typeText))
            {
                type = UserType.Hex;
            }
            else
            {
                type = (UserType)Enum.Parse(typeof(UserType), typeText);
            }

            int size;
            if (!int.TryParse(sizeText, out size))
                size = 1;

            TryParse(type, size, value, out result);

            return isValid;
        }

        public static bool TryParse(UserType type, int size, ulong value, out string result)
        {
            result = string.Empty;

            bool isValid = true;

            switch (type)
            {
                case UserType.Bin:
                    result = Convert.ToString((long)value, 2);
                    result = result.PadLeft(64, '0');

                    if (size == 1)
                    {
                        result = result.Remove(0, 56);
                    }
                    else if (size == 2)
                    {
                        result = result.Remove(0, 48);
                        result = result.Insert(8, " ");
                        result = result.Insert(17, " ");
                    }
                    else if (size == 4)
                    {
                        result = result.Remove(0, 32);
                        result = result.Insert(8, " ");
                        result = result.Insert(17, " ");
                        result = result.Insert(26, " ");
                    }
                    else if (size == 8)
                    {
                        result = result.Insert(8, " ");     // 8+0
                        result = result.Insert(17, " ");    // 16+1
                        result = result.Insert(26, " ");    // 24+2
                        result = result.Insert(35, " ");    // 32+3
                        result = result.Insert(44, " ");    // 40+4
                        result = result.Insert(53, " ");    // 48+5
                        result = result.Insert(62, " ");    // 56+6
                    }

                    break;

                case UserType.Dec:
                    if (size == 1)
                    {
                        result = ((SByte)value).ToString();
                    }
                    else if (size == 2)
                    {
                        result = ((Int16)value).ToString();
                    }
                    else if (size == 4)
                    {
                        result = ((Int32)value).ToString();
                    }
                    else
                    {
                        result = ((Int64)value).ToString();
                    }

                    break;

                case UserType.UsD:
                    result = value.ToString();

                    break;

                case UserType.Hex:
                    result = Convert.ToString((long)value, 16);
                    result = result.PadLeft(16, '0');
                    result = result.ToUpper();

                    if (size == 1)
                    {
                        result = result.Remove(0, 14);
                    }
                    else if (size == 2)
                    {
                        result = result.Remove(0, 12);
                    }
                    else if (size == 4)
                    {
                        result = result.Remove(0, 8);
                    }

                    break;

                case UserType.FLT:
                    if(size == 4)
                    {
                        var tmp = BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
                        result = tmp.ToString();
                    }
                    else
                    {
                        isValid = false;
                    }

                    break;

                case UserType.DBL:
                    if (size == 8)
                    {
                        var tmp = BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
                        result = tmp.ToString();
                    }
                    else
                    {
                        isValid = false;
                    }

                    break;

                default:
                    isValid = false;
                    break;

            }

            return isValid;

        }

    }
}
