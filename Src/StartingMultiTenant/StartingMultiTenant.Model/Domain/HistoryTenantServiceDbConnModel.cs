using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class HistoryTenantServiceDbConnModel
    {
        public Int64 Id { get; set; }
        public Int64 DbConnId { get; set; }
        public string CreateScriptName { get; set; }
        public int CreateScriptVersion { get; set; }
        public int CurSchemaVersion { get; set; }
        public Int64 DbServerId { get; set; }
        public string EncryptedConnStr { get; set; }
    }
}
