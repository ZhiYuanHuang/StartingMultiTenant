using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class ExternalTenantServiceDbConnModel
    {
        public Int64 Id { get; set; }
        public string TenantIdentifier { get; set; }
        public string TenantDomain { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public string? EncryptedConnStr { get; set; }
        public string? OverrideEncryptedConnStr { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
