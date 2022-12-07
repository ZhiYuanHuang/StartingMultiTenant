using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace StartingMultiTenant.Framework
{
    public interface IDbFunc
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        int ExecuteNonQuery(string sql, object p);
        int ExecuteNonQuery(string sql, Dictionary<string, object> p);
        object ExecuteScalar(string sql, object p);
        object ExecuteScalar(string sql, Dictionary<string, object> p);
        T Query<T>(string sql, Dictionary<string, object> p) where T : new();
        List<T> QueryList<T>(string sql, Dictionary<string, object> p) where T : new();
        T Query<T>(string sql, object p) where T : new();
        List<T> QueryList<T>(string sql, object p) where T : new();
    }
}
