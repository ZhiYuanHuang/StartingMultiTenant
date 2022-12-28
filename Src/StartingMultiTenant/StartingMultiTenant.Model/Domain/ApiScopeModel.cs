using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class ApiScopeModel
    {
        public Int64 Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
    }
}
