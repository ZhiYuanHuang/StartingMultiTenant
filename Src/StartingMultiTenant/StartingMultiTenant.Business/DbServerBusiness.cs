using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StartingMultiTenant.Business
{
    public interface IDbServerBusiness {
        Task<List<DbServerModel>> GetDbServers(Int64? dbServerId=null);
    }
    public class DbServerBusiness
    {
        public async Task<List<DbServerModel>> GetDbServers(Int64? dbServerId=null) {
            return new List<DbServerModel>();
        }
    }
}
