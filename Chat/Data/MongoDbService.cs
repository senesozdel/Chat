﻿using MongoDB.Driver;

namespace Chat.Data
{
    public class MongoDbService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;
        public MongoDbService(IConfiguration configuration)
        {
            _configuration = configuration;

            var connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
            }

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoDatabase Database => _database;
    }
}
