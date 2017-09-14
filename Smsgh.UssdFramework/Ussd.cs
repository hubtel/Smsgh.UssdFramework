using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Smsgh.UssdFramework.LoggingStores;
using Smsgh.UssdFramework.Stores;

namespace Smsgh.UssdFramework
{
    public class Ussd
    {
        /// <summary>
        /// Process USSD requests. Automatically routes to nested routes.
        /// </summary>
        /// <param name="store">Session store</param>
        /// <param name="request"></param>
        /// <param name="initiationController">Initiation controller</param>
        /// <param name="initiationAction">Initiation action</param>
        /// <param name="data">Data available to controllers</param>
        /// <param name="loggingStore">Logging store</param>
        /// <param name="arbitraryLogData">Arbitrary data to add to session log</param>
        /// <returns></returns>
        public static async Task<UssdResponse> Process(IStore store, UssdRequest request,
            string initiationController, string initiationAction,
            Dictionary<string, string> data = null, ILoggingStore loggingStore = null, string arbitraryLogData = null)
        {
            return
                    await
                        ProcessRequest(store, request, initiationController, initiationAction, data, loggingStore,
                            arbitraryLogData);
        }

        /// <summary>
        /// Process USSD requests.
        /// </summary>
        /// <param name="store">Session store</param>
        /// <param name="request"></param>
        /// <param name="initiationController">Initiation controller</param>
        /// <param name="initiationAction">Initiation action</param>
        /// <param name="data">Data available to controllers</param>
        /// <param name="loggingStore">Logging store</param>
        /// <param name="arbitraryLogData">Arbitrary data to add to session log</param>
        /// <param name="dispose">Dispose stores</param>
        /// <returns></returns>
        public static async Task<UssdResponse> ProcessRequest(IStore store, UssdRequest request,
            string initiationController, string initiationAction, Dictionary<string, string> data = null, 
            ILoggingStore loggingStore = null, string arbitraryLogData = null)
        {
            var startTime = DateTime.UtcNow;
            if (data == null)
            {
                data = new Dictionary<string, string>();
            }
            var context = new UssdContext(store, request, data);
            UssdResponse response;
            DateTime endTime;
            await PreLog(loggingStore, request, startTime, arbitraryLogData);
            try
            {
                switch (request.RequestType)
                {
                    case UssdRequestTypes.Initiation:
                        var route = string.Format("{0}Controller.{1}",
                            initiationController, initiationAction);
                        response = await OnInitiation(context, route);
                        break;
                    case UssdRequestTypes.Response:
                        response = await OnResponse(context);
                        break;
                    default:
                        response = UssdResponse.Render(string.Format("Request Type is {0}. Will ignore intended action.", request.Type));
                        break;
                }
            }
            catch (Exception e)
            {
                response = UssdResponse.Render(e.Message);
                response.SetException(e);
            }
            finally
            {
                context.Dispose();
                endTime = DateTime.UtcNow;
            }
            await PostLog(loggingStore, request, response, startTime, endTime);
            return response;
        }

        #region Events
        private static async Task<UssdResponse> OnInitiation(UssdContext context,
            string route)
        {
            await context.SessionClose();
            await context.SessionSetNextRoute(route);
            UssdResponse ussdResponse = await OnResponse(context);

            // Process auto dial requests, i.e. making requests like *714*2*3*1*1# when
            // the service code is *714*2# equivalent to texting *714*2# followed by
            // 3, 1 and 1 in three user inputs.
            if (ussdResponse.AutoDialOn && !ussdResponse.IsRelease)
            {
                UssdRequest ussdRequest = context.Request;
                string initiationMessage = ussdRequest.Message;
                string serviceCode = ussdRequest.ServiceCode;

                // To make searching for dial string and split more
                // straightforward, replace # with *.
                initiationMessage = initiationMessage.Replace("#", "*");
                serviceCode = serviceCode.Replace("#", "*");

                int extraIndex = initiationMessage.IndexOf(serviceCode);
                if (extraIndex == -1)
                {
                    throw new Exception(string.Format(
                            "Service code {0} not found in initiation "
                                    + "message {1}", ussdRequest.ServiceCode,
                                    ussdRequest.Message));
                }

                string extra = initiationMessage.Substring(
                        extraIndex + serviceCode.Length);
                string[] codes = extra.Split(new string[] { "*" }, 
                    StringSplitOptions.RemoveEmptyEntries);

                int i = 0;
                while (i < codes.Length)
                {
                    string nextMessage = codes[i];
                    ussdRequest.Type = UssdRequestTypes.Response.ToString();
                    ussdRequest.Message = nextMessage;
                    ussdRequest.AutoDialIndex = i;
                    ussdRequest.AutoDialOriginated = true;
                    ussdResponse = await OnResponse(context);
                    if (ussdResponse.IsRelease || !ussdResponse.AutoDialOn)
                    {
                        break;
                    }
                    i++;
                }
            }
            return ussdResponse;
        }

        private static async Task<UssdResponse> OnResponse(UssdContext context)
        {
            while (true)
            {
                var exists = await context.SessionExists();
                if (!exists)
                {
                    throw new Exception("Session does not exist.");
                }
                var response = await context.SessionExecuteAction();
                if (!response.IsRelease)
                {
                    await context.SessionSetNextRoute(response.NextRoute);
                }
                if (response.IsRedirect)
                {
                    continue;
                }
                return response;
            }
        }
        #endregion


        #region Logging
        private static async Task PreLog(ILoggingStore store, UssdRequest request, DateTime startTime, 
            string arbitraryData)
        {
            if (store == null) return;
            if (request.RequestType != UssdRequestTypes.Initiation) return;
            var log = new UssdSessionLog
            {
                Mobile = request.Mobile,
                SessionId = request.SessionId,
                StartTime = startTime,
                ArbitraryData = arbitraryData,
            };
            await store.Create(log);
        }

        private static async Task PostLog(ILoggingStore store, UssdRequest request, UssdResponse response,
            DateTime startTime, DateTime endTime)
        {
            if (store == null) return;
            var log = await store.Find(request.SessionId);
            if (log == null) return;
            log.EndTime = endTime;
            log.DurationInMilliseconds = endTime.Subtract(log.StartTime).TotalMilliseconds;
            log.ErrorStackTrace = response.Exception == null ? null : response.Exception.StackTrace;
            var entry = new UssdSessionLogEntry
            {
                StartTime = startTime,
                EndTime = endTime,
                DurationInMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,
                UssdRequest = request,
                UssdResponse = response,
            };
            await store.Update(log);
            await store.AddEntry(request.SessionId, entry);
            store.Dispose();
        }
        #endregion Logging

        private static List<string> GetInitiationMessages(UssdRequest request)
        {
            if (request.RequestType != UssdRequestTypes.Initiation) return null;
            var serviceCode = request.ServiceCode;
            var message = request.Message.Substring(0, request.Message.Length - 1);
            if (serviceCode == message) return null;
            var messages = new List<string>()
            {
                request.ServiceCode,
            };
            message = message.Substring(serviceCode.Length + 1);
            List<string> messageList = message.Split('*').ToList();
            return messages.Concat(messageList).ToList();
        } 
    }
}
