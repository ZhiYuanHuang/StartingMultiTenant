using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.MultiTenant.DbContext
{
    public class AspNetAccountDbContext : IdentityDbContext<ApplicationUser>
    {
        public AspNetAccountDbContext(DbContextOptions<AspNetAccountDbContext> options) : base(options) {

        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
        }
    }
}
