using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantInfoDto
    {
        public Int64 Id { get; set; }
        public string? TenantDomain { get; set; }
        public string? TenantIdentifier { get; set; }
        public string? TenantGuid { get; set; }
    }
}
