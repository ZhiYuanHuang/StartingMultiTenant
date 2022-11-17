using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class SchemaUpdateScriptModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string AfterSuccessVersion { get; set; }
        public string FilePath { get; set; }
        public string RollBackScriptPath { get; set; }
        public string CreateScriptName { get; set; }
        public string CreateScriptVersionStart { get; set; }
        public string CreateScriptVersionEnd { get; set; }
    }
}
