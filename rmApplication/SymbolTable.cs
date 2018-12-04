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
        public string Size { set; get; }

        public SymbolFactor()
        {
            Symbol = string.Empty;
            Address = "0x00000000";
            Size = "1";

        }

        public SymbolFactor(SymbolFactor data)
        {
            Symbol = data.Symbol;
            Address = data.Address;
            Size = data.Size;

        }

    }

    public class SymbolTable
    {
    }
}
