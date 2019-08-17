using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rmApplication
{
    public class RmAddressMap
    {
        public static bool Interpret(string[] textArray, List<SymbolFactor> symbolList)
        {
            for (int i = 1; i < textArray.Length; i++)
            {
                string[] splitLine = textArray[i].Split(',');

                if (splitLine.Length == 4)
                {
                    if (splitLine[1].Length < 2)
                        continue;

                    if (splitLine[1].Substring(0,2) == "0x")
                        splitLine[1] = splitLine[1].Remove(0,2);

                    if (!string.IsNullOrEmpty(splitLine[0]) &&
                        Regex.IsMatch(splitLine[1], @"^[0-9a-fA-F]+$") &&
                        Regex.IsMatch(splitLine[2], @"^[0-9]+$") &&
                        Regex.IsMatch(splitLine[3], @"^[0-9]+$"))
                    {
                        var data = new SymbolFactor();

                        data.Symbol = splitLine[0];
                        data.Address = "0x" + splitLine[1];
                        data.Offset = splitLine[2];
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

        public static bool Convert(List<string> textList, List<SymbolFactor> symbolList)
        {
            if ((textList == null) ||
                (symbolList == null))
            {
                return false;
            }

            foreach (var item in symbolList)
            {
                if ((!string.IsNullOrEmpty(item.Symbol)) &&
                    (!string.IsNullOrEmpty(item.Address)) &&
                    (!string.IsNullOrEmpty(item.Offset)) &&
                    (!string.IsNullOrEmpty(item.Size)))
                {
                    textList.Add(item.Symbol + "," + item.Address + ","  + item.Offset + "," + item.Size);
                }

            }

            if (textList.Count > 0)
                return true;
            else
                return false;

        }
        
    }
}
