using MongoDB.Driver;

namespace ExpensesPlanner.Client.Data
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("DbConnection"));
            _database = client.GetDatabase(configuration["MongoDb:DatabaseName"]);

            /*var connectionString = _configuration.GetConnectionString("DbConnection");
            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase(mongoUrl.DatabaseName);*/
        }

        public IMongoDatabase Database => _database;
    }
}
