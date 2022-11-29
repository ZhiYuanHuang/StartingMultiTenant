using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using StartingMultiTenant.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Service
{
    public interface IDbServerExecutor
    {
        public DbServerModel DbServer { get;  }
        bool CreateDb(CreateDbScriptModel createDbScriptModel, string tenantIdentifier, out string uniqueDbName);
        Task DeleteDb(string dbName);
    }

    public class DbServerExecutorFactory
    {
        private readonly SysConstService _sysConstService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly EncryptService _encryptService;
        public DbServerExecutorFactory(SysConstService sysConstService,
            ILoggerFactory loggerFactory,
            EncryptService encryptService) {
            _sysConstService = sysConstService;
            _loggerFactory = loggerFactory;
            _encryptService = encryptService;
        }
        
        public IDbServerExecutor CreateDbServerExecutor(DbServerModel dbServer) {
            IDbServerExecutor executor = null;
            switch ((DbTypeEnum)dbServer.DbType) {
                case DbTypeEnum.Mysql: {
                        var logger = _loggerFactory.CreateLogger<MysqlDbServerExecutor>();
                        executor = new MysqlDbServerExecutor(dbServer, logger, _sysConstService, _encryptService);
                    }
                    break;
                case DbTypeEnum.Postgres: {
                        var logger = _loggerFactory.CreateLogger<PgsqlDbServerExecutor>();
                        executor = new PgsqlDbServerExecutor(dbServer, logger, _sysConstService, _encryptService);
                    }
                    break;
                default:
                    throw new Exception("unknow db type");
            }
            return executor;
        }
    }

    

    
}
