using SQLite;
using System;
using Windows.Storage;


namespace UwpSqliteDal
{
    /// <summary>
    /// Represents Picture Filter
    /// </summary>
    public sealed class PicFilter
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Current active CommonFolderQuery 
        /// </summary>
        public string CommonFolderQuery { get; set; }

        /// <summary>
        /// Current Active VirtualFolder for CommonFolderQuery
        /// </summary>
        public string VirtualFolder { get; set; }
    }
}
