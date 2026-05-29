using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WalletApp.Data.Models;

namespace WalletApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        /// <summary>
        /// The holders defined by each ImsOrg.
        /// </summary>
        public DbSet<Holder> Holders { get; set; }

        /// <summary>
        /// Credentials received by holders.
        /// </summary>
        public DbSet<HolderCredential> HolderCredentials { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            base.OnConfiguring(builder);

            builder.ConfigureWarnings(action =>
            {
                action.Log(CoreEventId.LazyLoadOnDisposedContextWarning);
            });
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            if (builder == null) return;

            #region Application Entities

            builder.Entity<ApplicationUser>()
                .HasOne(x => x.SelectedHolder)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<HolderCredential>()
                .HasOne(x => x.Holder)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

        }
    }
}