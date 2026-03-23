using Cities.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Cities.Tests.Fixtures
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
     .Build();

        public CityDbContext Context { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            var options = new DbContextOptionsBuilder<CityDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            Context = new CityDbContext(options);
            await Context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
            await _postgres.DisposeAsync();
        }
    }
}