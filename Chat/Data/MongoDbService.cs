using MongoDB.Driver;

namespace Chat.Data
{
    public class MongoDbService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;
        private readonly IWebHostEnvironment _environment;

        public MongoDbService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;

            string connectionString;

            if (_environment.IsDevelopment())
            {
                connectionString = _configuration.GetSection("MongoDB:ConnectionString").Value;
            }
            else
            {
                connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
            }

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
        }

        public IMongoDatabase Database => _database;
    }
}
