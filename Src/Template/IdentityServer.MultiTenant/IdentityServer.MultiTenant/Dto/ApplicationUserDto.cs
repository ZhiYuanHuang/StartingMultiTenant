using IdentityServer.MultiTenant.DbContext;

namespace IdentityServer.MultiTenant.Dto
{
    public class ApplicationUserDto: ApplicationUser
    {
        public string PlainPassword { get; set; }
    }
}
