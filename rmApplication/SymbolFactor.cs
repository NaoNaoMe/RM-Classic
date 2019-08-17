using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rmApplication
{
    public class SymbolFactor
    {
        public string Symbol { set; get; }
        public string Address { set; get; }
        public string Offset { set; get; }
        public string Size { set; get; }

        public SymbolFactor()
        {
            Symbol = string.Empty;
            Address = "0x00000000";
            Offset = "0";
            Size = "1";

        }

        public SymbolFactor(SymbolFactor data)
        {
            Symbol = data.Symbol;
            Address = data.Address;
            Offset = data.Offset;
            Size = data.Size;

        }

    }

}
