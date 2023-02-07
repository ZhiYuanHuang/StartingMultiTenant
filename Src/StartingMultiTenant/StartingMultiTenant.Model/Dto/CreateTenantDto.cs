using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class CreateTenantDto: TenantInfoDto
    {

        public bool OverrideWhenExisted { get; set; }
        public List<string>? CreateDbScripts { get; set; }
        public List<Int64>? CreateDbScriptIds { get; set; }
        public List<Int64>? NewCreateDbScriptIds { get; set; }
        public Int64 TenantDomainId { get; set; }
        public Dictionary<string, Int64>? CreateDbs { get; set; }
    }
}
