using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class DbInfoRepository : BaseRepository<DbInfoModel>
    {
        public override string TableName => "DbInfo";

        public DbInfoRepository(TenantDbDataContext tenantDbDataContext):base(tenantDbDataContext) { 
        }

        public override bool Insert(DbInfoModel dbInfo,out Int64 id) {
            string sql = @"Insert Into DbInfo (Name,Identifier,ServiceInfoId,Description)
                           Values (@name,@identifier,@ServiceInfoId,@description) RETURNING Id";

            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql,dbInfo);
            return true;
        }

        public override bool Update(DbInfoModel dbInfo) {
            string sql = @"Update DbInfo Set Name=@name,Identifier=@identifier,Description=@description Where Id=@id";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, dbInfo) > 0;
        }

        public PagingData<DbInfoModel> GetPage(int pageSize, int pageIndex, string name = null, string identifier = null,Int64? serviceInfoId=null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) {
                p["Name"] = name;
            }
            if (!string.IsNullOrEmpty(identifier)) {
                p["Identifier"] = identifier;
            }
            if (serviceInfoId.HasValue) {
                p["ServiceInfoId"]= serviceInfoId.Value;
            }

            return GetPage(pageSize, pageIndex, p);
        }

        public List<DbInfoModel> GetByIdentifier(List<string> identifierList) {
            string sql = "Select * From DbInfo Where Identifier=ANY(@identifierList)";
            return _tenantDbDataContext.Slave.QueryList<DbInfoModel>(sql, new { identifierList = identifierList.ToArray() });
        }

        public List<DbInfoModel> GetDbInfosByService(Int64 serviceInfoId) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "ServiceInfoId",serviceInfoId}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
