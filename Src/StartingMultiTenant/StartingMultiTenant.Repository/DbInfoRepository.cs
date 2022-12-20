using StartingMultiTenant.Model.Domain;
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

        public bool Insert(DbInfoModel dbInfo) {
            string sql = @"Insert Into DbInfo (Name,Identifier,ServiceIdentifier,Description)
                           Values (@name,@identifier,@serviceIdentifier,@description)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,dbInfo)>0;
        }

        public bool Update(DbInfoModel dbInfo) {
            string sql = @"Update DbInfo Set Name=@name,Identifier=@identifier,Description=@description Where Id=@id";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, dbInfo) > 0;

        }

        public bool Delete(Int64 id) {
            string sql = "Delete From DbInfo Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id = id }) > 0;
        }

        public List<DbInfoModel> GetDbInfosByService(string serviceIdentifier) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "ServiceIdentifier",serviceIdentifier}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
