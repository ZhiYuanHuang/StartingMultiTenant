using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class SchemaUpdateScriptModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string RollBackScriptPath { get; set; }
        public string CreateScriptName { get; set; }
        public int BaseMajorVersion { get; set; }
        public int MinorVersion { get; set; }

        public string DbNameWildcard { get; set; }
    }
}
