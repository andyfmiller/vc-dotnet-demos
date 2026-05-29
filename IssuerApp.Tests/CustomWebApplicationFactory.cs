using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace IssuerApp.Integration.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection _connection = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Remove the existing ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Create and open a persistent SQLite in-memory connection
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // Add ApplicationDbContext using the persistent in-memory SQLite database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to seed the database before the app starts
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Database.EnsureCreated();

                SeedTestDataAsync(scope).GetAwaiter().GetResult();
            });

            // Override authentication AFTER Identity's ConfigureServices has run
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                    options.DefaultSignInScheme = TestAuthHandler.SchemeName;
                    options.DefaultSignOutScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
            });
        }

        private static async Task SeedTestDataAsync(IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed roles so SeedData.SeedRoles in Program.cs finds them already present
            foreach (var role in Constants.Roles.UserRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed test organization profile
            var orgProfile = new Profile
            {
                Id = "https://example.com/org/1",
                Name = "Test Organization",
                Type = new[] { "Profile" }
            };
            _ = orgProfile.AdditionalProperties;
            db.Profiles.Add(orgProfile);
            await db.SaveChangesAsync();

            var organization = new Organization { ProfileKey = orgProfile.ProfileKey };
            db.Organizations.Add(organization);
            await db.SaveChangesAsync();

            var member = new Member
            {
                Name = "Test Member",
                OrganizationKey = organization.OrganizationKey
            };
            db.Members.Add(member);
            await db.SaveChangesAsync();

            // Create the test user so UserManager.GetUserAsync(User) resolves during requests.
            // The Id must match TestAuthHandler.TestUserId, which is carried in the NameIdentifier claim.
            // A Profile is required because _Layout.cshtml renders user.Profile.Name.
            var userProfile = new Profile
            {
                Id = TestAuthHandler.TestUserName,
                Name = "Test Admin",
                Type = new[] { "Profile" }
            };
            _ = userProfile.AdditionalProperties;
            db.Profiles.Add(userProfile);
            await db.SaveChangesAsync();

            var testUser = new ApplicationUser
            {
                Id = TestAuthHandler.TestUserId,
                UserName = TestAuthHandler.TestUserName,
                Email = TestAuthHandler.TestUserName,
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SelectedOrganizationKey = organization.OrganizationKey
            };
            await userManager.CreateAsync(testUser);
            await userManager.AddToRoleAsync(testUser, Constants.Roles.Admin);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Configure Serilog for testing
            builder.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .MinimumLevel.Warning()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });

            return base.CreateHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
        }
    }
}
