using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace code.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialPhaseResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OfficialPhaseResults",
                columns: table => new
                {
                    Phase = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FirstPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SecondPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ThirdPlace = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsHomologated = table.Column<bool>(type: "boolean", nullable: false),
                    HomologatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficialPhaseResults", x => x.Phase);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OfficialPhaseResults");
        }
    }
}
