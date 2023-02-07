using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class TenantIdentifierModel
    {
        public Int64 Id { get; set; }
        public string TenantGuid { get; set; }
        public string TenantIdentifier { get; set; }
        public string TenantDomain { get; set; }
        public string TenantName { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
