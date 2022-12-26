using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class UpdateSchemaScriptDto
    {
        public string Name { get; set; }
        public string CreateScriptName { get; set; }
        public int BaseMajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string? DbNameWildcard { get; set; }
        public IFormFile UpdateScriptFile { get; set; }
        public IFormFile RollbackScriptFile { get; set; }
    }
}
