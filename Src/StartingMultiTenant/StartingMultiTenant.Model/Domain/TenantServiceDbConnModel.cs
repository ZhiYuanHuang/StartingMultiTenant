using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class TenantServiceDbConnModel
    {
        public Int64 Id { get; set; }
        public Int64 TenantId { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public bool BeControlled { get; set; }
        public string CreateScriptVersion { get; set; }
        public string CurSchemaVersion { get; set; }
        public Int64 DbServerId { get; set; }
        public string EncryptedConnStr { get; set; }

       
    }
}
