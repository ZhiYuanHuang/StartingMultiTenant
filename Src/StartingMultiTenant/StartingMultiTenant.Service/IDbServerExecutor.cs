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
    }

    public class DbServerExecutorFactory
    {
        private readonly SysConstService _sysConstService;
        private readonly ILoggerFactory _loggerFactory;
        public DbServerExecutorFactory(SysConstService sysConstService,
            ILoggerFactory loggerFactory) {
            _sysConstService = sysConstService;
            _loggerFactory = loggerFactory;
        }
        
        public IDbServerExecutor CreateDbServerExecutor(DbServerDto dbServerDto) {
            IDbServerExecutor executor = null;
            switch ((DbTypeEnum)dbServerDto.DbType) {
                case DbTypeEnum.Mysql:
                    executor = new MysqlDbServerExecutor();
                    break;
                case DbTypeEnum.Postgres:
                    var logger = _loggerFactory.CreateLogger<PgsqlDbServerExecutor>();
                    executor = new PgsqlDbServerExecutor(dbServerDto, logger,_sysConstService);
                    break;
                default:
                    throw new Exception("unknow db type");
            }
            return executor;
        }
    }

    

    
}
