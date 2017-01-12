using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rmApplication
{
    public class MapFactor
    {
        public string VariableName { set; get; }

        public string Address { set; get; }

        public string Size { set; get; }

        public MapFactor()
        {
            VariableName = "";
            Address = "0x00000000";
            Size = "1";

        }

        public MapFactor(MapFactor data)
        {
            VariableName = data.VariableName;
            Address = data.Address;
            Size = data.Size;

        }

    }

}
