using Microsoft.EntityFrameworkCore;
using Activities.Infrastructure.Data;


namespace Activities.Tests.Fixtures
{
    public class InMemoryFixture
    {
        public ActivityDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ActivityDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ActivityDbContext(options);
        }

    }
}
