using Cities.Core.Features.Cities;
using Cities.Core.Features.Countries;
using Microsoft.EntityFrameworkCore;


namespace Cities.Infrastructure.Data
{
    public class CityDbContext : DbContext
    {
        public CityDbContext(DbContextOptions<CityDbContext> options) : base(options) { }

        public DbSet<City> City => Set<City>();
        public DbSet<Country> Country => Set<Country>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CityDbContext).Assembly);
            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");

                // This creates the unique index in PostgreSQL
                entity.HasIndex(c => c.DisplayName)
                      .IsUnique();
            });
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
