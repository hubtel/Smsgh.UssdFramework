using System;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Demo.UssdControllers
{
    public class MainController : UssdController
    {
        public async Task<UssdResponse> Start()
        {
            var menu = UssdMenu.New("Welcome", Environment.NewLine + "by SMSGH")
                .AddItem("Greet me", "GreetingForm")
                .AddItem("What's the time?", "Time")
                .AddZeroItem("Exit", "Exit");
            return await RenderMenu(menu);
        }


        public async Task<UssdResponse> GreetingForm()
        {
            var form = UssdForm.New("Greet Me!", "Greeting")
                .AddInput(UssdInput.New("Name"))
                .AddInput(
                    UssdInput.New("Sex")
                        .Option("M", "Male")
                        .Option("F", "Female"));
            return await RenderForm(form);
        } 

        public async Task<UssdResponse> Greeting()
        {
            var formData = await GetFormData();
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
            var name = formData["Name"];
            var prefix = formData["Sex"] == "M" ? "Master" : "Madam";
            return Render(string.Format("{0}, {1} {2}!", greeting, prefix, name));
        }

        public async Task<UssdResponse> Time()
        {
            return await Task.FromResult(Render(string.Format("{0:t}", 
                DateTime.UtcNow)));
        }

        public async Task<UssdResponse> Exit()
        {
            return await Task.FromResult(Render("Bye bye!"));
        } 
    }
}