using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantIdentifierDto: TenantIdentifierModel
    {
        public List<Int64>? CreateDbScriptIds { get; set; }
        public Dictionary<string, Int64>? CreateDbs { get; set; }
    }
}
