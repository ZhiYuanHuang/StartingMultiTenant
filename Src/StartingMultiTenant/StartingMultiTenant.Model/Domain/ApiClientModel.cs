using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class ApiClientModel
    {
        public Int64? Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string? Role { get; set; }
    }
}
