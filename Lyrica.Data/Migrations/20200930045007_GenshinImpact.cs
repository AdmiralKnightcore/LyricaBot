using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyrica.Data.Migrations
{
    public partial class GenshinImpact : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stats_StatsId",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatsId",
                table: "Users",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "GenshinAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenshinAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenshinAccount_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssociatedAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    GenshinAccountId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociatedAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssociatedAccount_GenshinAccount_GenshinAccountId",
                        column: x => x.GenshinAccountId,
                        principalTable: "GenshinAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Save",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Region = table.Column<int>(type: "integer", nullable: false),
                    GenshinAccountId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Save", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Save_GenshinAccount_GenshinAccountId",
                        column: x => x.GenshinAccountId,
                        principalTable: "GenshinAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssociatedAccount_GenshinAccountId",
                table: "AssociatedAccount",
                column: "GenshinAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_GenshinAccount_UserId",
                table: "GenshinAccount",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Save_GenshinAccountId",
                table: "Save",
                column: "GenshinAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stats_StatsId",
                table: "Users",
                column: "StatsId",
                principalTable: "Stats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Stats_StatsId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AssociatedAccount");

            migrationBuilder.DropTable(
                name: "Save");

            migrationBuilder.DropTable(
                name: "GenshinAccount");

            migrationBuilder.AlterColumn<Guid>(
                name: "StatsId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Stats_StatsId",
                table: "Users",
                column: "StatsId",
                principalTable: "Stats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
