using Microsoft.EntityFrameworkCore;
using PaymentService.Models;

namespace PaymentService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>()
            .Property(p => p.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.BaseFare)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.ServiceFee)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.BookingId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.RazorpayOrderId)
            .IsUnique()
            .HasFilter("[RazorpayOrderId] IS NOT NULL");


    }
}
