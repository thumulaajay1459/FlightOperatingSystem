using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>()
                .Property(n => n.Status)
                .HasConversion<string>();   // Store "Pending"/"Sent"/"Failed" in DB, not 0/1/2

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>();

            // Seed HTML templates — placeholders use {{Key}} format
            modelBuilder.Entity<EmailTemplate>().HasData(
                new EmailTemplate
                {
                    EmailTemplateId = 1,
                    TemplateName = "TicketConfirmation",
                    Subject = "Your Flight Booking Confirmation - {{FlightNumber}}",
                    HtmlBody = @"
<!DOCTYPE html>
<html>
<head><style>
  body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }
  .container { background: white; max-width: 600px; margin: auto; border-radius: 8px; padding: 30px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
  .header { background: #1a73e8; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }
  .detail-row { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee; }
  .label { color: #666; font-size: 14px; }
  .value { font-weight: bold; color: #333; }
  .footer { text-align: center; color: #999; font-size: 12px; margin-top: 20px; }
  .price { color: #1a73e8; font-size: 22px; font-weight: bold; }
</style></head>
<body>
  <div class='container'>
    <div class='header'><h2>✈ Booking Confirmed!</h2></div>
    <p>Dear <strong>{{RecipientName}}</strong>,</p>
    <p>Your flight has been successfully booked. Here are your details:</p>
    <div class='detail-row'><span class='label'>Booking ID</span><span class='value'>{{BookingId}}</span></div>
    <div class='detail-row'><span class='label'>Flight Number</span><span class='value'>{{FlightNumber}}</span></div>
    <div class='detail-row'><span class='label'>From</span><span class='value'>{{DepartureCity}}</span></div>
    <div class='detail-row'><span class='label'>To</span><span class='value'>{{ArrivalCity}}</span></div>
    <div class='detail-row'><span class='label'>Departure</span><span class='value'>{{DepartureTime}}</span></div>
    <div class='detail-row'><span class='label'>Arrival</span><span class='value'>{{ArrivalTime}}</span></div>
    <div class='detail-row'><span class='label'>Seat</span><span class='value'>{{SeatNumber}}</span></div>
    <div class='detail-row'><span class='label'>Total Paid</span><span class='value price'>₹{{TotalPrice}}</span></div>
    <p>Thank you for choosing FlightMS. Have a safe journey!</p>
    <div class='footer'>This is an automated email. Please do not reply.</div>
  </div>
</body>
</html>",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new EmailTemplate
                {
                    EmailTemplateId = 2,
                    TemplateName = "BoardingPass",
                    Subject = "Your Boarding Pass - {{FlightNumber}} | {{DepartureCity}} → {{ArrivalCity}}",
                    HtmlBody = @"
<!DOCTYPE html>
<html>
<head><style>
  body { font-family: Arial, sans-serif; background: #f4f4f4; margin: 0; padding: 20px; }
  .container { background: white; max-width: 600px; margin: auto; border-radius: 8px; padding: 30px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
  .header { background: #0d47a1; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center; }
  .boarding-card { border: 2px dashed #0d47a1; border-radius: 8px; padding: 20px; margin: 20px 0; }
  .route { display: flex; justify-content: space-between; align-items: center; font-size: 24px; font-weight: bold; color: #0d47a1; }
  .detail-row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #eee; }
  .label { color: #666; font-size: 13px; }
  .value { font-weight: bold; color: #333; }
  .gate { background: #0d47a1; color: white; padding: 10px 20px; border-radius: 4px; font-size: 20px; font-weight: bold; }
  .footer { text-align: center; color: #999; font-size: 12px; margin-top: 20px; }
</style></head>
<body>
  <div class='container'>
    <div class='header'><h2>🛫 Boarding Pass</h2></div>
    <p>Dear <strong>{{PassengerName}}</strong>, your boarding pass is ready!</p>
    <div class='boarding-card'>
      <div class='route'><span>{{DepartureCity}}</span><span>→</span><span>{{ArrivalCity}}</span></div>
      <div class='detail-row'><span class='label'>Flight</span><span class='value'>{{FlightNumber}}</span></div>
      <div class='detail-row'><span class='label'>Booking ID</span><span class='value'>{{BookingId}}</span></div>
      <div class='detail-row'><span class='label'>Departure</span><span class='value'>{{DepartureTime}}</span></div>
      <div class='detail-row'><span class='label'>Seat</span><span class='value'>{{SeatNumber}}</span></div>
      <div class='detail-row'><span class='label'>Gate</span><span class='value'><span class='gate'>{{Gate}}</span></span></div>
    </div>
    <p>Please arrive at the gate at least 45 minutes before departure.</p>
    <div class='footer'>This is an automated email. Please do not reply.</div>
  </div>
</body>
</html>",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
