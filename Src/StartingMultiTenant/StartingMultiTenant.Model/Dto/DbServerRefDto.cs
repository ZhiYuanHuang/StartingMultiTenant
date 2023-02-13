using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class DbServerRefDto
    {
        public Int64 DbServerId { get; set; }
        public int TenantDomainCount { get; set; }
        public int TenantCount { get; set; }
        public int DbConnCount { get; set; }
        public int ServiceCount { get; set; }
    }
}
