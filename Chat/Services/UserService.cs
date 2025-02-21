using Chat.Data;
using Chat.Entities;
using MongoDB.Driver;
using System.Collections;

namespace Chat.Services
{
    public class UserService : BaseService<User>
    {
        private readonly IMongoCollection<User> _collection;
        public UserService(MongoDbService mongoDbService) : base(mongoDbService, "Users")
        {
            var database = mongoDbService.Database;
            _collection = database.GetCollection<User>("Users");
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _collection.Find(e => e.Email == email && e.IsDeleted == false).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByNameAsync(string username)
        {
            return await _collection.Find(e => e.Name == username && e.IsDeleted == false).FirstOrDefaultAsync();
        }


    }
}