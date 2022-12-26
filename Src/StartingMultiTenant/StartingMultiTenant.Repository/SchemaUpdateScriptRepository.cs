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

        public SchemaUpdateScriptModel GetSchemaUpdateScripts(string scriptName) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"Name",scriptName }
            };

            return GetEntityByQuery(p);
        }

        public List<SchemaUpdateScriptModel> GetSchemaUpdateScripts(string createScriptName,int baseMajorVersion) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"CreateScriptName",createScriptName },
                 { "BaseMajorVersion",baseMajorVersion}
            };

            return GetEntitiesByQuery(p);
        }

        public bool Insert(SchemaUpdateScriptModel schemaUpdateScript) {
            string sql = @"Insert Into SchemaUpdateScript (Name,CreateScriptName,BaseMajorVersion,MinorVersion,DbNameWildcard,BinaryContent,RollBackScriptBinaryContent)
                           Values
                           (@name,@createScriptName,@baseMajorVersion,@minorVersion,@dbNameWildcard,@binaryContent,@rollBackScriptBinaryContent)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,schemaUpdateScript)>0;
        }

        public bool Delete(Int64 scriptId) {
            string sql = "Delete From SchemaUpdateScript Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,new {id=scriptId })>0;
        }
    }
}
