using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSGraph.Response
{
    public class ParseCalendarEventResponse
    {
            public ObservableCollection<CalendarEventItem> Value
            {
                get;
                set;
            }
        [JsonProperty("@odata.nextLink")]
        public string NextLink
        {
            get;
            set;
        }
    }
}
