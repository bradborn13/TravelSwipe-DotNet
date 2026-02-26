using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelSwipe.Core.Features.Activities;

namespace TravelSwipe.Infrastructure.Data
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

            // 3. Trigger the Fluent Mappings we discussed earlier
            MongoMapping.Configure();
        }
        public IMongoCollection<Activity> Activities =>
        _database.GetCollection<Activity>("activities");
    }
}
