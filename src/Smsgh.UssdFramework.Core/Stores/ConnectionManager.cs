using Smsgh.UssdFramework.Core.Stores.Redis;

namespace Smsgh.UssdFramework.Core.Stores
{
    public class ConnectionManager
    {
        public static void Init()
        {
            if (_instance == null)
            {
                _instance = new ConnectionManager();
            }
        }

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
}