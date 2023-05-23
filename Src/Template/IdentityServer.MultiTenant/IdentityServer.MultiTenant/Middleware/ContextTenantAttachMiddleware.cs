using Finbuckle.MultiTenant;
using StartingMultiTenantLib;
using System.Net;

namespace IdentityServer.MultiTenant.Middleware
{
    public class ContextTenantAttachMiddleware
    {
        private readonly RequestDelegate _next;
        public ContextTenantAttachMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ContextTenantInfo contextTenant) {
            var tenantInfo = context.GetMultiTenantContext<TenantDbConnsDto>();
          
            if (tenantInfo != null && tenantInfo.TenantInfo != null) {
                contextTenant.CurrentTenantInfo = tenantInfo.TenantInfo;
            }

            await _next(context);
        }

    }
}
