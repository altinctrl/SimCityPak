using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Package;

namespace SimCityPak
{
    class DatabaseIndexData
    {
        public DatabaseIndexData(DatabaseIndex index, byte[] data)
        {
            _index = index;
            _data = data;
        }

        private DatabaseIndex _index;

        public DatabaseIndex Index
        {
            get { return _index; }
            set { _index = value; }
        }
        private byte[] _data;

        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }

    }
}
