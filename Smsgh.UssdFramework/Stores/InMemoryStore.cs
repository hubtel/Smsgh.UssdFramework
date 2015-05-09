#region *   License     *
/*
    SimpleHelpers - MemoryCache   

    Copyright © 2013 Khalid Salomão

    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the “Software”), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE. 

    License: http://www.opensource.org/licenses/mit-license.php
    Website: https://github.com/khalidsalomao/SimpleHelpers.Net
 */
#endregion

namespace Smsgh.UssdFramework.Stores
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Simple in-memory <see cref="IStore"/> implementation based on https://github.com/khalidsalomao/SimpleHelpers.Net/blob/master/SimpleHelpers/MemoryCache.cs />
    /// </summary>
    public class InMemoryStore : IStore
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, CachedItem> m_cacheMap =
            new System.Collections.Concurrent.ConcurrentDictionary<string, CachedItem>(StringComparer.Ordinal);

        private static TimeSpan m_timeout = TimeSpan.FromMinutes(5);

        private static TimeSpan m_maintenanceStep = TimeSpan.FromMinutes(5);

        private static bool m_ignoreNullValues = true;

        /// <summary>
        /// Expiration TimeSpan of stored items. Default value is 5 minutes.
        /// </summary>
        public static TimeSpan Expiration
        {
            get
            {
                return m_timeout;
            }
            set
            {
                m_timeout = value;
            }
        }

        /// <summary>
        /// Interval duration between checks for expired cached items by the internal timer thread.
        /// Default value is 5 minutes.
        /// </summary>
        public static TimeSpan MaintenanceStep
        {
            get
            {
                return m_maintenanceStep;
            }
            set
            {
                if (m_maintenanceStep != value)
                {
                    m_maintenanceStep = value;
                    StopMaintenance();
                    StartMaintenance();
                }
            }
        }

        /// <summary>
        /// If the Set method should silently ignore any null value.
        /// Default value is true.
        /// </summary>
        public static bool IgnoreNullValues
        {
            get
            {
                return m_ignoreNullValues;
            }
            set
            {
                m_ignoreNullValues = value;
            }
        }

        public void Dispose() { }

        public Task<string> GetHashValue(string name, string key)
        {
            return GetValue(key);
        }

        public Task SetHashValue(string name, string key, string value)
        {
            return SetValue(key, value);
        }

        public Task<bool> HashExists(string name)
        {
            return Task.FromResult(m_cacheMap.ContainsKey(name));
        }

        public Task<bool> HashValueExists(string name, string key)
        {
            return Task.FromResult(GetValue(key) != null);
        }

        public Task DeleteHash(string name)
        {
            return DeleteValue(name);
        }

        public Task DeleteHashValue(string name, string key)
        {
            return DeleteValue(key);
        }

        public Task SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                if (m_ignoreNullValues)
                {
                    return Task.FromResult(false);
                }
                ;
                throw new ArgumentNullException("key");
            }
            // add or update item
            m_cacheMap[key] = new CachedItem { Updated = DateTime.UtcNow, Data = value };
            // check if the timer is active
            StartMaintenance();

            return Task.FromResult(true);
        }

        public Task<string> GetValue(string key)
        {
            if (key != null)
            {
                CachedItem item;
                if (m_cacheMap.TryGetValue(key, out item))
                {
                    return Task.FromResult(item.Data);
                }
            }
            return null;
        }

        public Task<bool> ValueExists(string key)
        {
            return Task.FromResult(GetValue(key) != null);
        }

        public Task DeleteValue(string key)
        {
            if (key != null)
            {
                CachedItem item;
                m_cacheMap.TryRemove(key, out item);
            }

            return Task.FromResult(true);
        }

        private class CachedItem
        {
            public DateTime Updated;

            public string Data;
        }

        #region *   Cache Maintenance Task  *

        private static System.Threading.Timer m_maintenanceTask = null;

        private static readonly object m_lock = new object();

        private static int m_executing = 0;

        private static void StartMaintenance()
        {
            if (m_maintenanceTask == null)
            {
                lock (m_lock)
                {
                    if (m_maintenanceTask == null)
                    {
                        m_maintenanceTask = new System.Threading.Timer(
                            ExecuteMaintenance,
                            null,
                            m_maintenanceStep,
                            m_maintenanceStep);
                    }
                }
            }
        }

        private static void StopMaintenance()
        {
            lock (m_lock)
            {
                if (m_maintenanceTask != null)
                {
                    m_maintenanceTask.Dispose();
                }
                m_maintenanceTask = null;
            }
        }

        private static void ExecuteMaintenance(object state)
        {
            // check if a step is already executing
            if (System.Threading.Interlocked.CompareExchange(ref m_executing, 1, 0) != 0)
            {
                return;
            }
            // try to fire OnExpiration event
            try
            {
                // stop timed task if queue is empty
                if (m_cacheMap.Count == 0)
                {
                    StopMaintenance();
                    // check again if the queue is empty
                    if (m_cacheMap.Count != 0)
                    {
                        StartMaintenance();
                    }
                }
                else
                {
                    CachedItem item;
                    DateTime oldThreshold = DateTime.UtcNow - m_timeout;

                    // select elegible records
                    var expiredItems = m_cacheMap.Where(i => i.Value.Updated < oldThreshold).Select(i => i.Key);
                    // remove from cache and fire OnExpiration event
                    foreach (var key in expiredItems)
                    {
                        m_cacheMap.TryRemove(key, out item);
                    }
                }
            }
            finally
            {
                // release lock
                System.Threading.Interlocked.Exchange(ref m_executing, 0);
            }
        }

        #endregion
    }
}