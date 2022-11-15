using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class TenantDomainModel
    {
        public Int64 Id { get; set; }
        public string TenantDomain { get; set; }
    }
}
