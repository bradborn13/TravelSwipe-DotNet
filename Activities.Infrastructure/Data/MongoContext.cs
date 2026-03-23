using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Activities.Core.Features.Activities;

namespace Activities.Infrastructure.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;
        public MongoContext(IConfiguration configuration)
        {
            // 1. Get connection details from appsettings.json
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));

            // 2. Specify your database name (same as used in Node.js)
            _database = client.GetDatabase("travelswipe");
            EnsureCollectionsExist();

            // 3. Trigger the Fluent Mappings we discussed earlier
            MongoMapping.Configure();
        }
        private void EnsureCollectionsExist()
        {
            var existingCollections = _database.ListCollectionNames().ToList();

            if (!existingCollections.Contains("activities"))
                _database.CreateCollection("activities");

            if (!existingCollections.Contains("cities"))
                _database.CreateCollection("cities");

            if (!existingCollections.Contains("countries"))
                _database.CreateCollection("countries");
        }
        public IMongoCollection<Activity> Activities =>
        _database.GetCollection<Activity>("activities");
    }
}
