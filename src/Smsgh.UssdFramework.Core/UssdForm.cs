using System.Collections.Generic;

namespace Smsgh.UssdFramework.Core
{
    /// <summary>
    /// Setup a new form to collect a series of inputs.
    /// </summary>
    public class UssdForm
    {
        public string Title { get; set; }
        public List<UssdInput> Inputs { get; set; }
        public int ProcessingPosition { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public UssdForm() : this(null, null, null)
        {
            
        }
        UssdForm(string title, string action, string controller)
        {
            Title = title;
            Inputs = new List<UssdInput>();
            Action = action;
            Controller = controller;
            ProcessingPosition = 0;
            Data = new Dictionary<string, string>();
        }

        /// <summary>
        /// New creates an instance of UssdForm
        /// </summary>
        /// <param name="title"></param>
        /// <param name="action"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static UssdForm New(string title, string action, string controller = null)
        {
            return new UssdForm(title, action, controller);
        }

        /// <summary>
        /// Add input field to form.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public UssdForm AddInput(UssdInput input)
        {
            Inputs.Add(input);
            return this;
        }
    }
}
