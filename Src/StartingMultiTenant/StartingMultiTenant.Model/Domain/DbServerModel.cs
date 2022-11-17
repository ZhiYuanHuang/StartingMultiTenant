using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class DbServerModel
    {
        public Int64 Id { get; set; }
        public int DbType { get; set; }
        public string ServerHost { get; set; }
        public string ServerPort { get; set; }
        public string UserName { get; set; }
        public string EncryptUserpwd { get; set; }
    }
}
