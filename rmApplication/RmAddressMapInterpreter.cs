using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace rmApplication
{
	public class RmAddressMap
	{
		private static string RmVersion = "RM Address Map V1.00";

		public static bool Interpret(string[] textArray, List<MapFactor> mapList)
		{
			int foundIndex = 0;

			string searchWord = RmVersion;
			bool detectFlg = false;

			foundIndex = textArray[0].IndexOf(searchWord);

			if (foundIndex != -1)
			{
				detectFlg = true;

			}

			if (detectFlg == true)
			{
				for (int i = 1; i < textArray.Length; i++)
				{
					string[] splitLine = textArray[i].Split(' ');

					if (splitLine.Length == 3)
					{
						var data = new MapFactor();

						data.VariableName = splitLine[0];
						data.Address = splitLine[1];
						data.Size = splitLine[2];

						mapList.Add(data);

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
		
		public static bool Convert( List<string> textList, List<MapFactor> mapList )
		{
			bool ret = false;
			
			if( ( textList == null ) ||
				( mapList == null ) )
			{
				return ret;
				
			}

			textList.Add("//" + RmVersion);

			foreach (var item in mapList)
			{
				if ( (item.VariableName != "") &&
					(item.Address != "") &&
					(item.Size != "") )
				{
					var tmpAddress = item.Address;

					textList.Add( item.VariableName + " " + tmpAddress + " " + item.Size );

				}

			}

			if (textList.Count != 0)
			{
				ret = true;

			}

			return ret;
			
		}
		
		
	}
	
}
