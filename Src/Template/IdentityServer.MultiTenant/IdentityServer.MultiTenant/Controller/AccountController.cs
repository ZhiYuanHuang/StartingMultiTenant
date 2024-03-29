﻿using IdentityServer.MultiTenant.DbContext;
using IdentityServer.MultiTenant.Dto;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenantLib.Const;
using System.Text;

namespace IdentityServer.MultiTenant.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase {
        ConfigurationDbContext _configurationDbContext;
        AspNetAccountDbContext _aspNetAccountDbContext;
        UserManager<ApplicationUser> _userMgr;
        public AccountController(ConfigurationDbContext configurationDbContext,
            AspNetAccountDbContext aspNetAccountDbContext,
            UserManager<ApplicationUser> userMgr) {
            _configurationDbContext = configurationDbContext;
            _aspNetAccountDbContext = aspNetAccountDbContext;
            _userMgr = userMgr;
        }

        
        [HttpPost]
        [Authorize]
        public async Task<AppResponseDto> AddOrUpdate(ApplicationUserDto applicationUserDto) {
            var existedUser = await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            IdentityResult result = null;
            if (existedUser != null) {
                var samePassword= await _userMgr.CheckPasswordAsync(existedUser,applicationUserDto.PlainPassword);
                if (!samePassword) {
                    if (!applicationUserDto.NotChangePasswordWhenExist) {
                        string token = await _userMgr.GeneratePasswordResetTokenAsync(existedUser);
                        result = await _userMgr.ResetPasswordAsync(existedUser, token, applicationUserDto.PlainPassword);
                        if (!result.Succeeded) {
                            return new AppResponseDto(false);
                        }
                    }
                }

                var existClaims = await _userMgr.GetClaimsAsync(existedUser);
                if (applicationUserDto.Claims == null || !applicationUserDto.Claims.Any()) {
                    if (existClaims.Any()) {
                        await _userMgr.RemoveClaimsAsync(existedUser, existClaims);
                    }
                } else {
                    if (existClaims.Any()) {
                        await _userMgr.RemoveClaimsAsync(existedUser, existClaims);
                    }

                    IEnumerable<System.Security.Claims.Claim> claims = applicationUserDto.Claims
                            .Select(x => new System.Security.Claims.Claim(x.Type, x.Value));
                    await _userMgr.AddClaimsAsync(existedUser, claims);
                }
            } else {
                
                result = await _userMgr.CreateAsync(applicationUserDto, applicationUserDto.PlainPassword);
                
                if (!result.Succeeded) {
                    return new AppResponseDto(false);
                }

                if(applicationUserDto.Claims!=null && applicationUserDto.Claims.Any()) {
                    IEnumerable<System.Security.Claims.Claim> claims = applicationUserDto.Claims
                        .Select(x => new System.Security.Claims.Claim(x.Type, x.Value));
                    await _userMgr.AddClaimsAsync(applicationUserDto, claims);
                }
            }

            return new AppResponseDto();
        }

        [HttpPost]
        [Authorize]
        public async Task<AppResponseDto> Add(ApplicationUserDto applicationUserDto) {
            var existedUser=await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            if (existedUser != null) {
                return new AppResponseDto(false) { ErrorMsg = $"user {applicationUserDto.UserName} existed!" };
            }
            
            var result = await _userMgr.CreateAsync(applicationUserDto, applicationUserDto.PlainPassword);

            if (!result.Succeeded) {
                return new AppResponseDto(false);
            }

            return new AppResponseDto();
        }


        [HttpPost]
        [Authorize]
        public async Task<AppResponseDto> Update(ApplicationUserDto applicationUserDto) {
            var existedUser =await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            if (existedUser == null) {
                //var result=await _userMgr.CreateAsync(applicationUser, "123456");

                //if (!result.Succeeded) {
                //    return new AppResponseDto(false);
                //}
                return new AppResponseDto(false) { ErrorMsg = $"user {applicationUserDto.UserName} not exist!" };
            }

            var result = await _userMgr.UpdateAsync(applicationUserDto);
            
            if (!result.Succeeded) {
                return new AppResponseDto(false);
            }

            return new AppResponseDto();
        }

