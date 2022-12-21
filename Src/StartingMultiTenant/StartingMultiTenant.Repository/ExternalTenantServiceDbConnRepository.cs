using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ExternalTenantServiceDbConnRepository : BaseRepository<ExternalTenantServiceDbConnModel>
    {
        public override string TableName => "ExternalTenantServiceDbConn";

        public ExternalTenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext):base(tenantDbDataContext) {

        }

        public bool InsertOrUpdate(ExternalTenantServiceDbConnModel externalTenantServiceDbConn) {
            string sql = @"Insert Into ExternalTenantServiceDbConn
                           (TenantIdentifier,TenantDomain,ServiceIdentifier,DbIdentifier,EncryptedConnStr,UpdateTime)
                           Values 
                           (@tenantIdentifier,@tenantDomain,@serviceIdentifier,@dbIdentifier,@encryptedConnStr,now())
                           ON CONFLICT ON CONSTRAINT u_externaltenantservicedbconn_1
                           Do Update Set
                             OverrideEncryptedConnStr=ExternalTenantServiceDbConn.EncryptedConnStr,
                             EncryptedConnStr=EXCLUDED.EncryptedConnStr,
                             UpdateTime=now()";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,externalTenantServiceDbConn)>0;
        }

        public bool Delete(Int64 dbConnId) {
            string sql = "Delete From ExternalTenantServiceDbConn Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id=dbConnId})>0;
        }

        public List<ExternalTenantServiceDbConnModel> GetByTenantAndService(string tenantDomain,string tenantIdentifier,string serviceIdentifier = null) {

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "tenantIdentifier",tenantIdentifier},
                { "tenantDomain",tenantDomain}
            };

            if (!string.IsNullOrEmpty(serviceIdentifier)) {
                
                p["serviceIdentifier"] = serviceIdentifier;
            }
            return GetEntitiesByQuery(p);        }
    }
}
