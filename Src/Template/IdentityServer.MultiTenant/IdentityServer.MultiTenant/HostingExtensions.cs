using IdentityServer.MultiTenant.DbContext;
using IdentityServer.MultiTenant.Filter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Serilog;
using StartingMultiTenantLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdentityServer.MultiTenant.Service;
using Duende.IdentityServer.Services;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Authorization;
using IdentityServer.MultiTenant.Const;

namespace IdentityServer.MultiTenant;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        //builder.Services.AddRazorPages();
        builder.Services.AddControllersWithViews(options => {
            options.Filters.Add < GlobalExceptionFilter>();
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

            //redis¡¢k8s just use one 

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
        }, ServiceLifetime.Scoped)
            .WithPerTenantOptions<JwtBearerOptions>((o, tenantInfo) => {
                var schema= builder.Configuration.GetValue<string>("MultiTenantOption:JwtBearerSchema","http");
                string jwtBearerUrl = $"{schema}://{tenantInfo.Identifier}.{tenantInfo.TenantDomain}";
                o.Authority = jwtBearerUrl;
                o.Audience = jwtBearerUrl + "/resources";

                o.RequireHttpsMetadata = false;
            });

        string specifiedDbIdentifier = builder.Configuration.GetValue("MultiTenantOption:SpecifiedDbIdentifier", string.Empty);
        builder.Services.AddDbContext<AspNetAccountDbContext>((provider, options) => {
            var contextTenant= provider.GetRequiredService<TenantDbConnsDto>();
            if (contextTenant != null && contextTenant.InnerDbConnList!=null && contextTenant.InnerDbConnList.Any()) {
                if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                    var foundDbConnDto = contextTenant.InnerDbConnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                    if (foundDbConnDto == null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                        options.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                    }
                } else {
                    options.UseMySql(contextTenant.InnerDbConnList[0].DbConn, MySqlServerVersion.AutoDetect(contextTenant.InnerDbConnList[0].DbConn));
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
        })
            .AddConfigurationStore(options => {
                options.ResolveDbContextOptions = (provider, builder) => {
                    var contextTenant = provider.GetService<TenantDbConnsDto>();
                    if (contextTenant != null) {
                        if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                            var foundDbConnDto = contextTenant.InnerDbConnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                            if (foundDbConnDto != null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                                builder.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                            }
                        } else {
                            builder.UseMySql(contextTenant.InnerDbConnList[0].DbConn, MySqlServerVersion.AutoDetect(contextTenant.InnerDbConnList[0].DbConn));
                        }
                    }
                };
            })
            .AddOperationalStore(options => {
                options.ResolveDbContextOptions = (provider, builder) => {
                    var contextTenant = provider.GetService<TenantDbConnsDto>();
                    if (contextTenant != null) {
                        if (!string.IsNullOrEmpty(specifiedDbIdentifier)) {
                            var foundDbConnDto = contextTenant.InnerDbConnList.FirstOrDefault(x => string.Compare(x.DbIdentifier, specifiedDbIdentifier, true) == 0);
                            if (foundDbConnDto != null && !string.IsNullOrEmpty(foundDbConnDto.DbConn)) {
                                builder.UseMySql(foundDbConnDto.DbConn, MySqlServerVersion.AutoDetect(foundDbConnDto.DbConn));
                            }
                        } else {
                            builder.UseMySql(contextTenant.InnerDbConnList[0].DbConn, MySqlServerVersion.AutoDetect(contextTenant.InnerDbConnList[0].DbConn));
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

            options.AddPolicy(IdsConst.AuthorPolicy_TenantAdmin, builder => {
                builder.AddAuthenticationSchemes("Bearer");
                builder.RequireAuthenticatedUser();
                builder.RequireClaim(  "aud", serviceIdentifier);
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
        app.UseSerilogRequestLogging();
    
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();

        app.UseRouting();
        app.UseCors("default");

        app.UseMultiTenant();

        app.UseIdentityServer();
        app.UseAuthorization();

        // uncomment if you want to add a UI
        //app.UseAuthorization();
        //app.MapRazorPages().RequireAuthorization();

        return app;
    }
}
