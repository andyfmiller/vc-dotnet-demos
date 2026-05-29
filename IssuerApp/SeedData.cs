using IssuerApp.Data;
using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IssuerApp
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
                    try
                    {
                        await context.Database.MigrateAsync();
                    }
                    catch (SqliteException ex) when (ex.Message.Contains("already exists"))
                    {
                        Log.Warning("Tables already exist, skipping migrations. Consider deleting {DbFile} if schema changes are needed.", "IssuerApp.db");
                    }
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
            string OrgDid(string slug) => $"did:web:{encodedHost}:organizations:{slug}";

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

            // Seed Organizations
            // did:web identifiers: did:web:{encodedHost}:organizations:{slug}
            // served at: https://{host}/organizations/{slug}/did.json
            await CreateOrganizationIfNotExists(context, "Sample School", OrgDid("507608a4-7c3a-4bbc-ad24-8d38a2644a75"));
            await CreateOrganizationIfNotExists(context, "Sample Organization", OrgDid("f4601ce1-3cc4-498d-85b0-4afb19d0012e"));
            await CreateOrganizationIfNotExists(context, "State Health Authority", OrgDid("29881644-52e9-43f1-9069-2fd9182bc7d6"));

            // Seed Members
            await CreateMemberIfNotExists(context, "Alex Trumble", OrgDid("507608a4-7c3a-4bbc-ad24-8d38a2644a75"));
            await CreateMemberIfNotExists(context, "Heather Creston", OrgDid("29881644-52e9-43f1-9069-2fd9182bc7d6"));

            // Seed Achievements
            await CreateAchievementIfNotExists(context,
                id: "urn:uuid:1364f157-9e71-48cb-aaaa-32b926296c5d",
                organizationId: OrgDid("507608a4-7c3a-4bbc-ad24-8d38a2644a75"),
                name: "Agile Project Management Course",
                achievementType: "Course",
                description: "This course teacher agile project management methods that can be applied to a wide variety of business areas.",
                fieldOfStudy: "Project Management",
                humanCode: "APM-001",
                specialization: "Agile Project Management",
                criteriaNarrative: "To be awarded this achievement, the subject must attend 90% or more of lectures, complete all homework, and pass all exams with a score of 80% or better.");

            await CreateAchievementIfNotExists(context,
                id: "urn:uuid:d271f4d3-3b57-4664-b0a0-7b217ac664a7",
                organizationId: OrgDid("29881644-52e9-43f1-9069-2fd9182bc7d6"),
                name: "Board of Cosmetology - Barber - Certification",
                achievementType: "Certification",
                description: "Board-approved curriculum includes: Barbering, State Laws and Rules, Career Development",
                fieldOfStudy: "Cosmetology",
                humanCode: null,
                specialization: "Barber",
                criteriaNarrative: "Applicant must provide a transcript showing proof of hours and passing score of a board-approved practical examination");

            // Seed AchievementCredentials
            await CreateAchievementCredentialIfNotExists(context,
                id: "urn:uuid:cc5b19ac-8b47-4f7d-9152-938859586008",
                name: "Agile Project Management",
                description: "Alex Trumble completed the Agile Project Management Course.",
                organizationId: OrgDid("507608a4-7c3a-4bbc-ad24-8d38a2644a75"),
                achievementId: "urn:uuid:1364f157-9e71-48cb-aaaa-32b926296c5d",
                memberName: "Alex Trumble",
                sourceOrganizationId: OrgDid("f4601ce1-3cc4-498d-85b0-4afb19d0012e"),
                licenseNumber: null,
                validFrom: new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)),
                validUntil: new DateTimeOffset(2031, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)),
                awardedDate: new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)));

            await CreateAchievementCredentialIfNotExists(context,
                id: "urn:uuid:0b2e4092-e42c-4c36-89ca-2ea16662fb52",
                name: "Barber Certification",
                description: "Heather Creston was certified by the State Health Authority to practice Barbering.",
                organizationId: OrgDid("29881644-52e9-43f1-9069-2fd9182bc7d6"),
                achievementId: "urn:uuid:d271f4d3-3b57-4664-b0a0-7b217ac664a7",
                memberName: "Heather Creston",
                sourceOrganizationId: null,
                licenseNumber: "boc-b-746465",
                validFrom: new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)),
                validUntil: new DateTimeOffset(2031, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)),
                awardedDate: new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.FromHours(-7)));

            // Seed Admin User
            await CreateUserIfNotExists(
                userManager,
                context,
                email: "demouser@example.com",
                userName: "DemoUser",
                role: Constants.Roles.Admin,
                selectedOrganization: OrgDid("507608a4-7c3a-4bbc-ad24-8d38a2644a75"));

            Log.Information("Organizations, members, and user seeding complete.");
        }

        private static async Task CreateOrganizationIfNotExists(
            ApplicationDbContext context,
            string name,
            string id)
        {
            var existingOrg = await context.Organizations
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(o => o.Profile!.Id == id);

            if (existingOrg == null)
            {
                var (publicKeyMultibase, privateKeyBase64) = new Ed25519SigningService().GenerateKeyPair();
                var organization = new Organization
                {
                    Profile = new Profile
                    {
                        Id = id,
                        Name = name,
                        Type = new List<string> { "Profile" },
                        AdditionalProperties = new Dictionary<string, object>()
                    },
                    SigningPublicKeyMultibase = publicKeyMultibase,
                    SigningPrivateKeyBase64 = privateKeyBase64
                };

                context.Organizations.Add(organization);
                try
                {
                    await context.SaveChangesAsync();
                    Log.Information($"Created organization: {name}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to create organization: {name}. Exception: {ex.Message}");
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
                Log.Debug($"Organization {name} already exists.");
            }
        }

        private static async Task CreateMemberIfNotExists(
            ApplicationDbContext context,
            string name,
            string organizationId)
        {
            var existingMember = await context.Members
                .FirstOrDefaultAsync(r => r.Name == name);
            if (existingMember == null)
            {
                var member = new Member
                {
                    Name = name
                };
                if (!string.IsNullOrEmpty(organizationId))
                {
                    var organization = await context.Organizations
                        .Include(o => o.Profile)
                        .FirstOrDefaultAsync(o => o.Profile != null && o.Profile.Id == organizationId);
                    if (organization != null)
                    {
                        member.Organization = organization;
                    }
                    else
                    {
                        Log.Warning($"Organization with id {organizationId} not found for member {name}.");
                    }
                }
                context.Members.Add(member);
                try
                {
                    await context.SaveChangesAsync();
                    Log.Information($"Created member: {name}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to create member: {name}. Exception: {ex.Message}");
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
                Log.Debug($"Member {name} already exists.");
            }
        }

        private static async Task CreateAchievementIfNotExists(
            ApplicationDbContext context,
            string id,
            string organizationId,
            string name,
            string achievementType,
            string description,
            string? fieldOfStudy,
            string? humanCode,
            string? specialization,
            string criteriaNarrative)
        {
            var existing = await context.Achievements.FirstOrDefaultAsync(a => a.Id == id);
            if (existing != null)
            {
                Log.Debug($"Achievement {name} already exists.");
                return;
            }

            var organization = await context.Organizations
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(o => o.Profile != null && o.Profile.Id == organizationId);

            if (organization == null)
            {
                Log.Warning($"Organization {organizationId} not found for achievement {name}.");
                return;
            }

            var achievement = new Achievement
            {
                Id = id,
                Name = name,
                AchievementType = achievementType != null ? Library.Models.OpenBadges.AchievementType.FromType(achievementType) : null,
                Description = description,
                FieldOfStudy = fieldOfStudy,
                HumanCode = humanCode,
                Specialization = specialization,
                Type = new List<string> { "Achievement" },
                AdditionalProperties = new Dictionary<string, object>(),
                Organization = organization,
                Criteria = new Criteria
                {
                    Narrative = criteriaNarrative,
                    AdditionalProperties = new Dictionary<string, object>()
                }
            };

            context.Achievements.Add(achievement);
            try
            {
                await context.SaveChangesAsync();
                Log.Information($"Created achievement: {name}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to create achievement: {name}. Exception: {ex.Message}");
                throw;
            }
        }

        private static async Task CreateAchievementCredentialIfNotExists(
            ApplicationDbContext context,
            string id,
            string name,
            string description,
            string organizationId,
            string achievementId,
            string memberName,
            string? sourceOrganizationId,
            string? licenseNumber,
            DateTimeOffset validFrom,
            DateTimeOffset validUntil,
            DateTimeOffset awardedDate)
        {
            var existing = await context.AchievementCredentials.FirstOrDefaultAsync(ac => ac.Id == id);
            if (existing != null)
            {
                Log.Debug($"AchievementCredential {name} already exists.");
                return;
            }

            var organization = await context.Organizations
                .Include(o => o.Profile)
                .FirstOrDefaultAsync(o => o.Profile != null && o.Profile.Id == organizationId);

            if (organization == null)
            {
                Log.Warning($"Organization {organizationId} not found for credential {name}.");
                return;
            }

            var achievement = await context.Achievements
                .FirstOrDefaultAsync(a => a.Id == achievementId);

            if (achievement == null)
            {
                Log.Warning($"Achievement {achievementId} not found for credential {name}.");
                return;
            }

            var member = await context.Members.FirstOrDefaultAsync(m => m.Name == memberName);
            if (member == null)
            {
                Log.Warning($"Member {memberName} not found for credential {name}.");
                return;
            }

            Profile? sourceProfile = null;
            if (sourceOrganizationId != null)
            {
                var sourceOrg = await context.Organizations
                    .Include(o => o.Profile)
                    .FirstOrDefaultAsync(o => o.Profile != null && o.Profile.Id == sourceOrganizationId);
                sourceProfile = sourceOrg?.Profile;
            }

            var issuerProfile = organization.Profile!;
            var credential = new AchievementCredential
            {
                Id = id,
                Name = name,
                Description = description,
                Organization = organization,
                Issuer = issuerProfile,
                ValidFrom = validFrom,
                ValidUntil = validUntil,
                AwardedDate = awardedDate,
                AdditionalProperties = new Dictionary<string, object>(),
                CredentialSubject = new AchievementSubject
                {
                    Achievement = achievement,
                    Member = member,
                    Source = sourceProfile,
                    LicenseNumber = licenseNumber,
                    AdditionalProperties = new Dictionary<string, object>(),
                    Type = new List<string>()
                }
            };

            context.AchievementCredentials.Add(credential);
            try
            {
                await context.SaveChangesAsync();
                Log.Information($"Created achievement credential: {name}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to create achievement credential: {name}. Exception: {ex.Message}");
                throw;
            }
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string userName,
            string role,
            string selectedOrganization)
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

                if (!string.IsNullOrEmpty(selectedOrganization))
                {
                    user.SelectedOrganization = await context.Organizations
                        .Include(o => o.Profile)
                        .Where(o => o.Profile != null && o.Profile.Id == selectedOrganization)
                        .FirstOrDefaultAsync();
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