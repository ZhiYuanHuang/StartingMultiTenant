using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantCenterCreateDto
    {
        public string TenantDomain { get; set; }
        public string TenantIdentifier { get; set; }
        public string? TenantName { get; set; }
        public string? Description { get; set; }
        public List<string>? CreateDbScripts { get; set; }
    }
}
