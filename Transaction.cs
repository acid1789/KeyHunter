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
        public long[] Sources;
        public Destination[] Destinations;
    }

    class Destination
    {
        public string Address;
        public long Value;
        public long ID;
    }
}
