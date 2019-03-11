using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Search;

namespace RWPBGTasks
{
    public static class Settings
    {
        #region trial
        /// <summary>
        ///  For Trial Test Purpose Set this True 
        /// </summary>
        public static bool TestTrial { get; } = false;
        //TODO: Remove this when Online ? 
        #endregion

        #region Background Tasks
        public const string ServicingCompleteTaskEntryPoint = "RWPBGTasks.ServicingComplete";
        public const string ServicingCompleteTaskName = "ServicingCompleteTask";
        public static string ServicingCompleteTaskProgress = "";
        public static string ServicingCompleteTaskResult = "";
        public static bool ServicingCompleteTaskRegistered = false;

        public const string ChangeWallpaperTaskEntryPoint = "RWPBGTasks.ChangeWallpaper";
        public const string ChangeWallpaperTaskName = "ChangeWallpaperTask";
        public static string ChangeWallpaperTaskResult = "";
        public static string ChangeWallpaperTaskProgress = "";
        public static bool ChangeWallpaperTaskRegistered = false;


        public const string SearchPicturesTaskEntryPoint = "RWPBGTasks.SearchPictures";
        public const string SearchPicturesTaskName = "SearchPicturesTask";
        public static string SearchPicturesTaskResult = "";
        public static string SearchPicturesTaskProgress = "";
        public static bool SearchPicturesTaskRegistered = false;

        public static List<string> TaskList { get; } = new List<string>
            {
                Settings.SearchPicturesTaskName,
                Settings.ChangeWallpaperTaskName,
                Settings.ServicingCompleteTaskName,
            };
        #endregion

        // Settings are saved in DB and 
        //public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        #region UI
        public const string APP_NAME = "Random Rated Wallpapers";
        public const string ProductIdinStore = "9N9Q908QKJR8";
        public const string SupportEmail = "pkarsten@live.de";
        public const string SupporterFirstName = "Peter";

        #endregion

        #region Picture Filter Lists
        /// <summary>
        /// Represents all CommonFolderQuerys Enums for the Full Version, bool define if Allowed to see /select in ListView
        /// </summary>
        public static Dictionary<CommonFolderQuery, bool> AllowedFullVersionPicFilterList { get; } = new Dictionary<CommonFolderQuery, bool>
        {
            { CommonFolderQuery.DefaultQuery,false},
            { CommonFolderQuery.GroupByAlbum,true},
            { CommonFolderQuery.GroupByAlbumArtist,false},
            { CommonFolderQuery.GroupByArtist,true},
            { CommonFolderQuery.GroupByAuthor,true},
            { CommonFolderQuery.GroupByComposer,false},
            { CommonFolderQuery.GroupByGenre,true},
            { CommonFolderQuery.GroupByMonth,true},
            { CommonFolderQuery.GroupByPublishedYear,true},
            { CommonFolderQuery.GroupByRating,true},
            { CommonFolderQuery.GroupByTag,true},
            { CommonFolderQuery.GroupByType,true},
            { CommonFolderQuery.GroupByYear,true},
        };
        /// <summary>
        /// Represents all CommonFolderQuerys Enums for the Trial Version, bool define if Allowed to see /select in ListView
        /// </summary>
        public static Dictionary<CommonFolderQuery, bool> AllowedTrialVersionPicFilterList { get; } = new Dictionary<CommonFolderQuery, bool>
        {
            { CommonFolderQuery.DefaultQuery,false},
            { CommonFolderQuery.GroupByAlbum,false},
            { CommonFolderQuery.GroupByAlbumArtist,false},
            { CommonFolderQuery.GroupByArtist,false},
            { CommonFolderQuery.GroupByAuthor,false},
            { CommonFolderQuery.GroupByComposer,false},
            { CommonFolderQuery.GroupByGenre,false},
            { CommonFolderQuery.GroupByMonth,true},//
            { CommonFolderQuery.GroupByPublishedYear,false},
            { CommonFolderQuery.GroupByRating,true},//
            { CommonFolderQuery.GroupByTag,true},//
            { CommonFolderQuery.GroupByType,false},
            { CommonFolderQuery.GroupByYear,false},
        };
        #endregion

    }
}
