using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CreateDbScriptController : ControllerBase
    {
        private readonly CreateDbScriptBusiness _createDbScriptBusiness;
        private readonly ILogger<CreateDbScriptController> _logger;
        public CreateDbScriptController(CreateDbScriptBusiness createDbScriptBusiness,
            ILogger<CreateDbScriptController> logger) {
            _createDbScriptBusiness= createDbScriptBusiness;
            _logger= logger;
        }

        [HttpPost]
        public async Task<AppResponseDto> AddCreateScript([FromForm] CreateDbScriptDto createDbScriptDto) {
            byte[]? scriptContentByte=null;
            try {
                using (var memoryStream = new MemoryStream()) {
                    await createDbScriptDto.ScriptFile.CopyToAsync(memoryStream);
                    scriptContentByte= memoryStream.ToArray();
                }
            }catch(Exception ex) {
                _logger.LogError(ex,"read upload create script byte error");
            }

            if(scriptContentByte==null && scriptContentByte.Length <= 0) {
                return new AppResponseDto(false);
            }

            CreateDbScriptModel createDbScript = new CreateDbScriptModel() {
                Name=createDbScriptDto.Name,
                MajorVersion=createDbScriptDto.MajorVersion,
                ServiceIdentifier=createDbScriptDto.ServiceIdentifier,
                DbIdentifier    =createDbScriptDto.DbIdentifier,
                DbNameWildcard =createDbScriptDto.DbNameWildcard,
                BinaryContent=scriptContentByte,
                DbType=createDbScriptDto.DbType,
            };

            var result= _createDbScriptBusiness.Insert(createDbScript);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            var result= _createDbScriptBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public AppResponseDto<CreateDbScriptModel> GetPageScriptsWithNoContent(AppRequestDto<PagingParam> requestDto) {
            var pageList= _createDbScriptBusiness.GetPageScriptsWithNoContent(requestDto.Data.PageSize,requestDto.Data.PageIndex);
            return new AppResponseDto<CreateDbScriptModel>() {ResultList=pageList };
        }

        [HttpPost]
        public IActionResult GetScriptContent(AppRequestDto<Int64> requestDto) {
            var createScript= _createDbScriptBusiness.GetById(requestDto.Data);
            if (createScript == null) {
                return NotFound();
            }

            byte[] byteArr= _createDbScriptBusiness.GetScriptContent(requestDto.Data);
            MemoryStream memoryStream= new MemoryStream(byteArr);
            return File(memoryStream, "application/octet-stream", createScript.Name);
        }
    }
}
