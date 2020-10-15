using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyrica.Data.Migrations
{
    public partial class UserBirthday : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasYear",
                table: "Users",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HasYear",
                table: "Users");
        }
    }
}
