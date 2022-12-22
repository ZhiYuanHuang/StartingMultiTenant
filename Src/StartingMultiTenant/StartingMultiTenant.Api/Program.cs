using StartingMultiTenant.Repository;
using StartingMultiTenant.Framework;
using Microsoft.Extensions.Configuration;
using StartingMultiTenant.Service;
using StartingMultiTenant.Business;

namespace StartingMultiTenant.Api
{
    public class Program
    {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbDataContext<TenantDbDataContext>(options => {
                builder.Configuration.GetSection("StartingMultiTenantDbOption").Bind(options);
            });
            
            builder.Services.AddTransient<ServiceInfoRepository>();
            builder.Services.AddTransient<DbInfoRepository>();
            builder.Services.AddTransient<DbServerRepository>();
            builder.Services.AddTransient<TenantServiceDbConnRepository>();
            builder.Services.AddTransient<TenantDomainRepository>();
            builder.Services.AddTransient<TenantIdentifierRepository>();
            builder.Services.AddTransient<ExternalTenantServiceDbConnRepository>();
            builder.Services.AddTransient<SchemaUpdateScriptRepository>();
            builder.Services.AddTransient<CreateDbScriptRepository>();
            builder.Services.AddTransient<HistoryTenantServiceDbConnRepository>();

            builder.Services.AddTransient<DbServerBusiness>();
            builder.Services.AddTransient<TenantDomainBusiness>();
            builder.Services.AddTransient<TenantIdentifierBusiness>();
            builder.Services.AddTransient<CreateDbScriptBusiness>();
            builder.Services.AddTransient<SchemaUpdateScriptBusiness>();
            builder.Services.AddTransient<TenantServiceDbConnBusiness>();

            builder.Services.AddSingleton<SysConstService>();
            builder.Services.AddSingleton<EncryptService>();
            builder.Services.AddSingleton<SingleTenantService>();

            builder.Services.AddSingleton<DbServerExecutorFactory>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}