using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantCreateScriptDto: CreateDbScriptDto
    {
        public Int64 TenantId { get; set; }
        //public Int64 CreateScriptId { get; set; }
    }
}
