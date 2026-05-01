using Activities.Core.Features.Activities;
using Activities.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Activities.Infrastructure.Repositories
{

    public class ActivityRepository : IActivityRepository
    {

        private readonly IMongoCollection<Activity> _activities;
        public ActivityRepository(MongoContext context)
        {
            _activities = context.Activities;
        }

        public async Task<bool> AddImage(string activityName, string city, List<ImageURL> images)
        {
            var filter = Builders<Activity>.Filter.And(
                 Builders<Activity>.Filter.Eq(x => x.Name, activityName),
                 Builders<Activity>.Filter.Eq(x => x.City, city)
             );

            var update = Builders<Activity>.Update
                .PushEach(x => x.ImagesURL, images);

            var result = await _activities.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
        public async Task<List<string>> GetAllActivityNamesByLocation(string city)
        {
            var filter = Builders<Activity>.Filter.Eq(x => x.City, city);
            var fields = Builders<Activity>.Projection.Include(x => x.Name);
            var result = await _activities.AsQueryable().Where(x => x.City == city).Select(x => x.Name).ToListAsync();
            return result;

        }
        public async Task<List<Activity>> GetActivitiesByCity(string city)
        {
            var result = await _activities.AsQueryable()
                              .Where(a => a.City.ToLowerInvariant() == city.ToLowerInvariant())
                              .ToListAsync();
            return result;
        }

        public async Task InsertActivityBatch(List<Activity> activities)
        {
            if (!activities.Any())
                return;

            var writes = activities.Select(activity =>
    new UpdateOneModel<Activity>(
        Builders<Activity>.Filter.Eq(x => x.FsqId, activity.FsqId),
          Builders<Activity>.Update
            .Set(x => x.Name, activity.Name)
            .Set(x => x.City, activity.City)
            .Set(x => x.Address, activity.Address)
            .Set(x => x.Latitude, activity.Latitude)
            .Set(x => x.Longitude, activity.Longitude)
            .Set(x => x.Suburb, activity.Suburb)
            .Set(x => x.Categories, activity.Categories)
            .Set(x => x.City, activity.City)
            .Set(x => x.Country, activity.Country)
            .Set(x => x.DateRefreshed, activity.DateRefreshed)
            .Set(x => x.DateCreated, activity.DateCreated)
            .Set(x => x.Details, activity.Details)
            .Set(x => x.Distance, activity.Distance)
            .Set(x => x.ImagesURL, activity.ImagesURL)
            .Set(x => x.Link, activity.Link)
            .Set(x => x.RelatedPlaces, activity.RelatedPlaces)
            .Set(x => x.SocialMedia, activity.SocialMedia)
            .Set(x => x.Tel, activity.Tel)
            .Set(x => x.Website, activity.Website)
        )
    {
        IsUpsert = true
    }
        ).ToList();


            var result = await _activities.BulkWriteAsync(
     writes,
     new BulkWriteOptions { IsOrdered = false });

            Console.WriteLine($"Upserts: {result.Upserts.Count}, Modified: {result.ModifiedCount}");
        }

        public async Task<List<Activity>> GetActivitiesWithoutImages(string city)
        {
            var filter = Builders<Activity>.Filter.And(
                Builders<Activity>.Filter.Eq(x => x.City, city),
                Builders<Activity>.Filter.Or(Builders<Activity>.Filter.Exists(x => x.ImagesURL, false),
                Builders<Activity>.Filter.Size(x => x.ImagesURL, 0),
                Builders<Activity>.Filter.Eq(x => x.ImagesURL, null)));
            var result = await _activities.Find(filter).ToListAsync();
            return result;
        }
        public async Task<long> UpdateImages(string city, string activityName, List<ImageURL> imageList)
        {

            var filter = Builders<Activity>.Filter.And(
                Builders<Activity>.Filter.Eq(x => x.City, city),
                Builders<Activity>.Filter.Eq(x => x.Name, activityName)
                );
            var update = Builders<Activity>.Update.Set(x => x.ImagesURL, imageList);
            var result = await _activities.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }

        public async Task<List<CityGeoLocation>> GetUniqueCityList()
        {
            var pipeline = new EmptyPipelineDefinition<Activity>()
           .Match(x =>
               x.City != null &&
               x.Country == null)
           .Group(x => x.City, g => new
           {
               City = g.Key,
               Latitude = g.First().Latitude,
               Longitude = g.First().Longitude
           })
           .Project(x => new CityGeoLocation
           {
               City = x.City,
               Latitude = x.Latitude,
               Longitude = x.Longitude
           });

            var result = await _activities.Aggregate(pipeline).ToListAsync();
            return result;
        }



    }
}
