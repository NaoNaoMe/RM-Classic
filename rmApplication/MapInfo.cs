using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmApplication
{
	public class MapInfo
	{
		public class Factor
		{
			public string VariableName { set; get; }

			public string Address { set; get; }

			public string Size { set; get; }

		}

		public class List
		{
			public List<Factor> Factor { get; set; }

		}

	}
	
}
