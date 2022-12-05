using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace StartingMultiTenant.Framework
{
    public class DbDataContextOption
    {
        public DbTypeEnum DbType { get; set; }
        public string MasterConnStr { get; set; }
        public string SlaveConnStr { get; set; }

    }
}
