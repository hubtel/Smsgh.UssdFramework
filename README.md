# Smsgh.UssdFramework

This is a natural successor to our [UssdFramework](http://github.com/smsgh/ussd-framework).

## Motivation

The previous [UssdFramework](http://github.com/smsgh/ussd-framework) was a nice foundation, but it fell short in a few areas.

* Explicit routing.
* Integrated session store.
* Step based flow.
* Unnecessary abstractions that limits flexiblity.

__Smsgh.UssdFramework__ addresses these and a lot more by focusing on simplicity.

__Smsgh.UssdFramework__ allows you to easily build dynamic USSD applications on our [USSD platform](http://developers.smsgh.com/documentations/ussd).

## Key Features

* Automated session management.
* Fully asynchronous.
* Pluggable session store. `RedisStore` included.
* ASP.NET MVC / Web Api -ish controllers. 
* Dynamic routing.
* `DataBag` helper in controllers for caching data across requests.

## Usage

### Installation

The easiest way to install Smsgh.UssdFramework is via [NuGet](http://www.nuget.org/).

```
Install-Package Smsgh.UssdFramework
```

### Setup

To process USSD requests, just call `Ussd.Process`.

Here is a sample setup in an [ASP.NET Web Api](http://www.asp.net/web-api) action.

```c#
using System.Threading.Tasks;
using System.Web.Http;
using Smsgh.UssdFramework.Stores;

namespace Smsgh.UssdFramework.Demo.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Index(UssdRequest request)
        {
            return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Menu"));
        } 
    }
}
```

This tells the framework to route all initial requests to `MainController`'s `Menu` action, which is a method with signature `Func<Task<UssdResponse>>`.

### Controller actions

Next we create our `MainController` which inherits `UssdController`.

```c#
using System;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Demo.Controllers
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
```

And that's it!

See [Smsgh.UssdFramework.Demo](https://github.com/smsgh/Smsgh.UssdFramework/tree/master/Smsgh.UssdFramework.Demo) folder in source for full sample source code.

You can simulate USSD sessions using [USSD Mocker](https://github.com/smsgh/ussd-mocker).


# License

MIT
