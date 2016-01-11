using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace rmApplication
{
	class BinaryEditor
	{
		public static string BytesToHexString(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			
			for (int i = 0; i < bytes.Length; i++)
			{
				sb.Append(bytes[i].ToString("X2"));
			}
			
			return sb.ToString();
			
		}

		public static List<byte> HexStringToBytes(string byteString)
		{
			int length = byteString.Length;
			
			if (length % 2 == 1)
			{
				byteString = "0" + byteString;
				length++;
			}

			List<byte> data = new List<byte>();

			for (int i = 0; i < length - 1; i = i + 2)
			{
				string buf = byteString.Substring(i, 2);
				
				if (Regex.IsMatch(buf, @"^[0-9a-fA-F]{2}$"))
				{
					data.Add(Convert.ToByte(buf, 16));
				}
				else
				{
					data.Add(Convert.ToByte("00", 16));
				}
			}
			
			return data;
		}
		
		public static List<byte> Swap(List<byte> bdata)
		{
			int index;
			int max_index;
			
			byte[] adata = new byte[bdata.Count];
			
			max_index = bdata.Count;
			
			for (index = 0; index < max_index; index++)
			{
				adata[(max_index-1)-index] = bdata[index];
			}
			
			return adata.ToList();
		}
		
	}
}
