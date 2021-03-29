using System;
using System.Collections.Generic;

namespace SimCityPak
{
    class TableGroupTypes : TGITable
    {
        public TableGroupTypes()
        {
            _tableName = "GroupTypes";
            _tableKeys = new string[] { "id", "name", "comments" };
            _dbMain = "database_main.s3db";
            _dbUser = "database_user.s3db";
        }
    }
}
