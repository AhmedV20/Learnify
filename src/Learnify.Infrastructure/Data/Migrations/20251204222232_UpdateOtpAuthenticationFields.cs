using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learnify.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpAuthenticationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmailVerificationTokenHash",
                table: "AspNetUsers",
                newName: "PasswordResetTokenHash");

            migrationBuilder.RenameColumn(
                name: "EmailVerificationTokenExpiry",
                table: "AspNetUsers",
                newName: "PasswordResetTokenExpiry");

            migrationBuilder.AddColumn<int>(
                name: "EmailOtpAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordResetOtpAttempts",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetOtpHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOtpAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailOtpExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailOtpHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpHash",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PasswordResetTokenHash",
                table: "AspNetUsers",
                newName: "EmailVerificationTokenHash");

            migrationBuilder.RenameColumn(
                name: "PasswordResetTokenExpiry",
                table: "AspNetUsers",
                newName: "EmailVerificationTokenExpiry");
        }
    }
}
