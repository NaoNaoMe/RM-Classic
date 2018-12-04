using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace rmApplication
{
    public class GCCLinkerMap
    {
        private static string keyword = "Archive member included to satisfy reference by file (symbol)";

        public static bool Interpret(string[] textArray, List<SymbolFactor> symbolList)
        {
            if (textArray[0].IndexOf(keyword) == -1)
                return false;

            for (int i = 1; i < textArray.Length; i++)
            {
                if ((textArray[i].IndexOf(".bss") == -1) &&
                    (textArray[i].IndexOf(".rodata") == -1) &&
                    (textArray[i].IndexOf(".data") == -1) )
                {
                    continue;
                }

                var modifiedLine = Regex.Replace(textArray[i], @" +", " ");

                var splitLine = modifiedLine.Split(' ');

                if (splitLine.Length == 5)
                {
                    var splietsplitLine = splitLine[1].Split('.');

                    if (splietsplitLine.Length > 2)
                    {
                        if (!string.IsNullOrEmpty(splietsplitLine[2]) &&
                            IsHexString(splitLine[2]) &&
                            IsHexString(splitLine[3]))
                        {
                            var data = new SymbolFactor();
                            data.Symbol = splietsplitLine[2];
                            data.Address = splitLine[2];
                            data.Size = Convert.ToInt64(splitLine[3], 16).ToString();

                            symbolList.Add(data);

                        }

                    }

                }
                else if (splitLine.Length == 2)
                {
                    var splietsplitLine = splitLine[1].Split('.');

                    if (splietsplitLine.Length > 2)
                    {
                        var name = splietsplitLine[2];

                        if (i < textArray.Length)
                            i++;
                        else
                            break;

                        modifiedLine = Regex.Replace(textArray[i], @" +", " ");
                        splitLine = modifiedLine.Split(' ');

                        if (splitLine.Length > 2)
                        {
                            if (!string.IsNullOrEmpty(name) && 
                                IsHexString(splitLine[1]) &&
                                IsHexString(splitLine[2]))
                            {
                                var data = new SymbolFactor();
                                data.Symbol = name;
                                data.Address = splitLine[1];
                                data.Size = Convert.ToInt64(splitLine[2], 16).ToString();

                                symbolList.Add(data);

                            }

                        }

                    }

                }

            }

            if (symbolList.Count > 0)
                return true;
            else
                return false;

        }

        private static bool IsHexString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (Regex.IsMatch(text, @"^(0[xX]){1}[A-Fa-f0-9]+$"))
                return true;
            else
                return false;
        }

    }
}
