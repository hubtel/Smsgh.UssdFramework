using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Smsgh.UssdFramework.LoggingStores;

namespace Smsgh.UssdFramework.Logging.MongoDb
{
    public class MongoDbSessionLog : UssdSessionLog
    {
        public ObjectId Id { get; set; }
    }
}
