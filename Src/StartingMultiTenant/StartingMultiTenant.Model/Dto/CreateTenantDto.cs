using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class CreateTenantDto
    {
        public string TenantDomain { get; set; }
        public string TenantIdentifier { get; set; }
        public List<string> CreateDbScripts { get; set; }
    }
}
