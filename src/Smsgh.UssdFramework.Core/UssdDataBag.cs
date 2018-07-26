using System.Threading.Tasks;
using Smsgh.UssdFramework.Core.Stores;

namespace Smsgh.UssdFramework.Core
{
    public class UssdDataBag
    {
        private string DataBagKey { get; set; }
        private IStore Store { get; set; }

        internal UssdDataBag(IStore store, string dataBagKey)
        {
            Store = store;
            DataBagKey = dataBagKey;
        }

        public async Task Set(string key, string value)
        {
            await Store.SetHashValue(DataBagKey, key, value);
        }

        public async Task<string> Get(string key)
        {
            return await Store.GetHashValue(DataBagKey, key);
        }

        public async Task<bool> Exists(string key)
        {
            return await Store.HashValueExists(DataBagKey, key);
        }

        public async Task Delete(string key)
        {
            await Store.DeleteHashValue(DataBagKey, key);
        }

        public async Task Clear()
        {
            await Store.DeleteHash(DataBagKey);
        }
    }
}
