using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework
{
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

        public static UssdInput New(string name, string displayName = null)
        {
            var input = new UssdInput
            {
                Name = name,
                DisplayName = displayName,
            };
            return input;
        }

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
