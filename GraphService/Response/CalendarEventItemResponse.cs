using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGraph.Response
{
    public class CalendarEventItem
    {
        [JsonProperty("subject")]
        public string Subject
        {
            get;
            set;
        }

        [JsonProperty("start")]
        public DateInfoResponse StartDateTime
        {
            get;
            set;
        }
        [JsonProperty("isAllDay")]
        public bool IsAllDay
        {
            get;
            set;
        }
    }

    public class DateInfoResponse
    {
        [JsonProperty("datetime")]
        public DateTime dateTime
        {
            get;
            set;
        }

        public string timeZone
        {
            get;
            set;
        }
    }
}
