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

        public bool ExchangeDbServer(Int64 dbConnId, Int64 newDbServerId,string newEncryptConnStr) {
            string sql = @"Update TenantServiceDbConn Set DbServerId=@dbServerId,EncryptedConnStr=@encryptedConnStr Where Id=@id";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,new { dbServerId=newDbServerId, encryptedConnStr =newEncryptConnStr})>0;
        }

        public List<TenantServiceDbConnModel> GetConnListByDbServer(Int64 dbServerId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "DbServerId",dbServerId}
            };

            return GetEntitiesByQuery(p);
        }

        public List<TenantServiceDbConnModel> GetTenantServiceDbConns(string tenantDomain, string tenantIdentifier, string createScriptName) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantDomain",tenantDomain},
                {"TenantIdentifier",tenantIdentifier },
                {"CreateScriptName",createScriptName }
            };

            return GetEntitiesByQuery(p);
        }

        public List<TenantServiceDbConnModel> GetTenantServiceDbConns(string createScriptName, int createScriptVersion) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                {"CreateScriptName",createScriptName },
                 { "CreateScriptVersion",createScriptVersion}
            };

            return GetEntitiesByQuery(p);
        }

        public List<TenantServiceDbConnModel> GetTenantServiceDbConns(long? dbConnId = null) {
            if(!dbConnId.HasValue) {
                return GetEntitiesByQuery();
            }

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "Id",dbConnId.Value},
            };

            return GetEntitiesByQuery(p);
        }
    }
}
