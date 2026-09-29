using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Domain.Entities;
using ReservationSystem.Infrastructure.Identity;

namespace ReservationSystem.Infrastructure.Persistence;

public class ReservationDbContext
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Specialist> Specialists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Specialist>(builder =>
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
            builder.Property(s => s.Specialization).HasMaxLength(100).IsRequired();

            builder.HasData(SpecialistSeed.All);
        });

        modelBuilder.Entity<Reservation>(builder =>
        {
            builder.HasKey(r => r.Id);

            builder.HasOne<Specialist>()
                   .WithMany()
                   .HasForeignKey(r => r.SpecialistId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.Status)
                   .HasConversion<int>();

            builder.Property(r => r.StartTime).IsRequired();
            builder.Property(r => r.EndTime).IsRequired();
        });
    }
}