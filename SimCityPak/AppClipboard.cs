using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimCityPak
{
    public class AppClipboard
    {
        private static AppClipboard _instance;
        public static AppClipboard Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppClipboard();
                }
                return _instance;
            }
        }

        public object Properties { get; set; }
    }
}
