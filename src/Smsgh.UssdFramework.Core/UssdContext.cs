using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Smsgh.UssdFramework.Core.Stores;

namespace Smsgh.UssdFramework.Core
{
    internal class UssdContext : IDisposable
    {
        private string NextRouteKey { get { return Request.Mobile + "NextRoute"; } }
        private string DataBagKey { get { return Request.Mobile + "DataBag"; } }
        private Func<Task<UssdResponse>> Action { get; set; }
        private UssdRequest Request { get; set; }
        private IStore Store { get; set; }
        private Dictionary<string, string> Data { get; set; } 
        private UssdDataBag DataBag { get; set; }

        public UssdContext(IStore store, UssdRequest request, Dictionary<string, string> data)
        {
            Store = store;
            Request = request;
            Data = data;
            DataBag = new UssdDataBag(Store, DataBagKey);
        }

        #region Session Management
        /// <summary>
        /// Set the next action to route to.
        /// </summary>
        /// <param name="nextRoute">route to next action</param>
        /// <returns></returns>
        internal async Task SessionSetNextRoute(string nextRoute)
        {
            await Store.SetValue(NextRouteKey, nextRoute);
        }

        /// <summary>
        /// Close session.
        /// </summary>
        /// <returns></returns>
        internal async Task SessionClose()
        {
            await Store.DeleteValue(NextRouteKey);
            await Store.DeleteHash(DataBagKey);
        }

        /// <summary>
        /// Verify session exists.
        /// </summary>
        /// <returns></returns>
        internal async Task<bool> SessionExists()
        {
            return await Store.ValueExists(NextRouteKey);
        }

        /// <summary>
        /// Set contexts's current action.
        /// </summary>
        /// <returns></returns>
        private async Task SessionSetAction()
        {
            var route = await Store.GetValue(NextRouteKey);
            var routeArray = route.Split('.');
            if (routeArray.Length != 2)
            {
                throw new Exception("Invalid route format. Must be `SomeController.Action`." +
                                    "Current route is " + route);
            }
            var controllerName = routeArray[0];
            var actionName = routeArray[1];
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            UssdController controller = null;
            foreach (var assembly in assemblies)
            {
                Type[] allTypes = null;

                try
                {
                    allTypes = assembly.GetTypes();
                }
                catch (Exception exception)
                {
                    allTypes = null;
                }

                if (allTypes != null)
                {
                    foreach (var type in allTypes)
                    {
                        if (type.Name == controllerName
                            && type.IsSubclassOf(typeof(UssdController)))
                        {
                            controller = (UssdController)assembly.CreateInstance(type.FullName);
                            break;
                        }
                    }
                }
            }
            if (controller == null)
            {
                throw new Exception(controllerName + " could not be found.");
            }
            controller.Request = Request;
            controller.DataBag = DataBag;
            controller.Data = Data;
            controller.FormData = await controller.GetFormData();
            var methodInfo = controller.GetType().GetMethod(actionName);
            if (methodInfo == null)
            {
                throw new Exception(string.Format("{0} does not have action {1}.",
                    controllerName, actionName));
            }
            this.Action = async () =>
            {
                object[] args = {};
                var response = (Task<UssdResponse>) methodInfo.Invoke(controller, args);
                return await response;
            };
        }

        /// <summary>
        /// Execute the current action.
        /// </summary>
        /// <returns></returns>
        internal async Task<UssdResponse> SessionExecuteAction()
        {
            await this.SessionSetAction();
            return await this.Action();
        } 
        #endregion

        public void Dispose()
        {
            Store.Dispose();
        }
    }
}
