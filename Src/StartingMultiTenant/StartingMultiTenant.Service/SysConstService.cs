using Microsoft.Extensions.Configuration;
using StartingMultiTenant.Model.Const;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Service
{
    public class SysConstService
    {
        public readonly string DbNameWildcard;
        public SysConstService(IConfiguration configuration) {
            DbNameWildcard = !string.IsNullOrEmpty(configuration["DbNameWildcard"]) ? configuration["DbNameWildcard"].ToString() :SysInnerConst.DbNameWildcard;
        }
    }
}
