using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;

namespace SimCityPak
{
    class TGIRegistry
    {
        public TableFileTypes FileTypes = new TableFileTypes();
        public TableGroups Groups = new TableGroups();
        public TableGroupTypes GroupTypes = new TableGroupTypes();
        public TableInstances Instances = new TableInstances();
        public TableInstanceTypes InstanceTypes = new TableInstanceTypes();
        public TableProperties Properties = new TableProperties();

        public TGIRegistry()
        {
        }

        private static TGIRegistry _instance;
        public static TGIRegistry Instance
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

        public void SqlCacheLoadAll()
        {
            FileTypes.LoadCache();
            Groups.LoadCache();
            GroupTypes.LoadCache();
            Instances.LoadCache();
            InstanceTypes.LoadCache();
            Properties.LoadCache();

            
        }

        public void SqlCacheFlushAll()
        {
            FileTypes.Close();
            Groups.Close();
            GroupTypes.Close();
            Instances.Close();
            InstanceTypes.Close();
            Properties.Close();
        }

        public void SqlCacheReloadAll()
        {
            SqlCacheFlushAll();
            SqlCacheLoadAll();
        }

        private static TGIRegistry Create()
        {
            TGIRegistry result = new TGIRegistry();
            result.SqlCacheLoadAll();

            return result;
        }
    }
}
