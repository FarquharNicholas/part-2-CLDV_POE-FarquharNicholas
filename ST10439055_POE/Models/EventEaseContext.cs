using Microsoft.EntityFrameworkCore;
using ST10439055_POE.Models;

namespace ST10439055_POE.Models
{
    public class EventEaseContext : DbContext
    {
        public EventEaseContext(DbContextOptions<EventEaseContext> options) : base(options)
        {
        }

        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Bookings
            modelBuilder.Entity<Bookings>(entity =>
            {
                entity.HasKey(b => b.BookingId);

                entity.Property(b => b.BookingDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(b => b.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(b => b.Event)
                    .WithMany(e => e.Bookings)
                    .HasForeignKey(b => b.EventId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Venue)
                    .WithMany(v => v.Bookings)
                    .HasForeignKey(b => b.VenueId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Venue
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.HasKey(v => v.VenueId);

                entity.Property(v => v.VenueName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(v => v.Location)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(v => v.Capacity)
                    .IsRequired();
            });

            // Configure Event
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);

                entity.Property(e => e.EventName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.EventDate)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.HasOne(e => e.Venue)
                    .WithMany(v => v.Events)
                    .HasForeignKey(e => e.VenueId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}