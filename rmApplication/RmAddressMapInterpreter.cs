using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace rmApplication
{
	public class RmAddressMap
	{
		public static bool Interpret( string path, MapInfo.List mapList )
		{
			var sr = new StreamReader(path, Encoding.GetEncoding("utf-8"));

			string wholeText = sr.ReadToEnd();
			string[] textArray = wholeText.Replace("\r\n", "\n").Split('\n');

			sr.Close();

			int foundIndex = 0;

			string searchWord = "RM Address Map V1.00";
			bool detectFlg = false;

			foundIndex = textArray[0].IndexOf(searchWord);

			if (foundIndex != -1)
			{
				detectFlg = true;

			}

			if (detectFlg == true)
			{
				for (int i = 0; i < textArray.Length; i++)
				{
					var modifiedLine = Regex.Replace(textArray[i], @" +", " ");

					string[] splitLine = modifiedLine.Split(' ');

					if (splitLine.Length == 3)
					{
						MapInfo.Factor data = new MapInfo.Factor();

						data.VariableName = splitLine[0];
						data.Address = splitLine[1];
						data.Size = splitLine[2];

						mapList.Factor.Add(data);

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
