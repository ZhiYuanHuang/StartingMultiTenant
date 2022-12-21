using StartingMultiTenant.Model.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Dto
{
    public class DbServerDto : DbServerModel
    {
        public string? UserPwd { get; set; }
    }
}
