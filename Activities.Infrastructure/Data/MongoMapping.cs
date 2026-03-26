
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using Activities.Core.Features.Activities;

namespace Activities.Infrastructure.Data
{
    public static class MongoMapping
    {
        public static void Configure()
        {

            var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true)
        };
            ConventionRegistry.Register("TravelSwipeConventions", pack, t => true);
            // 2. Map 'Activity' (The Main Document)
            if (!BsonClassMap.IsClassMapRegistered(typeof(Activity)))
            {
                BsonClassMap.RegisterClassMap<Activity>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);

                    // Map C# 'Id' to Mongo '_id'
                    cm.MapIdProperty(c => c.Id)
                      .SetIdGenerator(StringObjectIdGenerator.Instance)
                      .SetSerializer(new StringSerializer(BsonType.ObjectId));

                    // Manual Overrides for non-camelCase fields from your Node Schema
                    cm.MapProperty(c => c.FsqId).SetElementName("fsq_id");
                    cm.MapProperty(c => c.City).SetElementName("city");
                    cm.MapProperty(c => c.ImagesURL).SetElementName("imagesURL");
                    cm.MapProperty(c => c.DateCreated).SetElementName("dateCreated");
                    cm.MapProperty(c => c.DateRefreshed).SetElementName("dateRefreshed");
                    cm.MapProperty(c => c.RelatedPlaces).SetElementName("relatedPlaces");
                    cm.MapProperty(c => c.SocialMedia).SetElementName("socialMedia");
                });
            }

            // 3. Map 'SocialMedia' 
            if (!BsonClassMap.IsClassMapRegistered(typeof(SocialMedia)))
            {
                BsonClassMap.RegisterClassMap<SocialMedia>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(c => c.FacebookId).SetElementName("facebookId");
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(RelatedPlaces)))
            {
                BsonClassMap.RegisterClassMap<RelatedPlaces>(cm =>
                {
                    cm.AutoMap();
                    cm.MapProperty(c => c.Fsq_place_id).SetElementName("fsq_place_id");
                });
            }

        }
    }
}
