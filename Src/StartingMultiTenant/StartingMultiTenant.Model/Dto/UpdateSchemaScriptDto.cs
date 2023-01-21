using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class UpdateSchemaScriptDto
    {
        public string Name { get; set; }
        public Int64 CreateDbScriptId { get; set; }
        public int MinorVersion { get; set; }
        public List<Base64ScriptFile> UpdateScriptAttachments { get; set; }
        public List<Base64ScriptFile> RollBackScriptAttachments { get; set; }
    }
}
