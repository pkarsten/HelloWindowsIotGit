using SQLite.Net.Attributes;
using System;
using Windows.Storage;

namespace UwpSqliteDal
{
    /// <summary>
    /// Represents a Log Entry 
    /// </summary>
    public sealed class LogEntry
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the LogType.
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        /// Gets or Sets the Log Deescription
        /// </summary>
        public string Description { get; set; }

        //Gets or Sets the Log Entry Date 
        public string LogEntryDate { get; set; }

    }

    public enum LogType
    {
        Info,AppInfo, Error, Manual, Exception
    }
}
