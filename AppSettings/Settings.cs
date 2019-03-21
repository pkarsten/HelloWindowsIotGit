using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.Storage.Search;

namespace AppSettings
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

        public const string CreateMessageTaskEntryPoint = "RWPBGTasks.CreateMessage";
        public const string CreateMessageTaskName = "CreateMessageTask";
        public static string CreateMessageTaskResult = "";
        public static string CreateMessageTaskProgress = "";
        public static bool CreateMessageTaskRegistered = false;

        public static BitmapImage DashBoardImage { get; set; }

        public static List<string> TaskList { get; } = new List<string>
            {
                Settings.SearchPicturesTaskName,
                Settings.ChangeWallpaperTaskName,
                Settings.ServicingCompleteTaskName,
                Settings.CreateMessageTaskName
            };
        // Settings are saved in DB and 
        // public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public  static List<BGTaskModel> ListBgTasks { get; set; } = new List<BGTaskModel>
        {
            new BGTaskModel{Name =Settings.ServicingCompleteTaskName,EntryPoint="RWPBGTasks.ServicingComplete",Registered=false},
            new BGTaskModel{Name =Settings.ChangeWallpaperTaskName,EntryPoint="RWPBGTasks.ChangeWallpaper",Registered=false},
            new BGTaskModel{Name =Settings.SearchPicturesTaskName,EntryPoint="RWPBGTasks.SearchPictures",Registered=false},
            new BGTaskModel{Name =Settings.CreateMessageTaskName,EntryPoint="RWPBGTasks.CreateMessage",Registered=false},
        };

        public static bool RegisterAllBackgroundTasks { get; } = true;
        public static bool RegisterSystemTriggerBackgroundTasks{ get; } = false;

        public static void UpdateTaskRegisteredSettings(string taskname, bool taskregistered)
        {
            int index = ListBgTasks.FindIndex(m => m.Name == taskname);
            if (index >= 0)
            {
                ListBgTasks[index].Registered = taskregistered;
            }
        }
        public static void UpdateTaskResultSettings(string taskname, string taskresult)
        {
            int index = ListBgTasks.FindIndex(m => m.Name == taskname);
            if (index >= 0)
            {
                ListBgTasks[index].Result = taskresult;
            }
        }
        public static void UpdateTaskProgressSettings(string taskname, string taskprogress)
        {
            int index = ListBgTasks.FindIndex(m => m.Name == taskname);
            if (index >= 0)
            {
                ListBgTasks[index].Progress = taskprogress;
            }
        }

        public static BGTaskModel TaskSettings(string taskname)
        {
            BGTaskModel myTask = null;
            int index = ListBgTasks.FindIndex(m => m.Name == taskname);
            if (index >= 0)
            {
                myTask = ListBgTasks[index];
            }
            return myTask;
        }

        #endregion


        #region UI
        public const string APP_NAME = "Hello WIndows IOT";
        public const string ProductIdinStore = "";
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

    public class BGTaskModel
    {

        public string Name { get; set; }
        public string EntryPoint { get; set; }
        public string Result { get; set; }
        public string Progress { get; set; }
        public bool Registered { get; set; }
    }
}
