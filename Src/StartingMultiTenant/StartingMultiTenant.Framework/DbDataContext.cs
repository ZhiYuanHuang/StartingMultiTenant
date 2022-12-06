using System;
using System.Collections.Generic;
using System.Text;

namespace StartingMultiTenant.Framework
{
    public abstract class DbDataContext
    {
        public DbDataContext() { 
        }
        public IDbFunc Master { get; set; }
        public IDbFunc Slave { get; set; }
        public DbDataContextOption DbDataContextOption { get; set; }
    }

   

}
