using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace StartingMultiTenant.Repository
{
    public class ExternalTenantServiceDbConnRepository : BaseRepository<ExternalTenantServiceDbConnModel>
    {
        public override string TableName => "ExternalTenantServiceDbConn";

        public ExternalTenantServiceDbConnRepository(TenantDbDataContext tenantDbDataContext):base(tenantDbDataContext) {

        }

        public override bool Insert(ExternalTenantServiceDbConnModel externalTenantServiceDbConn,out Int64 id) {
            string sql = @"Insert Into ExternalTenantServiceDbConn
                           (TenantIdentifier,TenantDomain,ServiceIdentifier,DbIdentifier,EncryptedConnStr,UpdateTime)
                           Values 
                           (@tenantIdentifier,@tenantDomain,@serviceIdentifier,@dbIdentifier,@encryptedConnStr,now())
                           ON CONFLICT ON CONSTRAINT u_externaltenantservicedbconn_1
                           Do Update Set
                             OverrideEncryptedConnStr=ExternalTenantServiceDbConn.EncryptedConnStr,
                             EncryptedConnStr=EXCLUDED.EncryptedConnStr,
                             UpdateTime=now()
                            RETURNING Id";

            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql,externalTenantServiceDbConn);
            return true;
        }

        public override bool Update(ExternalTenantServiceDbConnModel t) {
            string sql = @"Update ExternalTenantServiceDbConn 
                             Set OverrideEncryptedConnStr=ExternalTenantServiceDbConn.EncryptedConnStr,
                             EncryptedConnStr=@encryptedConnStr,
                             UpdateTime=now() Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, t) > 0;
        }

        public PagingData<ExternalTenantServiceDbConnModel> GetPage(int pageSize,int pageIndex,string tenantDomain=null,string tenantIdentifier=null,string serviceIdentifier=null,string dbIdentifier=null) {
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
            return GetPage(pageSize,pageIndex,p);
        }

        public List<ExternalTenantServiceDbConnModel> GetByTenantAndService(string tenantDomain,string tenantIdentifier,string serviceIdentifier = null) {

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "tenantIdentifier",tenantIdentifier},
                { "tenantDomain",tenantDomain}
            };

            if (!string.IsNullOrEmpty(serviceIdentifier)) {
                
                p["serviceIdentifier"] = serviceIdentifier;
            }
            return GetEntitiesByQuery(p);  
        }
    }
}
