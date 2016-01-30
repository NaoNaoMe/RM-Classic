using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace rmApplication
{
	public class KeilMap
	{
		public static bool Interpret( string[] textArray, MapInfo.List mapList )
		{
			int foundIndex = 0;

			string searchWord = "armlink";
			bool detectFlg = false;

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

					}

				}

			}

			if (detectFlg == true)
			{
				string symbolPhrase = "Image Symbol Table";
				bool symbolFoundFlg = false;

				for (int i = 0; i < textArray.Length; i++)
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

						if (splitLine.Length == 6)
						{
							MapInfo.Factor data = new MapInfo.Factor();

							data.VariableName = splitLine[1];
							data.Address = splitLine[2];
							data.Size = splitLine[4];

							mapList.Factor.Add(data);

						}

					}

				}

			}

			bool ret = false;

			if (mapList.Factor.Count != 0)
			{
				ret = true;

			}

			return ret;
		}
		
	}
	
}
