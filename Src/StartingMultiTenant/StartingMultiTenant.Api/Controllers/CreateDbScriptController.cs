using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StartingMultiTenant.Business;
using StartingMultiTenant.Model.Const;
using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Model.Dto;
using System.Buffers.Text;
using System.Collections.Generic;

namespace StartingMultiTenant.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthorizePolicyConst.Sys_Policy)]
    public class CreateDbScriptController : ControllerBase
    {
        private readonly CreateDbScriptBusiness _createDbScriptBusiness;
        private readonly ILogger<CreateDbScriptController> _logger;
        private readonly ServiceInfoBusiness _serviceInfoBusiness;
        private readonly DbInfoBusiness _dbInfoBusiness;
        public CreateDbScriptController(CreateDbScriptBusiness createDbScriptBusiness,
            ServiceInfoBusiness serviceInfoBusiness,
            DbInfoBusiness dbInfoBusiness,
            ILogger<CreateDbScriptController> logger) {
            _createDbScriptBusiness= createDbScriptBusiness;
            _serviceInfoBusiness = serviceInfoBusiness;
            _dbInfoBusiness = dbInfoBusiness;
            _logger= logger;
        }

        [HttpPost]
        public async Task<AppResponseDto<Int64>> Add(AppRequestDto<CreateDbScriptFormDto> requestDto) {
            var createDbScriptDto = requestDto.Data;
            if (createDbScriptDto.Attachments==null || !createDbScriptDto.Attachments.Any()) {
                return new AppResponseDto<Int64>(false);
            }

            byte[]? scriptContentByte=null;
            try {
                int tmpIndex= createDbScriptDto.Attachments[0].Src.IndexOf(';');
                string tmpSrc = createDbScriptDto.Attachments[0].Src.Substring(tmpIndex+8);
                scriptContentByte =Convert.FromBase64String(tmpSrc);
                
                //using (var memoryStream = new MemoryStream()) {
                //    await createDbScriptDto.ScriptFile.CopyToAsync(memoryStream);
                //    scriptContentByte= memoryStream.ToArray();
                //}
            }catch(Exception ex) {
                _logger.LogError(ex,"read upload create script byte error");
            }

            if(scriptContentByte==null || scriptContentByte.Length <= 0) {
                return new AppResponseDto<Int64>(false);
            }

            var serviceInfoDto= _serviceInfoBusiness.GetByServiceInfo(createDbScriptDto.ServiceInfoId);
            if (serviceInfoDto == null) {
                return new AppResponseDto<Int64>(false);
            }
            var dbInfoDto= serviceInfoDto.DbInfos.FirstOrDefault(x => x.DbInfoId == createDbScriptDto.DbInfoId);
            if(dbInfoDto == null) {
                return new AppResponseDto<Int64>(false);
            }

            CreateDbScriptModel createDbScript = new CreateDbScriptModel() {
                Name=createDbScriptDto.Name,
                MajorVersion=createDbScriptDto.MajorVersion,
                ServiceIdentifier= serviceInfoDto.ServiceIdentifier,
                DbIdentifier    = dbInfoDto.DbIdentifier,
                DbNameWildcard =createDbScriptDto.DbNameWildcard,
                BinaryContent=scriptContentByte,
                DbType=createDbScriptDto.DbType,
            };

            var result= _createDbScriptBusiness.Insert(createDbScript,out Int64 id);
            return new AppResponseDto<Int64>(result) { Result=id};
        }

        [HttpDelete]
        public AppResponseDto Delete(AppRequestDto<Int64> requestDto) {
            var result= _createDbScriptBusiness.Delete(requestDto.Data);
            return new AppResponseDto(result.Item1) { ErrorMsg=result.Item2};
        }

        [HttpDelete]
        public AppResponseDto<List<Int64>> DeleteMany(AppRequestDto<List<Int64>> requestDto) {
            var resultTuple = _createDbScriptBusiness.DeleteMany(requestDto.Data);
            return new AppResponseDto<List<Int64>>(resultTuple.Item1) {
                Result = resultTuple.Item2,
                ErrorMsg = resultTuple.Item3
            };
        }

        [HttpPost]
        public AppResponseDto<PagingData<CreateDbScriptDto>> GetList(AppRequestDto<PagingParam<Dictionary<string, object>>> requestDto) {
            string name = null;
            if (requestDto.Data.Filter.Any() && requestDto.Data.Filter.ContainsKey("name") && requestDto.Data.Filter["name"] != null) {
                name = requestDto.Data.Filter["name"].ToString();
            }
            var pageList= _createDbScriptBusiness.GetPageNoContent(name,requestDto.Data.PageSize,requestDto.Data.PageIndex);

            var serviceDict = _serviceInfoBusiness.GetByServiceInfo(pageList.Data.Select(x=>x.ServiceIdentifier).Distinct().ToList());
            var dbDict = _dbInfoBusiness.GetByServiceInfo(pageList.Data.Select(x => x.DbIdentifier).Distinct().ToList());
            PagingData<CreateDbScriptDto> pageDtoList = new PagingData<CreateDbScriptDto>(pageList.PageSize, pageList.PageIndex, pageList.RecordCount, pageList.Data.Select(x => ConvertFromModel(x, serviceDict,dbDict)).ToList());
            return new AppResponseDto<PagingData<CreateDbScriptDto>>() {Result= pageDtoList };
        }

        [HttpGet]
        public AppResponseDto<CreateDbScriptModel> GetAll() {
            var allCreateDbScripts= _createDbScriptBusiness.GetAllNoContent();
            return new AppResponseDto<CreateDbScriptModel>() { ResultList=allCreateDbScripts};
        }

        [HttpGet]
        public AppResponseDto<CreateDbScriptDto> Get(Int64 id) {
            var model = _createDbScriptBusiness.Get(id);
            if (model == null) {
                return new AppResponseDto<CreateDbScriptDto>(false);
            }
            model.BinaryContent = null;

            var serviceDict= _serviceInfoBusiness.GetByServiceInfo(new List<string>() { model.ServiceIdentifier });
            var dbDict = _dbInfoBusiness.GetByServiceInfo(new List<string>() { model.DbIdentifier });

            return new AppResponseDto<CreateDbScriptDto>() { Result = ConvertFromModel(model,serviceDict,dbDict) };
        }

        private CreateDbScriptDto ConvertFromModel(CreateDbScriptModel model,Dictionary<string,Int64> serviceDict,Dictionary<string,Int64> dbDict) {
            CreateDbScriptDto dto = new CreateDbScriptDto() {
                Id = model.Id,
                Name = model.Name,
                MajorVersion = model.MajorVersion,
                ServiceIdentifier = model.ServiceIdentifier,
                ServiceInfoId = serviceDict.ContainsKey(model.ServiceIdentifier) ? serviceDict[model.ServiceIdentifier] : 0,
                DbIdentifier = model.DbIdentifier,
                DbInfoId = dbDict.ContainsKey(model.DbIdentifier) ? dbDict[model.DbIdentifier] : 0,
                DbType = model.DbType,
            };
            return dto;
        }

        [HttpPost]
        public AppResponseDto<CreateDbScriptModel> GetMany(AppRequestDto<List<Int64>> requestDto) {
            var models = _createDbScriptBusiness.Get(requestDto.Data);
            models.ForEach(x=>x.BinaryContent=null);
            return new AppResponseDto<CreateDbScriptModel>() { ResultList = models };
        }

        [HttpGet]
        public IActionResult GetScriptContent(Int64 scriptId) {
            var createScript= _createDbScriptBusiness.Get(scriptId);
            if (createScript == null) {
                return NotFound();
            }

            byte[] byteArr= _createDbScriptBusiness.GetScriptContent(scriptId);
            MemoryStream memoryStream= new MemoryStream(byteArr);
            return File(memoryStream, "application/octet-stream", createScript.Name+".sql");
        }
    }
}
