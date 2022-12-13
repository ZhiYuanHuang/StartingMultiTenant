using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class TenantServiceDbConnRepository : BaseRepository<TenantServiceDbConnModel>
    {
        public override string TableName => "TenantServiceDbConn";

        public TenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext)
            :base(tenantDbDataContext) { 
        }

        public bool BatchInsertDbConns(List<TenantServiceDbConnModel> dbConnList) {
            string sql = @"Insert Into TenantServiceDbConn (TenantIdentifier,TenantDomain,ServiceIdentifier,DbIdentifier,CreateScriptName,CreateScriptVersion,CurSchemaVersion,DbServerId,EncryptedConnStr)
                           Values (@tenantIdentifier,@tenantDomain,@serviceIdentifier,@dbIdentifier,@createScriptName,@createScriptVersion,@curSchemaVersion,@dbServerId,@encryptedConnStr)";

            bool success = false;
            try {
                BeginTransaction();

                foreach(var dbConn in dbConnList) {
                    bool insertResult= _tenantDbDataContext.Master.ExecuteNonQuery(sql,dbConn)>0;
                    if (!insertResult) {
                        success = false;
                        throw new Exception($"insert new dbconn error,dbconn:{Newtonsoft.Json.JsonConvert.SerializeObject(dbConn)}");
                    }
                }
                
                CommitTransaction();
                success = true;
            }
            catch(Exception ex) {
                success = false;
                RollbackTransaction();
            }

            return success;
        }

        public List<TenantServiceDbConnModel> GetConnListByDbServer(Int64 dbServerId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "DbServerId",dbServerId}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
