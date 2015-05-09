namespace Smsgh.UssdFramework.Demo.NancyFX
{
    using Nancy;
    using Nancy.ModelBinding;

    using Smsgh.UssdFramework.Stores;

    public class DefaultModule : NancyModule
    {
        private readonly IStore store;

        public DefaultModule(IStore store)
        {
            this.store = store;

            Get["/"] = parameters => "Welcome to the sample NancyFX USSD app. Make a POST to / to continue";

            Post["/", true] = async (_, token) =>
                {
                    var request = this.Bind<UssdRequest>();
                    return await Ussd.Process(store, request, "Main", "Menu").ConfigureAwait(false);
                };
        }
    }
}
