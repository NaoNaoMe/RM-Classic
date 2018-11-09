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
        FLT
    }

    public class UserUint
    {
        public static bool TryParse(UserType type, int size, string text, out uint result)
        {
            result = 0;

            if (string.IsNullOrEmpty(text))
                return false;

            if (!Regex.IsMatch(text, @"^[0-9a-fA-F.]+$"))
                return false;

            text = text.Replace(" ", "");

            try
            {
                switch (type)
                {
                    case UserType.Bin:
                        {
                            UInt64 min = 0;
                            UInt64 max = 0;
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

                            result = (uint)tmp;
                        }

                        break;

                    case UserType.Dec:
                        {
                            Int64 min = 0;
                            Int64 max = 0;
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

                            result = (uint)tmp;

                        }

                        break;

                    case UserType.UsD:
                        {
                            UInt64 min = 0;
                            UInt64 max = 0;
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

                            result = (uint)tmp;
                        }

                        break;

                    case UserType.Hex:
                        {
                            UInt64 min = 0;
                            UInt64 max = 0;
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

                            result = (uint)tmp;
                        }

                        break;

                    case UserType.FLT:
                        {
                            var tmp = float.Parse(text);
                            result = BitConverter.ToUInt32(BitConverter.GetBytes(tmp), 0);
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
        public static bool TryParse(string typeText, string sizeText, uint value, out string result)
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

        public static bool TryParse(UserType type, int size, uint value, out string result)
        {
            result = string.Empty;

            bool isValid = true;

            switch (type)
            {
                case UserType.Bin:
                    result = Convert.ToString(value, 2);
                    result = result.PadLeft(32, '0');

                    if (size == 1)
                    {
                        result = result.Remove(0, 24);
                    }
                    else if (size == 2)
                    {
                        result = result.Remove(0, 16);
                        result = result.Insert(8, " ");
                    }
                    else if (size == 4)
                    {
                        result = result.Insert(8, " ");
                        result = result.Insert(17, " ");
                        result = result.Insert(26, " ");
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

                    }

                    break;

                case UserType.UsD:
                    result = value.ToString();

                    break;

                case UserType.Hex:
                    result = Convert.ToString(value, 16);
                    result = result.PadLeft(8, '0');
                    result = result.ToUpper();

                    if (size == 1)
                    {
                        result = result.Remove(0, 6);
                    }
                    else if (size == 2)
                    {
                        result = result.Remove(0, 4);
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

                default:
                    isValid = false;
                    break;

            }

            return isValid;

        }

    }
}
