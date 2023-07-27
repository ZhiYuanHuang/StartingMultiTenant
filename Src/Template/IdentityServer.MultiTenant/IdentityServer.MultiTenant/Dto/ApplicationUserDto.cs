using IdentityServer.MultiTenant.DbContext;

namespace IdentityServer.MultiTenant.Dto
{
    public class ApplicationUserDto: ApplicationUser
    {
        public string PlainPassword { get; set; }
        public List<AppClaimDto>? Claims { get; set; }
        public bool NotChangePasswordWhenExist { get; set; }
    }
}
