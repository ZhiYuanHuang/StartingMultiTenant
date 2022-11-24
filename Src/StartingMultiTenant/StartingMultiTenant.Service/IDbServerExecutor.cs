using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Service
{
    public interface IDbServerExecutor
    {
    }

    public static class DbServerExecutorFactory
    {
        
        public static IDbServerExecutor CreateDbServerExecutor(DbServerDto dbServerDto) {
            IDbServerExecutor executor = null;
            switch ((DbTypeEnum)dbServerDto.DbType) {
                case DbTypeEnum.Mysql:
                    executor = new MysqlDbServerExecutor();
                    break;
                case DbTypeEnum.Postgres:
                    executor = new PgsqlDbServerExecutor(dbServerDto);
                    break;
                default:
                    throw new Exception("unknow db type");
            }
            return executor;
        }
    }
}
