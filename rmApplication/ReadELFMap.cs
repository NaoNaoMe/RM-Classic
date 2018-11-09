using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rmApplication
{
    class ReadELFMap
    {
        private static string keyword = "ELF Header";

        public static bool Interpret(string[] textArray, List<SymbolFactor> symbolList)
        {
            if (textArray[0].IndexOf(keyword) == -1)
                return false;

            int offsetIndex = 0;
            for (int i = 1; i < textArray.Length; i++)
            {
                if (textArray[i].IndexOf("Symbol table") != -1)
                {
                    offsetIndex = i;
                    break;
                }

            }

            if (offsetIndex == 0)
                return false;

            for (int i = offsetIndex; i < textArray.Length; i++)
            {
                var modifiedLine = Regex.Replace(textArray[i], @" +", " ");

                string[] splitLine = modifiedLine.Split(' ');

                if (splitLine.Length == 9)
                {
                    if (!string.IsNullOrEmpty(splitLine[8]) &&
                        Regex.IsMatch(splitLine[2], @"^[0-9a-fA-F]+$") &&
                        Regex.IsMatch(splitLine[3], @"^[0-9]+$"))
                    {
                        var data = new SymbolFactor();

                        data.VariableName = splitLine[8];
                        data.Address = "0x" + splitLine[2];
                        data.Size = splitLine[3];

                        symbolList.Add(data);

                    }

                }

            }

            if (symbolList.Count > 0)
                return true;
            else
                return false;

        }

    }
}
