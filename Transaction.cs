using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyHunter
{
    class Transaction
    {
        public string Hash;
        public MoneyPlace[] Sources;
        public MoneyPlace[] Destinations;
    }

    class MoneyPlace
    {
        public string Address;
        public long Value;
    }
}
