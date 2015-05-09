namespace Smsgh.UssdFramework.Demo.NancyFX
{
    using System;
    using System.Threading.Tasks;

    public class MainController : UssdController
    {
        public async Task<UssdResponse> Menu()
        {
            return this.Render("Welcome" + Environment.NewLine
                          + "1. Greet me" + Environment.NewLine
                          + "2. Exit",
                "MenuProcessor");
        }

        public async Task<UssdResponse> MenuProcessor()
        {
            switch (this.Request.TrimmedMessage)
            {
                case "1":
                    return this.Render("Enter Name", "Name");
                case "2":
                    return this.Render("Bye bye");
                default:
                    return this.Render("Invalid menu choice");
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
            return this.Render(string.Format("{0}, {1}",
                greeting, this.Request.TrimmedMessage));
        } 
    }
}