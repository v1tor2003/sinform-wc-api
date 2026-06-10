using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace code.Migrations
{
    /// <inheritdoc />
    public partial class AddSweepstakesAndParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sweepstakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Phase = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualifiedCount = table.Column<int>(type: "integer", nullable: false),
                    IncludeThird = table.Column<bool>(type: "boolean", nullable: false),
                    GuessesDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sweepstakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sweepstakes_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SweepstakesId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalScore = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Sweepstakes_SweepstakesId",
                        column: x => x.SweepstakesId,
                        principalTable: "Sweepstakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SweepstakesId_UserId",
                table: "Participants",
                columns: new[] { "SweepstakesId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_UserId",
                table: "Participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Sweepstakes_CreatorId",
                table: "Sweepstakes",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Sweepstakes_InviteCode",
                table: "Sweepstakes",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "Sweepstakes");
        }
    }
}
