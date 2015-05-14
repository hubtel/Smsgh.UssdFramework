using System;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Demo.UssdControllers
{
    public class MainController : UssdController
    {
        public async Task<UssdResponse> Menu()
        {
            return Render("Welcome" + Environment.NewLine
                          + "1. Greet me" + Environment.NewLine
                          + "2. Exit",
                "MenuProcessor");
        }

        public async Task<UssdResponse> MenuProcessor()
        {
            switch (Request.TrimmedMessage)
            {
                case "1":
                    return Render("Enter Name", "Name");
                case "2":
                    return Render("Bye bye");
                default:
                    return Render("Invalid menu choice");
            }
        }

        public async Task<UssdResponse> Name()
        {
            var hour = DateTime.UtcNow.Hour;
            var greeting = string.Empty;
            if (hour < 12)
            {
                greeting = "Good morning";
            }
            if (hour >= 12)
            {
                greeting = "Good afternoon";
            }
            if (hour >= 18)
            {
                greeting = "Good night";
            }
            return Render(string.Format("{0}, {1}",
                greeting, Request.TrimmedMessage));
        } 
    }
}