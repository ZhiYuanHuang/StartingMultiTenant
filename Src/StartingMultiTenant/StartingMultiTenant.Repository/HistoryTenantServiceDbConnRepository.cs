using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class HistoryTenantServiceDbConnRepository : BaseRepository<HistoryTenantServiceDbConnModel>
    {
        public override string TableName => "HistoryTenantServiceDbConn";

        public HistoryTenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext)
            :base(tenantDbDataContext) { 
        }

        public bool InsertHistoryDbConn(TenantServiceDbConnModel tenantServiceDbConn, DbConnActionTypeEnum dbConnActionType) {
            string sql = @"Insert into HistoryTenantServiceDbConn (DbConnId,CreateScriptName,CreateScriptVersion,CurSchemaVersion,DbServerId,EncryptedConnStr,ActionType,CreateTime)
                           Values (@dbConnId,@createScriptName,@createScriptVersion,@curSchemaVersion,@dbServerId,@encryptedConnStr,@actionType,now())";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, 
                new { 
                    dbConnId=tenantServiceDbConn.Id,
                    createScriptName=tenantServiceDbConn.CreateScriptName,
                    createScriptVersion=tenantServiceDbConn.CreateScriptVersion,
                    curSchemaVersion =tenantServiceDbConn.CurSchemaVersion,
                    dbServerId=tenantServiceDbConn.DbServerId,
                    encryptedConnStr=tenantServiceDbConn.EncryptedConnStr,
                    actionType=(int)dbConnActionType
                }
                )>0;
        }

        public List<HistoryTenantServiceDbConnModel> GetByDbConn(Int64 dbConnId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "DbConnId",dbConnId}
            };

            var set= GetEntitiesByQuery(p);
            return set.ToList();
        }
    }
}
