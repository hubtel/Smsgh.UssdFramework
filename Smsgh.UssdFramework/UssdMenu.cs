using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework
{
    /// <summary>
    /// USSD menu to redirect to appropriate choice.
    /// </summary>
    public class UssdMenu
    {
        public string Display { get; private set; }
        public Dictionary<string, string> Choices { get; private set; }

        private UssdMenu(string display)
        {
            Choices = new Dictionary<string, string>();
            Display = display;
        }

        /// <summary>
        /// Create a new USSD menu.
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        public static UssdMenu New(string display)
        {
            return new UssdMenu(display);
        }

        /// <summary>
        /// Redirect to appropriate menu choice.
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public UssdMenu Redirect(string choice, string action, string controller = null)
        {
            var value = action;
            if (controller != null)
            {
                value += ",";
                value += controller;
            }
            Choices.Add(choice, value);
            return this;
        }
    }
}
