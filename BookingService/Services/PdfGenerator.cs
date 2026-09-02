using BookingService.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BookingService.Services;

public static class FlightTicketPdfGenerator
{
    public static byte[] Generate(TicketDto ticket)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Background("#1a73e8").Padding(20).Row(row =>
                    {
                        row.RelativeItem().Text("✈️ Flight E-Ticket")
                            .FontSize(22).FontColor("#ffffff").Bold();
                        row.ConstantItem(150).AlignRight().Text($"PNR: {ticket.BookingReference}")
                            .FontSize(13).FontColor("#ffffff").Bold();
                    });

                    col.Item().Height(20);

                    // Passenger Info
                    col.Item().Border(1).BorderColor("#dddddd").Padding(15).Column(info =>
                    {
                        info.Item().Text("Passenger Information").FontSize(14).Bold().FontColor("#1a73e8");
                        info.Item().Height(8);
                        info.Item().Text($"Name: {ticket.PassengerName}").SemiBold();
                        info.Item().Text($"Email: {ticket.Email}");
                    });

                    col.Item().Height(15);

                    // Flight Details
                    col.Item().Border(1).BorderColor("#dddddd").Padding(15).Column(flight =>
                    {
                        flight.Item().Text("Flight Details").FontSize(14).Bold().FontColor("#1a73e8");
                        flight.Item().Height(8);

                        flight.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Flight: {ticket.FlightNumber}").SemiBold();
                                c.Item().Text($"From: {ticket.Origin}");
                                c.Item().Text($"To: {ticket.Destination}");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Departure: {ticket.DepartureTime:dd MMM yyyy, hh:mm tt}").SemiBold();
                                c.Item().Text($"Arrival: {ticket.ArrivalTime:dd MMM yyyy, hh:mm tt}");
                                c.Item().Text($"Status: {ticket.FlightStatus}").FontColor("#2e7d32");
                            });
                        });
                    });

                    col.Item().Height(15);

                    // Passenger List
                    col.Item().Border(1).BorderColor("#dddddd").Padding(15).Column(pass =>
                    {
                        pass.Item().Text("Passenger List").FontSize(14).Bold().FontColor("#1a73e8");
                        pass.Item().Height(8);

                        // Table Header
                        pass.Item().Background("#f5f5f5").Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text("Name").Bold();
                            row.RelativeItem().Text("Age").Bold();
                            row.RelativeItem().Text("Gender").Bold();
                            row.RelativeItem().Text("Passport").Bold();
                            row.RelativeItem().Text("Seat").Bold();
                        });

                        // Table Rows
                        foreach (var passenger in ticket.Passengers)
                        {
                            pass.Item().BorderBottom(1).BorderColor("#eeeeee").Padding(8).Row(row =>
                            {
                                row.RelativeItem().Text(passenger.FullName);
                                row.RelativeItem().Text(passenger.Age.ToString());
                                row.RelativeItem().Text(passenger.Gender);
                                row.RelativeItem().Text(passenger.PassportNumber);
                                row.RelativeItem().Text(passenger.SeatNumber);
                            });
                        }
                    });

                    col.Item().Height(15);

                    // Payment Summary
                    col.Item().Border(1).BorderColor("#dddddd").Padding(15).Column(pay =>
                    {
                        pay.Item().Text("Payment Summary").FontSize(14).Bold().FontColor("#1a73e8");
                        pay.Item().Height(8);
                        pay.Item().AlignRight().Text($"Total Amount: ₹{ticket.TotalAmount:F2}")
                            .FontSize(16).Bold().FontColor("#2e7d32");
                    });

                    col.Item().Height(20);

                    // Footer
                    col.Item().AlignCenter().Text("Thank you for flying with us! Have a safe journey. ✈️")
                        .FontSize(11).FontColor("#888888").Italic();
                });
            });
        }).GeneratePdf();
    }
}
