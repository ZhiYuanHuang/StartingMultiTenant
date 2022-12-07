using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Repository
{
    public interface IRepository<T>
    {
        List<T> GetEntitiesByQuery(Dictionary<string, object> fieldValueDict = null);
        T GetEntityByQuery(Dictionary<string, object> fieldValueDict = null);

        void BeginTransaction();
        //int ExecuteNonQuery(string sql, Dictionary<string, object> p);

        //object ExecuteScalar(string sql, Dictionary<string, object> p);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
