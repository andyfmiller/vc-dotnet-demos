using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IssuerApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCredentialStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CredentialStatus_AchievementCredentials_AchievementCredentialKey",
                table: "CredentialStatus");

            migrationBuilder.DropIndex(
                name: "IX_CredentialStatus_AchievementCredentialKey",
                table: "CredentialStatus");

            migrationBuilder.DropColumn(
                name: "AchievementCredentialKey",
                table: "CredentialStatus");

            migrationBuilder.AddColumn<string>(
                name: "CredentialStatus",
                table: "AchievementCredentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusListIndex",
                table: "AchievementCredentials",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CredentialStatus",
                table: "AchievementCredentials");

            migrationBuilder.DropColumn(
                name: "StatusListIndex",
                table: "AchievementCredentials");

            migrationBuilder.AddColumn<int>(
                name: "AchievementCredentialKey",
                table: "CredentialStatus",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CredentialStatus_AchievementCredentialKey",
                table: "CredentialStatus",
                column: "AchievementCredentialKey");

            migrationBuilder.AddForeignKey(
                name: "FK_CredentialStatus_AchievementCredentials_AchievementCredentialKey",
                table: "CredentialStatus",
                column: "AchievementCredentialKey",
                principalTable: "AchievementCredentials",
                principalColumn: "AchievementCredentialKey");
        }
    }
}
