using Microsoft.EntityFrameworkCore;
using StreetService.Models;
// This class represents the database context for the StreetService.
// It configures the Streets DbSet and enables the PostGIS extension.
namespace StreetService.Data
{
    public class StreetContext : DbContext
    {
        public DbSet<Street> Streets { get; set; }

        public StreetContext(DbContextOptions<StreetContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enables the PostGIS extension for spatial data handling.
            modelBuilder.HasPostgresExtension("postgis");   
            modelBuilder.Entity<Street>()
                .Property(s => s.Geometry)
                .HasColumnType("TEXT");
        }
    }
}