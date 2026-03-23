using Countries.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace Contries.Tests.Fixtures
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
     .Build();

        public CountryDbContext Context { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _postgres.StartAsync();

            var options = new DbContextOptionsBuilder<CountryDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            Context = new CountryDbContext(options);
            await Context.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
            await _postgres.DisposeAsync();
        }
    }
}
