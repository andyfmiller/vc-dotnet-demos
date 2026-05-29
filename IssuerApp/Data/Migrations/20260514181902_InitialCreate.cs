using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssuerApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                    Xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AchievementCredentials",
                columns: table => new
                {
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    CredentialSubjectAchievementSubjectKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Context = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Issuer = table.Column<string>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ConfidenceMethod = table.Column<string>(type: "TEXT", nullable: true),
                    RenderMethod = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false),
                    AwardedDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Endorsement = table.Column<string>(type: "TEXT", nullable: false),
                    EndorsementJwt = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementCredentials", x => x.AchievementCredentialKey);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: false),
                    AchievementType = table.Column<string>(type: "TEXT", nullable: true),
                    CreditsAvailable = table.Column<double>(type: "REAL", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Endorsement = table.Column<string>(type: "TEXT", nullable: false),
                    EndorsementJwt = table.Column<string>(type: "TEXT", nullable: false),
                    FieldOfStudy = table.Column<string>(type: "TEXT", nullable: true),
                    HumanCode = table.Column<string>(type: "TEXT", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    InLanguage = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Specialization = table.Column<string>(type: "TEXT", nullable: true),
                    Tag = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementKey);
                });

            migrationBuilder.CreateTable(
                name: "AchievementSubjects",
                columns: table => new
                {
                    AchievementSubjectKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementKey = table.Column<int>(type: "INTEGER", nullable: true),
                    SourceProfileKey = table.Column<int>(type: "INTEGER", nullable: true),
                    MemberKey = table.Column<int>(type: "INTEGER", nullable: true),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false),
                    ActivityEndDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    ActivityStartDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CreditsEarned = table.Column<double>(type: "REAL", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    LicenseNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Narrative = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true),
                    Term = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementSubjects", x => x.AchievementSubjectKey);
                    table.ForeignKey(
                        name: "FK_AchievementSubjects_Achievements_AchievementKey",
                        column: x => x.AchievementKey,
                        principalTable: "Achievements",
                        principalColumn: "AchievementKey",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    AddressCountry = table.Column<string>(type: "TEXT", nullable: true),
                    AddressCountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    AddressLocality = table.Column<string>(type: "TEXT", nullable: true),
                    AddressRegion = table.Column<string>(type: "TEXT", nullable: true),
                    Geo = table.Column<string>(type: "TEXT", nullable: true),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: true),
                    PostOfficeBoxNumber = table.Column<string>(type: "TEXT", nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressKey);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SelectedOrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    SelectedMemberKey = table.Column<int>(type: "INTEGER", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CredentialSchema",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: true),
                    VerifiableCredentialKey = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialSchema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredentialSchema_AchievementCredentials_AchievementCredentialKey",
                        column: x => x.AchievementCredentialKey,
                        principalTable: "AchievementCredentials",
                        principalColumn: "AchievementCredentialKey");
                });

            migrationBuilder.CreateTable(
                name: "CredentialStatus",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: true),
                    VerifiableCredentialKey = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CredentialStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CredentialStatus_AchievementCredentials_AchievementCredentialKey",
                        column: x => x.AchievementCredentialKey,
                        principalTable: "AchievementCredentials",
                        principalColumn: "AchievementCredentialKey");
                });

            migrationBuilder.CreateTable(
                name: "Criteria",
                columns: table => new
                {
                    CriteriaKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementKey = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Narrative = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Criteria", x => x.CriteriaKey);
                    table.ForeignKey(
                        name: "FK_Criteria_Achievements_AchievementKey",
                        column: x => x.AchievementKey,
                        principalTable: "Achievements",
                        principalColumn: "AchievementKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataIntegrityProof",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    ProofPurpose = table.Column<string>(type: "TEXT", nullable: false),
                    VerificationMethod = table.Column<string>(type: "TEXT", nullable: true),
                    Cryptosuite = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    Challenge = table.Column<string>(type: "TEXT", nullable: true),
                    ProofValue = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousProof = table.Column<string>(type: "TEXT", nullable: true),
                    Nonce = table.Column<string>(type: "TEXT", nullable: true),
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: true),
                    VerifiableCredentialKey = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataIntegrityProof", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataIntegrityProof_AchievementCredentials_AchievementCredentialKey",
                        column: x => x.AchievementCredentialKey,
                        principalTable: "AchievementCredentials",
                        principalColumn: "AchievementCredentialKey");
                });

            migrationBuilder.CreateTable(
                name: "IdentifierEntries",
                columns: table => new
                {
                    IdentifierEntryKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierEntries", x => x.IdentifierEntryKey);
                });

            migrationBuilder.CreateTable(
                name: "IdentityObjects",
                columns: table => new
                {
                    IdentityObjectKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Salt = table.Column<string>(type: "TEXT", nullable: true),
                    IdentityHash = table.Column<string>(type: "TEXT", nullable: false),
                    Hashed = table.Column<bool>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityObjects", x => x.IdentityObjectKey);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementKey = table.Column<int>(type: "INTEGER", nullable: true),
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Caption = table.Column<string>(type: "TEXT", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageKey);
                    table.ForeignKey(
                        name: "FK_Images_AchievementCredentials_AchievementCredentialKey",
                        column: x => x.AchievementCredentialKey,
                        principalTable: "AchievementCredentials",
                        principalColumn: "AchievementCredentialKey",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Images_Achievements_AchievementKey",
                        column: x => x.AchievementKey,
                        principalTable: "Achievements",
                        principalColumn: "AchievementKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    ProfileKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddressKey = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalName = table.Column<string>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Endorsement = table.Column<string>(type: "TEXT", nullable: false),
                    EndorsementJwt = table.Column<string>(type: "TEXT", nullable: false),
                    FamilyName = table.Column<string>(type: "TEXT", nullable: true),
                    FamilyNamePrefix = table.Column<string>(type: "TEXT", nullable: true),
                    GivenName = table.Column<string>(type: "TEXT", nullable: true),
                    HonorificPrefix = table.Column<string>(type: "TEXT", nullable: true),
                    HonorificSuffix = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Official = table.Column<string>(type: "TEXT", nullable: true),
                    PatronymicName = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.ProfileKey);
                    table.ForeignKey(
                        name: "FK_Profiles_Addresses_AddressKey",
                        column: x => x.AddressKey,
                        principalTable: "Addresses",
                        principalColumn: "AddressKey");
                    table.ForeignKey(
                        name: "FK_Profiles_Images_ImageKey",
                        column: x => x.ImageKey,
                        principalTable: "Images",
                        principalColumn: "ImageKey");
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProfileKey = table.Column<int>(type: "INTEGER", nullable: false),
                    SigningPublicKeyMultibase = table.Column<string>(type: "TEXT", nullable: true),
                    SigningPrivateKeyBase64 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationKey);
                    table.ForeignKey(
                        name: "FK_Organizations_Profiles_ProfileKey",
                        column: x => x.ProfileKey,
                        principalTable: "Profiles",
                        principalColumn: "ProfileKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberKey);
                    table.ForeignKey(
                        name: "FK_Members_Organizations_OrganizationKey",
                        column: x => x.OrganizationKey,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermsOfUse",
                columns: table => new
                {
                    TermsOfUseKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsOfUse", x => x.TermsOfUseKey);
                    table.ForeignKey(
                        name: "FK_TermsOfUse_Organizations_OrganizationKey",
                        column: x => x.OrganizationKey,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerifiableCredentials",
                columns: table => new
                {
                    VerifiableCredentialKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationKey = table.Column<int>(type: "INTEGER", nullable: true),
                    Context = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Issuer = table.Column<string>(type: "TEXT", nullable: false),
                    ValidFrom = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    CredentialSubject = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ConfidenceMethod = table.Column<string>(type: "TEXT", nullable: true),
                    RenderMethod = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalProperties = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifiableCredentials", x => x.VerifiableCredentialKey);
                    table.ForeignKey(
                        name: "FK_VerifiableCredentials_Organizations_OrganizationKey",
                        column: x => x.OrganizationKey,
                        principalTable: "Organizations",
                        principalColumn: "OrganizationKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    AchievementCredentialKey = table.Column<int>(type: "INTEGER", nullable: true),
                    VerifiableCredentialKey = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshService_AchievementCredentials_AchievementCredentialKey",
                        column: x => x.AchievementCredentialKey,
                        principalTable: "AchievementCredentials",
                        principalColumn: "AchievementCredentialKey");
                    table.ForeignKey(
                        name: "FK_RefreshService_VerifiableCredentials_VerifiableCredentialKey",
                        column: x => x.VerifiableCredentialKey,
                        principalTable: "VerifiableCredentials",
                        principalColumn: "VerifiableCredentialKey");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementCredentials_CredentialSubjectAchievementSubjectKey",
                table: "AchievementCredentials",
                column: "CredentialSubjectAchievementSubjectKey");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementCredentials_OrganizationKey",
                table: "AchievementCredentials",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_OrganizationKey",
                table: "Achievements",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementSubjects_AchievementKey",
                table: "AchievementSubjects",
                column: "AchievementKey");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementSubjects_MemberKey",
                table: "AchievementSubjects",
                column: "MemberKey");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementSubjects_SourceProfileKey",
                table: "AchievementSubjects",
                column: "SourceProfileKey");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_OrganizationKey",
                table: "Addresses",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SelectedMemberKey",
                table: "AspNetUsers",
                column: "SelectedMemberKey");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SelectedOrganizationKey",
                table: "AspNetUsers",
                column: "SelectedOrganizationKey");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CredentialSchema_AchievementCredentialKey",
                table: "CredentialSchema",
                column: "AchievementCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialSchema_VerifiableCredentialKey",
                table: "CredentialSchema",
                column: "VerifiableCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialStatus_AchievementCredentialKey",
                table: "CredentialStatus",
                column: "AchievementCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_CredentialStatus_VerifiableCredentialKey",
                table: "CredentialStatus",
                column: "VerifiableCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_AchievementKey",
                table: "Criteria",
                column: "AchievementKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Criteria_OrganizationKey",
                table: "Criteria",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_DataIntegrityProof_AchievementCredentialKey",
                table: "DataIntegrityProof",
                column: "AchievementCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_DataIntegrityProof_VerifiableCredentialKey",
                table: "DataIntegrityProof",
                column: "VerifiableCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_IdentifierEntries_OrganizationKey",
                table: "IdentifierEntries",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityObjects_OrganizationKey",
                table: "IdentityObjects",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Images_AchievementCredentialKey",
                table: "Images",
                column: "AchievementCredentialKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_AchievementKey",
                table: "Images",
                column: "AchievementKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_OrganizationKey",
                table: "Images",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Members_OrganizationKey",
                table: "Members",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_ProfileKey",
                table: "Organizations",
                column: "ProfileKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AddressKey",
                table: "Profiles",
                column: "AddressKey");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_ImageKey",
                table: "Profiles",
                column: "ImageKey");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshService_AchievementCredentialKey",
                table: "RefreshService",
                column: "AchievementCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshService_VerifiableCredentialKey",
                table: "RefreshService",
                column: "VerifiableCredentialKey");

            migrationBuilder.CreateIndex(
                name: "IX_TermsOfUse_OrganizationKey",
                table: "TermsOfUse",
                column: "OrganizationKey");

            migrationBuilder.CreateIndex(
                name: "IX_VerifiableCredentials_OrganizationKey",
                table: "VerifiableCredentials",
                column: "OrganizationKey");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementCredentials_AchievementSubjects_CredentialSubjectAchievementSubjectKey",
                table: "AchievementCredentials",
                column: "CredentialSubjectAchievementSubjectKey",
                principalTable: "AchievementSubjects",
                principalColumn: "AchievementSubjectKey");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementCredentials_Organizations_OrganizationKey",
                table: "AchievementCredentials",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Achievements_Organizations_OrganizationKey",
                table: "Achievements",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementSubjects_Members_MemberKey",
                table: "AchievementSubjects",
                column: "MemberKey",
                principalTable: "Members",
                principalColumn: "MemberKey",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementSubjects_Profiles_SourceProfileKey",
                table: "AchievementSubjects",
                column: "SourceProfileKey",
                principalTable: "Profiles",
                principalColumn: "ProfileKey",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Organizations_OrganizationKey",
                table: "Addresses",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                table: "AspNetUserClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                table: "AspNetUserLogins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                table: "AspNetUserRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Members_SelectedMemberKey",
                table: "AspNetUsers",
                column: "SelectedMemberKey",
                principalTable: "Members",
                principalColumn: "MemberKey");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations_SelectedOrganizationKey",
                table: "AspNetUsers",
                column: "SelectedOrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CredentialSchema_VerifiableCredentials_VerifiableCredentialKey",
                table: "CredentialSchema",
                column: "VerifiableCredentialKey",
                principalTable: "VerifiableCredentials",
                principalColumn: "VerifiableCredentialKey");

            migrationBuilder.AddForeignKey(
                name: "FK_CredentialStatus_VerifiableCredentials_VerifiableCredentialKey",
                table: "CredentialStatus",
                column: "VerifiableCredentialKey",
                principalTable: "VerifiableCredentials",
                principalColumn: "VerifiableCredentialKey");

            migrationBuilder.AddForeignKey(
                name: "FK_Criteria_Organizations_OrganizationKey",
                table: "Criteria",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataIntegrityProof_VerifiableCredentials_VerifiableCredentialKey",
                table: "DataIntegrityProof",
                column: "VerifiableCredentialKey",
                principalTable: "VerifiableCredentials",
                principalColumn: "VerifiableCredentialKey");

            migrationBuilder.AddForeignKey(
                name: "FK_IdentifierEntries_Organizations_OrganizationKey",
                table: "IdentifierEntries",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityObjects_Organizations_OrganizationKey",
                table: "IdentityObjects",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Organizations_OrganizationKey",
                table: "Images",
                column: "OrganizationKey",
                principalTable: "Organizations",
                principalColumn: "OrganizationKey",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementCredentials_AchievementSubjects_CredentialSubjectAchievementSubjectKey",
                table: "AchievementCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_AchievementCredentials_Organizations_OrganizationKey",
                table: "AchievementCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_Achievements_Organizations_OrganizationKey",
                table: "Achievements");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Organizations_OrganizationKey",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Images_Organizations_OrganizationKey",
                table: "Images");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CredentialSchema");

            migrationBuilder.DropTable(
                name: "CredentialStatus");

            migrationBuilder.DropTable(
                name: "Criteria");

            migrationBuilder.DropTable(
                name: "DataIntegrityProof");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "IdentifierEntries");

            migrationBuilder.DropTable(
                name: "IdentityObjects");

            migrationBuilder.DropTable(
                name: "RefreshService");

            migrationBuilder.DropTable(
                name: "TermsOfUse");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "VerifiableCredentials");

            migrationBuilder.DropTable(
                name: "AchievementSubjects");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "AchievementCredentials");

            migrationBuilder.DropTable(
                name: "Achievements");
        }
    }
}
