using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace rmApplication
{
	public class ReadElfMap
	{
		public static bool Interpret( string path, MapInfo.List mapList )
		{
			var sr = new StreamReader(path, Encoding.GetEncoding("utf-8"));

			string wholeText = sr.ReadToEnd();
			string[] textArray = wholeText.Replace("\r\n", "\n").Split('\n');

			sr.Close();

			int foundIndex = 0;

			string searchWord = "ELF Header";
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
				string symbolPhrase = "Symbol table";
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

						if (splitLine.Length == 9)
						{
							MapInfo.Factor data = new MapInfo.Factor();

							data.VariableName = splitLine[8];
							data.Address = "0x" + splitLine[2];
							data.Size = splitLine[3];

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
