using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class BaseRepository
    {
        protected readonly TenantDbDataContext _tenantDbDataContext;
        public BaseRepository(TenantDbDataContext tenantDbDataContext) {
            _tenantDbDataContext = tenantDbDataContext;
        }
    }
}
