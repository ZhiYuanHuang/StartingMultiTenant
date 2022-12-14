using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
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

        public PagingData<TenantIdentifierModel> GetPage(int pageSize,int pageIndex, string tenantDomain=null) {
            StringBuilder countBuilder = new StringBuilder("Select Count(*) From TenantIdentifier ");
            StringBuilder dataBuilder = new StringBuilder("Select * From TenantIdentifier ");

            Dictionary<string,object> p= new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };
            if (!string.IsNullOrEmpty(tenantDomain)) {
                countBuilder.Append(" Where TenantDomain=@tenantDomain ");
                dataBuilder.Append(" Where TenantDomain=@tenantDomain ");
                p["tenantDomain"] = tenantDomain;
            }

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");

            int count=(int)((long) _tenantDbDataContext.Slave.ExecuteScalar(countBuilder.ToString(), p));

            if (count == 0) {
                return new PagingData<TenantIdentifierModel>(pageIndex,pageSize,0,new List<TenantIdentifierModel>());
            }

            var list= _tenantDbDataContext.Slave.QueryList<TenantIdentifierModel>(dataBuilder.ToString(),p);
            return new PagingData<TenantIdentifierModel>(pageIndex,pageSize, count, list);
        }
    }
}
