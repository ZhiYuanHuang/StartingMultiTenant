using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityModel;
using IdentityServer.MultiTenant.DbContext;
using Microsoft.AspNetCore.Identity;

namespace IdentityServer.MultiTenant.Service
{
    public class ProfileService : IProfileService
    {
        UserManager<ApplicationUser> _userManager;
        public ProfileService(UserManager<ApplicationUser> userManager) {
            _userManager = userManager;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context) {
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());
            if (user != null) {
                var claims = await _userManager.GetClaimsAsync(user);
                context.IssuedClaims.AddRange(claims);

                var roles = await _userManager.GetRolesAsync(user);

                context.IssuedClaims.AddRange(roles.Select(x => new System.Security.Claims.Claim(JwtClaimTypes.Role, x)));
            }
            await Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context) {
            context.IsActive = true;
        }
    }
}
