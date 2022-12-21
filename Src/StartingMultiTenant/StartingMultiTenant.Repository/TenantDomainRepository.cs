using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class TenantDomainRepository : BaseRepository<TenantDomainModel>
    {
        public override string TableName => "TenantDomain";

        public TenantDomainRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {

        }

        public bool Insert(TenantDomainModel tenantDomain) {
            string sql = "Insert Into TenantDomain (TenantDomain) Values (@tenantDomain)";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,tenantDomain)>0;
        }

        public bool Delete(string tenantDomain) {
            string sql = "Delete From TenantDomain Where TenantDomain =@tenantDomain";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { tenantDomain=tenantDomain})>0;
        }
    }
}