        [HttpPost]
        [Authorize(Policy = SMTConsts.AuthorPolicy_SuperAdmin)]
        public async Task<AppResponseDto> AddAdmin(ApplicationUserDto applicationUserDto) {
            var existedUser =await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            if (existedUser != null) {
                return new AppResponseDto(false) { ErrorMsg = $"user {applicationUserDto.UserName} existed!" };
            }
            
            var result = await _userMgr.CreateAsync(applicationUserDto, applicationUserDto.PlainPassword);
            existedUser = await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            await _userMgr.AddToRoleAsync(existedUser,SMTConsts.Service_Admin_Role);

            return new AppResponseDto();
        }

        [HttpPost]
        [Authorize(Policy = SMTConsts.AuthorPolicy_SuperAdmin)]
        public async Task<AppResponseDto> ResetAmdinPasswd(ApplicationUserDto applicationUserDto) {
            var existedUser = await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            if (existedUser == null) {
                return new AppResponseDto(false) { ErrorMsg = $"user {applicationUserDto.UserName} not exist!" };
            }

            bool isAdmin=await _userMgr.IsInRoleAsync(existedUser, SMTConsts.Service_Admin_Role);
            if (!isAdmin) {
                return new AppResponseDto(false) { ErrorMsg=$"user {applicationUserDto.UserName} isn't admin" };
            }

            string token=await _userMgr.GeneratePasswordResetTokenAsync(existedUser);
            await _userMgr.ResetPasswordAsync(existedUser,token,applicationUserDto.PlainPassword);

            return new AppResponseDto();
        }

        [HttpPost]
        [Authorize(Policy = SMTConsts.AuthorPolicy_TenantAdmin)]
        public async Task<AppResponseDto> ResetPasswd(ApplicationUserDto applicationUserDto) {
            var existedUser = await _userMgr.FindByNameAsync(applicationUserDto.UserName);
            if (existedUser == null) {
                return new AppResponseDto(false) { ErrorMsg = $"user {applicationUserDto.UserName} not exist!" };
            }

            string token = await _userMgr.GeneratePasswordResetTokenAsync(existedUser);
            await _userMgr.ResetPasswordAsync(existedUser, token, applicationUserDto.PlainPassword);

            return new AppResponseDto();
        }

        [HttpPost]
        [Authorize]
        public async Task<AppResponseDto> ResetMyPasswd([FromForm]string oldPasswd, [FromForm]string newPasswd) {
            if (string.Compare(oldPasswd, newPasswd) == 0) {
                return new AppResponseDto(false) { ErrorMsg="old password cann't be same with new password"};
            }

            string accountName= HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(accountName)) {
                return new AppResponseDto(false) { ErrorMsg="cann't find accountName"};
            }

            var existedUser = await _userMgr.FindByNameAsync(accountName);
            if(existedUser == null) {
                return new AppResponseDto(false) { ErrorMsg = $"account {accountName} cann't found" };
            }

            var checkSuccess = await _userMgr.CheckPasswordAsync(existedUser, oldPasswd);
            if (!checkSuccess) {
                return new AppResponseDto(false) { ErrorMsg = "old password not right" };
            }

            string token = await _userMgr.GeneratePasswordResetTokenAsync(existedUser);
            var result = await _userMgr.ResetPasswordAsync(existedUser, token, newPasswd);

            if (!result.Succeeded) {
                StringBuilder errBuilder = new StringBuilder();
                if(result.Errors!=null && result.Errors.Any()) {
                    foreach(var error in result.Errors) {
                        errBuilder.Append($"{error.Code}-{error.Description};");
                    }
                }
                return new AppResponseDto(false) { ErrorMsg=errBuilder.ToString()};
            }


            return new AppResponseDto(true);
        }
    }
}
