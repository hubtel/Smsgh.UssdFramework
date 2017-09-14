using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework
{
    public class UssdResponse
    {
        public string Type { get; private set; }
        public string Message { get; private set; }
        public string ClientState { get; private set; }
        public Exception Exception { get; private set; }

        public UssdResponse()
        {
            Exception = null;
            AutoDialOn = true;
        }


        public string NextRoute { get; private set; }
        public bool IsRelease { get { return string.IsNullOrWhiteSpace(NextRoute); } }
        public bool IsRedirect { get; private set; }
        public bool AutoDialOn { get; set; }

        public static UssdResponse Render(string message, string nextRoute = null)
        {
            var type = string.IsNullOrWhiteSpace(nextRoute)
                ? UssdResponseTypes.Release.ToString()
                : UssdResponseTypes.Response.ToString();
            return new UssdResponse()
            {
                Type = type,
                Message = message,
                NextRoute = nextRoute,
            };
        }

        public static UssdResponse Redirect(string nextRoute)
        {
            return new UssdResponse()
            {
                NextRoute = nextRoute,
                IsRedirect = true,
            };
        }

        public UssdResponse SetException(Exception exception)
        {
            Exception = exception;
            return this;
        }


        private enum UssdResponseTypes
        {
            Response, Release,
        }
    }
}
