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
        public int MajorVersion { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public string DbNameWildcard { get; set; }
        public string FilePath { get; set; }
        public Object BinaryContent { get; set; }
        public int DbType { get; set; }
       
    }
}
