using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Model.Domain
{
    public class SysUserModel
    {
        public Int64 Id { get; set; }
        public string UserName { get; set; }
        public string EncryptPassword { get; set; }
    }
}
