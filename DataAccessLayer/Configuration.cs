namespace UwpSqliteDal
{

    /// <summary>
    /// Configuration for Windiows Runtime Component not accessible from extern 
    /// </summary>
    public sealed class Configuration
    {
        public static string PicFileNameInAppDataFolder { get; } = "mybgpicture";
        public static string DatabaseName { get; } = "Storage.WPTDB";
        public static Setup InitialSetupConfig { get; } = new Setup
        {
            Id = 1,
            EnableLogging = false,
            IntervalForSearchPictures = 60,
            IntervalForChangeWallPaper = 15
        };
        public static PicFilter InitialPicFilterConfig { get; } = new PicFilter
        {
            Id = 1,
            CommonFolderQuery = "GroupByRating",
            VirtualFolder = ""
        };

    }
}
