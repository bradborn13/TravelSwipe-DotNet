using Cities.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Cities.Tests.Fixtures
{
    public class InMemoryFixture
    {
        public CityDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<CityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CityDbContext(options);
        }

    }
}
