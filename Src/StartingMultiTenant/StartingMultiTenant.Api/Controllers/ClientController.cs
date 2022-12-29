using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly ApiClientBusiness _apiClientBusiness;
        private readonly EncryptService _encryptService;
        private readonly ApiScopeBusiness _apiScopeBusiness;
        public ClientController(ApiClientBusiness apiClientBusiness,
            EncryptService encryptService,
            ApiScopeBusiness apiScopeBusiness) {
            _apiClientBusiness = apiClientBusiness;
            _encryptService = encryptService;
            _apiScopeBusiness = apiScopeBusiness;
        }

        [HttpPost]
        public AppResponseDto AddClient(AppRequestDto<ApiClientModel> requestDto) {
            ApiClientModel apiClient = requestDto.Data;

            apiClient.ClientSecret = _encryptService.Encrypt_Aes(apiClient.ClientSecret);
            bool result= _apiClientBusiness.Add(apiClient.ClientId,apiClient.ClientSecret);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto DeleteClient(string clientId) {
            var result=_apiClientBusiness.Delete(clientId);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto AddScope(AppRequestDto<ApiScopeModel> requestDto) {
            bool result = _apiScopeBusiness.Insert(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto DeleteScope(string scopeName) {
            var result = _apiScopeBusiness.Delete(scopeName);
            return new AppResponseDto(result);
        }

        [HttpGet]
        public AppResponseDto<ApiClientModel> GetClients() {
            var clients= _apiClientBusiness.GetAll();
            foreach(var client in clients) {
                client.ClientSecret = _encryptService.Decrypt_Aes(client.ClientSecret);
            }
            return new AppResponseDto<ApiClientModel>() {
                ResultList = clients
            };
        }

        [HttpGet]
        public AppResponseDto<ApiScopeModel> GetScopes() {
            var scopes = _apiScopeBusiness.GetAll();
            
            return new AppResponseDto<ApiScopeModel>() {
                ResultList = scopes
            };
        }

        [HttpPost]
        public AppResponseDto Authorize(ClientDomainScopesDto clientDomainScopesDto) {
            var result= _apiClientBusiness.Authorize(clientDomainScopesDto);
            return new AppResponseDto(result);
        }
    }
}
