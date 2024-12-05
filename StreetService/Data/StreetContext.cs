using Microsoft.EntityFrameworkCore;
using StreetService.Models;

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
            modelBuilder.HasPostgresExtension("postgis");   
            modelBuilder.Entity<Street>()
                .Property(s => s.Geometry)
                .HasColumnType("TEXT");
        }
    }
}