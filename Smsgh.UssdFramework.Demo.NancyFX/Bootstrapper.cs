namespace Smsgh.UssdFramework.Demo.NancyFX
{
    using Nancy;
    using Nancy.TinyIoc;

    using Newtonsoft.Json;

    using Smsgh.UssdFramework.Stores;

    class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            container.Register<JsonSerializer, CustomJsonSerializer>();
            container.Register<IStore, InMemoryStore>().AsSingleton();
        }
    }
}
