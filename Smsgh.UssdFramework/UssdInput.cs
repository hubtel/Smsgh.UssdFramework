using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework
{
    /// <summary>
    /// USSD input.
    /// </summary>
    public class UssdInput
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public List<UssdInputOption> Options { get; set; }
        public bool HasOptions { get { return !(Options == null || Options.Count == 0); } }

        public UssdInput()
        {
            Options = new List<UssdInputOption>();
        }

        /// <summary>
        /// New USSD input.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public static UssdInput New(string name, string displayName = null)
        {
            var input = new UssdInput
            {
                Name = name,
                DisplayName = displayName,
            };
            return input;
        }

        /// <summary>
        /// Add possible input option to USSD input. 
        /// Makes input a selection based input.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="displayValue"></param>
        /// <returns></returns>
        public UssdInput Option(string value, string displayValue = null)
        {
            Options.Add(UssdInputOption.New(value, displayValue));
            return this;
        }
    }

    public class UssdInputOption
    {
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public UssdInputOption()
        {
        }

        public static UssdInputOption New(string value, string displayValue = null)
        {
            var option = new UssdInputOption
            {
                Value = value,
                DisplayValue = displayValue,
            };
            return option;
        }
    }
}
