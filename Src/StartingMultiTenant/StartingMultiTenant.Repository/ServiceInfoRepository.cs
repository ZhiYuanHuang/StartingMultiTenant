using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ServiceInfoRepository : BaseRepository<ServiceInfoModel>
    {
        public override string TableName => "ServiceInfo";

        public ServiceInfoRepository(TenantDbDataContext tenantDbDataContext):base(tenantDbDataContext) { 

        }

        public bool Insert(ServiceInfoModel serviceInfo) {
            string sql = @"Insert Into ServiceInfo (Name,Identifier,Description)
                           Values (@name,@identifier,@description)";

            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, serviceInfo) > 0;
        }

        public bool Update(ServiceInfoModel serviceInfo) {
            string sql = "Update ServiceInfo Set Name=@name,Identifier=@identifier,Description=@description Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,serviceInfo)>0;
        }

        public bool Delete(Int64 id) {
            string sql = "Delete From ServiceInfo Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,new { id=id})>0;
        }
    }
}
