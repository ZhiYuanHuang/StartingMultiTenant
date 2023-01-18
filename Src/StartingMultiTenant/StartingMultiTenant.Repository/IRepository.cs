using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Repository
{
    public interface IRepository<T> where T:new()
    {
        List<T> GetEntitiesByQuery(Dictionary<string, object> fieldValueDict = null);
        T GetEntityByQuery(Dictionary<string, object> fieldValueDict = null);
        T GetEntityById(Int64 Id);
        List<T> GetEntitiesByIds(List<Int64> ids);
        PagingData<T> GetPage(int pageSize, int pageIndex);
        bool Insert(T t, out Int64 id);
        bool Update(T t);
        bool Delete(Int64 id);
        void BeginTransaction();
        //int ExecuteNonQuery(string sql, Dictionary<string, object> p);

        //object ExecuteScalar(string sql, Dictionary<string, object> p);
        void CommitTransaction();
        void RollbackTransaction();
    }
}
