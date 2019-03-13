using SQLite.Net.Attributes;
using System;
using Windows.Storage;

namespace UwpSqliteDal
{
    /// <summary>
    /// Represents Setup Entry in Table  
    /// </summary>
    public sealed class Setup
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets If Logging is Enable
        /// </summary>
        public bool EnableLogging{ get; set; }

        /// <summary>
        /// Time Interval in Minutes for run the Background Task Search Pictures 
        /// </summary>
        public int IntervalForSearchPictures { get; set; }

        /// <summary>
        /// Time Interval in Minutes for run the Background Task Change WallPaper
        /// </summary>
        public int IntervalForChangeWallPaper { get; set; }


    }
}
