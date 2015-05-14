using System;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Demo.UssdControllers
{
    public class MainController : UssdController
    {
        public async Task<UssdResponse> Start()
        {
            var display = "Welcome" + Environment.NewLine
                          + "1. Greet me" + Environment.NewLine
                          + "2. Exit";
            var menu = UssdMenu.Create(display)
                .Redirect("1", "Greeting")
                .Redirect("2", "Exit");
            return await RenderMenu(menu);
        }

        public async Task<UssdResponse> Greeting()
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
            return Render(greeting);
        }

        public async Task<UssdResponse> Exit()
        {
            return await Task.FromResult(Render("Bye bye!"));
        } 
    }
}