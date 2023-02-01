using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
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

        public bool InsertOrUpdate(TenantServiceDbConnModel dbConn) {
            string sql = @"Insert Into TenantServiceDbConn (TenantIdentifier,TenantDomain,ServiceIdentifier,DbIdentifier,CreateScriptName,CreateScriptVersion,CurSchemaVersion,DbServerId,EncryptedConnStr)
                           Values (@tenantIdentifier,@tenantDomain,@serviceIdentifier,@dbIdentifier,@createScriptName,@createScriptVersion,@curSchemaVersion,@dbServerId,@encryptedConnStr)
                           ON CONFLICT ON CONSTRAINT u_tenantservicedbconn_1
                           DO Update Set
                             ServiceIdentifier=@serviceIdentifier,DbIdentifier=@dbIdentifier,CreateScriptVersion=@createScriptVersion,CurSchemaVersion=@curSchemaVersion,DbServerId=@dbServerId,EncryptedConnStr=@encryptedConnStr";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,dbConn)>0;
           
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

        public List<TenantServiceDbConnModel> GetTenantServiceDbConns(long? dbConnId = null) {
            if (!dbConnId.HasValue) {
                return GetEntitiesByQuery();
            }

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "Id",dbConnId.Value},
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

        public List<TenantServiceDbConnModel> GetConnByTenantAndService(string tenantDomain, string tenantIdentifier,string serviceIdentifier=null) {
           
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantIdentifier",tenantIdentifier},
                { "TenantDomain",tenantDomain}
            };

            if (!string.IsNullOrEmpty(serviceIdentifier)) {
                p.Add("ServiceIdentifier",serviceIdentifier);
            }

            return GetEntitiesByQuery(p);
        }

        public TenantServiceDbConnModel GetConnByTenantAndCreateScript(string tenantDomain, string tenantIdentifier, string createScriptName) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantDomain",tenantDomain},
                {"TenantIdentifier",tenantIdentifier },
                {"CreateScriptName",createScriptName },
            };

            return GetEntityByQuery(p);
        }

        public PagingData<TenantServiceDbConnDto> GetPage(int pageSize, int pageIndex,Func<TenantServiceDbConnModel,TenantServiceDbConnDto> mappingFunc, string tenantDomain = null, string tenantIdentifier = null, string serviceIdentifier = null, string dbIdentifier = null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(tenantIdentifier)) {
                p["TenantIdentifier"] = tenantIdentifier;
            }
            if (!string.IsNullOrEmpty(tenantDomain)) {
                p["TenantDomain"] = tenantDomain;
            }
            if (!string.IsNullOrEmpty(serviceIdentifier)) {
                p["ServiceIdentifier"] = serviceIdentifier;
            }
            if (!string.IsNullOrEmpty(dbIdentifier)) {
                p["DbIdentifier"] = dbIdentifier;
            }
            //return GetPage(pageSize, pageIndex, p,mappingFunc);

            Dictionary<string, bool> orderDict = new Dictionary<string, bool>() {
                { "CreateScriptName",true},
                { "CreateScriptVersion",true},
                { "CurSchemaVersion",true},
            };

            return GetPageWithMaping<TenantServiceDbConnDto>(pageSize, pageIndex, p, mappingFunc, orderDict);
        }
    }
}
