using Microsoft.EntityFrameworkCore;
using ReservationSystem.Domain.Entities;

namespace ReservationSystem.Infrastructure.Persistence;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>(builder =>
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Status)
                   .HasConversion<int>();

            builder.Property(r => r.StartTime).IsRequired();
            builder.Property(r => r.EndTime).IsRequired();
        });
    }
}