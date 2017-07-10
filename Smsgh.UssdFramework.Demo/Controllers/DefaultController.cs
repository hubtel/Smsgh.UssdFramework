using System.Threading.Tasks;
using System.Web.Http;
using Smsgh.UssdFramework.Stores;
using Smsgh.UssdFramework.Stores.Redis;

namespace Smsgh.UssdFramework.Demo.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Index(UssdRequest request)
        {

            var redisConnection = await ConnectionManager.Instance.GetRedisDbConnectionLayer().RedisConnection();

            if (!redisConnection.IsConnected)
            {
                await redisConnection.ConfigureAsync();
            }
            //return Ok(await Ussd.Process(new RedisStore(redisConnection), request, "Main", "Start", null, 
            //    new MongoDbLoggingStore("mongodb://localhost", "demoussd")));

            //return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Start", null,
            //    null));

            return Ok(await Ussd.Process(new RedisStore(redisConnection), request, "Main", "Start", null,
                null));
        } 
    }
}
