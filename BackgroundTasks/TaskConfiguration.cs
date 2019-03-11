using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Xaml.Data;

namespace RWPBGTasks
{
    public static class BackgroundTaskConfig
    {


        // Because WinRt can use, but can't return Task<T> h ttp://dotnetbyexample.blogspot.de/2014/11/returning-task-from-windows-runtime.html
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CallFromBackgroundTask">Define if Function was called from Background Task or Startet by Manual Button Click in UI </param>
        /// <returns></returns>
        public static IAsyncOperation<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            return InternalRegisterBackgroundTaskAsync(taskEntryPoint, name, trigger, condition).AsAsyncOperation();
        }

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        private static async Task<BackgroundTaskRegistration> InternalRegisterBackgroundTaskAsync(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            Dal.SaveLogEntry(LogType.Info, "Register BackgroundTask " + name);
            BackgroundExecutionManager.RemoveAccess();

            var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();

            if (hasAccess == BackgroundAccessStatus.DeniedByUser)
            {
                Dal.SaveLogEntry(LogType.Error, "BackgroundAccessStatus.Denied " + name);
                return null;
            }

            var builder = new BackgroundTaskBuilder();
            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);
            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
                Dal.SaveLogEntry(LogType.Info, "CancelOnConditionLoss " + name);
            }

            BackgroundTaskRegistration task = builder.Register();
            Dal.SaveLogEntry(LogType.Info, "Background Task " + name + " Registered " + " at " + DateTime.Now);
            UpdateBackgroundTaskRegistrationStatus(name, true);

            return task;
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks(String name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                    Dal.SaveLogEntry(LogType.Info, "Unregister " + name);
                }
            }

            UpdateBackgroundTaskRegistrationStatus(name, false);
        }

        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        public static void UpdateBackgroundTaskRegistrationStatus(String name, bool registered)
        {
            switch (name)
            {
                case Settings.SearchPicturesTaskName:
                    Settings.SearchPicturesTaskRegistered = registered;
                    break;
                case Settings.ServicingCompleteTaskName:
                    Settings.ServicingCompleteTaskRegistered = registered;
                    break;
                case Settings.ChangeWallpaperTaskName:
                    Settings.ChangeWallpaperTaskRegistered = registered;
                    break;
            }
            var ts = Dal.GetTaskStatusByTaskName(name);
            if (ts != null)
            {
                var settings = ApplicationData.Current.LocalSettings;
                string additionalStatus = "";
                if (settings.Values.ContainsKey(name))
                {
                    additionalStatus = settings.Values[name].ToString();
                }

                ts.CurrentRegisteredStatus = registered;
                ts.AdditionalStatus = additionalStatus;
                Dal.UpdateTaskStatus(ts);
                Dal.SaveLogEntry(LogType.Info, string.Format("Update {0} status registered: {1} ", name, registered));
            }
        }



        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;
            switch (name)
            {
                case Settings.SearchPicturesTaskName:
                    registered = Settings.SearchPicturesTaskRegistered;
                    break;
                case Settings.ServicingCompleteTaskName:
                    registered = Settings.ServicingCompleteTaskRegistered;
                    break;
                case Settings.ChangeWallpaperTaskName:
                    registered = Settings.ChangeWallpaperTaskRegistered;
                    break;
            }

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static bool GetBackgroundRegisteredTaskStatus(String name)
        {
            var registered = false;

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    registered = true;
                }
            }
            return registered;
        }

        /// <summary>
        /// Determine if task with given name requires background access.
        /// </summary>
        /// <param name="name">Name of background task to query background access requirement.</param>
        public static bool TaskRequiresBackgroundAccess(String name)
        {
            if ((name == Settings.ChangeWallpaperTaskName) || (name == Settings.SearchPicturesTaskName) || name == Settings.ServicingCompleteTaskName)
            {
                Dal.SaveLogEntry(LogType.Info, name + "requires Background access");
                return true;
            }
            else
            {
                return false;
            }
        }
        private static async Task InternalReRegisterTasks()
        {
            Dal.SaveLogEntry(LogType.Info, "Re Register Tasks) ");
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name != Settings.ServicingCompleteTaskName)
                {
                    cur.Value.Unregister(true);
                    Dal.SaveLogEntry(LogType.Info, "Unregister BackgroundTask for RE Register " + cur.Value.Name);
                }
            }
            foreach (var tstatus in Dal.GetAllTaskStatus())
            {
                if ((tstatus.TaskName == Settings.SearchPicturesTaskName) && tstatus.CurrentRegisteredStatus == true)
                {
                    Settings.SearchPicturesTaskResult = "";
                    var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                           Settings.SearchPicturesTaskName,
                                                                            Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                                                                           null);
                }
                if ((tstatus.TaskName == Settings.ChangeWallpaperTaskName) && tstatus.CurrentRegisteredStatus == true)
                {
                    var t = await BackgroundTaskConfig.RegisterBackgroundTask(Settings.ChangeWallpaperTaskEntryPoint, Settings.ChangeWallpaperTaskName, Dal.GetTimeIntervalForTask(Settings.ChangeWallpaperTaskName), null);
                }

                //if ((tstatus.TaskName == Configuration.ServicingCompleteTaskName) && tstatus.CurrentRegisteredStatus == true)
                //{
                //    var t = await BackgroundTaskConfig.RegisterBackgroundTask(Configuration.ServicingCompleteTaskEntryPoint,
                //                                                 Configuration.ServicingCompleteTaskName,
                //                                                 new SystemTrigger(SystemTriggerType.ServicingComplete, false),
                //                                                 null);
                //}
            }
        }

        public static IAsyncOperation<bool> CheckForRegisteredTasks()
        {
            return InternalCheckForRegisteredTasks().AsAsyncOperation();
        }

        /// <summary>
        /// Check if Our Tasks are registered and Update the Registration Status
        /// </summary>
        private static async Task<bool> InternalCheckForRegisteredTasks()
        {
            // First Check if Task Table is Filled 
            foreach (string s in Settings.TaskList)
            {
                var ts = Dal.GetTaskStatusByTaskName(s);
                if (ts == null)
                {
                    RWPBGTasks.TaskStatus t = new RWPBGTasks.TaskStatus();
                    t.TaskName = s;
                    t.CurrentRegisteredStatus = false;
                    t.LastTimeRun = "";
                    t.AdditionalStatus = "";
                    Dal.UpdateTaskStatus(t);
                }
            }
            // Look for running Tasks and Set the Status 
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == Settings.SearchPicturesTaskName)
                {
                    UpdateBackgroundTaskRegistrationStatus(Settings.SearchPicturesTaskName, true);
                }
                if (task.Value.Name == Settings.ChangeWallpaperTaskName)
                {
                    UpdateBackgroundTaskRegistrationStatus(Settings.ChangeWallpaperTaskName, true);
                }
                if (task.Value.Name == Settings.ServicingCompleteTaskName)
                {
                    UpdateBackgroundTaskRegistrationStatus(Settings.ServicingCompleteTaskName, true);
                }
            }

            // Register Servicing Complete Task if there is not registered
            if (Settings.ServicingCompleteTaskRegistered == false)
            {
                var servicetask = await RegisterBackgroundTask(Settings.ServicingCompleteTaskEntryPoint,
                                                                    Settings.ServicingCompleteTaskName,
                                                                    new SystemTrigger(SystemTriggerType.ServicingComplete, false),
                                                                    null);
            }


            if ((Dal.GetTaskStatusByTaskName(Settings.ChangeWallpaperTaskName).CurrentRegisteredStatus == true) && Settings.ChangeWallpaperTaskRegistered == false)
            {
                // Register ChangeWallpaper Task if is not registered but Status iN DB say it must be registered
                var changeWPTask = await RegisterBackgroundTask(Settings.ChangeWallpaperTaskEntryPoint, Settings.ChangeWallpaperTaskName, Dal.GetTimeIntervalForTask(Settings.ChangeWallpaperTaskName), null);
            }


            if ((Dal.GetTaskStatusByTaskName(Settings.SearchPicturesTaskName).CurrentRegisteredStatus == true) && Settings.SearchPicturesTaskRegistered == false)
            {
                // Register SearchPictures Task if is not registered but Status iN DB say it must be registered
                var searchPicTask = await RegisterBackgroundTask(Settings.SearchPicturesTaskEntryPoint,
                                                                           Settings.SearchPicturesTaskName,
                                                                            Dal.GetTimeIntervalForTask(Settings.SearchPicturesTaskName),
                                                                           null);
            }
            return true;

        }
    }
}
