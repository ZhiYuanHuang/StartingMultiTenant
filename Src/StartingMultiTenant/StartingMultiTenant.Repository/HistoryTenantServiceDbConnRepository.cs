using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class HistoryTenantServiceDbConnRepository : BaseRepository<HistoryTenantServiceDbConnModel>
    {
        public override string TableName => "HistoryTenantServiceDbConn";

        public HistoryTenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext)
            :base(tenantDbDataContext) { 
        }

        public bool InsertHistoryDbConn(TenantServiceDbConnModel tenantServiceDbConn) {
            string sql = @"Insert into HistoryTenantServiceDbConn (DbConnId,CreateScriptName,CreateScriptVersion,CurSchemaVersion,DbServerId,EncryptedConnStr)
                           Values (@dbConnId,@createScriptName,@createScriptVersion,@curSchemaVersion,@dbServerId,@encryptedConnStr)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, 
                new { 
                    dbConnId=tenantServiceDbConn.Id,
                    createScriptName=tenantServiceDbConn.CreateScriptName,
                    curSchemaVersion=tenantServiceDbConn.CurSchemaVersion,
                    dbServerId=tenantServiceDbConn.DbServerId,
                    encryptedConnStr=tenantServiceDbConn.EncryptedConnStr,
                }
                )>0;
        }
    }
}
