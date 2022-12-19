using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Abstractions;
using StartingMultiTenant.Business;
using StartingMultiTenant.Framework;
using StartingMultiTenant.Model.Enum;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Model.Domain;
using System.Text;
using StartingMultiTenant.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace StartingMultiTenant.Test
{
    internal class Program
    {
        static void Main(string[] args) {

             IServiceCollection serviceCollection = new ServiceCollection();
             serviceCollection.AddLogging(builder => builder
                 .AddConsole()
                 .AddFilter(level => level >= LogLevel.Information)
             );
             var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            bool isProd = false;
            if (args.Length > 0) {
                isProd = true;
            }

            if (isProd) {
                RabbitQueueNotice rabbitQueueNotice = new RabbitQueueNotice();
                rabbitQueueNotice.Init("127.0.0.1;5673");
                Random random = new Random();
                while (true) {
                    char char1 = (char)random.Next(65,90);
                    char char2 = (char)random.Next(65, 90);
                    string identifier = string.Concat(char1,char2);
                    var model = new Model.Dto.TenantActionInfoDto("ss.com", identifier, (int)TenantActionTypeEnum.StartCreate);
                    rabbitQueueNotice.NoticeTenantAction(model).GetAwaiter().GetResult();

                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} send:{Newtonsoft.Json.JsonConvert.SerializeObject(model)}");

                    Thread.Sleep(1000*(new Random().Next(5,10)));
                }
            }
            else {
                string queueName = "get_tenant_action_queue";
                var factory = new ConnectionFactory();
                factory.HostName = "127.0.0.1";
                factory.Port = 5673;
                //factory.UserName="root";
                //factory.Password="password";
                var connection= factory.CreateConnection();

                var channel= connection.CreateModel();
                channel.ExchangeDeclare("tenant_action_exchange",ExchangeType.Fanout,true, false);
                channel.QueueDeclare(queueName,true,false,true);
                channel.QueueBind(queueName, "tenant_action_exchange","");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (messageModel, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 订阅到消息:{message}");
                };

                channel.BasicConsume(queueName,true,consumer);
            }

            Console.ReadLine();

           // string connString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=startingmultitenant";

           // var logger= new NullLoggerFactory().CreateLogger("dd");


           // IServiceCollection serviceCollection = new ServiceCollection();
           // serviceCollection.AddLogging(builder => builder
           //     .AddConsole()
           //     .AddFilter(level => level >= LogLevel.Information)
           // );
           // var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

           // TenantDbDataContext tenantDbDataContext = new TenantDbDataContext() { 
           //     Master=new PostgresqlDb(loggerFactory.CreateLogger<PostgresqlDb>(), connString),
           //     Slave = new PostgresqlDb(loggerFactory.CreateLogger<PostgresqlDb>(), connString),
           // };

           // DbServerRepository dbServerRepository = new DbServerRepository(tenantDbDataContext);
           // DbServerBusiness dbServerBusiness = new DbServerBusiness(dbServerRepository,null, loggerFactory.CreateLogger<DbServerBusiness>());

           //var dbServerList= dbServerBusiness.GetDbServers().GetAwaiter().GetResult();
           // bool result= dbServerBusiness.Insert(new Model.Domain.DbServerModel() { 
           //     DbType=(int)DbTypeEnum.Postgres,
           //     ServerHost="192.168.1.14",
           //     ServerPort=5443,
           //     UserName="devuser1",
           //     EncryptUserpwd="23232",
           //     CanCreateNew=false
           // });
           // byte[] byteArr = null;
           // using (var fs=new System.IO.FileStream(@".\testtest.txt", FileMode.Open)) {
           //     long length = fs.Length;
           //     byteArr=new byte[length];
           //     fs.Read(byteArr,0,(int)length);
           // }

           //     CreateDbScriptRepository createDbScriptRepository = new CreateDbScriptRepository(tenantDbDataContext);
           // createDbScriptRepository.Insert(new CreateDbScriptModel() { 
           //     Name="sdfs",
           //     DbType=(int)DbTypeEnum.Postgres,
           //     BinaryContent= byteArr,
           // });

           // CreateDbScriptBusiness createDbScriptBusiness = new CreateDbScriptBusiness(createDbScriptRepository);
           // byte[] getBytes= createDbScriptBusiness.GetScriptContent(1);
           // string getStrs=Encoding.UTF8.GetString(getBytes);
           // StringBuilder builer = new StringBuilder(getStrs);
           // builer.AppendLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mmss")}");
           // getStrs = builer.ToString();
           // getBytes= Encoding.UTF8.GetBytes(getStrs);
           // using (var fs = new System.IO.FileStream(@$".\testtest{DateTime.Now.ToString("yyyy-MM-ddHHmmss")}.txt", FileMode.CreateNew)) {
           //     fs.Write(getBytes,0,getBytes.Length);
                
           // }

           // TenantServiceDbConnRepository tenantServiceDbConnRepository = new TenantServiceDbConnRepository(tenantDbDataContext);
           
           // var createDbSet = new List<TenantServiceDbConnModel>();
           // for (int i = 0; i < 10; i++) {
           //     createDbSet.Add(new TenantServiceDbConnModel() {
           //         TenantDomain = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
           //         TenantIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
           //         ServiceIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
           //         DbIdentifier = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
           //         CreateScriptName = new Random().Next(1, 10) > 5 ? $"aa" : "bb",
           //         CreateScriptVersion = 0,
           //         CurSchemaVersion = 0,
           //         DbServerId = new Random().Next(1, 10) > 5 ? 1 : 2,
           //         EncryptedConnStr = "adfsdfsd",
           //     });
           // }
           
        }

        private static void Consumer_Received(object? sender, BasicDeliverEventArgs e) {
            throw new NotImplementedException();
        }
    }
}