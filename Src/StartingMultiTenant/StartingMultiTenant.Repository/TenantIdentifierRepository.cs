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

        public override bool Insert(TenantIdentifierModel tenantIdentifier,out Int64 id) {
            string sql = "Insert Into TenantIdentifier (TenantGuid,TenantIdentifier,TenantDomain,TenantName,Description,CreateTime) Values (@tenantGuid,@tenantIdentifier,@tenantDomain,@tenantName,@description,now()) RETURNING Id";

            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql, tenantIdentifier);
            return true;
        }

        public override bool Update(TenantIdentifierModel t) {
            string sql = "Update TenantIdentifier Set TenantName=@tenantName,Description=@description,UpdateTime=now() Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, t) > 0;
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

        public bool ExistTenant(string tenantDomain, string tenantIdentifier, out TenantIdentifierModel model) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantIdentifier",tenantIdentifier},
                { "TenantDomain",tenantDomain}
            };

            model=GetEntityByQuery(p);
            return model != null;
        }

        public PagingData<TenantIdentifierDto> GetPage(int pageSize, int pageIndex, Func<TenantIdentifierModel, TenantIdentifierDto> mappingFunc, string tenantDomain = null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(tenantDomain)) {
                p["TenantDomain"] = tenantDomain;
            }
            //return GetPage(pageSize, pageIndex, p,mappingFunc);

            Dictionary<string, bool> orderDict = new Dictionary<string, bool>() {
                { "TenantDomain",true},
                { "TenantIdentifier",true}
            };

            return GetPageWithMaping<TenantIdentifierDto>(pageSize, pageIndex, p, mappingFunc, orderDict);
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
