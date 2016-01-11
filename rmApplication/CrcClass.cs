using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmApplication
{
	//http://sanity-free.org/146/crc8_implementation_in_csharp.html

	class Crc8
	{
		static byte[] table = new byte[256];
		
		const byte init = 0x00;
		const byte poly = 0xd5;		// x8 + x7 + x6 + x4 + x2 + 1
		
		public static byte Calculate(List<byte> bytes)
		{
			byte crc = init;
			if (bytes != null && bytes.Count > 0)
			{
				foreach (byte b in bytes)
				{
					crc = table[crc ^ b];
				}
			}
			return crc;
		}

		static Crc8()
		{
			for (int i = 0; i < 256; ++i)
			{
				int temp = i + (int)init;
				for (int j = 0; j < 8; ++j)
				{
					if ((temp & 0x80) != 0)
					{
						temp = (temp << 1) ^ poly;
					}
					else
					{
						temp <<= 1;
					}
				}
				table[i] = (byte)temp;
			}
		}
	}
}
