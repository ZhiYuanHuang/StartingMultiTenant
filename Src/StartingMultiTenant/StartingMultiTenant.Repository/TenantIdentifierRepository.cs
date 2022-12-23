using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class TenantIdentifierRepository : BaseRepository<TenantIdentifierModel>
    {
        public override string TableName => "TenantIdentifier";

        public TenantIdentifierRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {

        }

        public bool Insert(TenantIdentifierModel tenantIdentifier) {
            string sql = "Insert Into TenantIdentifier (TenantGuid,TenantIdentifier,TenantDomain) Values (@tenantGuid,@tenantIdentifier,@tenantDomain)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,tenantIdentifier)>0;
        }

        public bool Delete(string tenantGuid) {
            string sql = "Delete From TenantIdentifier Where TenantGuid=@tenantGuid";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { tenantGuid = tenantGuid })>0;
        }

        public List<TenantIdentifierModel> GetTenantListByDomain(string tenantDomain) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantDomain",tenantDomain}
            };

            return GetEntitiesByQuery(p);
        }

        public List<TenantIdentifierModel> GetPageByDomain(string tenantDomain,int pageSize,int pageIndex) {
            string sql = $"Select * From TenantIdentifier Where TenantDomain=@tenantDomain Limit @pageSize OFFSET @offSet";
            return _tenantDbDataContext.Slave.QueryList<TenantIdentifierModel>(sql, new { tenantDomain=tenantDomain,pageSize=pageSize,offSet= pageSize * pageIndex });
        }
    }
}
