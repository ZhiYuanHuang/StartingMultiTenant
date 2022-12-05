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
        int ExecuteNonQuery(string sql);
        int ExecuteNonQuery(string sql, Dictionary<string, object> p);
        object ExecuteScalar(string sql);
        object ExecuteScalar(string sql, Dictionary<string, object> p);
        DataTable ExecuteDataTable(string sql);
        DataTable ExecuteDataTable(string sql, Dictionary<string, object> p);
        DataTable ExecuteDataTable(string sql, int pageIndex, int pageSize);
        DataTable ExecuteDataTable(string sql, int pageIndex, int pageSize, Dictionary<string, object> p);
        T Query<T>(string sql) where T : new();
        T Query<T>(string sql, Dictionary<string, object> p) where T : new();
        T Query<T>(string sql, int pageIndex, int pageSize) where T : new();
        T Query<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new();
        List<T> QueryList<T>(string sql) where T : new();
        List<T> QueryList<T>(string sql, Dictionary<string, object> p) where T : new();
        List<T> QueryList<T>(string sql, int pageIndex, int pageSize) where T : new();
        List<T> QueryList<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new();
        PagingData<T> QueryPaging<T>(string sql, int pageIndex, int pageSize) where T : new();
        PagingData<T> QueryPaging<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new();
    }
}
