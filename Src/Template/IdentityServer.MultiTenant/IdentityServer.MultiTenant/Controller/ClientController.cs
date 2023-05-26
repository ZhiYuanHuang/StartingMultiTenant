﻿using IdentityServer.MultiTenant.Dto;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StartingMultiTenantLib.Const;

namespace IdentityServer.MultiTenant.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Policy = SMTConsts.AuthorPolicy_SuperAdmin)]
    //[Authorize(Roles ="admin")]
    public class ClientController : ControllerBase
    {
        ConfigurationDbContext _configurationDbContext;
        public ClientController(ConfigurationDbContext configurationDbContext) {
            _configurationDbContext = configurationDbContext;
        }

        [HttpPost]
        public async Task<AppResponseDto> AddOrUpdate(IdentityServer4.EntityFramework.Entities.Client clientInfo ) {
            if (clientInfo == null) {
                return new AppResponseDto(false) { ErrorMsg = "clientInfo 不可为空" };
            }

            if (string.IsNullOrEmpty(clientInfo.ClientId)) {
                return new AppResponseDto(false) { ErrorMsg = "ClientId 不可为空" };
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(clientInfo.ClientId, "[a-zA-Z0-9]{5,15}")) {
                return new AppResponseDto(false) { ErrorMsg = "ClientId 应为英文字母、数字组合，最小5位，最大15位" };
            }

            if (clientInfo.ClientSecrets ==null || !clientInfo.ClientSecrets.Any() || !System.Text.RegularExpressions.Regex.IsMatch(clientInfo.ClientSecrets[0].Value, "[a-zA-Z0-9]{6,20}")) {
                return new AppResponseDto(false) { ErrorMsg = "ClientSecret 应为英文字母、数字组合，最小5位，最大20位" };
            }

            IdentityServer4.EntityFramework.Entities.Client existedClient = await _configurationDbContext.Clients
                .Include(x => x.ClientSecrets)
                .Include(x => x.Claims)
                .Include(x => x.AllowedScopes)
                .FirstOrDefaultAsync(x => x.ClientId == clientInfo.ClientId);

            bool result = false;
            if (existedClient == null) {
                clientInfo.AccessTokenLifetime = 60 * 60 * 24 * 15;
                clientInfo.AllowedGrantTypes = new List<IdentityServer4.EntityFramework.Entities.ClientGrantType>() {
                    new IdentityServer4.EntityFramework.Entities.ClientGrantType(){ GrantType=GrantType.ClientCredentials}
                };
                for (int i = 0; i < clientInfo.ClientSecrets.Count; i++) {
                    clientInfo.ClientSecrets[i].Value = clientInfo.ClientSecrets[i].Value.Sha256();
                }

                if (clientInfo.AllowedScopes == null) {
                    clientInfo.AllowedScopes = new List<IdentityServer4.EntityFramework.Entities.ClientScope>();
                }

                _configurationDbContext.Clients.Add(clientInfo);
                result = _configurationDbContext.SaveChanges() > 0;
            } else {

                try {
                    existedClient.AllowOfflineAccess = true;
                    existedClient.ClientName = clientInfo.ClientName;
                    existedClient.Description = clientInfo.Description;

                    existedClient.AllowedScopes = clientInfo.AllowedScopes == null ? (new List<IdentityServer4.EntityFramework.Entities.ClientScope>() ): clientInfo.AllowedScopes;

                    result = _configurationDbContext.SaveChanges() > 0;
                    result = true;
                } catch (Exception ex) {
                    result = false;

                } finally {

                }
            }

            return new AppResponseDto(result);
        }
    }
}
