using Chat.Data;
using Chat.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

public class BaseService<TEntity> where TEntity : BaseEntity
{
    private readonly IMongoCollection<TEntity> _collection;
    private readonly IMongoCollection<BsonDocument> _counterCollection;

    public BaseService(MongoDbService mongoDbService, string collectionName)
    {
        var database = mongoDbService.Database;
        _collection = database.GetCollection<TEntity>(collectionName);
        _counterCollection = database.GetCollection<BsonDocument>("counter");
    }

    public async Task<int> GetNextSequenceValue(string entityName)
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

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await _collection.Find(e => e.IsDeleted == false).ToListAsync();
    }

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await _collection.Find(e => e.Id == id && e.IsDeleted == false).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(TEntity entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task<bool> UpdateAsync(int id, TEntity updatedEntity)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        var result = await _collection.ReplaceOneAsync(filter, updatedEntity);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var filter = Builders<TEntity>.Filter.Eq(e => e.Id, id);
        var update = Builders<TEntity>.Update.Set(e => e.IsDeleted, true);
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.MatchedCount > 0;
    }
}
