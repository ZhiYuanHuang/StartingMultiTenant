using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class CreateDbScriptDto
    {
        public string Name { get; set; }
        public int MajorVersion { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public string? DbNameWildcard { get; set; }
        public int DbType { get; set; }

        public IFormFile ScriptFile { get; set; }
    }
}
