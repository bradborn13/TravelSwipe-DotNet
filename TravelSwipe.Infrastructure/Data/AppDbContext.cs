using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Countries;
using TravelSwipe.Core.Features.Users;

namespace TravelSwipe.Infrastructure.data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // This maps to your 'users' table in Postgres
        public DbSet<User> Users => Set<User>();
        public DbSet<City> City => Set<City>();
        public DbSet<Country> Country => Set<Country>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");

                // This creates the unique index in PostgreSQL
                entity.HasIndex(c => c.NameClean)
                      .IsUnique();
            });
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Country>(entity =>
            {
                entity.ToTable("Country");

                // This creates the unique index in PostgreSQL
                entity.HasIndex(c => c.NameClean)
                      .IsUnique();
            });
        }
    }
}
