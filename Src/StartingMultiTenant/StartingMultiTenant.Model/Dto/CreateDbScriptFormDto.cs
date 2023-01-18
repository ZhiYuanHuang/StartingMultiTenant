using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class CreateDbScriptFormDto
    {
        public string Name { get; set; }
        public int MajorVersion { get; set; }
        //public string ServiceIdentifier { get; set; }
        //public string DbIdentifier { get; set; }
        public Int64 ServiceInfoId { get; set; }
        public Int64 DbInfoId { get; set; }
        public string? DbNameWildcard { get; set; }
        public int DbType { get; set; }

        public List<Base64ScriptFile> Attachments { get; set; }
    }

    public class Base64ScriptFile {
        public string Src { get; set; }
    }
}
