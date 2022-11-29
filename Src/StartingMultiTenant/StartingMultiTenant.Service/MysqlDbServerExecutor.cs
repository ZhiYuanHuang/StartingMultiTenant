using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;
using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public class MysqlDbServerExecutor : BaseDbServerExecutor
    {
        public MysqlDbServerExecutor(DbServerModel dbServer,
            ILogger<MysqlDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) : base(dbServer, logger, sysConstService, encryptService) { 

        }

        protected override async Task<bool> executeScript(string dbConnStr, string dbScriptStr) {
            MySqlConnection conn = new MySqlConnection(dbConnStr);

            bool result = false;
            try {

                var mysqlCommand = new MySqlCommand(dbScriptStr, conn);
                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    await mysqlCommand.ExecuteNonQueryAsync();
                    result = true;
                }
            } catch (Exception ex) {
                result = false;
                _logger.LogError($"execute script raise error,ex:{ex.Message}");
            } finally {
                conn?.Dispose();
            }

            return result;
        }

        protected override async Task<bool> executeScript(string dbConnStr, string updateSchemaScript, string rollbackScript) {
            MySqlConnection conn = new MySqlConnection(dbConnStr);

            bool result = false;
            try {

                var mysqlCommand = new MySqlCommand(updateSchemaScript, conn);
                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    try {
                        await mysqlCommand.ExecuteNonQueryAsync();
                        result = true;
                    } catch {
                        result = false;
                    }

                    if (!result) {
                        try {
                            mysqlCommand = new MySqlCommand(rollbackScript, conn);
                            await mysqlCommand.ExecuteNonQueryAsync();
                        } catch {
                        }
                    }

                }
            } catch (Exception ex) {
                result = false;
                _logger.LogError($"execute script raise error,ex:{ex.Message}");
            } finally {
                conn?.Dispose();
            }

            return result;
        }

        protected override string generateDbConnStr(DbServerModel dbServer, string database = null) {
            //Database=tenantstore.mulids;Data Source=127.0.0.1;Port=3307;User Id=root;Password=123456;Charset=utf8;
            string decryptUserPwd = _encryptService.Decrypt_DbServerPwd(dbServer.EncryptUserpwd);
            return $"Data Source={dbServer.ServerHost};Port={dbServer.ServerPort};User Id={dbServer.UserName};Password={decryptUserPwd};Charset=utf8;{(string.IsNullOrEmpty(database) ? "" : $";Database={database}")}";
        }

        protected override string resolveDatabaseName(string dbConnStr) {
            var match = Regex.Match(dbConnStr, "Database=([\\S]+?)(?=$|;)",RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
