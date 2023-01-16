using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class ApiClientDto
    {
        public string ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Role { get; set; }
        public List<string>? Scopes { get; set; }
    }

}
