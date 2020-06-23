using System;
using System.Collections.Generic;
using SQLite;
using System.Text;
using System.Threading.Tasks;

namespace UwpSqliteDal
{
    public class Message
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Content { get; set; }
    }
}
