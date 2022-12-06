using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Framework
{
    public static class DbDataContextExtension
    {
        public static IServiceCollection AddDbDataContext<T>(this IServiceCollection services,Action<DbDataContextOption> dbDataContextOptionAction) where T:DbDataContext,new(){
            return services.AddSingleton<T>((provider) => {
                DbDataContextOption dbDataContextOption = new DbDataContextOption();
                dbDataContextOptionAction(dbDataContextOption);

                if(string.IsNullOrEmpty(dbDataContextOption.SlaveConnStr)) {
                    dbDataContextOption.SlaveConnStr = dbDataContextOption.MasterConnStr;
                }

                IDbFunc masterDb = null;
                IDbFunc slaveDb = null;
                string paramSymbol = string.Empty;
                switch (dbDataContextOption.DbType) {
                    case DbTypeEnum.Postgres: {
                            var logger = provider.GetRequiredService<ILogger<PostgresqlDb>>();
                            masterDb = new PostgresqlDb(logger,dbDataContextOption.MasterConnStr);
                            
                            slaveDb = new PostgresqlDb(logger,dbDataContextOption.SlaveConnStr);
                            paramSymbol = "?";
                        }
                        break;
                    default:
                        break;
                }

                return new T() {
                    Master = masterDb,
                    Slave = slaveDb,
                    DbDataContextOption = dbDataContextOption
                };

            });
        }
    }
}
