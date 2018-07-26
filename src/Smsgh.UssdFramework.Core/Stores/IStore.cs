using System;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Core.Stores
{
    public interface IStore : IDisposable
    {
        // Hash store
        Task<string> GetHashValue(string name, string key);
        Task SetHashValue(string name, string key, string value);
        Task<bool> HashExists(string name);
        Task<bool> HashValueExists(string name, string key);
        Task DeleteHash(string name);
        Task DeleteHashValue(string name, string key);

        // Key-Value store
        Task SetValue(string key, string value);
        Task<string> GetValue(string key);
        Task<bool> ValueExists(string key);
        Task DeleteValue(string key);
    }
}
