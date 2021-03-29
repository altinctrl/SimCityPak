using System;
using System.Collections.Generic;

namespace SimCityPak
{
    class TableFileTypes : TGITable
    {
        public TableFileTypes()
        {
            _tableName = "FileTypes";
            _tableKeys = new string[]{ "id", "name", "comments", "viewer" };
            _dbMain = "database_main.s3db";
            _dbUser = "database_user.s3db";
        }

        public string GetViewer(uint id)
        {
            TGIRecord result;
            if (Cache.TryGetValue((uint)id, out result))
            {
                return result.GetString("viewer");
            }

            return "";
        }
    }
}
