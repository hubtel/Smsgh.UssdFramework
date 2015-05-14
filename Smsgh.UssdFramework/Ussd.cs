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
    public static class Ussd
    {
        #region Events
        private static async Task<UssdResponse> OnInitiation(UssdContext context, 
            string route)
        {
            await context.SessionClose();
            await context.SessionSetNextRoute(route);
            return await OnResponse(context);
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

        /// <summary>
        /// Process USSD
        /// </summary>
        /// <param name="store">Session store</param>
        /// <param name="request"></param>
        /// <param name="initiationController">Initiation controller</param>
        /// <param name="initiationAction">Initiation action</param>
        /// <param name="data"></param>
        /// <param name="loggingStore">Logging store</param>
        /// <param name="arbitraryLogData">Arbitrary data to add to session log</param>
        /// <returns></returns>
        public static async Task<UssdResponse> Process(IStore store, UssdRequest request,
            string initiationController, string initiationAction,
            Dictionary<string, string> data = null, ILoggingStore loggingStore = null, string arbitraryLogData = null)
        {
            var startTime = DateTime.UtcNow;
            if (data == null)
            {
                data = new Dictionary<string, string>();
            }
            var context = new UssdContext(store, request, data);
            UssdResponse response;
            DateTime endTime;
            string error = null;
            if (loggingStore != null) 
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
                    default:
                        response = await OnResponse(context);
                        break;
                }
            }
            catch (Exception e)
            {
                response = UssdResponse.Render(e.Message);
                error = e.StackTrace;
            }
            finally
            {
                context.Dispose();
                endTime = DateTime.UtcNow;
            }
            if (loggingStore != null)
                await PostLog(loggingStore, request, response, startTime, endTime, error);
            return response;
        }

        private static async Task PreLog(ILoggingStore store, UssdRequest request, DateTime startTime, 
            string arbitraryData)
        {
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
            DateTime startTime, DateTime endTime, string error)
        {
            var log = await store.Find(request.SessionId);
            if (log == null) return;
            var entry = new UssdSessionLogEntry
            {
                StartTime = startTime,
                EndTime = endTime,
                DurationInMilliseconds = endTime.Subtract(startTime).TotalMilliseconds,
                UssdRequest = request,
                UssdResponse = response,
                ErrorTrace = error,
            };
            log.EndTime = endTime;
            log.DurationInMilliseconds = endTime.Subtract(log.StartTime).TotalMilliseconds;
            await store.Update(log);
            await store.AddEntry(request.SessionId, entry);
        }
    }
}
