using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smsgh.UssdFramework
{
    public class UssdController : IDisposable
    {
        protected const string MenuProcessorDataKey = "MenuProcessorData";

        public UssdRequest Request { get; set; }
        public Dictionary<string, string> Data { get; set; } 
        public UssdDataBag DataBag { get; set; }

        #region Responders

        private string Route(string action, string controller = null)
        {
            if (controller == null)
            {
                controller = this.GetType().Name;
            }
            else
            {
                controller += "Controller";
            }
            return string.Format("{0}.{1}", controller, action);
        }

        public UssdResponse Redirect(string action, string controller = null)
        {
            return UssdResponse.Redirect(Route(action, controller));
        }


        public UssdResponse Render(string message, string action = null, 
            string controller = null)
        {
            string route = null;
            if (action != null)
            {
                route = Route(action, controller);
            }
            return UssdResponse.Render(message, route);
        }

        public async Task<UssdResponse> RenderMenu(UssdMenu ussdMenu)
        {
            var json = JsonConvert.SerializeObject(ussdMenu.Choices);
            await DataBag.Set(MenuProcessorDataKey, json);
            return Render(ussdMenu.Display, "MenuProcessor");
        }
        #endregion

        public async Task<UssdResponse> MenuProcessor()
        {
            var json = await DataBag.Get(MenuProcessorDataKey);
            var choices = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (!choices.ContainsKey(Request.TrimmedMessage))
            {
                return Render("Menu choice does not exist.");
            }
            var route = choices[Request.TrimmedMessage];
            var routeList = route.Split(',');
            var action = routeList[0];
            string controller = null;
            if (routeList.Length == 2)
            {
                controller = routeList[1];
            }
            await DataBag.Delete(MenuProcessorDataKey);
            return Redirect(action, controller);
        } 


        public virtual void Dispose()
        {
        }
    }
}
