using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IssuerApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use SQLite with a temporary connection string for design-time
            // Don't use lazy loading proxies here to avoid virtual property requirements
            optionsBuilder.UseSqlite("Data Source=issuerapp.db", x => x.MigrationsAssembly("IssuerApp"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
