using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class SchemaUpdateScriptModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public object BinaryContent { get; set; }
        public object RollBackScriptBinaryContent { get; set; }
        public string CreateScriptName { get; set; }
        public int BaseMajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string DbNameWildcard { get; set; }
    }
}
