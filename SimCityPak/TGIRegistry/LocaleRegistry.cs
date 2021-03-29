using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Package;
using Newtonsoft.Json;

namespace SimCityPak
{
    public class LocaleRegistry
    {
        public static LocaleRegistry Create()
        {
            LocaleRegistry registry = new LocaleRegistry();
            registry._localeData = new Dictionary<uint, Dictionary<uint, string>>();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LocaleFile))
            {

                FileInfo file = new FileInfo(Properties.Settings.Default.LocaleFile);
                if (file.Exists)
                {
                    DatabasePackedFile package = DatabasePackedFile.LoadFromFile(file.FullName);

                    //foreach JSON file
                    foreach (DatabaseIndex index in package.Indices.Where(i => i.TypeId == 0x0a98eaf0))
                    {
                        byte[] data = index.GetIndexData(true);

                        string test = ASCIIEncoding.UTF8.GetString(data, 3, data.Length - 3);

                        using (TextReader sr = new StringReader(test))
                        {
                            JsonTextReader reader = new JsonTextReader(sr);
                            reader.Read();
                            while (reader.Read())
                            {
                                if (reader.Value != null)
                                {
                                    if (reader.Value.Equals("//"))
                                    {
                                        //eat up the actual comment text
                                        reader.Read();
                                    }
                                    else
                                    {
                                        uint id = Convert.ToUInt32(((string)reader.Value).Substring(2), 16);
                                        string translation = reader.ReadAsString();
                                        if (!registry._localeData.ContainsKey(index.InstanceId))
                                        {
                                            registry._localeData[index.InstanceId] = new Dictionary<uint, string>();
                                        }

                                        registry._localeData[index.InstanceId][id] = translation;
                                    }
                                }

                            }
                        }
                    }

                }
            }
            return registry;
        }

        private Dictionary<uint, Dictionary<uint, string>> _localeData;

        public string GetLocalizedString(uint table, uint id)
        {
            if (_localeData.ContainsKey(table))
            {
                if (_localeData[table].ContainsKey(id))
                {
                    return _localeData[table][id];
                }
            }
            return string.Empty;
        }

        private static LocaleRegistry _instance;
        public static LocaleRegistry Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    _instance = Create();
                    return _instance;
                }

            }
            set { _instance = value; }
        }

    }
}
