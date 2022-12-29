using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ClientDomainScopesDto
    {
        public string ClientId { get; set; }
        public List<DomainScopesDto> DomainScopes { get; set; }
    }

    public class DomainScopesDto {
        public string TenantDomain { get; set; }
        public List<string> Scopes { get; set; }
    }
}
