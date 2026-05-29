using Library.Crypto;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WalletApp.Data;
using WalletApp.Data.Models;

namespace WalletApp
{
    public static class SeedData
    {
        public static async Task EnsureUserSeedData(string connectionString, string didWebHost)
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
                    Log.Information("Running database migrations...");
                    await context.Database.MigrateAsync();
                    Log.Information("Database migrations completed.");

                    await EnsureSeedData(serviceScope.ServiceProvider, didWebHost);
                    Log.Information("Seed data insertion completed.");
                    SqliteConnection.ClearAllPools();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Error during database seeding. Exception: {Message}", ex.Message);
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

        private static async Task EnsureSeedData(IServiceProvider serviceProvider, string didWebHost)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Encode the host per did:web spec (colons → %3A)
            var encodedHost = didWebHost.Replace(":", "%3A");
            string HolderDid(string slug) => $"did:web:{encodedHost}:holders:{slug}";

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

            // Seed Holders
            // did:web identifiers: did:web:{encodedHost}:holders:{slug}
            // served at: https://{host}/holders/{slug}/did.json
            await CreateHolderIfNotExists(context,
               holderId: HolderDid("fc499133-5d2a-4947-8f9e-9919a92b116c"),
               name: "Alex Trumble");
            await CreateHolderIfNotExists(context,
               holderId: HolderDid("bc65c2bd-2d73-42bc-bb22-02945dc5ffe7"),
               name: "Heather Creston");

            // Seed Admin User
            await CreateUserIfNotExists(
                userManager,
                context,
                email: "demouser@example.com",
                userName: "DemoUser",
                role: Constants.Roles.Admin,
                selectedHolderId: HolderDid("fc499133-5d2a-4947-8f9e-9919a92b116c"));

            Log.Information("Holders and user seeding complete.");
        }

        private static async Task CreateHolderIfNotExists(
            ApplicationDbContext context,
            string holderId,
            string name)
        {
            var existingHolder = await context.Holders
                .FirstOrDefaultAsync(r => r.Id == holderId);

            if (existingHolder == null)
            {
                var signingService = new Ed25519SigningService();
                var (publicKeyMultibase, privateKeyBase64) = signingService.GenerateKeyPair();

                var holder = new Holder
                {
                    Id = holderId,
                    Name = name,
                    SigningPublicKeyMultibase = publicKeyMultibase,
                    SigningPrivateKeyBase64 = privateKeyBase64
                };
                context.Holders.Add(holder);
                try
                {
                    await context.SaveChangesAsync();
                    Log.Information($"Created holder: {name}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to create holder: {name}. Exception: {ex.Message}");
                    var innerEx = ex.InnerException;
                    while (innerEx != null)
                    {
                        Log.Error("Inner exception: {InnerMessage}", innerEx.Message);
                        innerEx = innerEx.InnerException;
                    }
                    throw;
                }
            }
            else
            {
                Log.Debug($"Holder {name} already exists.");
            }
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string userName,
            string role,
            string selectedHolderId)
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

                if (!string.IsNullOrEmpty(selectedHolderId))
                {
                    user.SelectedHolder = await context.Holders
                        .Where(h => h.Id == selectedHolderId)
                        .FirstOrDefaultAsync() ?? user.SelectedHolder;
                }

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
    }
}
