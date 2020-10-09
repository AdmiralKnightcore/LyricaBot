using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lyrica.Data.Migrations
{
    public partial class Karaoke : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "KaraokeSettingId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "KaraokeId",
                table: "Guilds",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KaraokeSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    KaraokeVc = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    KaraokeChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SingingRole = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Intermission = table.Column<bool>(type: "boolean", nullable: false),
                    KaraokeMessage = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaraokeSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KaraokeEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Song = table.Column<string>(type: "text", nullable: true),
                    KaraokeSettingId = table.Column<Guid>(type: "uuid", nullable: true),
                    KaraokeSettingId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KaraokeEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KaraokeEntry_KaraokeSetting_KaraokeSettingId",
                        column: x => x.KaraokeSettingId,
                        principalTable: "KaraokeSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KaraokeEntry_KaraokeSetting_KaraokeSettingId1",
                        column: x => x.KaraokeSettingId1,
                        principalTable: "KaraokeSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KaraokeEntry_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_KaraokeSettingId",
                table: "Users",
                column: "KaraokeSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_Guilds_KaraokeId",
                table: "Guilds",
                column: "KaraokeId");

            migrationBuilder.CreateIndex(
                name: "IX_KaraokeEntry_KaraokeSettingId",
                table: "KaraokeEntry",
                column: "KaraokeSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_KaraokeEntry_KaraokeSettingId1",
                table: "KaraokeEntry",
                column: "KaraokeSettingId1");

            migrationBuilder.CreateIndex(
                name: "IX_KaraokeEntry_UserId",
                table: "KaraokeEntry",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Guilds_KaraokeSetting_KaraokeId",
                table: "Guilds",
                column: "KaraokeId",
                principalTable: "KaraokeSetting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_KaraokeSetting_KaraokeSettingId",
                table: "Users",
                column: "KaraokeSettingId",
                principalTable: "KaraokeSetting",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guilds_KaraokeSetting_KaraokeId",
                table: "Guilds");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_KaraokeSetting_KaraokeSettingId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "KaraokeEntry");

            migrationBuilder.DropTable(
                name: "KaraokeSetting");

            migrationBuilder.DropIndex(
                name: "IX_Users_KaraokeSettingId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Guilds_KaraokeId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "KaraokeSettingId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KaraokeId",
                table: "Guilds");
        }
    }
}
