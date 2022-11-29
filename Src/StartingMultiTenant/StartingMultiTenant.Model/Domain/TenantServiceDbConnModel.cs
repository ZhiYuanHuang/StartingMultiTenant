using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class TenantServiceDbConnModel
    {
        public Int64 Id { get; set; }
        public string TenantIdentifier { get; set; }
        public string TenantDomain { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public int CreateScriptVersion { get; set; }
        public int CurSchemaVersion { get; set; }
        public Int64 DbServerId { get; set; }
        public string EncryptedConnStr { get; set; }

       
    }
}
