using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Serialization;

namespace rmApplication
{
	[Serializable()]
	public class DataSetting
	{
		[System.Xml.Serialization.XmlElement("Check")]
		public bool Check { set; get; }
		
		[System.Xml.Serialization.XmlElement("Size")]
		public string Size { set; get; }
		
		[System.Xml.Serialization.XmlElement("Variant")]
		public string Variant { set; get; }

		[System.Xml.Serialization.XmlElement("AddrLock")]
		public bool AddrLock { set; get; }

		[System.Xml.Serialization.XmlElement("Address")]
		public string Address { set; get; }

		[System.Xml.Serialization.XmlElement("Offset")]
		public string Offset { set; get; }

		[System.Xml.Serialization.XmlElement("Name")]
		public string Name { set; get; }
		
		[System.Xml.Serialization.XmlElement("Type")]
		public string Type { set; get; }
		
		[System.Xml.Serialization.XmlElement("WriteValue")]
		public string WriteValue { set; get; }
		
	}

	[Serializable()]
	[System.Xml.Serialization.XmlRoot("ViewSetting")]
	public class ViewSetting
	{
		[System.Xml.Serialization.XmlElement("DataSetting")]
		public List<DataSetting> DataSetting { get; set; }
		
	}

	public class ViewSettingEntity
	{
		public static void InitialDataSource(List<DataSetting> data, int max_i )
		{
			for(int i = 0; i < max_i; i++)
			{
				data.Add(new DataSetting {	Check = false,
											Size = "1",
											Variant = null,
											AddrLock = false,
											Address = null,
											Offset = "0",
											Name = null,
											Type = "Hex",
											WriteValue = null
											});
			}

		}

		public static bool ReloadDataSource(List<DataSetting> ldataSet, List<DataSetting> ldgvSource, int pageNum, int max_column)
		{
			bool flg = false;
			
			if( (ldataSet == null) ||
				(ldgvSource == null) )
			{
				
			}
			else
			{
				int size = ldataSet.Count;
				
				if( (size / max_column) < pageNum )
				{
					
				}
				else
				{
					
					int startNum = (pageNum * max_column);
					int endNum = size - ((pageNum+1) * max_column);
					
					int i_max = 0;
					
					if( endNum >= 0 )
					{
						i_max = max_column;
						
					}
					else
					{
						i_max = (max_column + endNum);
						
					}
					
					for(int i = 0; i < i_max; i++)
					{
						ldgvSource[i] = ldataSet[startNum + i];
						
					}
					
					for(int i = i_max; i < (max_column - i_max); i++)
					{
						ldgvSource[i] = new DataSetting {	Check = false,
															Size = "1",
															Variant = null,
															AddrLock = false,
															Address = null,
															Offset = null,
															Name = null,
															Type = "Hex",
															WriteValue = null
															};
						
					}
					
					flg = true;
					
				}
				
			}
			
			return flg;
			
		}
		
	}
	
}
