using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using MSGraph;
using System.Collections.ObjectModel;
using MSGraph.Response;
using HelloWindowsIot.Models;

namespace AppSettings
{
    public static class BGTasksSettings
    {
        #region Background Tasks
        /// <summary>
        /// Name for the CreateMessage Background Task 
        /// </summary>
        public const string CreateMessageTaskName = "CreateMessageTask";
        /// <summary>
        /// Name for the Load Images From OneDrive Background Task 
        /// </summary>
        public const string LoadImagesFromOneDriveTaskName = "LoadImagesFromOneDriveTask";

        /// <summary>
        /// Name for the Load MS Graph Data (Calendar, Tasks...) Background Task 
        /// </summary>
        public const string LoadGraphDataTaskName = "LoadGraphDataTask";

        /// <summary>
        /// List where we put the Backgroundstask for register wenn the App Runs, the CreateMessageTask is more for testing/debugging purposes
        /// </summary>
        public static List<BGTaskModel> ListBgTasks { get; set; } = new List<BGTaskModel>
        {
            new BGTaskModel{Name =LoadImagesFromOneDriveTaskName,EntryPoint="RWPBGTasks.GetImageListFromOneDrive",Registered=false},
            new BGTaskModel{Name =LoadGraphDataTaskName,EntryPoint="RWPBGTasks.LoadGraphData",Registered=false},
        };

        public static BitmapImage DashBoardImage { get; set; }
        public static ObservableCollection<CalendarEventItem> NextEvents { get; set; } = new ObservableCollection<CalendarEventItem>();
        public static ObservableCollection<CalendarEventItem> TodayEvents { get; set; } = new ObservableCollection<CalendarEventItem>();

        public static List<string> TaskList { get; } = new List<string>
            {
                BGTasksSettings.CreateMessageTaskName,
                BGTasksSettings.LoadImagesFromOneDriveTaskName
            };

        


        // new BGTaskModel{Name =Settings.CreateMessageTaskName,EntryPoint="RWPBGTasks.CreateMessage",Registered=false},

        public static bool RegisterAllBackgroundTasks { get; } = true;
        public static bool RegisterSystemTriggerBackgroundTasks { get; } = false;

        /*public static void UpdateTaskRegisteredSettings(string taskname, bool taskregistered)
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

        */

        #endregion
    }
}
