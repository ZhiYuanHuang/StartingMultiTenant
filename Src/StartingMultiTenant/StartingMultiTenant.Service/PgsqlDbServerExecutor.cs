using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using StartingMultiTenant.Model.Dto;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Util;

namespace StartingMultiTenant.Service
{
    public class PgsqlDbServerExecutor: IDbServerExecutor
    {
        private readonly DbServerDto _dbServer;
        private readonly string _decryptUserPwd = string.Empty;
        private readonly ILogger<PgsqlDbServerExecutor> _logger;
        public PgsqlDbServerExecutor(DbServerDto dbServer,ILogger<PgsqlDbServerExecutor> logger) {
            _dbServer = dbServer;
            _logger = logger;
        }


        public bool ExecuteCreateDb(string createDbScriptTemplate) {
            string dbConnStr = generateDbConnStr(_dbServer);
            string createDbScript= SqlScriptHelper.GenerateCreateDbScriptByTemp(createDbScriptTemplate);

            NpgsqlConnection conn = null;

            try {
                var createDb_cmd = new NpgsqlCommand();
            }catch(Exception ex) {

            } finally {
                conn?.Dispose();
            }

            return true;
        }

        private static string generateDbConnStr(DbServerDto dbServer,string database=null) {
            return $"Host={dbServer.ServerHost};Port={dbServer.ServerPort};UserName={dbServer.UserName};Password={dbServer.DecryptUserPwd}{(string.IsNullOrEmpty(database)?"":$";Database={database}")}";
            //string connString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=postgres";
            
        }
    }
}
