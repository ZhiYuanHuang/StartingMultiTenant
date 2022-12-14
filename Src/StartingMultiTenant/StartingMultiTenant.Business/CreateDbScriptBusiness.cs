using StartingMultiTenant.Model.Domain;
using StartingMultiTenant.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class CreateDbScriptBusiness 
    {
        private readonly CreateDbScriptRepository _createDbScriptRepo;
        public CreateDbScriptBusiness(CreateDbScriptRepository createDbScriptRepo) {
            _createDbScriptRepo=createDbScriptRepo;
        }
        public async Task<List<CreateDbScriptModel>> GetListByNames(List<string> nameList) {
            return new List<CreateDbScriptModel>();
        }

        public byte[] GetScriptContent(Int64 scriptId) {
            var createScript= _createDbScriptRepo.GetEntityById(scriptId);
            if (createScript == null || createScript.BinaryContent==null) {
                return null;
            }

            byte[] contentByteArr= createScript.BinaryContent as byte[];
            if (contentByteArr == null || contentByteArr.Length==0) {
                return null;
            }

            return contentByteArr;
        }
    }
}
