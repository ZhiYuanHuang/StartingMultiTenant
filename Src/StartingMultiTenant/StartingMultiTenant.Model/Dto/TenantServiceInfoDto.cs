using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantServiceInfoDto:TenantInfoDto
    {
        public string? ServiceIdentifier { get; set; }
    }
}
