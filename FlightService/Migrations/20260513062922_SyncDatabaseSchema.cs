using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FlightService.Migrations
{
    /// <inheritdoc />
    public partial class SyncDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns already added manually, skip them
            
            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    AirlineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.AirlineId);
                });

            migrationBuilder.InsertData(
                table: "Airlines",
                columns: new[] { "AirlineId", "Code", "Country", "CreatedAt", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "AI", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2259), true, "Air India" },
                    { 2, "6E", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2362), true, "IndiGo" },
                    { 3, "UK", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2364), true, "Vistara" },
                    { 4, "SG", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2365), true, "SpiceJet" },
                    { 5, "G8", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2366), true, "Go First" },
                    { 6, "I5", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2367), true, "AirAsia India" },
                    { 7, "QP", "India", new DateTime(2026, 5, 13, 6, 29, 22, 105, DateTimeKind.Utc).AddTicks(2368), true, "Akasa Air" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Airlines");
            
            // Columns were added manually, keep them
        }
    }
}
