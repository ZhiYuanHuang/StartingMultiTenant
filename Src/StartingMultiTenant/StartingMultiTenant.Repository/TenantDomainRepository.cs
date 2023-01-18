using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
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

        public TenantDomainModel Get(string tenantDomain) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantDomain",tenantDomain}
            };

            return GetEntityByQuery(p);
        }

        public override bool Insert(TenantDomainModel tenantDomain,out Int64 id) {
            string sql = "Insert Into TenantDomain (TenantDomain) Values (@tenantDomain) RETURNING Id";
            id=(long) _tenantDbDataContext.Master.ExecuteScalar(sql,tenantDomain);
            return true;
        }

        public bool Delete(string tenantDomain) {
            string sql = "Delete From TenantDomain Where TenantDomain =@tenantDomain";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { tenantDomain=tenantDomain})>0;
        }

        public PagingData<TenantDomainModel> GetPage(int pageSize,int pageIndex,string tenantDomain=null) {
            StringBuilder countBuilder = new StringBuilder("Select Count(*) From TenantDomain ");
            StringBuilder dataBuilder = new StringBuilder("Select * From TenantDomain ");

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };
            if (!string.IsNullOrEmpty(tenantDomain)) {
                countBuilder.Append(" Where TenantDomain=@tenantDomain ");
                dataBuilder.Append(" Where TenantDomain=@tenantDomain ");
                p["tenantDomain"] = tenantDomain;
            }

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");
            int count = (int)((long)_tenantDbDataContext.Slave.ExecuteScalar(countBuilder.ToString(), p));

            if (count == 0) {
                return new PagingData<TenantDomainModel>(pageIndex, pageSize, 0, new List<TenantDomainModel>());
            }

            var list = _tenantDbDataContext.Slave.QueryList<TenantDomainModel>(dataBuilder.ToString(), p);
            return new PagingData<TenantDomainModel>(pageIndex, pageSize, count, list);

        }
    }
}
