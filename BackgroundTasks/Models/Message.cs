using System;
using System.Collections.Generic;
using SQLite.Net;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;

namespace RWPBGTasks
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
