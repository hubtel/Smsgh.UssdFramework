using System;
using System.Collections.Generic;

namespace Smsgh.UssdFramework.LoggingStores
{
    public class UssdSessionLog
    {
        public string Mobile { get; set; }
        public string SessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? DurationInMilliseconds { get; set; }
        public List<UssdSessionLogEntry> Entries { get; set; }
        public string ArbitraryData { get; set; }
        public string ErrorTrace { get; set; }

        public UssdSessionLog()
        {
            Entries = new List<UssdSessionLogEntry>();
        }
    }

    public class UssdSessionLogEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DurationInMilliseconds { get; set; }
        public UssdRequest UssdRequest { get; set; }
        public UssdResponse UssdResponse { get; set; }
    }
}
