using System.Threading.Tasks;
using StackExchange.Redis;

namespace Smsgh.UssdFramework.Stores.Redis
{
    public class RedisConnectionLayer : IConnectionLayer
    {
        private ConnectionMultiplexer _connection;
        public async Task<ConnectionMultiplexer> RedisConnection(string connectionString = "localhost")
        {
            if (_connection==null)
            {
                _connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }
            return _connection;
        }
    }
}