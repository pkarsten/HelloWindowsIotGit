using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UwpSqliteDal
{
    public sealed class CalendarEvent
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Subject {get;set;}
        public DateTime StartDateTime {get;set;}
        public bool TodayEvent{ get; set; }
        public bool IsAllDay { get; set; }
        public bool IgnoreEvent { get; set; }
    }
}
