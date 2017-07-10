using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using StackExchange.Redis;

[assembly: OwinStartup(typeof(Smsgh.UssdFramework.Demo.Startup))]

namespace Smsgh.UssdFramework.Demo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888


        }
    }

    public class ConnectionManager
    {

        private static ConnectionManager _instance;

        private readonly RedisConnectionLayer _redisConnectionLayer;
        public ConnectionManager()
        {
            _redisConnectionLayer = new RedisConnectionLayer();
        }

        public IConnectionLayer GetRedisDbConnectionLayer()
        {
            return _redisConnectionLayer;
        }
        public static ConnectionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConnectionManager();
                }

                return _instance;
            }
        }
    }

    public interface IConnectionLayer
    {
        Task<ConnectionMultiplexer> RedisConnection(string connectionString="localhost");
    }

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
