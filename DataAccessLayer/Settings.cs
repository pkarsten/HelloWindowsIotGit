using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using MSGraph;
using System.Collections.ObjectModel;
using MSGraph.Response;

namespace AppSettings
{
    public static class Settings
    {
        #region Background Tasks
        public const string CreateMessageTaskName = "CreateMessageTask";
        public const string LoadImagesFromOneDriveTaskName = "LoadImagesFromOneDriveTask";
        public const string LoadGraphDataTaskName = "LoadGraphDataTaskName";

        public static BitmapImage DashBoardImage { get; set; }
        public static ObservableCollection<CalendarEventItem> NextEvents { get; set; } = new ObservableCollection<CalendarEventItem>();
        public static ObservableCollection<CalendarEventItem> TodayEvents { get; set; } = new ObservableCollection<CalendarEventItem>();

        public static List<string> TaskList { get; } = new List<string>
            {
                Settings.CreateMessageTaskName,
                Settings.LoadImagesFromOneDriveTaskName
            };
        // Settings are saved in DB and 
        // public static ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        public static List<BGTaskModel> ListBgTasks { get; set; } = new List<BGTaskModel>
        {
            new BGTaskModel{Name =Settings.CreateMessageTaskName,EntryPoint="RWPBGTasks.CreateMessage",Registered=false},
            new BGTaskModel{Name =Settings.LoadImagesFromOneDriveTaskName,EntryPoint="RWPBGTasks.GetImageListFromOneDrive",Registered=false},
            new BGTaskModel{Name =Settings.LoadGraphDataTaskName,EntryPoint="RWPBGTasks.LoadGraphDataTaskName",Registered=false},
        };

        public static bool RegisterAllBackgroundTasks { get; } = true;
        public static bool RegisterSystemTriggerBackgroundTasks { get; } = false;

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
