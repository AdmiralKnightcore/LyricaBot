using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyrica.Data.Migrations
{
    public partial class AddMainSave : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ActiveGenshinAccountId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActiveSaveId",
                table: "Users",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveGenshinAccountId",
                table: "Users",
                column: "ActiveGenshinAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ActiveSaveId",
                table: "Users",
                column: "ActiveSaveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_GenshinAccount_ActiveGenshinAccountId",
                table: "Users",
                column: "ActiveGenshinAccountId",
                principalTable: "GenshinAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Save_ActiveSaveId",
                table: "Users",
                column: "ActiveSaveId",
                principalTable: "Save",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_GenshinAccount_ActiveGenshinAccountId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Save_ActiveSaveId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveGenshinAccountId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ActiveSaveId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveGenshinAccountId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActiveSaveId",
                table: "Users");
        }
    }
}
