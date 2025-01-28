using MongoDB.Bson;
using MongoDB.Driver;

namespace Chat.Counter
{
    public class SequentialIdGenerator
    {
        private readonly IMongoCollection<BsonDocument> _counterCollection;

        public SequentialIdGenerator(IMongoDatabase database)
        {
            _counterCollection = database.GetCollection<BsonDocument>("counter");
        }

        public int GetNextSequenceValue(string entityName)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", entityName);
            var update = Builders<BsonDocument>.Update.Inc("sequence_value", 1);
            var options = new FindOneAndUpdateOptions<BsonDocument>
            {
                ReturnDocument = ReturnDocument.After,
                IsUpsert = true
            };

            var result = _counterCollection.FindOneAndUpdate(filter, update, options);
            return result["sequence_value"].AsInt32;
        }
    }
}
