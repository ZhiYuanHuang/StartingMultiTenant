using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Repository
{
    public class TenantIdentifierRepository : BaseRepository<TenantIdentifierModel>
    {
        public override string TableName => "TenantIdentifier";

        public TenantIdentifierRepository(TenantDbDataContext tenantDbDataContext) : base(tenantDbDataContext) {

        }

        public List<TenantIdentifierModel> GetTenantListByDomain(string tenantDomain) {
            Dictionary<string, object> p = new Dictionary<string, object>() {
                { "TenantDomain",tenantDomain}
            };

            return GetEntitiesByQuery(p);
        }
    }
}
