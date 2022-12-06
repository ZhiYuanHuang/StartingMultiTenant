using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Repository
{
    public abstract class BaseRepository<T>:IRepository<T> where T : class, new ()
    {
        protected readonly TenantDbDataContext _tenantDbDataContext;
        public abstract string TableName { get; protected set; }
        public BaseRepository(TenantDbDataContext tenantDbDataContext) {
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

        public virtual int ExecuteNonQuery(string sql, Dictionary<string, object> p) {
            return _tenantDbDataContext.Master.ExecuteNonQuery(sql,p);
        }

        public virtual object ExecuteScalar(string sql, Dictionary<string, object> p) {
            return _tenantDbDataContext.Master.ExecuteScalar(sql,p);
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

            return _tenantDbDataContext.Slave.QueryList<T>(builder.ToString());
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

            return _tenantDbDataContext.Slave.Query<T>(builder.ToString());
        }

        public void CheckTableNameNotNull() {
            if (string.IsNullOrEmpty(TableName)) {
                throw new Exception("tableName is empty,cann't auto generate sql");
            }
        }

    }
}
