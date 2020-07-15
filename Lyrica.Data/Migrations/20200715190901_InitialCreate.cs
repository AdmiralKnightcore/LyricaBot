using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyrica.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Owner = table.Column<ulong>(type: "INTEGER", nullable: false),
                    LolaBlessGame = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rolls = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlessingResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    StatsId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlessingResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlessingResult_Stats_StatsId",
                        column: x => x.StatsId,
                        principalTable: "Stats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserCreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    StatsId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Stats_StatsId",
                        column: x => x.StatsId,
                        principalTable: "Stats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlessingResult_StatsId",
                table: "BlessingResult",
                column: "StatsId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatsId",
                table: "Users",
                column: "StatsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlessingResult");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Stats");
        }
    }
}
