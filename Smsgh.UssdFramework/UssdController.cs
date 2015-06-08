using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis.KeyspaceIsolation;

namespace Smsgh.UssdFramework
{
    public class UssdController : IDisposable
    {
        protected const string MenuProcessorDataKey = "MenuProcessorData";
        protected const string FormProcessorDataKey = "FormProcessorData";
        protected const string FormDataKey = "FormData";

        public UssdRequest Request { get; set; }
        public Dictionary<string, string> Data { get; set; } 
        public UssdDataBag DataBag { get; set; }
        public Dictionary<string, string> FormData { get; set; } 

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

        /// <summary>
        /// Redirect to specified <paramref name="controller"/>'s <paramref name="action"/>.
        /// If <paramref name="controller"/> is not specified this controller is used.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public UssdResponse Redirect(string action, string controller = null)
        {
            return UssdResponse.Redirect(Route(action, controller));
        }


        /// <summary>
        /// Render <paramref name="message"/>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Render USSD menu and redirect to appropriate user's choice.
        /// </summary>
        /// <param name="ussdMenu"></param>
        /// <returns></returns>
        public async Task<UssdResponse> RenderMenu(UssdMenu ussdMenu)
        {
            var json = JsonConvert.SerializeObject(ussdMenu);
            await DataBag.Set(MenuProcessorDataKey, json);
            return Render(ussdMenu.Render(), "MenuProcessor");
        }

        /// <summary>
        /// Render a form (series of inputs).
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public async Task<UssdResponse> RenderForm(UssdForm form)
        {
            var json = JsonConvert.SerializeObject(form);
            await DataBag.Set(FormProcessorDataKey, json);
            return Redirect("FormInputDisplay"); 
        } 
        #endregion


        #region Helpers

        /// <summary>
        /// Get Form's response data.
        /// </summary>
        /// <returns></returns>
        internal async Task<Dictionary<string, string>> GetFormData()
        {
            var json = await DataBag.Get(FormDataKey);
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return data;
            }
            catch (Exception)
            {
                return null;
            }
        } 
        #endregion

        #region Internal Actions

        public async Task<UssdResponse> MenuProcessor()
        {
            var json = await DataBag.Get(MenuProcessorDataKey);
            var menu = JsonConvert.DeserializeObject<UssdMenu>(json);
            UssdMenuItem item;
            try
            {
                var choice = Convert.ToInt16(Request.TrimmedMessage);
                if (choice == 0 && menu.ZeroItem != null)
                {
                    return Redirect(menu.ZeroItem.Action, menu.ZeroItem.Controller);
                }
                item = menu.Items[choice-1];
            }
            catch (Exception exception)
            {
                return Render(string.Format("Menu choice {0} does not exist.", 
                    Request.TrimmedMessage));
            }
            await DataBag.Delete(MenuProcessorDataKey);
            return Redirect(item.Action, item.Controller);
        }

        public async Task<UssdResponse> FormInputDisplay()
        {
            var form = await GetForm();
            var input = form.Inputs[form.ProcessingPosition];
            var displayName = string.IsNullOrWhiteSpace(input.DisplayName)
                ? input.Name : input.DisplayName;
            var message = string.Empty;
            if (!string.IsNullOrWhiteSpace(form.Title))
            {
                message += form.Title + Environment.NewLine;
            }
            if (!input.HasOptions)
            {
                message += string.Format("Enter {0}:" + Environment.NewLine, displayName);
            }
            else
            {
                message += string.Format("Choose {0}:" + Environment.NewLine, displayName);
                for (int i = 0; i < input.Options.Count; i++)
                {
                    var option = input.Options[i];
                    var value = string.IsNullOrWhiteSpace(option.DisplayValue)
                        ? option.Value : option.DisplayValue;
                    message += string.Format("{0}. {1}" + Environment.NewLine, i + 1, value);
                }
            }
            return Render(message, "FormInputProcessor");
        } 

        public async Task<UssdResponse> FormInputProcessor()
        {
            var form = await GetForm();
            var input = form.Inputs[form.ProcessingPosition];
            var key = input.Name;
            string value = null;
            if (!input.HasOptions)
            {
                value = Request.TrimmedMessage;
            }
            else
            {
                try
                {
                    var choice = Convert.ToInt16(Request.TrimmedMessage);
                    value = input.Options[choice - 1].Value;
                }
                catch (Exception exception)
                {
                    return Render(string.Format("Option {0} does not exist.", 
                        Request.TrimmedMessage));
                }
            }
            form.Data.Add(key, value);
            if (form.ProcessingPosition == (form.Inputs.Count - 1))
            {
                await DataBag.Delete(FormProcessorDataKey);
                var jsonData = JsonConvert.SerializeObject(form.Data);
                await DataBag.Set(FormDataKey, jsonData);
                return Redirect(form.Action, form.Controller);
            }
            ++form.ProcessingPosition;
            var json = JsonConvert.SerializeObject(form);
            await DataBag.Set(FormProcessorDataKey, json);
            return Redirect("FormInputDisplay");
        }

        private async Task<UssdForm> GetForm()
        {
            var json = await DataBag.Get(FormProcessorDataKey);
            var form = JsonConvert.DeserializeObject<UssdForm>(json);
            return form;
        } 

        #endregion


        public virtual void Dispose()
        {
        }
    }
}
