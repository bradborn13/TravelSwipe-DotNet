

using Countries.Core.Features.Countries;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace Countries.Infrastructure.Data
{
    public class CountryDbContext : DbContext
    {
        public CountryDbContext(DbContextOptions<CountryDbContext> options) : base(options) { }

        public DbSet<Country> Country => Set<Country>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CountryDbContext).Assembly);

            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                // This creates the unique index in PostgreSQL
                entity.HasIndex(c => c.DisplayName)
                      .IsUnique();
            });
        }
    }
}
