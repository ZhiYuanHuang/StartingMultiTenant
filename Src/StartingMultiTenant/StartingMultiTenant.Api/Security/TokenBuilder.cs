using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StartingMultiTenant.Api.Security
{
    public class TokenBuilder
    {
        private JwtTokenOptions _tokenOptions = null;
        public TokenBuilder(IOptions<JwtTokenOptions> tokenOptions) {
            _tokenOptions = tokenOptions.Value;
        }

        public string CreateJwtToken(ApiClientDto apiClientDto) {
            List<Claim> claimList = new List<Claim>()
                    {
                        new Claim(type: ClaimTypes.Name, value: apiClientDto.ClientId), 
                    };
            if (apiClientDto.Scopes!=null && apiClientDto.Scopes.Any()) {
                apiClientDto.Scopes.ForEach(x => claimList.Add(new Claim("scope",x)));
            }
            if (!string.IsNullOrEmpty(apiClientDto.Role)) {
                claimList.Add(new Claim(ClaimTypes.Role, apiClientDto.Role));
            }
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer:_tokenOptions.Issuer,
                audience:_tokenOptions.Audience,
                claims:claimList,
                signingCredentials:new SigningCredentials(_tokenOptions.SigningKey,SecurityAlgorithms.HmacSha256)
                );
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return jwtToken;
        }
    }
}
