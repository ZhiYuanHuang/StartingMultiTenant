using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class GroupCreateDbScriptDto
    {
        public string Name { get; set; }
        public string? ServiceName { get; set; }
        public string? DbName { get; set; }
        public List<VersionScriptDto> VersionScripts { get; set; }
    }

    public class VersionScriptDto {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set;}
        public string ServiceName { get; set; }
        public string DbName { get; set; }
        public string? ServiceIdentifier { get; set; }
        public string? DbIdentifier { get; set; }
    }
}
