using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace StartingMultiTenant.Api.Security
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; }
        public bool ValidateIssuer { get; set; }
        public string Audience { get; set; }
        public bool ValidateAudience { get; set; }
        public string RawSigningKey { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }

        private SecurityKey _signingKey;
        public SecurityKey SigningKey {
            get {
                if(_signingKey==null) {
                    _signingKey= new SymmetricSecurityKey(Encoding.ASCII.GetBytes(RawSigningKey));
                }
                return _signingKey;
            }
        }

        public TokenValidationParameters ToTokenValidationParams() {
            return new TokenValidationParameters() { 
                ValidateIssuer=ValidateIssuer,
                ValidIssuer=Issuer,
                ValidateAudience=ValidateAudience,
                ValidAudience=Audience,
                ValidateIssuerSigningKey=ValidateIssuerSigningKey,
                IssuerSigningKey=SigningKey,
                RequireExpirationTime=false,
                ValidateLifetime=false,
            };
        }
    }
}
