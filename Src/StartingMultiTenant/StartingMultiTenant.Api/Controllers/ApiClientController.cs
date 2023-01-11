using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApiClientController : ControllerBase {
        private readonly ApiClientBusiness _apiClientBusiness;
        private readonly EncryptService _encryptService;
        private readonly ApiScopeBusiness _apiScopeBusiness;
        public ApiClientController(ApiClientBusiness apiClientBusiness,
            EncryptService encryptService,
            ApiScopeBusiness apiScopeBusiness) {
            _apiClientBusiness = apiClientBusiness;
            _encryptService = encryptService;
            _apiScopeBusiness = apiScopeBusiness;
        }

        [HttpGet]
        public IActionResult Init() {
            var adminClients = _apiClientBusiness.GetAdmins();

            StringBuilder builder = new StringBuilder("<html lang=\"en\">");
            builder.AppendLine("<head>\r\n<meta charset=\"UTF-8\">\r\n<Meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0,maximum-scale=1.0, user-scalable=no\"/><script>\r\n  copyInnerTextOfCell = (event) => {\r\n    let innerText = event.target.innerText;\r\n    var tmpInput = document.createElement(\"input\");\r\n    document.body.appendChild(tmpInput);\r\n    tmpInput.value = innerText;\r\n    tmpInput.select();\r\n    document.execCommand(\"cut\"); // copy\r\n    tmpInput.remove();\r\n    }\r\n  </script>\r\n</head>");

            builder.AppendLine("<body>");

            if (adminClients.Any()) {
                builder.AppendLine(string.Format("<h3>already inited!Admin clients : {0}</h3>", string.Join(',', adminClients.Select(x => x.ClientId))));
            } else {
                builder.AppendLine("<h3>添加管理client</h3>");
                builder.AppendLine(@"<form action=""/api/client/init"" method=""post"">
  ClientId: <input type=""text"" name=""clientId""><br>
  ClientSecret: <input type=""text"" name=""clientSecret""><br>
  <input type=""submit"" value=""提交"">
</form>");
            }

            builder.AppendLine("</body></html>");

            return Content(builder.ToString(), "text/html");
        }

        [HttpPost]
        public IActionResult Init([FromForm] string clientId, [FromForm] string clientSecret) {
            string encryptSecret = _encryptService.Encrypt_Aes(clientSecret);
            var result = _apiClientBusiness.Insert(clientId, encryptSecret, out _, RoleConst.Role_Admin);
            if (result) {
                return Content("初始化成功");
            } else {
                return Content("初始化失败");
            }
        }

        [HttpGet]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto<ApiClientModel> Get(Int64 id) {
            var model = _apiClientBusiness.Get(id);
            if (model == null) {
                return new AppResponseDto<ApiClientModel>(false);
            }
            return new AppResponseDto<ApiClientModel>() { Result = model };
        }

        [HttpGet]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto<ApiClientModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _apiClientBusiness.Get(requestDto.Data);
            return new AppResponseDto<ApiClientModel>() { ResultList = models };
        }

        [HttpPost]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto<Int64> Add(AppRequestDto<ApiClientModel> requestDto) {
            ApiClientModel apiClient = requestDto.Data;

            apiClient.ClientSecret = _encryptService.Encrypt_Aes(apiClient.ClientSecret);
            bool result = _apiClientBusiness.Insert(apiClient.ClientId, apiClient.ClientSecret, out Int64 id);
            return new AppResponseDto<Int64>(result) { Result = id };
        }

        [HttpDelete]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            var result = _apiClientBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result.Item1) { ErrorMsg = result.Item2 };
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _apiClientBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpPost]
        public AppResponseDto<PagingData<ApiClientModel>> GetList(AppRequestDto<PagingParam<Dictionary<string, string>>> requestDto) {
            string clientId = null;
            if(requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("clientId") && requestDto.Data.Filter["clientId"] != null) {
                clientId = requestDto.Data.Filter["clientId"];
            }

            PagingData<ApiClientModel> pagingData = null;
            if (string.IsNullOrEmpty(clientId)) {
                pagingData= _apiClientBusiness.GetPage(requestDto.Data.PageSize,requestDto.Data.PageIndex);
            } else {
                pagingData=_apiClientBusiness.GetPage(clientId,requestDto.Data.PageSize, requestDto.Data.PageIndex);
            }
            foreach (var client in pagingData.Data) {
                client.ClientSecret = _encryptService.Decrypt_Aes(client.ClientSecret);
            }
            return new AppResponseDto<PagingData<ApiClientModel>>() { Result = pagingData };
        }


        [HttpPost]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto AddScope(AppRequestDto<ApiScopeModel> requestDto) {
            bool result = _apiScopeBusiness.Insert(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpGet]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto DeleteScope(string scopeName) {
            var result = _apiScopeBusiness.Delete(scopeName);
            return new AppResponseDto(result);
        }

        [HttpGet]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
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
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto<ApiScopeModel> GetScopes() {
            var scopes = _apiScopeBusiness.GetAll();
            
            return new AppResponseDto<ApiScopeModel>() {
                ResultList = scopes
            };
        }

        [HttpPost]
        [Authorize(AuthorizePolicyConst.Sys_Policy)]
        public AppResponseDto Authorize(ClientDomainScopesDto clientDomainScopesDto) {
            var result= _apiClientBusiness.Authorize(clientDomainScopesDto);
            return new AppResponseDto(result);
        }
    }
}
