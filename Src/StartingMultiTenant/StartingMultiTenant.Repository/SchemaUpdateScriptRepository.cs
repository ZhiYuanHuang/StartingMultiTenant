using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class SchemaUpdateScriptRepository : BaseRepository<SchemaUpdateScriptModel>
    {
        public override string TableName => "SchemaUpdateScript";
        public SchemaUpdateScriptRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public List<SchemaUpdateScriptModel> GetSchemaUpdateScripts(string createScriptName,int baseMajorVersion) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"CreateScriptName",createScriptName },
                 { "BaseMajorVersion",baseMajorVersion}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
