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
        public string EncryptedConnStr { get; set; }
        public string CreateScriptName { get; set; }
        public string BaseCreateScriptVersion { get; set; }
        public string CurCreateScriptVersion { get; set; }
    }
}
