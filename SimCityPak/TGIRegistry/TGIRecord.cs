using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityPak
{
    public class TGIRecord
    {
        public Dictionary<string, string> Keys = new Dictionary<string, string>();

        public string GetString(string key)
        {
            string result;

            if (Keys.TryGetValue(key, out result))
            {
                return result;
            }

            return "";
        }

        public uint GetUint(string key)
        {

            string res = GetString(key);
            if (res != string.Empty)
            {
                return (uint)(System.Convert.ToInt64(res) & 0xFFFFFFFF); // SQLite stores in int
            }
            return 0;
        }

        public uint Id
        {
            get { return GetUint("id"); }
            set { Keys["id"] = value.ToString(); }
        }

        public string FormatSqlKeys()
        {
            return string.Join(",", Keys.Select(x => x.Key).ToArray());
        }

        public string FormatSqlValues()
        {
            return string.Join("','", Keys.Select(x => x.Value).ToArray());
        }

        public string Name
        {
            get { return GetString("name"); }
            set { Keys["name"] = value; }
        }

        public string Comments
        {
            get { return GetString("comments"); }
            set { Keys["comments"] = value; }
        }

        public string DisplayName // Not sure why we're still using this...
        {
            get { return GetString("name"); }
            set { Keys["name"] = value; }
        }

        public uint HexIdValue
        {
            get { return GetUint("id"); }
        }

        public string HexId
        {
            get { return "0x" + GetUint("id").ToString("X8"); }
            set { Keys["name"] = Convert.ToUInt32(value.Substring(2), 16).ToString(); }
        }
    }
}
