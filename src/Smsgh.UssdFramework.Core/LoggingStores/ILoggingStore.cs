using System;
using System.Linq;
using System.Threading.Tasks;

namespace Smsgh.UssdFramework.Core.LoggingStores
{
    public interface ILoggingStore : IDisposable
    {
        Task<IQueryable<UssdSessionLog>> FindAll(TimeSpan since);
        Task<UssdSessionLog> Find(string sessionId);
        Task Create(UssdSessionLog log);
        Task Update(UssdSessionLog log);
        Task AddEntry(string sessionId, UssdSessionLogEntry entry);
    }
}
