using IssuerApp.Data.Models;
using IssuerApp.Data.Models.OpenBadges;
using IssuerApp.Data.Models.Vc;
using IssuerApp.Extensions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace IssuerApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        /// <summary>
        /// Virtual organizations within the application (e.g. schools).
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Virtual members within an organization (e.g. students).
        /// </summary>
        public DbSet<Member> Members { get; set; }


        #region Open Badges Entities for CRUD operations

        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<AchievementCredential> AchievementCredentials { get; set; }
        public DbSet<AchievementSubject> AchievementSubjects { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Criteria> Criteria { get; set; }
        public DbSet<IdentifierEntry> IdentifierEntries { get; set; }
        public DbSet<IdentityObject> IdentityObjects { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        #endregion

        #region VerifiableCredentials Entities for CRUD operations

        public DbSet<TermsOfUse> TermsOfUse { get; set; }
        public DbSet<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>> VerifiableCredentials { get; set; }

        #endregion

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (builder == null) return;

            #region Application Entities

            builder.Entity<Organization>()
                .HasOne(x => x.Profile)
                .WithOne()
                .HasForeignKey<Organization>(o => o.ProfileKey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Organization>()
                .HasMany(o => o.Members)
                .WithOne(r => r.Organization)
                .HasForeignKey(r => r.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Organization>()
                .HasMany(o => o.Achievements)
                .WithOne(a => a.Organization)
                .HasForeignKey(a => a.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Organization>()
                .HasMany(o => o.AchievementCredentials)
                .WithOne(ac => ac.Organization)
                .HasForeignKey(ac => ac.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // Auto-include Profile when querying Organizations
            builder.Entity<Organization>()
                .Navigation(o => o.Profile)
                .AutoInclude();

            // Delete behavior

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.SelectedOrganization)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            #endregion

            #region Open Badges Entities

            // Ignore value object types (non-entities)
            builder.Ignore<Library.Models.OpenBadges.AchievementType>();
            builder.Ignore<Library.Models.OpenBadges.AlignmentTargetType>();
            builder.Ignore<Library.Models.OpenBadges.EndorsementCredential>();
            builder.Ignore<Library.Models.OpenBadges.GeoCoordinates>();
            builder.Ignore<Library.Models.OpenBadges.IdentifierType>();
            builder.Ignore<Library.Models.OpenBadges.ResultType>();
            builder.Ignore<Dictionary<string, object>>();

            // Ignore Library base entity types - only use IssuerApp extended types in the model
            builder.Ignore<Library.Models.OpenBadges.Achievement>();
            builder.Ignore<Library.Models.OpenBadges.AchievementCredential>();
            builder.Ignore<Library.Models.OpenBadges.AchievementSubject>();
            builder.Ignore<Library.Models.OpenBadges.Address>();
            builder.Ignore<Library.Models.OpenBadges.Alignment>();
            builder.Ignore<Library.Models.OpenBadges.Criteria>();
            builder.Ignore<Library.Models.OpenBadges.IdentifierEntry>();
            builder.Ignore<Library.Models.OpenBadges.IdentityObject>();
            builder.Ignore<Library.Models.OpenBadges.Image>();
            builder.Ignore<Library.Models.OpenBadges.Profile>();
            builder.Ignore<Library.Models.OpenBadges.Related>();
            builder.Ignore<Library.Models.OpenBadges.Result>();
            builder.Ignore<Library.Models.OpenBadges.ResultDescription>();
            builder.Ignore<Library.Models.OpenBadges.RubricCriterionLevel>();
            builder.Ignore<Library.Models.Vc.CredentialSubject>();
            builder.Ignore<Library.Models.Vc.Evidence>();
            builder.Ignore<Library.Models.Vc.Issuer>();
            builder.Ignore<Library.Models.Vc.TermsOfUse>();
            builder.Ignore<Library.Models.Vc.VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>();

            // Explicitly configure primary keys for IssuerApp entity types
            builder.Entity<Achievement>()
                .HasKey(a => a.AchievementKey);

            builder.Entity<AchievementCredential>()
                .HasKey(ac => ac.AchievementCredentialKey);

            builder.Entity<AchievementSubject>()
                .HasKey(s => s.AchievementSubjectKey);

            builder.Entity<Address>()
                .HasKey(a => a.AddressKey);

            builder.Entity<Criteria>()
                .HasKey(c => c.CriteriaKey);

            builder.Entity<IdentifierEntry>()
                .HasKey(i => i.IdentifierEntryKey);

            builder.Entity<IdentityObject>()
                .HasKey(i => i.IdentityObjectKey);

            builder.Entity<Image>()
                .HasKey(i => i.ImageKey);

            builder.Entity<Profile>()
                .HasKey(p => p.ProfileKey);

            builder.Entity<TermsOfUse>()
                .HasKey(t => t.TermsOfUseKey);

            // Property conversions and configurations

            builder.Entity<Achievement>()
                .Property(x => x.AchievementType)
                .HasConversion(
                    v => v!.Type,
                    v => new Library.Models.OpenBadges.AchievementType { Type = v }
                    );

            builder.Entity<Achievement>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<Achievement>()
                .Property(e => e.Endorsement)
                .HasJsonConversion();

            builder.Entity<Achievement>()
                .Property(e => e.EndorsementJwt)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.ConfidenceMethod)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.Context)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.Issuer)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.RenderMethod)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.Type)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.Endorsement)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.EndorsementJwt)
                .HasJsonConversion();

            builder.Entity<AchievementCredential>()
                .Property(e => e.CredentialStatus)
                .HasJsonConversion();

            builder.Entity<AchievementSubject>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<Address>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<Address>()
                .Property(x => x.Type)
                .HasJsonConversion();

            builder.Entity<Address>()
                .Property(x => x.Geo)
                .HasJsonConversion();

            builder.Entity<Criteria>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<Profile>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<Profile>()
                .Property(e => e.Endorsement)
                .HasJsonConversion();

            builder.Entity<Profile>()
                .Property(e => e.EndorsementJwt)
                .HasJsonConversion();

            builder.Entity<TermsOfUse>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(e => e.AdditionalProperties)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.ConfidenceMethod)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.RenderMethod)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.Context)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.Type)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.CredentialSubject)
                .HasJsonConversion();

            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .Property(x => x.Issuer)
                .HasJsonConversion();

            // OnDelete behavior

            // Explicit foreign key configurations to avoid shadow properties

            // Address -> Organization relationship
            builder.Entity<Address>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // Criteria -> Organization relationship
            builder.Entity<Criteria>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // Achievement -> Criteria one-to-one relationship
            builder.Entity<Achievement>()
                .HasOne(a => a.Criteria)
                .WithOne(c => c.Achievement)
                .HasForeignKey<Criteria>(c => c.AchievementKey)
                .OnDelete(DeleteBehavior.Cascade);

            // IdentifierEntry -> Organization relationship
            builder.Entity<IdentifierEntry>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // IdentityObject -> Organization relationship
            builder.Entity<IdentityObject>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // Image -> Organization relationship
            builder.Entity<Image>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // Achievement -> Image one-to-one relationship
            builder.Entity<Achievement>()
                .HasOne(a => a.Image)
                .WithOne(i => i.Achievement)
                .HasForeignKey<Image>(i => i.AchievementKey)
                .OnDelete(DeleteBehavior.Cascade);

            // AchievementCredential -> Image one-to-one relationship
            builder.Entity<AchievementCredential>()
                .HasOne(ac => ac.Image)
                .WithOne(i => i.AchievementCredential)
                .HasForeignKey<Image>(i => i.AchievementCredentialKey)
                .OnDelete(DeleteBehavior.Cascade);

            // AchievementSubject -> Achievement relationship
            builder.Entity<AchievementSubject>()
                .HasOne(s => s.Achievement)
                .WithMany()
                .HasForeignKey(s => s.AchievementKey)
                .OnDelete(DeleteBehavior.SetNull);

            // AchievementSubject -> Source profile relationship
            builder.Entity<AchievementSubject>()
                .HasOne(s => s.Source)
                .WithMany()
                .HasForeignKey(s => s.SourceProfileKey)
                .HasPrincipalKey(p => p.ProfileKey)
                .OnDelete(DeleteBehavior.SetNull);

            // AchievementSubject -> Member relationship
            builder.Entity<AchievementSubject>()
                .HasOne(s => s.Member)
                .WithMany()
                .HasForeignKey(s => s.MemberKey)
                .OnDelete(DeleteBehavior.SetNull);

            // TermsOfUse -> Organization relationship
            builder.Entity<TermsOfUse>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            // VerifiableCredential -> Organization relationship
            builder.Entity<VerifiableCredential<Library.Models.Vc.CredentialSubject, Library.Models.Vc.Issuer>>()
                .HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationKey)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion
        }
    }
}

