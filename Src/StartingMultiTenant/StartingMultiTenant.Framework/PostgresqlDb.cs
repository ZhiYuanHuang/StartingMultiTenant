using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Dto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using System.Collections;
using Npgsql;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.IO;
using StartingMultiTenant.Util;
using MySqlConnector;
using System.Linq;

namespace StartingMultiTenant.Framework
{
    public class PostgresqlDb : IDbFunc
    {
        public string DbHost { get; private set; }
        public string DbPort { get; private set; }
        public string DbName { get; private set; }

        private readonly string _connectionString = null;
        public string ConnectionString { get { return _connectionString; } }

        private readonly ILogger<PostgresqlDb> _logger;

        private ConcurrentQueue<NpgsqlConnection> _queue = new ConcurrentQueue<NpgsqlConnection>();
        private const int ALARM_THRESHOLD_VALUE = 1000000;
        private const int COMMAND_TIMEOUT = 300;
        private const string POSTGRESQL_CONNECTION = "PostgresqlConnection";
        private const string POSTGRESQL_TRANSACTION = "PostgresqlTransaction";
        private const string POSTGRESSQL_TRANSACTION_COUNTER = "PostgresqlTransactionCounter";

        public PostgresqlDb(ILogger<PostgresqlDb> logger, string connectionString) {

            _logger = logger;

            _connectionString = connectionString;
            DbHost = resolveHost(_connectionString);
            DbPort= resolvePort(_connectionString);
            DbName = resolveDatabaseName(_connectionString);
        }


        public void BeginTransaction() {
            CallContext.SetData(POSTGRESSQL_TRANSACTION_COUNTER, ConvertUtil.ToInt(CallContext.GetData(POSTGRESSQL_TRANSACTION_COUNTER), 0) + 1);
            var connection = CallContext.GetData(POSTGRESQL_CONNECTION) as NpgsqlConnection;

            if (connection == null) {
                connection = GetConnection();
                NpgsqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

                CallContext.SetData(POSTGRESQL_CONNECTION, connection);
                CallContext.SetData(POSTGRESQL_TRANSACTION, transaction);
            }
        }

        public void CommitTransaction() {
            CallContext.SetData(POSTGRESSQL_TRANSACTION_COUNTER, ConvertUtil.ToInt(CallContext.GetData(POSTGRESSQL_TRANSACTION_COUNTER), 0) - 1);

            int counter = ConvertUtil.ToInt(CallContext.GetData(POSTGRESSQL_TRANSACTION_COUNTER), 0);
            if (counter > 0) {
                return;
            }

            NpgsqlTransaction transaction = CallContext.GetData(POSTGRESQL_TRANSACTION) as NpgsqlTransaction;
            if (transaction != null) {
                transaction.Commit();
                transaction.Dispose();

                NpgsqlConnection connection = CallContext.GetData(POSTGRESQL_CONNECTION) as NpgsqlConnection;
                FreeConnection(connection);

                CallContext.SetData(POSTGRESQL_TRANSACTION, null);
                CallContext.SetData(POSTGRESQL_CONNECTION, null);
            }
        }

        public void RollbackTransaction() {
            CallContext.SetData(POSTGRESSQL_TRANSACTION_COUNTER, null);

            NpgsqlTransaction transaction = CallContext.GetData(POSTGRESQL_TRANSACTION) as NpgsqlTransaction;
            if (transaction != null) {
                transaction.Rollback();
                transaction.Dispose();

                NpgsqlConnection connection = CallContext.GetData(POSTGRESQL_CONNECTION) as NpgsqlConnection;
                FreeConnection(connection);

                CallContext.SetData(POSTGRESQL_TRANSACTION, null);
                CallContext.SetData(POSTGRESQL_CONNECTION, null);
            }
        }

