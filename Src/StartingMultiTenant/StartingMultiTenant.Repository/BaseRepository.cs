using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Repository
{
    public abstract class BaseRepository<T>:IRepository<T> where T : class, new ()
    {
        protected readonly TenantDbDataContext _tenantDbDataContext;
        public abstract string TableName { get; }
        public BaseRepository(TenantDbDataContext tenantDbDataContext
            ) {
            _tenantDbDataContext = tenantDbDataContext;
        }

        public virtual void BeginTransaction() {
            _tenantDbDataContext.Master.BeginTransaction();
        }

        public virtual void CommitTransaction() {
            _tenantDbDataContext.Master.CommitTransaction();
        }

        public virtual void RollbackTransaction() {
            _tenantDbDataContext.Master.RollbackTransaction();
        }

        public List<T> GetEntitiesByQuery(Dictionary<string, object> fieldValueDict = null) {
            CheckTableNameNotNull();

            StringBuilder builder = new StringBuilder($"select * from {TableName} ");

            if (fieldValueDict != null && fieldValueDict.Count > 0) {

                int i = 0;
                builder.Append(" where ");

                foreach (KeyValuePair<string, object> kvp in fieldValueDict) {
                    builder.Append(string.Format($" {kvp.Key.ToUpper()}=@{kvp.Key} "));
                    if (++i < fieldValueDict.Count) {
                        builder.Append(" And ");
                    }
                }

                return _tenantDbDataContext.Slave.QueryList<T>(builder.ToString(), fieldValueDict);
            }

            return _tenantDbDataContext.Slave.QueryList<T>(builder.ToString(),null);
        }

        public T GetEntityByQuery(Dictionary<string, object> fieldValueDict = null) {
            CheckTableNameNotNull();

            StringBuilder builder = new StringBuilder($"select * from {TableName} ");

            if (fieldValueDict != null && fieldValueDict.Count > 0) {

                int i = 0;
                builder.Append(" where ");
                foreach (KeyValuePair<string, object> kvp in fieldValueDict) {
                    builder.Append(string.Format($" {kvp.Key.ToUpper()}=@{kvp.Key} "));
                    if (++i < fieldValueDict.Count) {
                        builder.Append(" And ");
                    }
                }

                return _tenantDbDataContext.Slave.Query<T>(builder.ToString(), fieldValueDict);
            }

            return _tenantDbDataContext.Slave.Query<T>(builder.ToString(),null);
        }

        public T GetEntityById(Int64 Id) {
            string sql = $"Select * From {TableName} Where Id=@id";
            return _tenantDbDataContext.Slave.Query<T>(sql,new { id=Id});
        }

        public List<T> GetEntitiesByIds(List<Int64> ids) {
            if(ids==null || !ids.Any()) {
                return new List<T>();
            }
            string sql = $"Select * From {TableName} Where Id=ANY(@ids)";
            return _tenantDbDataContext.Slave.QueryList<T>(sql, new { ids = ids.ToArray() });
        }

        public bool Delete(Int64 id) {
            string sql = $"Delete From {TableName} Where Id=@id";
    
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql, new { id = id })>0;
        }

        public PagingData<T> GetPage(int pageSize,int pageIndex) {
            StringBuilder countBuilder = new StringBuilder($"Select Count(*) From {TableName} ");
            StringBuilder dataBuilder = new StringBuilder($"Select * From {TableName} ");

            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "pageSize",pageSize},
                { "offSet",pageSize*pageIndex}
            };

            dataBuilder.Append(" Limit @pageSize OFFSET @offSet");

            int count = (int)((long)_tenantDbDataContext.Slave.ExecuteScalar(countBuilder.ToString(), p));
            if (count == 0) {
                return new PagingData<T>(pageIndex, pageSize, 0, new List<T>());
            }

            var list = _tenantDbDataContext.Slave.QueryList<T>(dataBuilder.ToString(), p);
            return new PagingData<T>(pageIndex, pageSize, count, list);
        }

        public void CheckTableNameNotNull() {
            if (string.IsNullOrEmpty(TableName)) {
                throw new Exception("tableName is empty,cann't auto generate sql");
            }
        }

    }
}
