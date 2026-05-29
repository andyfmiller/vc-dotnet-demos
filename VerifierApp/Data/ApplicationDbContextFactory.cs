using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VerifierApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use SQLite with a temporary connection string for design-time
            optionsBuilder.UseSqlite("Data Source=verifierapp.db", x => x.MigrationsAssembly("VerifierApp"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
