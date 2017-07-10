using System;
using System.IO;
using Microsoft.Owin;
using Owin;
using Smsgh.UssdFramework.Stores;

[assembly: OwinStartup(typeof(Smsgh.UssdFramework.Demo.Startup))]

namespace Smsgh.UssdFramework.Demo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            
            ConnectionManager.Init();
        }
    }
}
