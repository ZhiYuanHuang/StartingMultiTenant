using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class CreateDbScriptRepository : BaseRepository<CreateDbScriptModel>
    {
        public override string TableName => "CreateDbScript";
        public CreateDbScriptRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {
        }

        public PagingData<CreateDbScriptModel> GetPageNoContent(int pageSize,int pageIndex,string name=null) {
            Dictionary<string, object> p = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) {
                p["Name"] = name;
            }

            var pagingData= GetPage(pageSize, pageIndex, p);
            pagingData.Data.ForEach(x=>x.BinaryContent=null);
            return pagingData;
        }

        public List<CreateDbScriptModel> GetListByNames(List<string> nameList) {
            string sql = @"Select * From CreateDbScript Where Name = ANY( @names)";
            //return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql, new { 
            //    names=nameList
            //});
            return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql, new Dictionary<string, object>() {
                { "names",nameList.ToArray()}
            });
        }

        public CreateDbScriptModel GetByNameByVersion(string createScriptName,int majorVersion) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "Name",createScriptName},
                { "MajorVersion",majorVersion}
            };

            return GetEntityByQuery(p);
        }

        public override bool Insert(CreateDbScriptModel createDbScript,out Int64 id) {
            string sql = @"Insert into CreateDbScript (Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,BinaryContent,DbType)
                            Values (@name,@majorVersion,@serviceIdentifier,@dbIdentifier,@dbNameWildcard,@binaryContent,@dbType) RETURNING Id";
           
            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql,createDbScript);
            return true; 
        }

         
    }
}
