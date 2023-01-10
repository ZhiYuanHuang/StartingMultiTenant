using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantInfoDto
    {
        public string? TenantDomain { get; set; }
        public string? TenantIdentifier { get; set; }
    }
}
