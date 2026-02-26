using MongoDB.Driver;
using MongoDB.Driver.Linq;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Infrastructure.data;
using TravelSwipe.Infrastructure.Data;

namespace TravelSwipe.Infrastructure.Repositories
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



        public async Task<IEnumerable<Activity>> GetActivitiesByCity(string city)
        {
            var result = await _activities.AsQueryable()
                              .Where(a => a.City == city)
                              .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Activity>> GetActivitiesWithoutImages(string city)
        {
            var filter = Builders<Activity>.Filter.And(
                Builders<Activity>.Filter.Eq(x => x.City, city),
                Builders<Activity>.Filter.Or(Builders<Activity>.Filter.Exists(x => x.ImagesURL, false),
                Builders<Activity>.Filter.Size(x => x.ImagesURL, 0),
                Builders<Activity>.Filter.Eq(x => x.ImagesURL, null)));
            var result = await _activities.Find(filter).ToListAsync();
            return result;
        }
    }
}
