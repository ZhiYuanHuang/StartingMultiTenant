using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Api.Security;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Repository;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ConnectController : ControllerBase
    {
        private readonly TokenBuilder _tokenBuilder;
        private readonly ApiClientBusiness _apiClientBusiness;
        private readonly EncryptService _encryptService;
        public ConnectController(TokenBuilder tokenBuilder,
            ApiClientBusiness apiClientBusiness,
            EncryptService encryptService) {
            _tokenBuilder= tokenBuilder;
            _apiClientBusiness= apiClientBusiness;
            _encryptService= encryptService;
        }

        [HttpPost]
        public AppResponseDto<string> Token(AppRequestDto<ApiClientModel> requestDto) {
            if (requestDto.Data == null) {
                return new AppResponseDto<string>(false);
            }

            var existedClient= _apiClientBusiness.Get(requestDto.Data.ClientId);
            if (existedClient == null) {
                return new AppResponseDto<string>(false);
            }

            string encryptSecret= _encryptService.Encrypt_Aes(requestDto.Data.ClientSecret);
            if (string.Compare(encryptSecret, existedClient.ClientSecret) != 0) {
                return new AppResponseDto<string>(false) { ErrorMsg="no exist user or error password"};
            }

            string token= _tokenBuilder.CreateJwtToken(existedClient);
            return new AppResponseDto<string>() { Result= token };
        }
    }
}
