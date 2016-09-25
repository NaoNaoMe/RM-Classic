using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace rmApplication
{
	public class ReadElfMap
	{
		public static bool Interpret(string[] textArray, List<MapFactor> mapList)
		{
			int foundIndex = 0;

			string searchWord = "ELF Header";
			bool detectFlg = false;
			int startIndex = 0;

			for (int i = 0; i < textArray.Length; i++)
			{
				if ((detectFlg == false) &&
					(i > 10))
				{
					break;
				}
				else
				{
					foundIndex = textArray[i].IndexOf(searchWord);

					if (foundIndex != -1)
					{
						detectFlg = true;
						startIndex = i;
						break;

					}

				}

			}

			if (detectFlg == true)
			{
				string symbolPhrase = "Symbol table";
				bool symbolFoundFlg = false;

				for (int i = startIndex; i < textArray.Length; i++)
				{
					foundIndex = textArray[i].IndexOf(symbolPhrase);

					if (foundIndex != -1)
					{
						symbolFoundFlg = true;
					}

					if (symbolFoundFlg == true)
					{
						var modifiedLine = Regex.Replace(textArray[i], @" +", " ");

						string[] splitLine = modifiedLine.Split(' ');

						if (splitLine.Length == 9)
						{
							var data = new MapFactor();

							data.VariableName = splitLine[8];
							data.Address = "0x" + splitLine[2];
							data.Size = splitLine[3];

							mapList.Add(data);

						}

					}

				}

			}

			bool ret = false;

			if (mapList.Count != 0)
			{
				ret = true;

			}

			return ret;
		}
		
	}
	
}
