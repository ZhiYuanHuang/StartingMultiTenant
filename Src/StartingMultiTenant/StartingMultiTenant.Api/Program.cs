using StartingMultiTenant.Repository;
using StartingMultiTenant.Framework;
using Microsoft.Extensions.Configuration;
using StartingMultiTenant.Service;
using StartingMultiTenant.Business;
using StartingMultiTenant.Api.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using StartingMultiTenant.Model.Const;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Api.Hubs;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api
{
    public class Program
    {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            if (System.IO.Directory.Exists("./conf.d")) {
                builder.Host.ConfigureAppConfiguration((hostingContext, config) => {
                    config.AddJsonFile(System.IO.Path.Combine(".", "conf.d", "appsettings.json"),
                        optional: true,
                        reloadOnChange: true);
                });
            }

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                    Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, Array.Empty<string>() }
                });
            });

            builder.Services.AddCors(options => {
                options.AddPolicy("default", policy => {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
                options.AddPolicy("any", builder => {
                    
                    builder.SetIsOriginAllowed(x=>true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });

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
            builder.Services.AddTransient<ApiClientRepository>();
            builder.Services.AddTransient<ApiScopeRepository>();
            builder.Services.AddTransient<ClientScopesRepository>();

            builder.Services.AddTransient<DbServerBusiness>();
            builder.Services.AddTransient<TenantDomainBusiness>();
            builder.Services.AddTransient<TenantIdentifierBusiness>();
            builder.Services.AddTransient<CreateDbScriptBusiness>();
            builder.Services.AddTransient<SchemaUpdateScriptBusiness>();
            builder.Services.AddTransient<TenantServiceDbConnBusiness>();
            builder.Services.AddTransient<ApiClientBusiness>();
            builder.Services.AddTransient<ApiScopeBusiness>();
            builder.Services.AddTransient<ServiceInfoBusiness>();
            builder.Services.AddTransient<DbInfoBusiness>();
            builder.Services.AddTransient<ExternalTenantServiceDbConnBusiness>();

            builder.Services.AddSingleton<SysConstService>();
            builder.Services.AddSingleton<EncryptService>();
            builder.Services.AddSingleton<SingleTenantService>();
            builder.Services.AddSingleton<MultiTenantService>();
            builder.Services.AddSingleton<TenantActionNoticeService>();
            builder.Services.AddSingleton<ExternalStoreSyncService>();

            builder.Services.AddSingleton<DbServerExecutorFactory>();
            builder.Services.AddSingleton<QueueNoticeFactory>();
            builder.Services.AddSingleton<ExternalStoreFactory>();

            builder.Services.AddSingleton<TokenBuilder>();
            builder.Services.Configure<JwtTokenOptions>(
                builder.Configuration.GetSection("JwtTokenOptions"));
            
            JwtTokenOptions tokenOptions =builder.Configuration.GetSection("JwtTokenOptions").Get<JwtTokenOptions>();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                  .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => {
                      options.SaveToken = true;
                      options.TokenValidationParameters = tokenOptions.ToTokenValidationParams();
                      options.Events = new JwtBearerEvents {
                          OnMessageReceived = context =>
                          {
                              var accessToken = context.Request.Query["access_token"];

                              // If the request is for our hub...
                              var path = context.HttpContext.Request.Path;
                              if (!string.IsNullOrEmpty(accessToken) &&
                                  (path.StartsWithSegments("/api/TaskNotification"))) {
                                  // Read the token out of the query string
                                  context.Token = accessToken;
                              }
                              return Task.CompletedTask;
                          }
                      };
                  });
            builder.Services.AddAuthorization(options => {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().Build();

                options.AddPolicy(AuthorizePolicyConst.Sys_Policy, builder => {
                    builder.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    builder.RequireAuthenticatedUser();
                    builder.RequireRole(RoleConst.Role_Admin);
                });

                options.AddPolicy(AuthorizePolicyConst.User_Read_Policy, builder => {
                    builder.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    builder.RequireAuthenticatedUser();
                    builder.RequireClaim("scope",ScopeNameConst.ReadScope);
                });
                options.AddPolicy(AuthorizePolicyConst.User_Write_Policy, builder => {
                    builder.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    builder.RequireAuthenticatedUser();
                    builder.RequireClaim("scope", ScopeNameConst.WriteScope);
                });
            });

            builder.Services.AddSignalR(options => {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.EnableDetailedErrors = true;
            }).AddJsonProtocol(options => {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("default");
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

           
            app.MapHub<TaskNotificationHub>("/api/TaskNotification").RequireCors("any");

            app.Run();
        }
    }
}