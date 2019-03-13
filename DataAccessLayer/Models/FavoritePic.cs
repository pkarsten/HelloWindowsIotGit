using SQLite.Net.Attributes;
using System;
using Windows.Storage;

namespace UwpSqliteDal
{
    /// <summary>
    /// Represents a Rated Picture 
    /// </summary>
    public sealed class FavoritePic
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// Gets or Sets the Number of Rated Stars
        /// </summary>
        public int Stars { get; set; }

        public string Name { get; set; }

        public string LibraryPath { get; set; }

        public bool Viewed { get; set; }

        public bool IsCurrentWallPaper { get; set; }
    }
}
