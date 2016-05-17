using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASISCompiler
{

    class Symbol
    {
        public enum Types { None, Bool, Byte, Word };
        public string Name;
        public Types Type;
        public int Address;
    }

    class SymbolTable
    {
        Dictionary<string, Symbol> symbols = new Dictionary<string, Symbol>();
        public void define(Symbol sym) { symbols.Add(sym.Name, sym); }
        public Symbol resolve(string name)
        {
            Symbol sym;
            if (symbols.TryGetValue(name, out sym))
                return sym;
            else
                return null;
        }
        public void Empty()
        {
            symbols.Clear();
        }

        public bool isAddressUsed(int Addr)
        {
            var values =
            from value in symbols.Values // Note: we're looking through the values only,
                                         // not all the key/value pairs in the dictionary
            where value.Address == Addr
            select value;

            return values.Count() != 0;
        }
        public int firstaddrB = 0;
        public int firstaddrL = 0;
    }

}
