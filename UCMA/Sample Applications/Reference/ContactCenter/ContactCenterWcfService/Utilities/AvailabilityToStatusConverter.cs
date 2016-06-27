
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    internal static class AvailabilityToStatusConverter
    {
       private static readonly List<StatusAvailabilityRange> StatusAvailabilityRanges =
       new List<StatusAvailabilityRange> { 
              new StatusAvailabilityRange { Begin = long.MinValue, End = -1, Status = "Invalid" },
              new StatusAvailabilityRange { Begin = 0, End = 2999, Status = "None" },
              new StatusAvailabilityRange { Begin = 3000, End = 4499, Status = "Available" },
              new StatusAvailabilityRange { Begin = 4500, End = 5999, Status = "Away" }, //IdleOnline
              new StatusAvailabilityRange { Begin = 6000, End = 7499, Status = "Busy" },
              new StatusAvailabilityRange { Begin = 7500, End = 8999, Status = "Away" }, //IdleBusy
              new StatusAvailabilityRange { Begin = 9000, End = 11999, Status = "Do Not Disturb" },
              new StatusAvailabilityRange { Begin = 12000, End = 14999, Status = "Be Right Back" },
              new StatusAvailabilityRange { Begin = 15000, End = 17999, Status = "Away" },
              new StatusAvailabilityRange { Begin = 18000, End = long.MaxValue, Status = "Unavailable" },
            };

        public static string Convert(long availability)
        {
            return StatusAvailabilityRanges.First(s => s.InRange(availability)).Status;
        }

        //helper class to handle converting range of presence values
        internal class StatusAvailabilityRange
        {

            internal long Begin { get; set; }
            internal long End { get; set; }
            internal string Status { get; set; }

            internal bool InRange(long availability)
            {
                return (availability >= Begin && availability <= End);
            }
        }
    }
}
