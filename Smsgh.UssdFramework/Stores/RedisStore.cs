using System;
using System.Diagnostics;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Smsgh.UssdFramework.Stores
{
    public class RedisStore : IStore
    {
        private ConnectionMultiplexer Connection { get; set; }
        private IDatabase Db { get; set; }

        //public RedisStore(string dbAddress = "localhost", int dbNumber = 0)
        //{
        //    Connection = ConnectionMultiplexer.Connect(dbAddress);
        //    Db = Connection.GetDatabase(dbNumber);
        //}

        public RedisStore(ConnectionMultiplexer connectionMultiplexer,int dbNumber=0)
        {
            Connection = connectionMultiplexer;
            Db = Connection.GetDatabase(dbNumber);
        }

        public async Task<string> GetHashValue(string name, string key)
        {
            var value = await Db.HashGetAsync(name, key);
            await ResetExpiry(name);
            return value;
        }

        public async Task SetHashValue(string name, string key, string value)
        {
            await Db.HashSetAsync(name, key, value);
            await ResetExpiry(name);
        }

        public async Task<bool> HashExists(string name)
        {
            return await Db.KeyExistsAsync(name);
        }

        public async Task<bool> HashValueExists(string name, string key)
        {
            return await Db.HashExistsAsync(name, key);
        }

        public async Task DeleteHash(string name)
        {
            await Db.KeyDeleteAsync(name);
        }

        public async Task DeleteHashValue(string name, string key)
        {
            await Db.HashDeleteAsync(name, key);
        }

        public async Task SetValue(string key, string value)
        {
            await Db.StringSetAsync(key, value);
            await ResetExpiry(key);
        }

        public async Task<string> GetValue(string key)
        {
            var value = await Db.StringGetAsync(key);
            await ResetExpiry(key);
            return value;
        }

        public async Task<bool> ValueExists(string key)
        {
            return await Db.KeyExistsAsync(key);
        }

        public async Task DeleteValue(string key)
        {
            await Db.KeyDeleteAsync(key);
        }

        private async Task ResetExpiry(string key)
        {
            await Db.KeyExpireAsync(key, TimeSpan.FromSeconds(90));
            
        }

        public void Dispose()
        {
            //Connection.Dispose();
        }
    }
}
