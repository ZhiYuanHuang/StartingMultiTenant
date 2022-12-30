using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
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

        public string CreateJwtToken(ApiClientModel apiClientModel) {
            List<Claim> claimList = new List<Claim>()
                    {
                        new Claim(type: ClaimTypes.Name, value: apiClientModel.ClientId), 
                    };
            if (!string.IsNullOrEmpty(apiClientModel.Role)) {
                claimList.Add(new Claim(ClaimTypes.Role, apiClientModel.Role));
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
