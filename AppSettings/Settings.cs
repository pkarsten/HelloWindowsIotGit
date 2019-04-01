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
        #region Background Tasks
        public const string ChangeWallpaperTaskEntryPoint = "RWPBGTasks.ChangeWallpaper";
        public const string ChangeWallpaperTaskName = "ChangeWallpaperTask";
        public static string ChangeWallpaperTaskResult = "";
        public static string ChangeWallpaperTaskProgress = "";
        public static bool ChangeWallpaperTaskRegistered = false;

        public const string CreateMessageTaskEntryPoint = "RWPBGTasks.CreateMessage";
        public const string CreateMessageTaskName = "CreateMessageTask";
        public static string CreateMessageTaskResult = "";
        public static string CreateMessageTaskProgress = "";
        public static bool CreateMessageTaskRegistered = false;

        public const string LoadImagesFromOneDriveTaskName ="LoadImagesFromOneDriveTask";
        public const string LoadGraphDataTaskName = "LoadGraphDataTaskName";

        public static BitmapImage DashBoardImage { get; set; }
        public static List<string> TaskList { get; } = new List<string>
            {
                Settings.ChangeWallpaperTaskName,
                Settings.CreateMessageTaskName,
                Settings.LoadImagesFromOneDriveTaskName
            };
        // Settings are saved in DB and 
        // public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public  static List<BGTaskModel> ListBgTasks { get; set; } = new List<BGTaskModel>
        {
            new BGTaskModel{Name =Settings.ChangeWallpaperTaskName,EntryPoint="RWPBGTasks.ChangeWallpaper",Registered=false},
            new BGTaskModel{Name =Settings.CreateMessageTaskName,EntryPoint="RWPBGTasks.CreateMessage",Registered=false},
            new BGTaskModel{Name =Settings.LoadImagesFromOneDriveTaskName,EntryPoint="RWPBGTasks.GetImageListFromOneDrive",Registered=false},
            new BGTaskModel{Name =Settings.LoadGraphDataTaskName,EntryPoint="RWPBGTasks.LoadGraphDataTaskName",Registered=false},
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
        public static bool LoadPictureListManually { get; set; }

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
