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
            return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Start", null, 
                new MongoDbLoggingStore("mongodb://localhost", "demoussd")));
        } 
    }
}
