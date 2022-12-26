using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantServiceDbConnsDto
    {
        public string TenantDomain { get; set; }
        public string TenantIdentifier { get; set; }

        public List<ServiceDbConnsDto> InnerDbConnList { get; set; }
        public List<ServiceDbConnsDto> ExternalDbConnList { get; set; }
        public List<ServiceDbConnsDto> MergeDbConnList { get; set; }
    }
}
