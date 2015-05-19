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
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public List<UssdMenuItem> Items { get; set; }
        public UssdMenuItem ZeroItem { get; set; }

        public UssdMenu() : this(null, null)
        {
            
        }
        private UssdMenu(string prefix, string postfix)
        {
            Prefix = prefix;
            Postfix = postfix;
            Items = new List<UssdMenuItem>();
            ZeroItem = null;
        }

        /// <summary>
        /// Create a new USSD menu.
        /// </summary>
        /// <param name="prefix">Displayed before menu items.</param>
        /// <param name="postfix">Displayed after menu items.</param>
        /// <returns></returns>
        public static UssdMenu New(string prefix, string postfix = null)
        {
            return new UssdMenu(prefix, postfix);
        }

        /// <summary>
        /// Add item to menu items.
        /// </summary>
        /// <param name="display"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public UssdMenu AddItem(string display, string action, string controller = null)
        {
            Items.Add(new UssdMenuItem()
            {
                Display = display,
                Action = action,
                Controller = controller,
            });
            return this;
        }

        /// <summary>
        /// Add an item to be rendered as last 0 option on menu
        /// </summary>
        /// <param name="display"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public UssdMenu AddZeroItem(string display, string action, string controller = null)
        {
            ZeroItem = new UssdMenuItem()
            {
                Display = display,
                Action = action,
                Controller = controller,
            };
            return this;
        }

        /// <summary>
        /// Render the menu to be displayed.
        /// </summary>
        /// <returns></returns>
        public string Render()
        {
            var display = string.Empty;
            display += Prefix + Environment.NewLine;
            for (int i = 0; i < Items.Count; i++)
            {
                display += string.Format("{0}. {1}" + Environment.NewLine, 
                    i+1, Items[i].Display);
            }
            if (ZeroItem != null)
            {
                display += string.Format("{0}. {1}" + Environment.NewLine,
                    0, ZeroItem.Display);
            }
            if (!string.IsNullOrWhiteSpace(Postfix))
            {
                display += Postfix;
            }
            return display;
        }
    }

    public class UssdMenuItem
    {
        public string Display { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
