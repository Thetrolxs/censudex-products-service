
using censudex_products_service.src.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace censudex_products_service.src.Data
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IMongoDatabase database)
        {
            _database = database;
            
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products"); 
    }
}