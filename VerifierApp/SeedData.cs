using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using VerifierApp.Data;
using VerifierApp.Data.Models;

namespace VerifierApp
{
    public static class SeedData
    {
        public static async Task EnsureUserSeedData(string connectionString, bool isDevelopment)
        {
            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
            services.AddLogging();
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            await using var serviceProvider = services.BuildServiceProvider();

            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

            Log.Information("Seeding database...");

            await using var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
            if (context != null)
            {
                try
                {
                    if (!isDevelopment)
                    {
                        await context.Database.EnsureDeletedAsync();
                        Log.Information("Database deleted for fresh seed.");
                    }


                    Log.Information("Running database migrations...");
                    await context.Database.MigrateAsync();
                    Log.Information("Database migrations completed.");

                    await EnsureSeedData(serviceScope.ServiceProvider);
                    Log.Information("Seed data insertion completed.");
                    SqliteConnection.ClearAllPools();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Error during database creation. Exception: {Message}", ex.Message);
                    var innerEx = ex.InnerException;
                    while (innerEx != null)
                    {
                        Log.Fatal("Inner exception: {InnerMessage}", innerEx.Message);
                        innerEx = innerEx.InnerException;
                    }
                    throw;
                }
            }
        }

        private static async Task EnsureSeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure roles exist first

            Log.Information("Creating roles...");

            foreach (var role in Constants.Roles.UserRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    Log.Information($"Created role: {role}");
                }
            }

            // Ensure requirements exist

            Log.Information("Creating requirements...");

            await CreateCredentialRequirementIfNotExists(context,
                name: "Barber Certification",
                reason: "You must have a barbering certification from this state or a reciprocal state.",
                achievementType: "Certification",
                credentialType: "OpenBadgeCredential"
                );

            await CreateCredentialRequirementIfNotExists(context,
                name: "Project Management Training",
                reason: "We need to know if you have received project management training.",
                achievementType: "Course",
                credentialType: "OpenBadgeCredential"
                );

            // Seed Admin User

            Log.Information("Creating admin user...");

            await CreateUserIfNotExists(
                userManager,
                context,
                email: "demouser@example.com",
                userName: "DemoUser",
                role: Constants.Roles.Admin);

            Log.Information("Organizations, members, and user seeding complete.");
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string userName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                    Log.Information($"Created user: {email} with role: {role}");
                }
                else
                {
                    Log.Error($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Log.Debug($"User {email} already exists.");
            }
        }

        private static async Task CreateCredentialRequirementIfNotExists(
            ApplicationDbContext context,
            string name,
            string reason,
            string achievementType,
            string credentialType)
        {
            var existing = await context.Requirements.FirstOrDefaultAsync(r => r.Name == name);
            if (existing == null)
            {
                var requirement = new CredentialRequirement
                {
                    Name = name,
                    Reason = reason,
                    AchievementType = achievementType,
                    CredentialType = credentialType
                };
                context.Requirements.Add(requirement);
                await context.SaveChangesAsync();
                Log.Information($"Created credential requirement: {name}");
            }
            else
            {
                Log.Debug($"Credential requirement {name} already exists.");
            }
        }
    }
}
