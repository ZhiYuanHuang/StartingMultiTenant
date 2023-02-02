using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class TenantServiceDbConnDto: TenantServiceDbConnModel
    {
        public string? DbConnStr { get; set; }
        public List<ServiceDbConnsDto>? HistoryConns { get; set; }
    }
}
