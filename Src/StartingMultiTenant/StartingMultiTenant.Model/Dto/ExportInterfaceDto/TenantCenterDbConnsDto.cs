using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto.ExportInterfaceDto
{
    public class TenantCenterDbConnDto {
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public string DbConn { get; set; }
    }
    public class TenantCenterDbConnsDto
    {
        public string TenantDomain { get; set; }
        public string TenantIdentifier { get; set; }

        public bool NoExist { get; set; }
        public List<TenantCenterDbConnDto> InnerDbConnList { get; set; }
        public List<TenantCenterDbConnDto> ExternalDbConnList { get; set; }
    }
}
