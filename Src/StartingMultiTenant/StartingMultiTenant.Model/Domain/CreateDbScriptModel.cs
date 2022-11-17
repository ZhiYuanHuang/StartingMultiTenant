using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class CreateDbScriptModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string RollBackScriptPath { get; set; }
        public int DbType { get; set; }
        public string ServiceIdentifier { get; set; }
    }
}
