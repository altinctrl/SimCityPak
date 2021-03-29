using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityPak
{
    public class TGIRandomGenerator
    {
        static Random random = new Random();

        public static uint GetNext()
        {
            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;

            return fullRange;
        }
    }
}
