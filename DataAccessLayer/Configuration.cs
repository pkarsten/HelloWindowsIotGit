using SQLite;
using System;
using System.IO;

namespace UwpSqliteDal
{

    /// <summary>
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// Configuration for Windows Runtime Component not accessible from extern
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public sealed class Configuration
    {
        public static string DatabaseName { get; } = "HelloWindowsIotDB.Sqlite";
        public static Setup InitialSetupConfig { get; } = new Setup
        {
            Id = 1,
            EnableLogging = false,
            IntervalForDiashow =10,
            IntervalForLoadPictures = 60,
            IntervalForLoadCalendarAndTasksInterval = 15,
            EnableCalendarAddon = false,
            EnableCalendarNextEvents =false,
            EnablePictureAddOn = false,
            EnableClock = true,
            EnablePurchaseTask = false,
            EnableTodayEvents =false,

        };
        public static PicFilter InitialPicFilterConfig { get; } = new PicFilter
        {
            Id = 1,
            CommonFolderQuery = "GroupByRating",
            VirtualFolder = ""
        };

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, Configuration.DatabaseName);
            }
        }

    }
}
