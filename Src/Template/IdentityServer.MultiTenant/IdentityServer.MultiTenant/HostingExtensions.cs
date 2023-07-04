using IdentityServer.MultiTenant.DbContext;
using IdentityServer.MultiTenant.Filter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Serilog;
using StartingMultiTenantLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityServer.MultiTenant.Service;
using IdentityServer4.Services;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using Finbuckle.MultiTenant;
using IdentityServer.MultiTenant.Middleware;
using StartingMultiTenantLib.Const;
using Microsoft.AspNetCore.HttpOverrides;
using IdentityServer4.Extensions;
using Microsoft.Extensions.Logging;

namespace IdentityServer.MultiTenant;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        //builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews(options => {
            options.Filters.Add<GlobalExceptionFilter>();
        });

        //add cache
        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddStartingMultiTenantClient((provider, optionBuilder) => {

            string redisConnStr= builder.Configuration.GetValue<string>("MultiTenantOption:RedisConn", string.Empty);
            string kubeConfigFile = builder.Configuration.GetValue<string>("MultiTenantOption:KubeConfigFile", string.Empty);

            string requestUrl = builder.Configuration.GetValue<string>("MultiTenantOption:RequestUrl", string.Empty);
            if (!string.IsNullOrEmpty(requestUrl)) {
                optionBuilder.UseRequest(requestUrl,
                    builder.Configuration.GetValue<string>("MultiTenantOption:ClientId", string.Empty),
                    builder.Configuration.GetValue<string>("MultiTenantOption:ClientSecret", string.Empty));
            }

            //redis、k8s just use one 

            if (!string.IsNullOrEmpty(redisConnStr)) {
                optionBuilder.UseRedis(redisConnStr);

                //don't use k8s again
                return;
            }

            if (!string.IsNullOrEmpty(kubeConfigFile)) {
                optionBuilder.UseK8sSecret(kubeConfigFile);
            }
        });

        string serviceIdentifier = builder.Configuration.GetValue("MultiTenantOption:SpecifiedServiceIdentifier", string.Empty);

        //add multi tenant resovle middleware
        builder.Services.WithMultiTenant(options => {
            options.CacheMilliSec = 1000 * 60 * 3;
            options.ServiceIdentifier = serviceIdentifier;
            //不存在租户使用空数据源
            options.UseEmptySourceWhenNoExistTenant = true;
        }, ServiceLifetime.Scoped)
            .WithPerTenantOptions<JwtBearerOptions>((o, tenantInfo) => {
                var schema= builder.Configuration.GetValue<string>("MultiTenantOption:JwtBearerSchema","http");
                string jwtBearerUrl = $"{schema}://{tenantInfo.Identifier}.{tenantInfo.TenantDomain}";
                o.Authority = jwtBearerUrl;
                o.Audience = jwtBearerUrl + "/resources";

                o.RequireHttpsMetadata = false;
            });

        string specifiedDbIdentifier = builder.Configuration.GetValue("MultiTenantOption:SpecifiedDbIdentifier", string.Empty);
        bool useInnerConn = builder.Configuration.GetValue("MultiTenantOption:UseInnerConn", true);
        builder.Services.AddDbContext<AspNetAccountDbContext>((provider, options) => {
            var contextTenant= provider.GetRequiredService<ContextTenantInfo>();
            if (contextTenant != null && contextTenant.CurrentTenantInfo!=null) {
                var tenantDbConnsDto = contextTenant.CurrentTenantInfo;
                var dbconnList = useInnerConn ? tenantDbConnsDto.InnerDbConnList : tenantDbConnsDto.ExternalDbConnList;
                if (dbconnList != null && dbconnList.Any()) {
                    if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                        var foundDbConnDto = dbconnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                        if (foundDbConnDto != null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                            options.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                        }
                    } else {
                        options.UseMySql(dbconnList[0].DbConn, MySqlServerVersion.AutoDetect(dbconnList[0].DbConn));
                    }
                }
            }
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AspNetAccountDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddIdentityServer(options => {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;
           
            // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
            options.EmitStaticAudienceClaim = true;
            options.InputLengthRestrictions.Scope = 2000;
            options.Authentication.CookieSameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        }).AddDeveloperSigningCredential()
            .AddConfigurationStore(options => {
                options.ResolveDbContextOptions = (provider, builder) => {
                    var contextTenant = provider.GetRequiredService<ContextTenantInfo>();
                    if (contextTenant != null && contextTenant.CurrentTenantInfo != null) {
                        var tenantDbConnsDto = contextTenant.CurrentTenantInfo;
                        var dbconnList = useInnerConn ? tenantDbConnsDto.InnerDbConnList : tenantDbConnsDto.ExternalDbConnList;
                        if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                           
                            var foundDbConnDto = dbconnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                            if (foundDbConnDto != null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                                builder.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                            }
                        } else {
                            builder.UseMySql(dbconnList[0].DbConn, MySqlServerVersion.AutoDetect(dbconnList[0].DbConn));
                        }
                    }
                };
            })
            .AddOperationalStore(options => {
                options.ResolveDbContextOptions = (provider, builder) => {
                    var contextTenant = provider.GetRequiredService<ContextTenantInfo>();
                    if (contextTenant != null && contextTenant.CurrentTenantInfo != null) {
                        var tenantDbConnsDto = contextTenant.CurrentTenantInfo;
                        var dbconnList = useInnerConn ? tenantDbConnsDto.InnerDbConnList : tenantDbConnsDto.ExternalDbConnList;
                        if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                            var foundDbConnDto = dbconnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                            if (foundDbConnDto != null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                                builder.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                            }
                        } else {
                            builder.UseMySql(dbconnList[0].DbConn, MySqlServerVersion.AutoDetect(dbconnList[0].DbConn));
                        }
                    }
                };
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>();
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryClients(Config.Clients);

        builder.Services.AddSingleton<ICorsPolicyService>((container) => {
            var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
            return new DefaultCorsPolicyService(logger) {
                AllowAll = true,
            };
        });

        IdentityModelEventSource.ShowPII = true;

        builder.Services.AddAuthorization(options => {
            options.DefaultPolicy = new AuthorizationPolicyBuilder("Bearer")
                .RequireAuthenticatedUser().Build();

            options.AddPolicy(SMTConsts.AuthorPolicy_SuperAdmin, builder => {
                builder.AddAuthenticationSchemes("Bearer");
                builder.RequireAuthenticatedUser();
                builder.RequireClaim("scope", SMTConsts.Service_Super_Admin_Scope);
            });

            options.AddPolicy(SMTConsts.AuthorPolicy_TenantAdmin, builder => {
                builder.AddAuthenticationSchemes("Bearer");
                builder.RequireAuthenticatedUser();
                builder.RequireRole(SMTConsts.Service_Admin_Role);
            });
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts => {
                opts.RequireHttpsMetadata = false;
                opts.Authority = "http://localhost:5000";
                opts.Audience = "http://localhost:5000/resources";
            });

        builder.Services.AddCors(options => {
            options.AddPolicy("default", policy => {
                policy.AllowAnyOrigin();
                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
            });
        });


        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        var fordwardedHeaderOptions = new ForwardedHeadersOptions {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        fordwardedHeaderOptions.KnownNetworks.Clear();
        fordwardedHeaderOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(fordwardedHeaderOptions);

        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            //app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors("default");

        app.UseMultiTenant();

        app.UseMiddleware<ContextTenantAttachMiddleware>();

        app.Use(async (ctx, next) => {
            string prefix = ctx.Request.Headers["X-Forwarded-Prefix"];
            if (!string.IsNullOrWhiteSpace(prefix)) {
                string host = ctx.Request.Host.Value;
                //ctx.Request.Host = new HostString($"{host}/{prefix}");
                //ctx.SetIdentityServerBasePath($"/{prefix}");
                //ctx.SetIdentityServerOrigin($"{(!string.IsNullOrEmpty(proto) ? $"{proto}://" : "http://")}{host}/{prefix}");
                ctx.SetIdentityServerOrigin($"{ctx.Request.Scheme}://{host}/{prefix}");
            }
            await next();

        });

        app.UseIdentityServer();
        app.UseAuthorization();


        app.UseEndpoints(endpoints => {
            endpoints.MapControllerRoute("default", "api/{controller=Home}/{action=Index}");
        });

        // uncomment if you want to add a UI
        //app.UseAuthorization();
        //app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
