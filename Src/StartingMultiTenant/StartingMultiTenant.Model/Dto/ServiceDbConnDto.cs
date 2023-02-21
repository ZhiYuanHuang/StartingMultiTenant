using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ServiceDbConnDto
    {
        public Int64 Id { get; set; }
        public string ServiceIdentifier { get; set; }
        public string DbIdentifier { get; set; }
        public string? EncryptedConnStr { get; set; }
        public string DecryptDbConn { get; set; }
        public string OverrideDbConn { get; set; }

        public int MajorVersion { get; set; }
        public int MinorVersion { get; set;}

        public int ActionType { get; set; }
        public Int64? DbServerId { get; set; }
        public string CreateScriptName { get; set; }
        public DateTime? CreateTime { get; set; }

        public string TenantDomain { get; set; }
        public string TenantIdentifier { get; set; }
    }
}
