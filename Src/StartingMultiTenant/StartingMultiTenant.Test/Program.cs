using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using StartingMultiTenant.Business;
using StartingMultiTenant.Framework;
using StartingMultiTenant.Model.Enum;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Model.Domain;

namespace StartingMultiTenant.Test
{
    internal class Program
    {
        static void Main(string[] args) {
            string connString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=startingmultitenant";

            var logger= new NullLoggerFactory().CreateLogger("dd");


            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            TenantDbDataContext tenantDbDataContext = new TenantDbDataContext() { 
                Master=new PostgresqlDb(loggerFactory.CreateLogger<PostgresqlDb>(), connString),
                Slave = new PostgresqlDb(loggerFactory.CreateLogger<PostgresqlDb>(), connString),
            };

            DbServerRepository dbServerRepository = new DbServerRepository(tenantDbDataContext);
            DbServerBusiness dbServerBusiness = new DbServerBusiness(dbServerRepository,null, loggerFactory.CreateLogger<DbServerBusiness>());

           var dbServerList= dbServerBusiness.GetDbServers().GetAwaiter().GetResult();
            bool result= dbServerBusiness.Insert(new Model.Domain.DbServerModel() { 
                DbType=(int)DbTypeEnum.Postgres,
                ServerHost="192.168.1.14",
                ServerPort=5443,
                UserName="devuser1",
                EncryptUserpwd="23232",
                CanCreateNew=false
            });

            TenantServiceDbConnRepository tenantServiceDbConnRepository = new TenantServiceDbConnRepository(tenantDbDataContext);
            TenantServiceDbConnBusiness tenantServiceDbConnBusiness = new TenantServiceDbConnBusiness(tenantServiceDbConnRepository);

            var createDbSet = new List<TenantServiceDbConnModel>();
            for (int i = 0; i < 10; i++) {
                createDbSet.Add(new TenantServiceDbConnModel() {
                    TenantDomain = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
                    TenantIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
                    ServiceIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
                    DbIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
                    CreateScriptName = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
                    CreateScriptVersion = 0,
                    CurSchemaVersion = 0,
                    DbServerId = new Random().Next(1, 10) > 5 ? 1 : 2,
                    EncryptedConnStr = "adfsdfsd",
                });
            }
            tenantServiceDbConnBusiness.BatchInsertDbConns(createDbSet);
        }
    }
}