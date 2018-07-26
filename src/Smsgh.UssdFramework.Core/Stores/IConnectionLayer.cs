using System.Threading.Tasks;
using StackExchange.Redis;

namespace Smsgh.UssdFramework.Core.Stores
{
    public interface IConnectionLayer
    {
        Task<ConnectionMultiplexer> RedisConnection(string connectionString="localhost");

        //todo: you may include other connection types here: e.g. MSSQL or OracleDB
    }
}