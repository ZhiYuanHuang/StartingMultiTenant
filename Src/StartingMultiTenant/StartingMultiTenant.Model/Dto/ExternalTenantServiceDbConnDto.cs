using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ExternalTenantServiceDbConnDto:ExternalTenantServiceDbConnModel
    {
        public string? DbConnStr { get; set; }
        public string? OverrideDbConnStr { get; set; }
        public Int64 TenantDomainId { get; set; }
        public Int64 ServiceInfoId { get; set; }
        public Int64 DbInfoId { get; set; }
    }
}
