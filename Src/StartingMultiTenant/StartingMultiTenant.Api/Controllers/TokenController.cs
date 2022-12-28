using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Api.Security;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly TokenBuilder _tokenBuilder;
        public TokenController(TokenBuilder tokenBuilder) {
            _tokenBuilder= tokenBuilder;
        }

        public AppResponseDto<string> Token(AppRequestDto<ApiClientModel> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto<string>(false);
            }

            _tokenBuilder.CreateJwtToken(requestDto.Data);
        }
    }
}
