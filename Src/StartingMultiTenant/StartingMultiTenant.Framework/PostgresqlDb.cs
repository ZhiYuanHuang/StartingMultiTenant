using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace StartingMultiTenant.Framework
{
    public class PostgresqlDb : IDbFunc
    {
        private readonly string _connectionString = null;
        public string ConnectionString { get { return _connectionString; } }
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DbName { get; }

        private ILogger<PostgresqlDb> _logger;

        public PostgresqlDb(ILogger<PostgresqlDb> logger, string connectionString) {

            _logger = logger;

            _connectionString = connectionString;
            {
                // get db_name from connection_string
                foreach (string s in _connectionString.Split(';')) {
                    if (s.Trim().ToLower().StartsWith("database")) {
                        DbName = s.Split('=')[1].Trim();
                        break;
                    }
                }
            }
        }


        public void BeginTransaction() {
            throw new NotImplementedException();
        }

        public void CommitTransaction() {
            throw new NotImplementedException();
        }

        public DataTable ExecuteDataTable(string sql) {
            throw new NotImplementedException();
        }

        public DataTable ExecuteDataTable(string sql, Dictionary<string, object> p) {
            throw new NotImplementedException();
        }

        public DataTable ExecuteDataTable(string sql, int pageIndex, int pageSize) {
            throw new NotImplementedException();
        }

        public DataTable ExecuteDataTable(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string sql) {
            throw new NotImplementedException();
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> p) {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string sql) {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(string sql, Dictionary<string, object> p) {
            throw new NotImplementedException();
        }

        public T Query<T>(string sql) where T : new() {
            throw new NotImplementedException();
        }

        public T Query<T>(string sql, Dictionary<string, object> p) where T : new() {
            throw new NotImplementedException();
        }

        public T Query<T>(string sql, int pageIndex, int pageSize) where T : new() {
            throw new NotImplementedException();
        }

        public T Query<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new() {
            throw new NotImplementedException();
        }

        public List<T> QueryList<T>(string sql) where T : new() {
            throw new NotImplementedException();
        }

        public List<T> QueryList<T>(string sql, Dictionary<string, object> p) where T : new() {
            throw new NotImplementedException();
        }

        public List<T> QueryList<T>(string sql, int pageIndex, int pageSize) where T : new() {
            throw new NotImplementedException();
        }

        public List<T> QueryList<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new() {
            throw new NotImplementedException();
        }

        public PagingData<T> QueryPaging<T>(string sql, int pageIndex, int pageSize) where T : new() {
            throw new NotImplementedException();
        }

        public PagingData<T> QueryPaging<T>(string sql, int pageIndex, int pageSize, Dictionary<string, object> p) where T : new() {
            throw new NotImplementedException();
        }

        public void RollbackTransaction() {
            throw new NotImplementedException();
        }
    }
}
