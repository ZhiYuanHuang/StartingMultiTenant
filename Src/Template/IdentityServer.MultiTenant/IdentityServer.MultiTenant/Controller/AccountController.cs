using IdentityServer.MultiTenant.DbContext;
using IdentityServer.MultiTenant.Dto;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.MultiTenant.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        ConfigurationDbContext _configurationDbContext;
        AspNetAccountDbContext _aspNetAccountDbContext;
        UserManager<ApplicationUser> _userMgr;
        public AccountController(ConfigurationDbContext configurationDbContext,
            AspNetAccountDbContext aspNetAccountDbContext,
            UserManager<ApplicationUser> userMgr) {
            _configurationDbContext = configurationDbContext;
            _aspNetAccountDbContext = aspNetAccountDbContext;
            _userMgr=userMgr;
        }

        [HttpPost]
        public async Task<AppResponseDto> AddOrUpdate(ApplicationUser applicationUser) {
            var existedUser= _userMgr.FindByNameAsync(applicationUser.UserName);
            if (existedUser == null) {
                var result=await _userMgr.CreateAsync(applicationUser, "123456");
               
                if (!result.Succeeded) {
                    return new AppResponseDto(false);
                }
            } else {
                var result=await _userMgr.UpdateAsync(applicationUser);
                if (!result.Succeeded) {
                    return new AppResponseDto(false);
                }
            }
            return new AppResponseDto();
        }
    }
}
