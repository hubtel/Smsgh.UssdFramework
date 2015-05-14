using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Smsgh.UssdFramework.LoggingStores;

namespace Smsgh.UssdFramework.Logging.MongoDb
{
    public class MongoDbLoggingStore : ILoggingStore
    {
        private IMongoCollection<MongoDbSessionLog> Collection { get; set; }

        public MongoDbLoggingStore(string connectionString, string databaseName, 
            string collectionName = "ussdsessionlog")
        {
           Collection = new MongoClient(connectionString).GetDatabase(databaseName)
               .GetCollection<MongoDbSessionLog>(collectionName);
        }


        public async Task<IQueryable<UssdSessionLog>> FindAll(TimeSpan since)
        {
            var filter = Builders<MongoDbSessionLog>.Filter.Gt(x => x.StartTime, 
                DateTime.UtcNow.Subtract(since));
            var results = await Collection.Find(filter).ToListAsync();
            return results.AsQueryable();
        }

        public async Task<UssdSessionLog> Find(string sessionId)
        {
            var filter = Builders<MongoDbSessionLog>.Filter.Where(x => x.SessionId == sessionId);
            var results = await Collection.Find(filter).ToListAsync();
            return results.FirstOrDefault();
        }

        public async Task Create(UssdSessionLog log)
        {
            var mongoLog = new MongoDbSessionLog
            {
                StartTime = log.StartTime,
                ArbitraryData = log.ArbitraryData,
                SessionId = log.SessionId,
                Mobile = log.Mobile,
                EndTime = log.EndTime,
                Entries = log.Entries,
                DurationInMilliseconds = log.DurationInMilliseconds,
            };
            await Collection.InsertOneAsync(mongoLog);
        }

        public async Task Update(UssdSessionLog log)
        {
            var filter = Builders<MongoDbSessionLog>.Filter.Where(x => x.SessionId == log.SessionId);
            var update = Builders<MongoDbSessionLog>.Update.Set(x => x.EndTime, log.EndTime)
                .Set(x => x.DurationInMilliseconds, log.DurationInMilliseconds)
                .Set(x => x.ErrorTrace, log.ErrorTrace);
            await Collection.FindOneAndUpdateAsync(filter, update);
        }

        public async Task AddEntry(string sessionId, UssdSessionLogEntry entry)
        {
            var filter = Builders<MongoDbSessionLog>.Filter.Where(x => x.SessionId == sessionId);
            var update = Builders<MongoDbSessionLog>.Update.AddToSet(x => x.Entries, entry);
            await Collection.UpdateOneAsync(filter, update);
        }


        public void Dispose()
        {
        }
    }

    public class MongoDbSessionLog : UssdSessionLog
    {
        public ObjectId Id { get; set; }
    }
}
