using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class ServiceInfoRepository : BaseRepository<ServiceInfoModel>
    {
        public override string TableName => "ServiceInfo";

        private readonly ILogger<ServiceInfoRepository> _logger;
        public ServiceInfoRepository(TenantDbDataContext tenantDbDataContext,
            ILogger<ServiceInfoRepository> logger):base(tenantDbDataContext) {
            _logger = logger;
        }

        public override bool Insert(ServiceInfoModel serviceInfo,out Int64 id) {
            string sql = @"Insert Into ServiceInfo (Name,Identifier,Description)
                           Values (@name,@identifier,@description) RETURNING Id";
            id = 0;
            try {
                id = (long)_tenantDbDataContext.Master.ExecuteScalar(sql, serviceInfo);
            }catch(Exception ex) {
                _logger.LogError("insert serviceinfo error",ex);
                return false;
            }
            return true;
        }

        public override bool Update(ServiceInfoModel serviceInfo) {
            string sql = "Update ServiceInfo Set Name=@name,Identifier=@identifier,Description=@description Where Id=@id";
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,serviceInfo)>0;
        }

        public List<ServiceInfoModel> GetByIdentifier(List<string> identifierList) {
            string sql = "Select * From ServiceInfo Where Identifier=ANY(@identifierList)";
            return _tenantDbDataContext.Slave.QueryList<ServiceInfoModel>(sql,new { identifierList = identifierList .ToArray()});
        }

        public PagingData<ServiceInfoModel> GetPage(int pageSize,int pageIndex,string name=null,string identifier=null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) {
                p["Name"] = name;
            }
            if (!string.IsNullOrEmpty(identifier)) {
                p["Identifier"] = identifier;
            }

            return GetPage(pageSize, pageIndex, p);
        }
    }
}