        public int ExecuteNonQuery(string sql, object p = null) {
            int ret = 0;

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            try {
                ret = connection.Execute(sql, p, transaction, COMMAND_TIMEOUT, CommandType.Text);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return ret;
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> p=null) {
            int ret = 0;

            GetContextConnectionAndTrans(out NpgsqlConnection connection,out NpgsqlTransaction transaction);

            DynamicParameters dynamicParameters = generateParam(p);

            try {
                ret = connection.Execute(sql, dynamicParameters, transaction, COMMAND_TIMEOUT, CommandType.Text);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql+"|"+$"{(p!=null?Newtonsoft.Json.JsonConvert.SerializeObject(p):"")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return ret;
        }

        public object ExecuteScalar(string sql, object p = null) {
            object ret = null;

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            try {
                ret = connection.ExecuteScalar(sql, p, transaction, COMMAND_TIMEOUT, CommandType.Text);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return ret;
        }

        public object ExecuteScalar(string sql, Dictionary<string, object> p=null) {
            object ret = null;

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            DynamicParameters dynamicParameters = generateParam(p);

            try {
                ret= connection.ExecuteScalar(sql,dynamicParameters,transaction,COMMAND_TIMEOUT,CommandType.Text);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return ret;
        }

        public T Query<T>(string sql, object p) where T : new() {
            T t = default(T);

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            try {
                t = connection.QueryFirstOrDefault<T>(sql, p, transaction, COMMAND_TIMEOUT, CommandType.Text);
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return t;
        }

        public T Query<T>(string sql, Dictionary<string, object> p) where T : new() {
            T t = default(T);

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            DynamicParameters dynamicParameters = generateParam(p);

            try {
                t= connection.QueryFirstOrDefault<T>(sql,dynamicParameters,transaction,COMMAND_TIMEOUT,CommandType.Text);
            }catch(Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return t;
        }

        public List<T> QueryList<T>(string sql, object p=null) where T : new() {
            List<T> resultList = new List<T>();

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            try {
                var resultSet = connection.Query<T>(sql, p, transaction, false, COMMAND_TIMEOUT, CommandType.Text);
                if (resultSet != null) {
                    resultList = resultSet.ToList();
                }
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return resultList;
        }

        public List<T> QueryList<T>(string sql, Dictionary<string, object> p) where T : new() {
            List<T> resultList = new List<T>();

            GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction);

            DynamicParameters dynamicParameters = generateParam(p);

            try {
                var resultSet = connection.Query<T>(sql, dynamicParameters, transaction,false, COMMAND_TIMEOUT, CommandType.Text);
                if(resultSet != null ) {
                    resultList= resultSet.ToList();
                }
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + sql + "|" + $"{(p != null ? Newtonsoft.Json.JsonConvert.SerializeObject(p) : "")}");
                throw new Exception(ex.Message, ex);
            } finally {
                if (transaction == null) {
                    FreeConnection(connection);
                }
            }

            return resultList;
        }

        private void GetContextConnectionAndTrans(out NpgsqlConnection connection, out NpgsqlTransaction transaction) {
            transaction = null;
            connection = CallContext.GetData(POSTGRESQL_CONNECTION) as NpgsqlConnection;
            if (connection == null) {
                connection = GetConnection();
            } else {
                transaction = CallContext.GetData(POSTGRESQL_TRANSACTION) as NpgsqlTransaction;
            }
        }

        private DynamicParameters generateParam(Dictionary<string, object> p = null) {
            DynamicParameters dynamicParameters = null;
            if (p != null && p.Any()) {
                dynamicParameters = new DynamicParameters();
                foreach (var pair in p) {
                    dynamicParameters.Add(pair.Key, pair.Value);
                }
            }
            return dynamicParameters;
        }



        private NpgsqlConnection GetConnection() {
            long startTicks = DateTime.Now.Ticks;

            bool ok = _queue.TryDequeue(out NpgsqlConnection connection);
            if (!ok) {
                connection = new NpgsqlConnection(_connectionString);
            }

            try {
                connection.Open();
            } catch (Exception ex) {
                _logger.LogError(ex.ToString() + "|" + _connectionString);
                throw new Exception(ex.Message, ex);
            }

            long elapsedTicks = DateTime.Now.Ticks - startTicks;
            if (elapsedTicks > ALARM_THRESHOLD_VALUE) {
                _logger.LogWarning($"打开连接超过预定伐值:{ALARM_THRESHOLD_VALUE / 10000}(毫秒), Queue Count:{_queue.Count}, ConnectionString:{DbHost}:{DbPort}-{DbName}, Mileseconds:{elapsedTicks / 10000}");
            }

            return connection;
        }

        private void FreeConnection(NpgsqlConnection connection) {
            connection.Close();

            if (_queue.Count < 1000) {
                _queue.Enqueue(connection);
            } else {
                connection = null;
            }
        }

        private string resolveDatabaseName(string dbConnStr) {
            var match = Regex.Match(dbConnStr, "Database=([\\S]+?)(?=$|;)", RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        private string resolveHost(string dbConnStr) {
            var match = Regex.Match(dbConnStr, "Host=([\\S]+?)(?=$|;)", RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

        private string resolvePort(string dbConnStr) {
            var match = Regex.Match(dbConnStr, "Port=([\\S]+?)(?=$|;)", RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }

    }
}
