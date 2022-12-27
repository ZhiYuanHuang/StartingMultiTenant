using StartingMultiTenant.Model.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantActionInfoDto
    {
        public TenantActionInfoDto(string tenantDomain,string tenantIdentifier,int actionType) {
            TenantDomain = tenantDomain;
            TenantIdentifier = tenantIdentifier;
            ActionType= actionType;
        }

        public TenantActionInfoDto(string tenantDomain, string tenantIdentifier, TenantActionTypeEnum actionType)
            :this(tenantDomain,tenantIdentifier,(int)actionType){

        }

        public string TenantIdentifier { get; set; }
        public string TenantDomain { get; set; }

        /// <summary>
        /// TenantActionTypeEnum
        /// </summary>
        public int ActionType { get; set; }
    }

    public class TenantActionInfoDto<T> : TenantActionInfoDto
    {
        public TenantActionInfoDto(string tenantDomain, string tenantIdentifier,int actionType):base(tenantDomain,tenantIdentifier,actionType) {
        }

        public TenantActionInfoDto(string tenantDomain, string tenantIdentifier, TenantActionTypeEnum actionType) : this(tenantDomain, tenantIdentifier, (int)actionType) {
        }

        public T DetailInfo { get; set; }
    }
}
