using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VirtualRadarServer.Models
{
    public partial class StandingDataContainer : DbContext
    {
        public virtual DbSet<Airport> Airports { get; set; }
        public virtual DbSet<Country> Countries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
Console.Out.WriteLine("Boo!");
                optionsBuilder.UseSqlite(@"NOTHING");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>(entity =>
            {
                entity.ToTable("Airport");

                entity.HasIndex(e => e.Iata);

                entity.HasIndex(e => e.Icao);

                entity.HasIndex(e => new { e.Latitude, e.Longitude });

                entity.Property(e => e.AirportId).ValueGeneratedNever();

                entity.Property(e => e.CountryId).HasColumnType("BIGINT");

                entity.Property(e => e.Iata).HasColumnType("CHAR(3)");

                entity.Property(e => e.Icao).HasColumnType("CHAR(4)");

                entity.Property(e => e.Latitude).HasColumnType("DOUBLE");

                entity.Property(e => e.Location)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(80)");

                entity.Property(e => e.Longitude).HasColumnType("DOUBLE");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(80)");

                entity.HasOne(d => d.Country)
                    .WithMany(p => p.Airports)
                    .HasForeignKey(d => d.CountryId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                entity.HasIndex(e => e.Name)
                    .IsUnique();

                entity.Property(e => e.CountryId).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnType("NVARCHAR(80)");
            });
        }
    }
}
