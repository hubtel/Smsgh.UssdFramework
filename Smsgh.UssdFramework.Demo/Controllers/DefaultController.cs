using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Smsgh.UssdFramework.Logging;
using Smsgh.UssdFramework.Stores;

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
            return Ok(await Ussd.Process(new RedisStore(redisConnection), request, "Main", "Start", null, 
                new MongoDbLoggingStore("mongodb://localhost", "demoussd")));
        } 
    }
}
