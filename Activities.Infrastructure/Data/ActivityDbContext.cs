using Microsoft.EntityFrameworkCore;
using Activities.Core.Features.Cities;
using Activities.Core.Features.Countries;

namespace Activities.Infrastructure.Data
{
    public class ActivityDbContext : DbContext
    {
        public ActivityDbContext(DbContextOptions<ActivityDbContext> options) : base(options) { }

        public DbSet<City> City => Set<City>();
        public DbSet<Country> Country => Set<Country>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActivityDbContext).Assembly);
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
