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
    public class SchemaUpdateScriptController : ControllerBase
    {
        private readonly SchemaUpdateScriptBusiness _schemaUpdateScriptBusiness;
        private readonly ILogger<SchemaUpdateScriptController> _logger;
        private readonly MultiTenantService _multiTenantService;
        public SchemaUpdateScriptController(SchemaUpdateScriptBusiness schemaUpdateScriptBusiness,
            MultiTenantService multiTenantService,
            ILogger<SchemaUpdateScriptController> logger) {
            _schemaUpdateScriptBusiness= schemaUpdateScriptBusiness;
            _multiTenantService= multiTenantService;
            _logger= logger;
        }

        [HttpPost]
        public async Task<AppResponseDto> AddSchemaUpdateScript([FromForm] UpdateSchemaScriptDto updateSchemaScriptDto) {
            byte[]? updateScriptByteArr = null;
            byte[]? rollbackScriptByteArr = null;

            try {
                using (var memoryStream=new MemoryStream()) {
                    await updateSchemaScriptDto.UpdateScriptFile.CopyToAsync(memoryStream);
                    updateScriptByteArr= memoryStream.ToArray();
                }

                using (var memoryStream = new MemoryStream()) {
                    await updateSchemaScriptDto.RollbackScriptFile.CopyToAsync(memoryStream);
                    rollbackScriptByteArr = memoryStream.ToArray();
                }
            }
            catch(Exception ex) {
                _logger.LogError(ex,"read upload script error");
            }

            if(updateScriptByteArr==null || updateScriptByteArr.Length<=0
                || rollbackScriptByteArr==null || rollbackScriptByteArr.Length <= 0) {
                return new AppResponseDto(false);
            }

            SchemaUpdateScriptModel schemaUpdateScript = new SchemaUpdateScriptModel() { 
                Name=updateSchemaScriptDto.Name,
                CreateScriptName=updateSchemaScriptDto.CreateScriptName,
                BaseMajorVersion=updateSchemaScriptDto.BaseMajorVersion,
                MinorVersion=updateSchemaScriptDto.MinorVersion,
                DbNameWildcard=updateSchemaScriptDto.DbNameWildcard,
                BinaryContent=updateScriptByteArr,
                RollBackScriptBinaryContent=rollbackScriptByteArr
            };

            var result = _schemaUpdateScriptBusiness.Insert(schemaUpdateScript);
            return new AppResponseDto(false);
        }

        [HttpGet]
        public AppResponseDto Delete(Int64 scriptId) {
            var result = _schemaUpdateScriptBusiness.Delete(scriptId);
            return new AppResponseDto(result);
        }

        [HttpPost]
        public async Task<AppResponseDto> ExecuteSchemaUpdate(AppRequestDto<Int64> requestDto) {
            if (requestDto.Data <= 0) {
                return new AppResponseDto(false);
            }

            var existedScript= _schemaUpdateScriptBusiness.GetById(requestDto.Data);
            if (existedScript == null) {
                return new AppResponseDto(false);
            }
            var executeResultTuple=await _multiTenantService.ExecuteSchemaUpdate(existedScript.Name);
            if (!executeResultTuple.Item1) {
                if (executeResultTuple.Item2 > 0) {
                    return new AppResponseDto(false) {
                        ErrorMsg = $"part success,successCount:{executeResultTuple.Item2},failureCount:{executeResultTuple.Item3}"
                    };
                }
                return new AppResponseDto(false);
            }


            return new AppResponseDto(true);
        }
    }
}
