using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCWALib.Models
{
    public class OnlineMeeting
    {
        public string OnlineMeetingUri { get; set; }
        public string OrganizerUri { get; set; }
        public string OrganizerName { get; set; }
        public string DisclaimerBody { get; set; }
        public string DisclaimerTitle { get; set; }
        public string HostingNetwork { get; set; }
        public string LargeMeeting { get; set; }
        public string JoinUrl { get; set; }
        public IDictionary<string, string> Links { get; set; } 
    }
}
