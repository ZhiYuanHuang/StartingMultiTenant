using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using StartingMultiTenant.Service;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
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
        public AppResponseDto<Int64> Add(AppRequestDto< UpdateSchemaScriptDto> requestDto) {
            var updateSchemaScriptDto = requestDto.Data;
            byte[]? updateScriptByteArr = null;
            byte[]? rollbackScriptByteArr = null;

            if(updateSchemaScriptDto.UpdateScriptAttachments==null || !updateSchemaScriptDto.UpdateScriptAttachments.Any()) {
                return new AppResponseDto<long>(false);
            }

            try {
                int tmpIndex = updateSchemaScriptDto.UpdateScriptAttachments[0].Src.IndexOf(';');
                string tmpSrc = updateSchemaScriptDto.UpdateScriptAttachments[0].Src.Substring(tmpIndex + 8);
                updateScriptByteArr = Convert.FromBase64String(tmpSrc);

                if(updateSchemaScriptDto.RollBackScriptAttachments!=null && updateSchemaScriptDto.RollBackScriptAttachments.Any()) {
                    tmpIndex = updateSchemaScriptDto.RollBackScriptAttachments[0].Src.IndexOf(';');
                    tmpSrc = updateSchemaScriptDto.RollBackScriptAttachments[0].Src.Substring(tmpIndex + 8);
                    rollbackScriptByteArr = Convert.FromBase64String(tmpSrc);
                }
                
            } catch (Exception ex) {
                _logger.LogError(ex,"read upload script error");
            }

            if(updateScriptByteArr==null || updateScriptByteArr.Length<=0
                ) {
                return new AppResponseDto<Int64>(false);
            }

            SchemaUpdateScriptModel schemaUpdateScript = new SchemaUpdateScriptModel() { 
                Name=updateSchemaScriptDto.Name,
                CreateDbScriptId=updateSchemaScriptDto.CreateDbScriptId,
                MinorVersion=updateSchemaScriptDto.MinorVersion,
                BinaryContent=updateScriptByteArr,
                RollBackScriptBinaryContent=rollbackScriptByteArr
            };

            var result = _schemaUpdateScriptBusiness.Insert(schemaUpdateScript,out Int64 id);
            return new AppResponseDto<Int64>(result) {Result=id };
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            var result = _schemaUpdateScriptBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result.Item1) { ErrorMsg = result.Item2 };
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _schemaUpdateScriptBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpPost]
        public AppResponseDto<PagingData<SchemaUpdateScriptModel>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string name = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("name") && requestDto.Data.Filter["name"] != null) {
                name = requestDto.Data.Filter["name"].ToString();
            }
            Int64? createDbScriptId = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("createDbScriptId") && requestDto.Data.Filter["createDbScriptId"] != null) {
                if (Int64.TryParse(requestDto.Data.Filter["createDbScriptId"].ToString(),out Int64 tmpId)){
                    createDbScriptId= tmpId;
                }
            }
            var pageList = _schemaUpdateScriptBusiness.GetPageNoContent(name,createDbScriptId, requestDto.Data.PageSize, requestDto.Data.PageIndex);

            return new AppResponseDto<PagingData<SchemaUpdateScriptModel>>() { Result = pageList };
        }

        [HttpGet]
        public AppResponseDto<SchemaUpdateScriptModel> Get(Int64 id) {
            var model = _schemaUpdateScriptBusiness.Get(id);
            if (model == null) {
                return new AppResponseDto<SchemaUpdateScriptModel>(false);
            }
            
            return new AppResponseDto<SchemaUpdateScriptModel>() { Result = model};
        }

        [HttpPost]
        public AppResponseDto<SchemaUpdateScriptModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _schemaUpdateScriptBusiness.Get(requestDto.Data);

            return new AppResponseDto<SchemaUpdateScriptModel>() { ResultList = models };
        }


        [HttpPost]
        public async Task<AppResponseDto> ExecuteSchemaUpdate(AppRequestDto<Int64> requestDto) {
            if (requestDto.Data <= 0) {
                return new AppResponseDto(false);
            }

            var existedScript= _schemaUpdateScriptBusiness.Get(requestDto.Data);
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

        [HttpGet]
        public IActionResult GetScriptContent(Int64 scriptId) {
            var createScript = _schemaUpdateScriptBusiness.Get(scriptId);
            if (createScript == null) {
                return NotFound();
            }

            byte[] byteArr = _schemaUpdateScriptBusiness.GetScriptContent(scriptId);
            MemoryStream memoryStream = new MemoryStream(byteArr);
            return File(memoryStream, "application/octet-stream", createScript.Name + ".sql");
        }

        [HttpGet]
        public IActionResult GetRollBackScriptContent(Int64 scriptId) {
            var createScript = _schemaUpdateScriptBusiness.Get(scriptId);
            if (createScript == null) {
                return NotFound();
            }

            byte[] byteArr = _schemaUpdateScriptBusiness.GetScriptContent(scriptId,true);
            MemoryStream memoryStream = null;
            if (byteArr == null) {
                memoryStream = new MemoryStream();
            }
            else {
                memoryStream = new MemoryStream(byteArr);
            }
             
            return File(memoryStream, "application/octet-stream", createScript.Name + ".sql");
        }
    }
}
