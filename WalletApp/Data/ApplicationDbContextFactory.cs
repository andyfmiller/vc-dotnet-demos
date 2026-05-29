using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WalletApp.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("Default")
                ?? "DataSource=WalletApp.db;Pooling=False";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqliteOptions.MigrationsAssembly("WalletApp");
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
