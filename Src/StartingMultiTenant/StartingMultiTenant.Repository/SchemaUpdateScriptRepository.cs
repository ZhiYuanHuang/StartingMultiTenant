using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class SchemaUpdateScriptRepository : BaseRepository<SchemaUpdateScriptModel>
    {
        public override string TableName => "SchemaUpdateScript";
        public SchemaUpdateScriptRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public PagingData<SchemaUpdateScriptModel> GetPageNoContent(int pageSize, int pageIndex, string name = null,Int64? createDbScriptId=null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) {
                p["Name"] = name;
            }

            var orderDict = new Dictionary<string, bool>() {
                { "Name", true }
            };

            if (createDbScriptId.HasValue && createDbScriptId.Value > 0) {
                p["CreateDbScriptId"] = createDbScriptId.Value;
                orderDict = new Dictionary<string, bool>() {
                    { "MinorVersion", true }
                 };
            }
            
            List<string> selectFields = new List<string>() {
                "Id","Name","CreateDbScriptId","MinorVersion"
            };
            var pagingData = GetPage(pageSize, pageIndex, p, orderDict, selectFields);
            return pagingData;
        }

        public SchemaUpdateScriptModel GetNoContent(Int64 id) {
            string sql = $"Select Id,Name,CreateDbScriptId,MinorVersion From {TableName} Where Id=@id";
            return _tenantDbDataContext.Slave.Query<SchemaUpdateScriptModel>(sql, new { id = id });
        }

        public List<SchemaUpdateScriptModel> GetNoContent(List<Int64> ids) {
            if (ids == null || !ids.Any()) {
                return new List<SchemaUpdateScriptModel>();
            }
            string sql = $"Select Id,Name,CreateDbScriptId,MinorVersion From {TableName} Where Id=ANY(@ids)";
            return _tenantDbDataContext.Slave.QueryList<SchemaUpdateScriptModel>(sql, new { ids = ids.ToArray() });
        }

        public SchemaUpdateScriptModel GetSchemaUpdateScripts(string scriptName) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"Name",scriptName }
            };

            return GetEntityByQuery(p);
        }



        public List<SchemaUpdateScriptModel> GetSchemaUpdateScripts(Int64 createDbScriptId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"CreateDbScriptId",createDbScriptId }
            };

            return GetEntitiesByQuery(p);
        }

        public override bool Insert(SchemaUpdateScriptModel schemaUpdateScript,out Int64 id) {

            string sql = @"Select Max(MinorVersion) From SchemaUpdateScript Where CreateDbScriptId=@createDbScriptId";
            object curMinorVersionObj = _tenantDbDataContext.Master.ExecuteScalar(sql, new { createDbScriptId = schemaUpdateScript.CreateDbScriptId });
            int minorVersion = 1;
            if (curMinorVersionObj != null) {
                minorVersion = Convert.ToInt32(curMinorVersionObj) + 1;
            }
            schemaUpdateScript.MinorVersion = minorVersion;
            
            sql = @"Insert Into SchemaUpdateScript (Name,CreateDbScriptId,MinorVersion,BinaryContent,RollBackScriptBinaryContent)
                           Values
                           (@name,@createDbScriptId,@minorVersion,@binaryContent,@rollBackScriptBinaryContent) RETURNING Id";

            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql,schemaUpdateScript);
            return true;
        }

    }
}