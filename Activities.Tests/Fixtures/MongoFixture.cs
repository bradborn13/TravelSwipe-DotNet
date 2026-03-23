using Microsoft.Extensions.Configuration;
using Mongo2Go;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Activities.Infrastructure.Data;

namespace Activities.Tests.Fixtures
{
    public class MongoFixture : IDisposable
    {
        private readonly MongoDbRunner _runner;

        public MongoFixture()
        {
            _runner = MongoDbRunner.Start();

        }
        public MongoContext CreateContext()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                { "ConnectionStrings:MongoDb", _runner.ConnectionString }
                })
                .Build();

            return new MongoContext(config);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
