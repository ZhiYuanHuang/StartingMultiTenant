using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public interface ICreateDbScriptBusiness {
        Task<List<CreateDbScriptModel>> GetListByNames(List<string> nameList);
    }
    public class CreateDbScriptBusiness : ICreateDbScriptBusiness
    {
        public async Task<List<CreateDbScriptModel>> GetListByNames(List<string> nameList) {
            return new List<CreateDbScriptModel>();
        }
    }
}
