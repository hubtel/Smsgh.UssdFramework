# Smsgh.UssdFramework

This is a natural successor to our [UssdFramework](http://github.com/smsgh/ussd-framework).

## Motivation

The previous [UssdFramework](http://github.com/smsgh/ussd-framework) was a nice foundation, but it fell short in a few areas.

* Explicit routing.
* Integrated session store.
* Step based flow.
* Unnecessary abstractions that limit flexiblity.

__Smsgh.UssdFramework__ addresses these and a lot more by focusing on simplicity.

__Smsgh.UssdFramework__ allows you to easily build dynamic USSD applications on our [USSD platform](http://developers.smsgh.com/documentations/ussd).

## Key Features

* Automated session management.
* Fully asynchronous.
* Pluggable session store. `RedisStore` included.
* ASP.NET MVC / Web Api -ish controllers. 
* Dynamic routing.
* `DataBag` helper in controllers for caching data across requests.
* Create forms to collect user input.
* Session logging (optional).

## Quick Start

### Dependencies

Smsgh.UssdFramework requires a session store. This defaults to [Redis](http://redis.io).

On Windows, Redis can be installed using [github.com/MSOpenTech/redis](https://github.com/MSOpenTech/redis).


### Installation

The easiest way to install Smsgh.UssdFramework is via [NuGet](http://www.nuget.org/).

```
PM> Install-Package Smsgh.UssdFramework
```

### Setup

To process USSD requests, make a static call to `Ussd.Process`.

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
            return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Start"));
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

namespace Smsgh.UssdFramework.Demo.UssdControllers
{
    public class MainController : UssdController
    {
        public async Task<UssdResponse> Start()
        {
            var menu = UssdMenu.New("Welcome", Environment.NewLine + "by SMSGH")
                .AddItem("Greet me", "GreetingForm")
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

        public async Task<UssdResponse> Exit()
        {
            return await Task.FromResult(Render("Bye bye!"));
        } 
    }
}
```

And that's it!

See [Smsgh.UssdFramework.Demo](https://github.com/smsgh/Smsgh.UssdFramework/tree/master/Smsgh.UssdFramework.Demo) folder in source for full sample source code.

You can simulate USSD sessions using [USSD Mocker](https://github.com/smsgh/ussd-mocker).


## Logging

It is often convenient to log USSD sessions. Smsgh.UssdFramework supports logging to [MongoDB](https://www.mongodb.org/).

### Dependencies

Visit [mongodb.org/downloads](https://www.mongodb.org/downloads) to install MongoDB.

### Installation

Install `Smsgh.UssdFramework.Logging` from [NuGet](http://www.nuget.org/).

```
PM> Install-Package Smsgh.UssdFramework.Logging
```

### Setup 

To enable logging pass an instance of `MongoDbLoggingStore` when calling `Ussd.Process`.

```c#
using System.Threading.Tasks;
using System.Web.Http;
using Smsgh.UssdFramework.Logging;
using Smsgh.UssdFramework.Stores;

namespace Smsgh.UssdFramework.Demo.Controllers
{
    public class DefaultController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Index(UssdRequest request)
        {
            return Ok(await Ussd.Process(new RedisStore(), request, "Main", "Start", null, 
                new MongoDbLoggingStore("mongodb://localhost", "demoussd")));
        } 
    }
}
```

That's it! 

You now have detailed session logs in your MongoDB database. You can use a tool like [UMongo](http://http://edgytech.com/umongo/) to view logs.


# License

MIT
