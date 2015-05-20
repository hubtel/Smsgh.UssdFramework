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
        public string Header { get; set; }
        public string Footer { get; set; }
        public List<UssdMenuItem> Items { get; set; }
        public UssdMenuItem ZeroItem { get; set; }

        public UssdMenu() : this(null, null)
        {
            
        }
        private UssdMenu(string header, string footer)
        {
            Header = header;
            Footer = footer;
            Items = new List<UssdMenuItem>();
            ZeroItem = null;
        }

        /// <summary>
        /// Create a new USSD menu.
        /// </summary>
        /// <param name="header">Displayed before menu items.</param>
        /// <param name="footer">Displayed after menu items.</param>
        /// <returns></returns>
        public static UssdMenu New(string header, string footer = null)
        {
            return new UssdMenu(header, footer);
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
        /// Add an item to be rendered as last 0 option on menu.
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
            display += Header + Environment.NewLine;
            for (int i = 0; i < Items.Count; i++)
            {
                display += string.Format("{0}. {1}" + Environment.NewLine, 
                    i+1, Items[i].Display);
            }
            if (ZeroItem != null)
            {
                display += string.Format("0. {0}" + Environment.NewLine,
                    ZeroItem.Display);
            }
            if (!string.IsNullOrWhiteSpace(Footer))
            {
                display += Footer;
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
