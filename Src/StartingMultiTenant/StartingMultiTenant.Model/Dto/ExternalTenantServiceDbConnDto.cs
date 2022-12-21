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
    }
}
