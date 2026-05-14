using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace rmApplication
{
    class NmMap
    {
        private static readonly HashSet<char> targetTypes = new HashSet<char>
        {
            'b', 'B',   // .bss
            'd', 'D',   // .data
        };

        public static bool Interpret(string[] textArray, List<SymbolFactor> symbolList)
        {
            if (textArray == null || textArray.Length == 0)
                return false;

            foreach (var line in textArray)
            {
                var modifiedLine = Regex.Replace(line.Trim(), @" +", " ");
                string[] fields = modifiedLine.Split(' ');

                if (fields.Length != 4)
                    continue;

                string addressField = fields[0];
                string sizeField = fields[1];
                string typeField = fields[2];
                string symbolName = fields[3];

                if (typeField.Length != 1)
                    continue;

                char typeChar = typeField[0];

                if (!targetTypes.Contains(typeChar))
                    continue;

                if (!Regex.IsMatch(addressField, @"^[0-9a-fA-F]+$"))
                    continue;
                if (!Regex.IsMatch(sizeField, @"^[0-9a-fA-F]+$"))
                    continue;

                if (string.IsNullOrEmpty(symbolName))
                    continue;

                uint address = Convert.ToUInt32(addressField, 16);
                uint size = Convert.ToUInt32(sizeField, 16);

                var data = new SymbolFactor();
                data.Symbol = symbolName;
                data.Address = "0x" + address.ToString("X");
                data.Size = size.ToString();

                symbolList.Add(data);
            }

            return symbolList.Count > 0;
        }
    }
}