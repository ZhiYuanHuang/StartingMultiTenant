using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var orderDict = new Dictionary<string, bool>() { 
                { "Name", true },
                { "MajorVersion",true}
            };
            List<string> selectFields = new List<string>() {
                "Id","Name","MajorVersion","ServiceIdentifier","DbIdentifier","DbNameWildcard","DbType"
            };
            var pagingData= GetPage(pageSize, pageIndex, p, orderDict,selectFields);
            return pagingData;
        }

        public CreateDbScriptModel GetNoContent(Int64 id) {
            string sql = $"Select Id,Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,DbType From {TableName} Where Id=@id";
            return _tenantDbDataContext.Slave.Query<CreateDbScriptModel>(sql, new { id = id });
        }

        public List<CreateDbScriptModel> GetNoContent(List<Int64> ids) {
            if (ids == null || !ids.Any()) {
                return new List<CreateDbScriptModel>();
            }
            string sql = $"Select Id,Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,DbType From {TableName} Where Id=ANY(@ids)";
            return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql, new { ids = ids.ToArray() });
        }

        public List<CreateDbScriptModel> GetAllNoContent() {
            
            string sql = $"Select Id,Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,DbType From {TableName} Order By Name,MajorVersion Desc";
            return _tenantDbDataContext.Slave.QueryList<CreateDbScriptModel>(sql);
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
            string sql = @"Select Max(MajorVersion) From CreateDbScript Where Name=@name";
            object curMajorVersionObj = _tenantDbDataContext.Master.ExecuteScalar(sql,new { name=createDbScript.Name});
            int majorVersion = 1;
            if (curMajorVersionObj != null) {
                majorVersion= Convert.ToInt32(curMajorVersionObj)+1;
            }
            createDbScript.MajorVersion= majorVersion;
            sql = @"Insert into CreateDbScript (Name,MajorVersion,ServiceIdentifier,DbIdentifier,DbNameWildcard,BinaryContent,DbType)
                            Values (@name,@majorVersion,@serviceIdentifier,@dbIdentifier,@dbNameWildcard,@binaryContent,@dbType) RETURNING Id";
           
            id=(Int64) _tenantDbDataContext.Master.ExecuteScalar(sql,createDbScript);
            return true; 
        }

        public Dictionary<Int64,List<Int64>> GetTenantCreateScripts(List<Int64> tenantIds) {
            string sql = @"Select b.Id As TenantId,c.Id As CreateScriptId From TenantServiceDbConn a
                            Inner Join TenantIdentifier b Using(TenantIdentifier,TenantDomain)
                            Inner Join CreateDbScript c
                                On (a.CreateScriptName=c.Name And a.CreateScriptVersion=c.MajorVersion)
                            Where b.Id = Any(@tenantIds)";
            var dtos= _tenantDbDataContext.Slave.QueryList<TenantCreateScriptDto>(sql,new { tenantIds =tenantIds.ToArray()});
            return dtos.GroupBy(x => x.TenantId).ToDictionary(x => x.Key, v => v.Select(x => x.CreateScriptId).ToList());
        }

    }
}
