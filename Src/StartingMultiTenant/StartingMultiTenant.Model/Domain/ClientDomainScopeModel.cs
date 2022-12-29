using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class ClientDomainScopeModel
    {
        public Int64 Id { get; set; }
        public string ClientId { get; set; }
        public string TenantDomain { get; set; }
        public string Scope { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
