using System;
using System.Text.RegularExpressions;

namespace rmApplication
{
    /// <summary>
    /// Pure validation utilities with no UI dependency.
    /// Static helper methods extracted from SubViewControl.
    /// </summary>
    internal static class ValidationHelper
    {
        /// <summary>
        /// Returns true if the string is a valid hexadecimal literal with a "0x" or "0X" prefix.
        /// </summary>
        public static bool IsHexString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return Regex.IsMatch(text, @"^(0[xX]){1}[A-Fa-f0-9]+$");
        }

        /// <summary>
        /// Returns true if the string represents a data size valid in the RM protocol (1, 2, 4, or 8 bytes).
        /// On success, <paramref name="result"/> is set to the parsed integer value.
        /// </summary>
        public static bool ValidateSize(string value, ref int result)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (!uint.TryParse(value, out var tmp))
                return false;

            if (tmp == 1 || tmp == 2 || tmp == 4 || tmp == 8)
            {
                result = (int)tmp;
                return true;
            }

            return false;
        }
    }
}
