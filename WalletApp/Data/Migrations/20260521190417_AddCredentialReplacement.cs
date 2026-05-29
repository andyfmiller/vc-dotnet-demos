using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalletApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCredentialReplacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousCredentialJson",
                table: "HolderCredentials",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReplacedAt",
                table: "HolderCredentials",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousCredentialJson",
                table: "HolderCredentials");

            migrationBuilder.DropColumn(
                name: "ReplacedAt",
                table: "HolderCredentials");
        }
    }
}
