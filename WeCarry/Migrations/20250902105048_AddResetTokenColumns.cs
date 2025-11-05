using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeCarry.Migrations
{
    public partial class AddResetTokenColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResetToken",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetTokenExpires",
                table: "User",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetToken",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ResetTokenExpires",
                table: "User");
        }
    }
}
