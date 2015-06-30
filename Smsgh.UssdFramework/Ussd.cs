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
            // TODO: auto process sub dial
            //var messages = GetInitiationMessages(request);
            //if (messages == null)
            //{
            //    return
            //        await
            //            ProcessRequest(store, request, initiationController, initiationAction, data, loggingStore,
            //                arbitraryLogData);
            //}
            //UssdResponse response = null;
            //for (int i = 0; i < messages.Count; i++)
            //{
            //    request.Message = messages[i];
            //    if (i != 0) request.Type = UssdRequestTypes.Response.ToString();
            //    bool dispose = (i == messages.Count-1);
            //    response =
            //        await
            //            ProcessRequest(store, request, initiationController, initiationAction, data, loggingStore,
            //                arbitraryLogData, dispose);
            //}
            //return response;
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
            ILoggingStore loggingStore = null, string arbitraryLogData = null, bool dispose = true)
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
                    default:
                        response = await OnResponse(context);
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
                if (dispose) context.Dispose();
                endTime = DateTime.UtcNow;
            }
            await PostLog(loggingStore, request, response, startTime, endTime, dispose);
            return response;
        }

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
            DateTime startTime, DateTime endTime, bool dispose)
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
            if (dispose) store.Dispose();
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
