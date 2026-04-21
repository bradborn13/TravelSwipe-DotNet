using Activities.Core.Features.Activities;
using Activities.Infrastructure.Data;
using Activities.Infrastructure.Repositories;
using Activities.Tests.Fixtures;
using FluentAssertions;

using MongoDB.Driver;


namespace Activities.Tests
{
    public class ActivityRepositoryTests
    {
        private readonly MongoContext _context;
        private IActivityRepository _repo;
        private readonly MongoFixture _fixture;

        public ActivityRepositoryTests()
        {
            _fixture = new MongoFixture();
            _context = _fixture.CreateContext();
            _repo = new ActivityRepository(_context);
        }
        [Fact]
        public async Task GetUniqueCountryList_Returns_CountryList()
        {
            List<Activity> mockActivities = new List<Activity>() {
            new Activity() { City = "Aarhus",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Aaros" },
            new Activity() { City = "Aarhus",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Infinity Bridge" },
            new Activity(){City="Aalborg",Name="Heidi",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100 } };
            await _context.Activities.InsertManyAsync(mockActivities);
            var uniqueMockCities = mockActivities.Select(x => x.City).Distinct().ToList();


            var locationList = await _repo.GetUniqueCityList();

            locationList.Should().NotBeNull();
            locationList.Should().HaveCount(2);
            locationList.Should().AllSatisfy(activity => uniqueMockCities.Should().ContainSingle(x => x == activity.City));
        }
        [Fact]
        public async Task GetActivitiesWithoutImages_Returns_Activities()
        {
            List<Activity> mockActivities = new List<Activity>() {
            new Activity() { City = "Aarhus",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Aaros" },
            new Activity() { City = "Aarhus",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Infinity Bridge" },
            new Activity() {City="Aalborg",Name="Heidi",Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100 } };
            await _context.Activities.InsertManyAsync(mockActivities);
            List<string> uniqueMockCities = mockActivities.Select(x => x.City).Distinct().ToList();


            var activityListA = await _repo.GetActivitiesWithoutImages(uniqueMockCities[0]);
            var activityListB = await _repo.GetActivitiesWithoutImages(uniqueMockCities[1]);

            activityListA.Should().NotBeNull();
            activityListA.Should().HaveCount(2);

            activityListB.Should().NotBeNull();
            activityListB.Should().HaveCount(1);
        }

        [Fact]
        public async Task InsertActivityBatch_Should_AddActivities()
        {
            List<Activity> mockActivities = new List<Activity>() {
            new Activity() { City = "Aarhus",  FsqId = "fsq_1", Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Aaros" },
            new Activity() { City = "Aarhus", FsqId = "fsq_2", Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Infinity Bridge" },
            new Activity() { City = "Aalborg", FsqId = "fsq_3", Name="Heidi", Latitude= new Random().NextDouble() * 100, Longitude=new Random().NextDouble() * 100 } };

            await _repo.InsertActivityBatch(mockActivities);
            List<Activity> storedActivities = await _context.Activities.Find(x => true).ToListAsync();

            storedActivities.Should().NotBeNull();
            storedActivities.Should().HaveCount(3);

        }

        [Fact]
        public async Task GetActivitiesWithoutImages_Returns_Activities_MultipleLocations()
        {
            List<Activity> mockActivities = new List<Activity>() {
            new Activity() { City = "Aarhus",  ImagesURL = [], Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Aaros" },
            new Activity() { City = "Aarhus", ImagesURL = [], Latitude=new Random().NextDouble() * 100,Longitude=new Random().NextDouble() * 100, Name = "Infinity Bridge" },
            new Activity() { City = "Aalborg", ImagesURL = [], Name="Heidi", Latitude= new Random().NextDouble() * 100, Longitude=new Random().NextDouble() * 100 } };
            await _context.Activities.InsertManyAsync(mockActivities);

            List<Activity> actvitiListA = await _repo.GetActivitiesWithoutImages(mockActivities[0].City);
            List<Activity> actvitiListB = await _repo.GetActivitiesWithoutImages(mockActivities[2].City);

            actvitiListA.Should().NotBeNull();
            actvitiListA.Should().HaveCount(2);

            actvitiListB.Should().NotBeNull();
            actvitiListB.Should().HaveCount(1);

        }
        [Fact]
        public async Task AddImage_Returns_True()
        {
            Activity mockActivity =
            new Activity() { City = "Aarhus", ImagesURL = [], Latitude = new Random().NextDouble() * 100, Longitude = new Random().NextDouble() * 100, Name = "Aaros" };
            await _context.Activities.InsertOneAsync(mockActivity);
            List<ImageURL> imageList = new List<ImageURL>() {
                new ImageURL() {
                    ImgSource="mockSource 01",
                    Link="mockLink 02"
                },
                  new ImageURL() {
                    ImgSource="mockSource 02",
                    Link="mockLink 02 "
            }
            };

            var response = await _repo.AddImage(mockActivity.Name, mockActivity.City, imageList);
            Activity updatedActivity = await _context.Activities.Find(x => x.Name == mockActivity.Name).SingleOrDefaultAsync();

            response.Should().BeTrue();
            updatedActivity.Should().NotBe(null);
            updatedActivity.ImagesURL.Should().HaveCount(2);

        }
    }
}
