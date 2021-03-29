using System;
using System.Collections.Generic;

namespace SimCityPak
{
    class TableGroups : TGITable
    {
        public TableGroups()
        {
            _tableName = "Groups";
            _tableKeys = new string[] { "id", "name", "comments" };
            _dbMain = "database_main.s3db";
            _dbUser = "database_user.s3db";
        }
    }
}
