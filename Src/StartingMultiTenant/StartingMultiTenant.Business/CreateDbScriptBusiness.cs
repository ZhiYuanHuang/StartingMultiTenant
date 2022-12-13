using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public class CreateDbScriptBusiness 
    {
        public async Task<List<CreateDbScriptModel>> GetListByNames(List<string> nameList) {
            return new List<CreateDbScriptModel>();
        }
    }
}
