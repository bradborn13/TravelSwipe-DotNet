using Countries.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Contries.Tests.Fixtures
{
    public class InMemoryFixture
    {
        public CountryDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<CountryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CountryDbContext(options);
        }

    }
}
