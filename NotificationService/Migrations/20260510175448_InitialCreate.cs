using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    EmailTemplateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HtmlBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.EmailTemplateId);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                });

            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "EmailTemplateId", "CreatedAt", "HtmlBody", "IsActive", "Subject", "TemplateName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "\n<!DOCTYPE html>\n<html>\n<head><style>\n  body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }\n  .container { background: white; max-width: 600px; margin: auto; border-radius: 8px; padding: 30px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }\n  .header { background: #1a73e8; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }\n  .detail-row { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }\n  .label { color: #666; font-size: 14px; }\n  .value { font-weight: bold; color: #333; }\n  .footer { text-align: center; color: #999; font-size: 12px; margin-top: 20px; }\n  .price { color: #1a73e8; font-size: 22px; font-weight: bold; }\n</style></head>\n<body>\n  <div class='container'>\n    <div class='header'><h2>✈ Booking Confirmed!</h2></div>\n    <p>Dear <strong>{{RecipientName}}</strong>,</p>\n    <p>Your flight has been successfully booked. Here are your details:</p>\n    <div class='detail-row'><span class='label'>Booking ID</span><span class='value'>{{BookingId}}</span></div>\n    <div class='detail-row'><span class='label'>Flight Number</span><span class='value'>{{FlightNumber}}</span></div>\n    <div class='detail-row'><span class='label'>From</span><span class='value'>{{DepartureCity}}</span></div>\n    <div class='detail-row'><span class='label'>To</span><span class='value'>{{ArrivalCity}}</span></div>\n    <div class='detail-row'><span class='label'>Departure</span><span class='value'>{{DepartureTime}}</span></div>\n    <div class='detail-row'><span class='label'>Arrival</span><span class='value'>{{ArrivalTime}}</span></div>\n    <div class='detail-row'><span class='label'>Seat</span><span class='value'>{{SeatNumber}}</span></div>\n    <div class='detail-row'><span class='label'>Total Paid</span><span class='value price'>₹{{TotalPrice}}</span></div>\n    <p>Thank you for choosing FlightMS. Have a safe journey!</p>\n    <div class='footer'>This is an automated email. Please do not reply.</div>\n  </div>\n</body>\n</html>", true, "Your Flight Booking Confirmation - {{FlightNumber}}", "TicketConfirmation", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "\n<!DOCTYPE html>\n<html>\n<head><style>\n  body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }\n  .container { background: white; max-width: 600px; margin: auto; border-radius: 8px; padding: 30px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }\n  .header { background: #0d47a1; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }\n  .boarding-card { border: 2px dashed #0d47a1; border-radius: 8px; padding: 20px; margin: 20px 0; }\n  .route { display: flex; justify-content: space-between; align-items: center; font-size: 24px; font-weight: bold; color: #0d47a1; }\n  .detail-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }\n  .label { color: #666; font-size: 13px; }\n  .value { font-weight: bold; color: #333; }\n  .gate { background: #0d47a1; color: white; padding: 10px 20px; border-radius: 4px; font-size: 20px; font-weight: bold; }\n  .footer { text-align: center; color: #999; font-size: 12px; margin-top: 20px; }\n</style></head>\n<body>\n  <div class='container'>\n    <div class='header'><h2>🛫 Boarding Pass</h2></div>\n    <p>Dear <strong>{{PassengerName}}</strong>, your boarding pass is ready!</p>\n    <div class='boarding-card'>\n      <div class='route'><span>{{DepartureCity}}</span><span>→</span><span>{{ArrivalCity}}</span></div>\n      <div class='detail-row'><span class='label'>Flight</span><span class='value'>{{FlightNumber}}</span></div>\n      <div class='detail-row'><span class='label'>Booking ID</span><span class='value'>{{BookingId}}</span></div>\n      <div class='detail-row'><span class='label'>Departure</span><span class='value'>{{DepartureTime}}</span></div>\n      <div class='detail-row'><span class='label'>Seat</span><span class='value'>{{SeatNumber}}</span></div>\n      <div class='detail-row'><span class='label'>Gate</span><span class='value'><span class='gate'>{{Gate}}</span></span></div>\n    </div>\n    <p>Please arrive at the gate at least 45 minutes before departure.</p>\n    <div class='footer'>This is an automated email. Please do not reply.</div>\n  </div>\n</body>\n</html>", true, "Your Boarding Pass - {{FlightNumber}} | {{DepartureCity}} → {{ArrivalCity}}", "BoardingPass", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "Notifications");
        }
    }
}
