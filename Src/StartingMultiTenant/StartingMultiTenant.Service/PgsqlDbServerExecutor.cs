using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using StartingMultiTenant.Model.Dto;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Util;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StartingMultiTenant.Service
{
    public class PgsqlDbServerExecutor: BaseDbServerExecutor
    {
        public PgsqlDbServerExecutor(DbServerModel dbServer,
            ILogger<PgsqlDbServerExecutor> logger,
            SysConstService sysConstService,
            EncryptService encryptService) :base(dbServer,logger,sysConstService, encryptService) {
        }

        protected override async Task<bool> executeScript(string dbConnStr,string dbScriptStr) {
            NpgsqlConnection conn = new NpgsqlConnection(dbConnStr);

            bool result = false;
            try {

                var npgsqlCommand = new NpgsqlCommand(dbScriptStr, conn);
                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    await npgsqlCommand.ExecuteNonQueryAsync();
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
            NpgsqlConnection conn = new NpgsqlConnection(dbConnStr);

            bool result = false;
            try {

                var npgsqlCommand = new NpgsqlCommand(updateSchemaScript, conn);
                await conn.OpenAsync();

                if (conn.State == System.Data.ConnectionState.Open) {
                    try {
                        await npgsqlCommand.ExecuteNonQueryAsync();
                        result = true;
                    } catch {
                        result = false;
                    }

                    if (!result) {
                        try {
                            npgsqlCommand = new NpgsqlCommand(rollbackScript,conn);
                            await npgsqlCommand.ExecuteNonQueryAsync();
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

        protected override string generateDbConnStr(DbServerModel dbServer,string database=null) {
            string decryptUserPwd = _encryptService.Decrypt_DbServerPwd(dbServer.EncryptUserpwd);
            return $"Host={dbServer.ServerHost};Port={dbServer.ServerPort};UserName={dbServer.UserName};Password={decryptUserPwd}{(string.IsNullOrEmpty(database)?"":$";Database={database};")}";
            //string connString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=postgres";
            
        }

        protected override string resolveDatabaseName(string dbConnStr) {
            var match= Regex.Match(dbConnStr, "Database=([\\S]+?)(?=$|;)", RegexOptions.IgnoreCase);
            if (match.Success) {
                return match.Groups[1].Value;
            }
            return string.Empty;
        }
    }
}
